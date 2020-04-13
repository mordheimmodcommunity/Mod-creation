using Pathfinding.Recast;
using Pathfinding.Serialization;
using Pathfinding.Util;
using Pathfinding.Voxels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

namespace Pathfinding
{
    [Serializable]
    [JsonOptIn]
    public class RecastGraph : NavGraph, IUpdatableGraph, IRaycastableGraph, INavmesh, INavmeshHolder
    {
        public enum RelevantGraphSurfaceMode
        {
            DoNotRequire,
            OnlyForCompletelyInsideTile,
            RequireForAll
        }

        public class NavmeshTile : INavmesh, INavmeshHolder
        {
            public int[] tris;

            public Int3[] verts;

            public int x;

            public int z;

            public int w;

            public int d;

            public TriangleMeshNode[] nodes;

            public BBTree bbTree;

            public bool flag;

            public void GetTileCoordinates(int tileIndex, out int x, out int z)
            {
                x = this.x;
                z = this.z;
            }

            public int GetVertexArrayIndex(int index)
            {
                return index & 0xFFF;
            }

            public Int3 GetVertex(int index)
            {
                int num = index & 0xFFF;
                return verts[num];
            }

            public void GetNodes(GraphNodeDelegateCancelable del)
            {
                if (nodes != null)
                {
                    for (int i = 0; i < nodes.Length && del(nodes[i]); i++)
                    {
                    }
                }
            }
        }

        public const int VertexIndexMask = 4095;

        public const int TileIndexMask = 524287;

        public const int TileIndexOffset = 12;

        public const int BorderVertexMask = 1;

        public const int BorderVertexOffset = 31;

        public bool dynamic = true;

        [JsonMember]
        public float characterRadius = 1.5f;

        [JsonMember]
        public float contourMaxError = 2f;

        [JsonMember]
        public float cellSize = 0.5f;

        [JsonMember]
        public float walkableHeight = 2f;

        [JsonMember]
        public float walkableClimb = 0.5f;

        [JsonMember]
        public float maxSlope = 30f;

        [JsonMember]
        public float maxEdgeLength = 20f;

        [JsonMember]
        public float minRegionSize = 3f;

        [JsonMember]
        public int editorTileSize = 128;

        [JsonMember]
        public int tileSizeX = 128;

        [JsonMember]
        public int tileSizeZ = 128;

        [JsonMember]
        public bool nearestSearchOnlyXZ;

        [JsonMember]
        public bool useTiles;

        public bool scanEmptyGraph;

        [JsonMember]
        public RelevantGraphSurfaceMode relevantGraphSurfaceMode;

        [JsonMember]
        public bool rasterizeColliders;

        [JsonMember]
        public bool rasterizeMeshes = true;

        [JsonMember]
        public bool rasterizeTerrain = true;

        [JsonMember]
        public bool rasterizeTrees = true;

        [JsonMember]
        public float colliderRasterizeDetail = 10f;

        [JsonMember]
        public Vector3 forcedBoundsCenter;

        [JsonMember]
        public Vector3 forcedBoundsSize = new Vector3(100f, 40f, 100f);

        [JsonMember]
        public LayerMask mask = -1;

        [JsonMember]
        public List<string> tagMask = new List<string>();

        [JsonMember]
        public bool showMeshOutline = true;

        [JsonMember]
        public bool showNodeConnections;

        [JsonMember]
        public bool showMeshSurface;

        [JsonMember]
        public int terrainSampleSize = 3;

        private Voxelize globalVox;

        public int tileXCount;

        public int tileZCount;

        private NavmeshTile[] tiles;

        private bool batchTileUpdate;

        private List<int> batchUpdatedTiles = new List<int>();

        private List<NavmeshTile> stagingTiles = new List<NavmeshTile>();

        public Bounds forcedBounds => new Bounds(forcedBoundsCenter, forcedBoundsSize);

        private float CellHeight
        {
            get
            {
                Vector3 size = forcedBounds.size;
                return Mathf.Max(size.y / 64000f, 0.001f);
            }
        }

        private int CharacterRadiusInVoxels => Mathf.CeilToInt(characterRadius / cellSize - 0.1f);

        private int TileBorderSizeInVoxels => CharacterRadiusInVoxels + 3;

        private float TileBorderSizeInWorldUnits => (float)TileBorderSizeInVoxels * cellSize;

        public event Action<NavmeshTile[]> OnRecalculatedTiles;

        GraphUpdateThreading IUpdatableGraph.CanUpdateAsync(GraphUpdateObject o)
        {
            return (!o.updatePhysics) ? GraphUpdateThreading.SeparateThread : ((GraphUpdateThreading)7);
        }

        void IUpdatableGraph.UpdateAreaInit(GraphUpdateObject o)
        {
            if (o.updatePhysics)
            {
                if (!dynamic)
                {
                    throw new Exception("Recast graph must be marked as dynamic to enable graph updates");
                }
                RelevantGraphSurface.UpdateAllPositions();
                IntRect touchingTiles = GetTouchingTiles(o.bounds);
                Bounds tileBounds = GetTileBounds(touchingTiles);
                tileBounds.Expand(new Vector3(1f, 0f, 1f) * TileBorderSizeInWorldUnits * 2f);
                List<RasterizationMesh> inputMeshes = CollectMeshes(tileBounds);
                if (globalVox == null)
                {
                    globalVox = new Voxelize(CellHeight, cellSize, walkableClimb, walkableHeight, maxSlope);
                    globalVox.maxEdgeLength = maxEdgeLength;
                }
                globalVox.inputMeshes = inputMeshes;
            }
        }

        void IUpdatableGraph.UpdateArea(GraphUpdateObject guo)
        {
            IntRect touchingTiles = GetTouchingTiles(guo.bounds);
            if (!guo.updatePhysics)
            {
                for (int i = touchingTiles.ymin; i <= touchingTiles.ymax; i++)
                {
                    for (int j = touchingTiles.xmin; j <= touchingTiles.xmax; j++)
                    {
                        NavmeshTile graph = tiles[i * tileXCount + j];
                        NavMeshGraph.UpdateArea(guo, graph);
                    }
                }
                return;
            }
            if (!dynamic)
            {
                throw new Exception("Recast graph must be marked as dynamic to enable graph updates with updatePhysics = true");
            }
            Voxelize voxelize = globalVox;
            if (voxelize == null)
            {
                throw new InvalidOperationException("No Voxelizer object. UpdateAreaInit should have been called before this function.");
            }
            for (int k = touchingTiles.xmin; k <= touchingTiles.xmax; k++)
            {
                for (int l = touchingTiles.ymin; l <= touchingTiles.ymax; l++)
                {
                    stagingTiles.Add(BuildTileMesh(voxelize, k, l));
                }
            }
            uint graphIndex = (uint)AstarPath.active.astarData.GetGraphIndex(this);
            for (int m = 0; m < stagingTiles.Count; m++)
            {
                NavmeshTile navmeshTile = stagingTiles[m];
                GraphNode[] nodes = navmeshTile.nodes;
                for (int n = 0; n < nodes.Length; n++)
                {
                    nodes[n].GraphIndex = graphIndex;
                }
            }
        }

        void IUpdatableGraph.UpdateAreaPost(GraphUpdateObject guo)
        {
            for (int i = 0; i < stagingTiles.Count; i++)
            {
                NavmeshTile navmeshTile = stagingTiles[i];
                int num = navmeshTile.x + navmeshTile.z * tileXCount;
                NavmeshTile navmeshTile2 = tiles[num];
                for (int j = 0; j < navmeshTile2.nodes.Length; j++)
                {
                    navmeshTile2.nodes[j].Destroy();
                }
                tiles[num] = navmeshTile;
            }
            for (int k = 0; k < stagingTiles.Count; k++)
            {
                NavmeshTile tile = stagingTiles[k];
                ConnectTileWithNeighbours(tile);
            }
            if (this.OnRecalculatedTiles != null)
            {
                this.OnRecalculatedTiles(stagingTiles.ToArray());
            }
            stagingTiles.Clear();
        }

        public NavmeshTile GetTile(int x, int z)
        {
            return tiles[x + z * tileXCount];
        }

        public Int3 GetVertex(int index)
        {
            int num = (index >> 12) & 0x7FFFF;
            return tiles[num].GetVertex(index);
        }

        public int GetTileIndex(int index)
        {
            return (index >> 12) & 0x7FFFF;
        }

        public int GetVertexArrayIndex(int index)
        {
            return index & 0xFFF;
        }

        public void GetTileCoordinates(int tileIndex, out int x, out int z)
        {
            z = tileIndex / tileXCount;
            x = tileIndex - z * tileXCount;
        }

        public NavmeshTile[] GetTiles()
        {
            return tiles;
        }

        public Bounds GetTileBounds(IntRect rect)
        {
            return GetTileBounds(rect.xmin, rect.ymin, rect.Width, rect.Height);
        }

        public Bounds GetTileBounds(int x, int z, int width = 1, int depth = 1)
        {
            Bounds result = default(Bounds);
            Vector3 min = new Vector3((float)(x * tileSizeX) * cellSize, 0f, (float)(z * tileSizeZ) * cellSize) + forcedBounds.min;
            float x2 = (float)((x + width) * tileSizeX) * cellSize;
            Vector3 size = forcedBounds.size;
            result.SetMinMax(min, new Vector3(x2, size.y, (float)((z + depth) * tileSizeZ) * cellSize) + forcedBounds.min);
            return result;
        }

        public Int2 GetTileCoordinates(Vector3 p)
        {
            p -= forcedBounds.min;
            p.x /= cellSize * (float)tileSizeX;
            p.z /= cellSize * (float)tileSizeZ;
            return new Int2((int)p.x, (int)p.z);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            TriangleMeshNode.SetNavmeshHolder(active.astarData.GetGraphIndex(this), null);
        }

        public override void RelocateNodes(Matrix4x4 oldMatrix, Matrix4x4 newMatrix)
        {
            if (tiles != null)
            {
                Matrix4x4 inverse = oldMatrix.inverse;
                Matrix4x4 matrix4x = newMatrix * inverse;
                if (tiles.Length > 1)
                {
                    throw new Exception("RelocateNodes cannot be used on tiled recast graphs");
                }
                for (int i = 0; i < tiles.Length; i++)
                {
                    NavmeshTile navmeshTile = tiles[i];
                    if (navmeshTile != null)
                    {
                        Int3[] verts = navmeshTile.verts;
                        for (int j = 0; j < verts.Length; j++)
                        {
                            verts[j] = (Int3)matrix4x.MultiplyPoint((Vector3)verts[j]);
                        }
                        for (int k = 0; k < navmeshTile.nodes.Length; k++)
                        {
                            TriangleMeshNode triangleMeshNode = navmeshTile.nodes[k];
                            triangleMeshNode.UpdatePositionFromVertices();
                        }
                        navmeshTile.bbTree.RebuildFrom(navmeshTile.nodes);
                    }
                }
            }
            SetMatrix(newMatrix);
        }

        private static NavmeshTile NewEmptyTile(int x, int z)
        {
            NavmeshTile navmeshTile = new NavmeshTile();
            navmeshTile.x = x;
            navmeshTile.z = z;
            navmeshTile.w = 1;
            navmeshTile.d = 1;
            navmeshTile.verts = new Int3[0];
            navmeshTile.tris = new int[0];
            navmeshTile.nodes = new TriangleMeshNode[0];
            navmeshTile.bbTree = ObjectPool<BBTree>.Claim();
            return navmeshTile;
        }

        public override void GetNodes(GraphNodeDelegateCancelable del)
        {
            if (tiles == null)
            {
                return;
            }
            for (int i = 0; i < tiles.Length; i++)
            {
                if (tiles[i] == null || tiles[i].x + tiles[i].z * tileXCount != i)
                {
                    continue;
                }
                TriangleMeshNode[] nodes = tiles[i].nodes;
                if (nodes != null)
                {
                    for (int j = 0; j < nodes.Length && del(nodes[j]); j++)
                    {
                    }
                }
            }
        }

        [Obsolete("Use node.ClosestPointOnNode instead")]
        public Vector3 ClosestPointOnNode(TriangleMeshNode node, Vector3 pos)
        {
            return Polygon.ClosestPointOnTriangle((Vector3)GetVertex(node.v0), (Vector3)GetVertex(node.v1), (Vector3)GetVertex(node.v2), pos);
        }

        [Obsolete("Use node.ContainsPoint instead")]
        public bool ContainsPoint(TriangleMeshNode node, Vector3 pos)
        {
            if (VectorMath.IsClockwiseXZ((Vector3)GetVertex(node.v0), (Vector3)GetVertex(node.v1), pos) && VectorMath.IsClockwiseXZ((Vector3)GetVertex(node.v1), (Vector3)GetVertex(node.v2), pos) && VectorMath.IsClockwiseXZ((Vector3)GetVertex(node.v2), (Vector3)GetVertex(node.v0), pos))
            {
                return true;
            }
            return false;
        }

        public void SnapForceBoundsToScene()
        {
            List<RasterizationMesh> list = CollectMeshes(new Bounds(Vector3.zero, new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity)));
            if (list.Count != 0)
            {
                Bounds bounds = list[0].bounds;
                for (int i = 1; i < list.Count; i++)
                {
                    bounds.Encapsulate(list[i].bounds);
                }
                forcedBoundsCenter = bounds.center;
                forcedBoundsSize = bounds.size;
            }
        }

        public IntRect GetTouchingTiles(Bounds b)
        {
            b.center -= forcedBounds.min;
            Vector3 min = b.min;
            int xmin = Mathf.FloorToInt(min.x / ((float)tileSizeX * cellSize));
            Vector3 min2 = b.min;
            int ymin = Mathf.FloorToInt(min2.z / ((float)tileSizeZ * cellSize));
            Vector3 max = b.max;
            int xmax = Mathf.FloorToInt(max.x / ((float)tileSizeX * cellSize));
            Vector3 max2 = b.max;
            IntRect a = new IntRect(xmin, ymin, xmax, Mathf.FloorToInt(max2.z / ((float)tileSizeZ * cellSize)));
            return IntRect.Intersection(a, new IntRect(0, 0, tileXCount - 1, tileZCount - 1));
        }

        public IntRect GetTouchingTilesRound(Bounds b)
        {
            b.center -= forcedBounds.min;
            Vector3 min = b.min;
            int xmin = Mathf.RoundToInt(min.x / ((float)tileSizeX * cellSize));
            Vector3 min2 = b.min;
            int ymin = Mathf.RoundToInt(min2.z / ((float)tileSizeZ * cellSize));
            Vector3 max = b.max;
            int xmax = Mathf.RoundToInt(max.x / ((float)tileSizeX * cellSize)) - 1;
            Vector3 max2 = b.max;
            IntRect a = new IntRect(xmin, ymin, xmax, Mathf.RoundToInt(max2.z / ((float)tileSizeZ * cellSize)) - 1);
            return IntRect.Intersection(a, new IntRect(0, 0, tileXCount - 1, tileZCount - 1));
        }

        private void ConnectTileWithNeighbours(NavmeshTile tile, bool onlyUnflagged = false)
        {
            if (tile.w != 1 || tile.d != 1)
            {
                throw new ArgumentException("Tile widths or depths other than 1 are not supported. The fields exist mainly for possible future expansions.");
            }
            for (int i = -1; i <= 1; i++)
            {
                int num = tile.z + i;
                if (num < 0 || num >= tileZCount)
                {
                    continue;
                }
                for (int j = -1; j <= 1; j++)
                {
                    int num2 = tile.x + j;
                    if (num2 >= 0 && num2 < tileXCount && j == 0 != (i == 0))
                    {
                        NavmeshTile navmeshTile = tiles[num2 + num * tileXCount];
                        if (!onlyUnflagged || !navmeshTile.flag)
                        {
                            ConnectTiles(navmeshTile, tile);
                        }
                    }
                }
            }
        }

        private void RemoveConnectionsFromTile(NavmeshTile tile)
        {
            if (tile.x > 0)
            {
                int num = tile.x - 1;
                for (int i = tile.z; i < tile.z + tile.d; i++)
                {
                    RemoveConnectionsFromTo(tiles[num + i * tileXCount], tile);
                }
            }
            if (tile.x + tile.w < tileXCount)
            {
                int num2 = tile.x + tile.w;
                for (int j = tile.z; j < tile.z + tile.d; j++)
                {
                    RemoveConnectionsFromTo(tiles[num2 + j * tileXCount], tile);
                }
            }
            if (tile.z > 0)
            {
                int num3 = tile.z - 1;
                for (int k = tile.x; k < tile.x + tile.w; k++)
                {
                    RemoveConnectionsFromTo(tiles[k + num3 * tileXCount], tile);
                }
            }
            if (tile.z + tile.d < tileZCount)
            {
                int num4 = tile.z + tile.d;
                for (int l = tile.x; l < tile.x + tile.w; l++)
                {
                    RemoveConnectionsFromTo(tiles[l + num4 * tileXCount], tile);
                }
            }
        }

        private void RemoveConnectionsFromTo(NavmeshTile a, NavmeshTile b)
        {
            if (a == null || b == null || a == b)
            {
                return;
            }
            int num = b.x + b.z * tileXCount;
            for (int i = 0; i < a.nodes.Length; i++)
            {
                TriangleMeshNode triangleMeshNode = a.nodes[i];
                if (triangleMeshNode.connections == null)
                {
                    continue;
                }
                for (int j = 0; j < triangleMeshNode.connections.Length; j++)
                {
                    TriangleMeshNode triangleMeshNode2 = triangleMeshNode.connections[j] as TriangleMeshNode;
                    if (triangleMeshNode2 != null)
                    {
                        int vertexIndex = triangleMeshNode2.GetVertexIndex(0);
                        vertexIndex = ((vertexIndex >> 12) & 0x7FFFF);
                        if (vertexIndex == num)
                        {
                            triangleMeshNode.RemoveConnection(triangleMeshNode.connections[j]);
                            j--;
                        }
                    }
                }
            }
        }

        public override NNInfoInternal GetNearest(Vector3 position, NNConstraint constraint, GraphNode hint)
        {
            return GetNearestForce(position, null);
        }

        public override NNInfoInternal GetNearestForce(Vector3 position, NNConstraint constraint)
        {
            if (tiles == null)
            {
                return default(NNInfoInternal);
            }
            Vector3 vector = position - forcedBounds.min;
            int value = Mathf.FloorToInt(vector.x / (cellSize * (float)tileSizeX));
            int value2 = Mathf.FloorToInt(vector.z / (cellSize * (float)tileSizeZ));
            value = Mathf.Clamp(value, 0, tileXCount - 1);
            value2 = Mathf.Clamp(value2, 0, tileZCount - 1);
            int num = Math.Max(tileXCount, tileZCount);
            NNInfoInternal nNInfoInternal = default(NNInfoInternal);
            float distance = float.PositiveInfinity;
            bool flag = nearestSearchOnlyXZ || (constraint?.distanceXZ ?? false);
            for (int i = 0; i < num && (flag || !(distance < (float)(i - 1) * cellSize * (float)Math.Max(tileSizeX, tileSizeZ))); i++)
            {
                int num2 = Math.Min(i + value2 + 1, tileZCount);
                for (int j = Math.Max(-i + value2, 0); j < num2; j++)
                {
                    int num3 = Math.Abs(i - Math.Abs(j - value2));
                    if (-num3 + value >= 0)
                    {
                        int num4 = -num3 + value;
                        NavmeshTile navmeshTile = tiles[num4 + j * tileXCount];
                        if (navmeshTile != null)
                        {
                            if (flag)
                            {
                                nNInfoInternal = navmeshTile.bbTree.QueryClosestXZ(position, constraint, ref distance, nNInfoInternal);
                                if (distance < float.PositiveInfinity)
                                {
                                    break;
                                }
                            }
                            else
                            {
                                nNInfoInternal = navmeshTile.bbTree.QueryClosest(position, constraint, ref distance, nNInfoInternal);
                            }
                        }
                    }
                    if (num3 == 0 || num3 + value >= tileXCount)
                    {
                        continue;
                    }
                    int num5 = num3 + value;
                    NavmeshTile navmeshTile2 = tiles[num5 + j * tileXCount];
                    if (navmeshTile2 == null)
                    {
                        continue;
                    }
                    if (flag)
                    {
                        nNInfoInternal = navmeshTile2.bbTree.QueryClosestXZ(position, constraint, ref distance, nNInfoInternal);
                        if (distance < float.PositiveInfinity)
                        {
                            break;
                        }
                    }
                    else
                    {
                        nNInfoInternal = navmeshTile2.bbTree.QueryClosest(position, constraint, ref distance, nNInfoInternal);
                    }
                }
            }
            nNInfoInternal.node = nNInfoInternal.constrainedNode;
            nNInfoInternal.constrainedNode = null;
            nNInfoInternal.clampedPosition = nNInfoInternal.constClampedPosition;
            return nNInfoInternal;
        }

        public GraphNode PointOnNavmesh(Vector3 position, NNConstraint constraint)
        {
            if (tiles == null)
            {
                return null;
            }
            Vector3 vector = position - forcedBounds.min;
            int num = Mathf.FloorToInt(vector.x / (cellSize * (float)tileSizeX));
            int num2 = Mathf.FloorToInt(vector.z / (cellSize * (float)tileSizeZ));
            if (num < 0 || num2 < 0 || num >= tileXCount || num2 >= tileZCount)
            {
                return null;
            }
            return tiles[num + num2 * tileXCount]?.bbTree.QueryInside(position, constraint);
        }

        public override IEnumerable<Progress> ScanInternal()
        {
            TriangleMeshNode.SetNavmeshHolder(AstarPath.active.astarData.GetGraphIndex(this), this);
            foreach (Progress item in ScanAllTiles())
            {
                yield return item;
            }
        }

        private void InitializeTileInfo()
        {
            Vector3 size = forcedBounds.size;
            int num = Mathf.Max((int)(size.x / cellSize + 0.5f), 1);
            Vector3 size2 = forcedBounds.size;
            int num2 = Mathf.Max((int)(size2.z / cellSize + 0.5f), 1);
            if (!useTiles)
            {
                tileSizeX = num;
                tileSizeZ = num2;
            }
            else
            {
                tileSizeX = editorTileSize;
                tileSizeZ = editorTileSize;
            }
            tileXCount = (num + tileSizeX - 1) / tileSizeX;
            tileZCount = (num2 + tileSizeZ - 1) / tileSizeZ;
            if (tileXCount * tileZCount > 524288)
            {
                throw new Exception("Too many tiles (" + tileXCount * tileZCount + ") maximum is " + 524288 + "\nTry disabling ASTAR_RECAST_LARGER_TILES under the 'Optimizations' tab in the A* inspector.");
            }
            tiles = new NavmeshTile[tileXCount * tileZCount];
        }

        private void BuildTiles(Queue<Int2> tileQueue, List<RasterizationMesh>[] meshBuckets, ManualResetEvent doneEvent, int threadIndex)
        {
            //Discarded unreachable code: IL_00b2
            try
            {
                Voxelize voxelize = new Voxelize(CellHeight, cellSize, walkableClimb, walkableHeight, maxSlope);
                voxelize.maxEdgeLength = maxEdgeLength;
                while (true)
                {
                    Int2 @int;
                    lock (tileQueue)
                    {
                        if (tileQueue.Count == 0)
                        {
                            return;
                        }
                        @int = tileQueue.Dequeue();
                    }
                    voxelize.inputMeshes = meshBuckets[@int.x + @int.y * tileXCount];
                    tiles[@int.x + @int.y * tileXCount] = BuildTileMesh(voxelize, @int.x, @int.y, threadIndex);
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            finally
            {
                doneEvent?.Set();
            }
        }

        private void ConnectTiles(Queue<Int2> tileQueue, ManualResetEvent doneEvent)
        {
            //Discarded unreachable code: IL_00dc
            try
            {
                while (true)
                {
                    Int2 @int;
                    lock (tileQueue)
                    {
                        if (tileQueue.Count == 0)
                        {
                            return;
                        }
                        @int = tileQueue.Dequeue();
                    }
                    if (@int.x < tileXCount - 1)
                    {
                        ConnectTiles(tiles[@int.x + @int.y * tileXCount], tiles[@int.x + 1 + @int.y * tileXCount]);
                    }
                    if (@int.y < tileZCount - 1)
                    {
                        ConnectTiles(tiles[@int.x + @int.y * tileXCount], tiles[@int.x + (@int.y + 1) * tileXCount]);
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            finally
            {
                doneEvent?.Set();
            }
        }

        private List<RasterizationMesh>[] PutMeshesIntoTileBuckets(List<RasterizationMesh> meshes)
        {
            List<RasterizationMesh>[] array = new List<RasterizationMesh>[tiles.Length];
            Vector3 amount = new Vector3(1f, 0f, 1f) * TileBorderSizeInWorldUnits * 2f;
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = new List<RasterizationMesh>();
            }
            for (int j = 0; j < meshes.Count; j++)
            {
                RasterizationMesh rasterizationMesh = meshes[j];
                Bounds bounds = rasterizationMesh.bounds;
                bounds.Expand(amount);
                IntRect touchingTiles = GetTouchingTiles(bounds);
                for (int k = touchingTiles.ymin; k <= touchingTiles.ymax; k++)
                {
                    for (int l = touchingTiles.xmin; l <= touchingTiles.xmax; l++)
                    {
                        array[l + k * tileXCount].Add(rasterizationMesh);
                    }
                }
            }
            return array;
        }

        protected IEnumerable<Progress> ScanAllTiles()
        {
            InitializeTileInfo();
            if (scanEmptyGraph)
            {
                FillWithEmptyTiles();
                yield break;
            }
            yield return new Progress(0f, "Finding Meshes");
            List<RasterizationMesh> meshes = CollectMeshes(forcedBounds);
            walkableClimb = Mathf.Min(walkableClimb, walkableHeight);
            List<RasterizationMesh>[] buckets = PutMeshesIntoTileBuckets(meshes);
            Queue<Int2> tileQueue = new Queue<Int2>();
            for (int z = 0; z < tileZCount; z++)
            {
                for (int x = 0; x < tileXCount; x++)
                {
                    tileQueue.Enqueue(new Int2(x, z));
                }
            }
            int threadCount = Mathf.Min(tileQueue.Count, Mathf.Max(1, AstarPath.CalculateThreadCount(ThreadCount.AutomaticHighLoad)));
            ManualResetEvent[] waitEvents = new ManualResetEvent[threadCount];
            for (int k = 0; k < waitEvents.Length; k++)
            {
                waitEvents[k] = new ManualResetEvent(initialState: false);
                ThreadPool.QueueUserWorkItem(delegate (object state)
                {
                    BuildTiles(tileQueue, buckets, waitEvents[(int)state], (int)state);
                }, k);
            }
            int timeoutMillis = Application.isPlaying ? 1 : 200;
            while (!WaitHandle.WaitAll(waitEvents, timeoutMillis))
            {
                int count;
                lock (tileQueue)
                {
                    count = tileQueue.Count;
                }
                yield return new Progress(Mathf.Lerp(0.1f, 0.9f, (float)(tiles.Length - count + 1) / (float)tiles.Length), "Generating Tile " + (tiles.Length - count + 1) + "/" + tiles.Length);
            }
            yield return new Progress(0.9f, "Assigning Graph Indices");
            uint graphIndex = (uint)AstarPath.active.astarData.GetGraphIndex(this);
            GetNodes(delegate (GraphNode node)
            {
                node.GraphIndex = graphIndex;
                return true;
            });
            for (int coordinateSum = 0; coordinateSum <= 1; coordinateSum++)
            {
                for (int j = 0; j < tiles.Length; j++)
                {
                    if ((tiles[j].x + tiles[j].z) % 2 == coordinateSum)
                    {
                        tileQueue.Enqueue(new Int2(tiles[j].x, tiles[j].z));
                    }
                }
                for (int i = 0; i < waitEvents.Length; i++)
                {
                    waitEvents[i].Reset();
                    ThreadPool.QueueUserWorkItem(delegate (object state)
                    {
                        ConnectTiles(tileQueue, state as ManualResetEvent);
                    }, waitEvents[i]);
                }
                while (!WaitHandle.WaitAll(waitEvents, timeoutMillis))
                {
                    int count2;
                    lock (tileQueue)
                    {
                        count2 = tileQueue.Count;
                    }
                    yield return new Progress(Mathf.Lerp(0.9f, 1f, (float)(tiles.Length - count2 + 1) / (float)tiles.Length), "Connecting Tile " + (tiles.Length - count2 + 1) + "/" + tiles.Length + " (Phase " + (coordinateSum + 1) + ")");
                }
            }
            if (this.OnRecalculatedTiles != null)
            {
                this.OnRecalculatedTiles(tiles.Clone() as NavmeshTile[]);
            }
        }

        private List<RasterizationMesh> CollectMeshes(Bounds bounds)
        {
            List<RasterizationMesh> list = new List<RasterizationMesh>();
            RecastMeshGatherer recastMeshGatherer = new RecastMeshGatherer(bounds, terrainSampleSize, mask, tagMask, colliderRasterizeDetail);
            if (rasterizeMeshes)
            {
                recastMeshGatherer.CollectSceneMeshes(list);
            }
            recastMeshGatherer.CollectRecastMeshObjs(list);
            if (rasterizeTerrain)
            {
                float desiredChunkSize = cellSize * (float)Math.Max(tileSizeX, tileSizeZ);
                recastMeshGatherer.CollectTerrainMeshes(rasterizeTrees, desiredChunkSize, list);
            }
            if (rasterizeColliders)
            {
                recastMeshGatherer.CollectColliderMeshes(list);
            }
            if (list.Count == 0)
            {
                Debug.LogWarning("No MeshFilters were found contained in the layers specified by the 'mask' variables");
            }
            return list;
        }

        private void FillWithEmptyTiles()
        {
            for (int i = 0; i < tileZCount; i++)
            {
                for (int j = 0; j < tileXCount; j++)
                {
                    tiles[i * tileXCount + j] = NewEmptyTile(j, i);
                }
            }
        }

        private Bounds CalculateTileBoundsWithBorder(int x, int z)
        {
            float num = (float)tileSizeX * cellSize;
            float num2 = (float)tileSizeZ * cellSize;
            Vector3 min = forcedBounds.min;
            Vector3 max = forcedBounds.max;
            Bounds result = default(Bounds);
            result.SetMinMax(new Vector3((float)x * num, 0f, (float)z * num2) + min, new Vector3((float)(x + 1) * num + min.x, max.y, (float)(z + 1) * num2 + min.z));
            result.Expand(new Vector3(1f, 0f, 1f) * TileBorderSizeInWorldUnits * 2f);
            return result;
        }

        protected NavmeshTile BuildTileMesh(Voxelize vox, int x, int z, int threadIndex = 0)
        {
            vox.borderSize = TileBorderSizeInVoxels;
            vox.forcedBounds = CalculateTileBoundsWithBorder(x, z);
            vox.width = tileSizeX + vox.borderSize * 2;
            vox.depth = tileSizeZ + vox.borderSize * 2;
            if (!useTiles && relevantGraphSurfaceMode == RelevantGraphSurfaceMode.OnlyForCompletelyInsideTile)
            {
                vox.relevantGraphSurfaceMode = RelevantGraphSurfaceMode.RequireForAll;
            }
            else
            {
                vox.relevantGraphSurfaceMode = relevantGraphSurfaceMode;
            }
            vox.minRegionSize = Mathf.RoundToInt(minRegionSize / (cellSize * cellSize));
            vox.Init();
            vox.VoxelizeInput();
            vox.FilterLedges(vox.voxelWalkableHeight, vox.voxelWalkableClimb, vox.cellSize, vox.cellHeight, vox.forcedBounds.min);
            vox.FilterLowHeightSpans(vox.voxelWalkableHeight, vox.cellSize, vox.cellHeight, vox.forcedBounds.min);
            vox.BuildCompactField();
            vox.BuildVoxelConnections();
            vox.ErodeWalkableArea(CharacterRadiusInVoxels);
            vox.BuildDistanceField();
            vox.BuildRegions();
            VoxelContourSet cset = new VoxelContourSet();
            vox.BuildContours(contourMaxError, 1, cset, 1);
            vox.BuildPolyMesh(cset, 3, out VoxelMesh mesh);
            for (int i = 0; i < mesh.verts.Length; i++)
            {
                mesh.verts[i] = vox.VoxelToWorldInt3(mesh.verts[i]);
            }
            return CreateTile(vox, mesh, x, z, threadIndex);
        }

        private NavmeshTile CreateTile(Voxelize vox, VoxelMesh mesh, int x, int z, int threadIndex = 0)
        {
            if (mesh.tris == null)
            {
                throw new ArgumentNullException("mesh.tris");
            }
            if (mesh.verts == null)
            {
                throw new ArgumentNullException("mesh.verts");
            }
            NavmeshTile navmeshTile = new NavmeshTile();
            navmeshTile.x = x;
            navmeshTile.z = z;
            navmeshTile.w = 1;
            navmeshTile.d = 1;
            navmeshTile.tris = mesh.tris;
            navmeshTile.verts = mesh.verts;
            navmeshTile.bbTree = new BBTree();
            NavmeshTile navmeshTile2 = navmeshTile;
            if (navmeshTile2.tris.Length % 3 != 0)
            {
                throw new ArgumentException("Indices array's length must be a multiple of 3 (mesh.tris)");
            }
            if (navmeshTile2.verts.Length >= 4095)
            {
                if (tileXCount * tileZCount == 1)
                {
                    throw new ArgumentException("Too many vertices per tile (more than " + 4095 + ").\n<b>Try enabling tiling in the recast graph settings.</b>\n");
                }
                throw new ArgumentException("Too many vertices per tile (more than " + 4095 + ").\n<b>Try reducing tile size or enabling ASTAR_RECAST_LARGER_TILES under the 'Optimizations' tab in the A* Inspector</b>");
            }
            navmeshTile2.verts = Utility.RemoveDuplicateVertices(navmeshTile2.verts, navmeshTile2.tris);
            TriangleMeshNode[] array = navmeshTile2.nodes = new TriangleMeshNode[navmeshTile2.tris.Length / 3];
            uint num = (uint)(AstarPath.active.astarData.graphs.Length + threadIndex);
            if (num > 255)
            {
                throw new Exception("Graph limit reached. Multithreaded recast calculations cannot be done because a few scratch graph indices are required.");
            }
            int num2 = x + z * tileXCount;
            num2 <<= 12;
            TriangleMeshNode.SetNavmeshHolder((int)num, navmeshTile2);
            lock (AstarPath.active)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    TriangleMeshNode triangleMeshNode = array[i] = new TriangleMeshNode(active);
                    triangleMeshNode.GraphIndex = num;
                    triangleMeshNode.v0 = (navmeshTile2.tris[i * 3] | num2);
                    triangleMeshNode.v1 = (navmeshTile2.tris[i * 3 + 1] | num2);
                    triangleMeshNode.v2 = (navmeshTile2.tris[i * 3 + 2] | num2);
                    if (!VectorMath.IsClockwiseXZ(triangleMeshNode.GetVertex(0), triangleMeshNode.GetVertex(1), triangleMeshNode.GetVertex(2)))
                    {
                        int v = triangleMeshNode.v0;
                        triangleMeshNode.v0 = triangleMeshNode.v2;
                        triangleMeshNode.v2 = v;
                    }
                    triangleMeshNode.Walkable = true;
                    triangleMeshNode.Penalty = initialPenalty;
                    triangleMeshNode.UpdatePositionFromVertices();
                }
            }
            navmeshTile2.bbTree.RebuildFrom(array);
            CreateNodeConnections(navmeshTile2.nodes);
            TriangleMeshNode.SetNavmeshHolder((int)num, null);
            return navmeshTile2;
        }

        private void CreateNodeConnections(TriangleMeshNode[] nodes)
        {
            List<MeshNode> list = ListPool<MeshNode>.Claim();
            List<uint> list2 = ListPool<uint>.Claim();
            Dictionary<Int2, int> obj = ObjectPoolSimple<Dictionary<Int2, int>>.Claim();
            obj.Clear();
            for (int i = 0; i < nodes.Length; i++)
            {
                TriangleMeshNode triangleMeshNode = nodes[i];
                int vertexCount = triangleMeshNode.GetVertexCount();
                for (int j = 0; j < vertexCount; j++)
                {
                    Int2 key = new Int2(triangleMeshNode.GetVertexIndex(j), triangleMeshNode.GetVertexIndex((j + 1) % vertexCount));
                    if (!obj.ContainsKey(key))
                    {
                        obj.Add(key, i);
                    }
                }
            }
            foreach (TriangleMeshNode triangleMeshNode2 in nodes)
            {
                list.Clear();
                list2.Clear();
                int vertexCount2 = triangleMeshNode2.GetVertexCount();
                for (int l = 0; l < vertexCount2; l++)
                {
                    int vertexIndex = triangleMeshNode2.GetVertexIndex(l);
                    int vertexIndex2 = triangleMeshNode2.GetVertexIndex((l + 1) % vertexCount2);
                    if (!obj.TryGetValue(new Int2(vertexIndex2, vertexIndex), out int value))
                    {
                        continue;
                    }
                    TriangleMeshNode triangleMeshNode3 = nodes[value];
                    int vertexCount3 = triangleMeshNode3.GetVertexCount();
                    for (int m = 0; m < vertexCount3; m++)
                    {
                        if (triangleMeshNode3.GetVertexIndex(m) == vertexIndex2 && triangleMeshNode3.GetVertexIndex((m + 1) % vertexCount3) == vertexIndex)
                        {
                            uint costMagnitude = (uint)(triangleMeshNode2.position - triangleMeshNode3.position).costMagnitude;
                            list.Add(triangleMeshNode3);
                            list2.Add(costMagnitude);
                            break;
                        }
                    }
                }
                triangleMeshNode2.connections = list.ToArray();
                triangleMeshNode2.connectionCosts = list2.ToArray();
            }
            obj.Clear();
            ObjectPoolSimple<Dictionary<Int2, int>>.Release(ref obj);
            ListPool<MeshNode>.Release(list);
            ListPool<uint>.Release(list2);
        }

        private void ConnectTiles(NavmeshTile tile1, NavmeshTile tile2)
        {
            if (tile1 == null || tile2 == null)
            {
                return;
            }
            if (tile1.nodes == null)
            {
                throw new ArgumentException("tile1 does not contain any nodes");
            }
            if (tile2.nodes == null)
            {
                throw new ArgumentException("tile2 does not contain any nodes");
            }
            int num = Mathf.Clamp(tile2.x, tile1.x, tile1.x + tile1.w - 1);
            int num2 = Mathf.Clamp(tile1.x, tile2.x, tile2.x + tile2.w - 1);
            int num3 = Mathf.Clamp(tile2.z, tile1.z, tile1.z + tile1.d - 1);
            int num4 = Mathf.Clamp(tile1.z, tile2.z, tile2.z + tile2.d - 1);
            int num5;
            int i;
            int num6;
            int num7;
            float num8;
            if (num == num2)
            {
                num5 = 2;
                i = 0;
                num6 = num3;
                num7 = num4;
                num8 = (float)tileSizeZ * cellSize;
            }
            else
            {
                if (num3 != num4)
                {
                    throw new ArgumentException("Tiles are not adjacent (neither x or z coordinates match)");
                }
                num5 = 0;
                i = 2;
                num6 = num;
                num7 = num2;
                num8 = (float)tileSizeX * cellSize;
            }
            if (Math.Abs(num6 - num7) != 1)
            {
                Debug.Log(tile1.x + " " + tile1.z + " " + tile1.w + " " + tile1.d + "\n" + tile2.x + " " + tile2.z + " " + tile2.w + " " + tile2.d + "\n" + num + " " + num3 + " " + num2 + " " + num4);
                throw new ArgumentException("Tiles are not adjacent (tile coordinates must differ by exactly 1. Got '" + num6 + "' and '" + num7 + "')");
            }
            int num9 = (int)Math.Round(((float)Math.Max(num6, num7) * num8 + forcedBounds.min[num5]) * 1000f);
            TriangleMeshNode[] nodes = tile1.nodes;
            TriangleMeshNode[] nodes2 = tile2.nodes;
            foreach (TriangleMeshNode triangleMeshNode in nodes)
            {
                int vertexCount = triangleMeshNode.GetVertexCount();
                for (int k = 0; k < vertexCount; k++)
                {
                    Int3 vertex = triangleMeshNode.GetVertex(k);
                    Int3 vertex2 = triangleMeshNode.GetVertex((k + 1) % vertexCount);
                    if (Math.Abs(vertex[num5] - num9) >= 2 || Math.Abs(vertex2[num5] - num9) >= 2)
                    {
                        continue;
                    }
                    int num10 = Math.Min(vertex[i], vertex2[i]);
                    int num11 = Math.Max(vertex[i], vertex2[i]);
                    if (num10 == num11)
                    {
                        continue;
                    }
                    foreach (TriangleMeshNode triangleMeshNode2 in nodes2)
                    {
                        int vertexCount2 = triangleMeshNode2.GetVertexCount();
                        for (int m = 0; m < vertexCount2; m++)
                        {
                            Int3 vertex3 = triangleMeshNode2.GetVertex(m);
                            Int3 vertex4 = triangleMeshNode2.GetVertex((m + 1) % vertexCount);
                            if (Math.Abs(vertex3[num5] - num9) < 2 && Math.Abs(vertex4[num5] - num9) < 2)
                            {
                                int num12 = Math.Min(vertex3[i], vertex4[i]);
                                int num13 = Math.Max(vertex3[i], vertex4[i]);
                                if (num12 != num13 && num11 > num12 && num10 < num13 && ((vertex == vertex3 && vertex2 == vertex4) || (vertex == vertex4 && vertex2 == vertex3) || VectorMath.SqrDistanceSegmentSegment((Vector3)vertex, (Vector3)vertex2, (Vector3)vertex3, (Vector3)vertex4) < walkableClimb * walkableClimb))
                                {
                                    uint costMagnitude = (uint)(triangleMeshNode.position - triangleMeshNode2.position).costMagnitude;
                                    triangleMeshNode.AddConnection(triangleMeshNode2, costMagnitude);
                                    triangleMeshNode2.AddConnection(triangleMeshNode, costMagnitude);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void StartBatchTileUpdate()
        {
            if (batchTileUpdate)
            {
                throw new InvalidOperationException("Calling StartBatchLoad when batching is already enabled");
            }
            batchTileUpdate = true;
        }

        public void EndBatchTileUpdate()
        {
            if (!batchTileUpdate)
            {
                throw new InvalidOperationException("Calling EndBatchLoad when batching not enabled");
            }
            batchTileUpdate = false;
            int num = tileXCount;
            int num2 = tileZCount;
            for (int i = 0; i < num2; i++)
            {
                for (int j = 0; j < num; j++)
                {
                    tiles[j + i * tileXCount].flag = false;
                }
            }
            for (int k = 0; k < batchUpdatedTiles.Count; k++)
            {
                tiles[batchUpdatedTiles[k]].flag = true;
            }
            for (int l = 0; l < num2; l++)
            {
                for (int m = 0; m < num; m++)
                {
                    if (m < num - 1 && (tiles[m + l * tileXCount].flag || tiles[m + 1 + l * tileXCount].flag) && tiles[m + l * tileXCount] != tiles[m + 1 + l * tileXCount])
                    {
                        ConnectTiles(tiles[m + l * tileXCount], tiles[m + 1 + l * tileXCount]);
                    }
                    if (l < num2 - 1 && (tiles[m + l * tileXCount].flag || tiles[m + (l + 1) * tileXCount].flag) && tiles[m + l * tileXCount] != tiles[m + (l + 1) * tileXCount])
                    {
                        ConnectTiles(tiles[m + l * tileXCount], tiles[m + (l + 1) * tileXCount]);
                    }
                }
            }
            batchUpdatedTiles.Clear();
        }

        private void ClearTiles(int x, int z, int w, int d)
        {
            for (int i = z; i < z + d; i++)
            {
                for (int j = x; j < x + w; j++)
                {
                    NavmeshTile navmeshTile = tiles[j + i * tileXCount];
                    if (navmeshTile == null)
                    {
                        continue;
                    }
                    RemoveConnectionsFromTile(navmeshTile);
                    for (int k = 0; k < navmeshTile.nodes.Length; k++)
                    {
                        navmeshTile.nodes[k].Destroy();
                    }
                    for (int l = navmeshTile.z; l < navmeshTile.z + navmeshTile.d; l++)
                    {
                        for (int m = navmeshTile.x; m < navmeshTile.x + navmeshTile.w; m++)
                        {
                            NavmeshTile navmeshTile2 = tiles[m + l * tileXCount];
                            if (navmeshTile2 == null || navmeshTile2 != navmeshTile)
                            {
                                throw new Exception("This should not happen");
                            }
                            if (l < z || l >= z + d || m < x || m >= x + w)
                            {
                                tiles[m + l * tileXCount] = NewEmptyTile(m, l);
                                if (batchTileUpdate)
                                {
                                    batchUpdatedTiles.Add(m + l * tileXCount);
                                }
                            }
                            else
                            {
                                tiles[m + l * tileXCount] = null;
                            }
                        }
                    }
                    ObjectPool<BBTree>.Release(ref navmeshTile.bbTree);
                }
            }
        }

        public void ReplaceTile(int x, int z, Int3[] verts, int[] tris, bool worldSpace)
        {
            ReplaceTile(x, z, 1, 1, verts, tris, worldSpace);
        }

        public void ReplaceTile(int x, int z, int w, int d, Int3[] verts, int[] tris, bool worldSpace)
        {
            if (x + w > tileXCount || z + d > tileZCount || x < 0 || z < 0)
            {
                throw new ArgumentException("Tile is placed at an out of bounds position or extends out of the graph bounds (" + x + ", " + z + " [" + w + ", " + d + "] " + tileXCount + " " + tileZCount + ")");
            }
            if (w < 1 || d < 1)
            {
                throw new ArgumentException("width and depth must be greater or equal to 1. Was " + w + ", " + d);
            }
            ClearTiles(x, z, w, d);
            NavmeshTile navmeshTile = new NavmeshTile();
            navmeshTile.x = x;
            navmeshTile.z = z;
            navmeshTile.w = w;
            navmeshTile.d = d;
            navmeshTile.tris = tris;
            navmeshTile.verts = verts;
            navmeshTile.bbTree = ObjectPool<BBTree>.Claim();
            NavmeshTile navmeshTile2 = navmeshTile;
            if (navmeshTile2.tris.Length % 3 != 0)
            {
                throw new ArgumentException("Triangle array's length must be a multiple of 3 (tris)");
            }
            if (navmeshTile2.verts.Length > 65535)
            {
                throw new ArgumentException("Too many vertices per tile (more than 65535)");
            }
            if (!worldSpace)
            {
                if (!Mathf.Approximately((float)(x * tileSizeX) * cellSize * 1000f, (float)Math.Round((float)(x * tileSizeX) * cellSize * 1000f)))
                {
                    Debug.LogWarning("Possible numerical imprecision. Consider adjusting tileSize and/or cellSize");
                }
                if (!Mathf.Approximately((float)(z * tileSizeZ) * cellSize * 1000f, (float)Math.Round((float)(z * tileSizeZ) * cellSize * 1000f)))
                {
                    Debug.LogWarning("Possible numerical imprecision. Consider adjusting tileSize and/or cellSize");
                }
                Int3 @int = (Int3)(new Vector3((float)(x * tileSizeX) * cellSize, 0f, (float)(z * tileSizeZ) * cellSize) + forcedBounds.min);
                for (int i = 0; i < verts.Length; i++)
                {
                    verts[i] += @int;
                }
            }
            TriangleMeshNode[] array = navmeshTile2.nodes = new TriangleMeshNode[navmeshTile2.tris.Length / 3];
            int graphIndex = AstarPath.active.astarData.graphs.Length;
            TriangleMeshNode.SetNavmeshHolder(graphIndex, navmeshTile2);
            int num = x + z * tileXCount;
            num <<= 12;
            if (navmeshTile2.verts.Length > 4095)
            {
                Debug.LogError("Too many vertices in the tile (" + navmeshTile2.verts.Length + " > " + 4095 + ")\nYou can enable ASTAR_RECAST_LARGER_TILES under the 'Optimizations' tab in the A* Inspector to raise this limit.");
                tiles[num] = NewEmptyTile(x, z);
                return;
            }
            for (int j = 0; j < array.Length; j++)
            {
                TriangleMeshNode triangleMeshNode = array[j] = new TriangleMeshNode(active);
                triangleMeshNode.GraphIndex = (uint)graphIndex;
                triangleMeshNode.v0 = (navmeshTile2.tris[j * 3] | num);
                triangleMeshNode.v1 = (navmeshTile2.tris[j * 3 + 1] | num);
                triangleMeshNode.v2 = (navmeshTile2.tris[j * 3 + 2] | num);
                if (!VectorMath.IsClockwiseXZ(triangleMeshNode.GetVertex(0), triangleMeshNode.GetVertex(1), triangleMeshNode.GetVertex(2)))
                {
                    int v = triangleMeshNode.v0;
                    triangleMeshNode.v0 = triangleMeshNode.v2;
                    triangleMeshNode.v2 = v;
                }
                triangleMeshNode.Walkable = true;
                triangleMeshNode.Penalty = initialPenalty;
                triangleMeshNode.UpdatePositionFromVertices();
            }
            navmeshTile2.bbTree.RebuildFrom(array);
            CreateNodeConnections(navmeshTile2.nodes);
            for (int k = z; k < z + d; k++)
            {
                for (int l = x; l < x + w; l++)
                {
                    tiles[l + k * tileXCount] = navmeshTile2;
                }
            }
            if (batchTileUpdate)
            {
                batchUpdatedTiles.Add(x + z * tileXCount);
            }
            else
            {
                ConnectTileWithNeighbours(navmeshTile2);
            }
            TriangleMeshNode.SetNavmeshHolder(graphIndex, null);
            graphIndex = AstarPath.active.astarData.GetGraphIndex(this);
            for (int m = 0; m < array.Length; m++)
            {
                array[m].GraphIndex = (uint)graphIndex;
            }
        }

        public bool Linecast(Vector3 origin, Vector3 end)
        {
            NNInfoInternal nearest = GetNearest(origin, NNConstraint.None);
            return Linecast(origin, end, nearest.node);
        }

        public bool Linecast(Vector3 origin, Vector3 end, GraphNode hint, out GraphHitInfo hit)
        {
            return NavMeshGraph.Linecast(this, origin, end, hint, out hit, null);
        }

        public bool Linecast(Vector3 origin, Vector3 end, GraphNode hint)
        {
            GraphHitInfo hit;
            return NavMeshGraph.Linecast(this, origin, end, hint, out hit, null);
        }

        public bool Linecast(Vector3 tmp_origin, Vector3 tmp_end, GraphNode hint, out GraphHitInfo hit, List<GraphNode> trace)
        {
            return NavMeshGraph.Linecast(this, tmp_origin, tmp_end, hint, out hit, trace);
        }

        public override void OnDrawGizmos(bool drawNodes)
        {
            if (drawNodes)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawWireCube(forcedBounds.center, forcedBounds.size);
                PathHandler debugData = AstarPath.active.debugPathData;
                GraphNodeDelegateCancelable del = delegate (GraphNode _node)
                {
                    TriangleMeshNode triangleMeshNode = _node as TriangleMeshNode;
                    if (AstarPath.active.showSearchTree && debugData != null)
                    {
                        bool flag = NavGraph.InSearchTree(triangleMeshNode, AstarPath.active.debugPath);
                        if (flag && showNodeConnections)
                        {
                            PathNode pathNode = debugData.GetPathNode(triangleMeshNode);
                            if (pathNode.parent != null)
                            {
                                Gizmos.color = NodeColor(triangleMeshNode, debugData);
                                Gizmos.DrawLine((Vector3)triangleMeshNode.position, (Vector3)debugData.GetPathNode(triangleMeshNode).parent.node.position);
                            }
                        }
                        if (showMeshOutline)
                        {
                            Gizmos.color = ((!triangleMeshNode.Walkable) ? AstarColor.UnwalkableNode : NodeColor(triangleMeshNode, debugData));
                            if (!flag)
                            {
                                Gizmos.color *= new Color(1f, 1f, 1f, 0.1f);
                            }
                            Gizmos.DrawLine((Vector3)triangleMeshNode.GetVertex(0), (Vector3)triangleMeshNode.GetVertex(1));
                            Gizmos.DrawLine((Vector3)triangleMeshNode.GetVertex(1), (Vector3)triangleMeshNode.GetVertex(2));
                            Gizmos.DrawLine((Vector3)triangleMeshNode.GetVertex(2), (Vector3)triangleMeshNode.GetVertex(0));
                        }
                    }
                    else
                    {
                        if (showNodeConnections)
                        {
                            Gizmos.color = NodeColor(triangleMeshNode, null);
                            for (int i = 0; i < triangleMeshNode.connections.Length; i++)
                            {
                                Gizmos.DrawLine((Vector3)triangleMeshNode.position, Vector3.Lerp((Vector3)triangleMeshNode.connections[i].position, (Vector3)triangleMeshNode.position, 0.4f));
                            }
                        }
                        if (showMeshOutline)
                        {
                            Gizmos.color = ((!triangleMeshNode.Walkable) ? AstarColor.UnwalkableNode : NodeColor(triangleMeshNode, debugData));
                            Gizmos.DrawLine((Vector3)triangleMeshNode.GetVertex(0), (Vector3)triangleMeshNode.GetVertex(1));
                            Gizmos.DrawLine((Vector3)triangleMeshNode.GetVertex(1), (Vector3)triangleMeshNode.GetVertex(2));
                            Gizmos.DrawLine((Vector3)triangleMeshNode.GetVertex(2), (Vector3)triangleMeshNode.GetVertex(0));
                        }
                    }
                    return true;
                };
                GetNodes(del);
            }
        }

        public override void DeserializeSettingsCompatibility(GraphSerializationContext ctx)
        {
            base.DeserializeSettingsCompatibility(ctx);
            characterRadius = ctx.reader.ReadSingle();
            contourMaxError = ctx.reader.ReadSingle();
            cellSize = ctx.reader.ReadSingle();
            ctx.reader.ReadSingle();
            walkableHeight = ctx.reader.ReadSingle();
            maxSlope = ctx.reader.ReadSingle();
            maxEdgeLength = ctx.reader.ReadSingle();
            editorTileSize = ctx.reader.ReadInt32();
            tileSizeX = ctx.reader.ReadInt32();
            nearestSearchOnlyXZ = ctx.reader.ReadBoolean();
            useTiles = ctx.reader.ReadBoolean();
            relevantGraphSurfaceMode = (RelevantGraphSurfaceMode)ctx.reader.ReadInt32();
            rasterizeColliders = ctx.reader.ReadBoolean();
            rasterizeMeshes = ctx.reader.ReadBoolean();
            rasterizeTerrain = ctx.reader.ReadBoolean();
            rasterizeTrees = ctx.reader.ReadBoolean();
            colliderRasterizeDetail = ctx.reader.ReadSingle();
            forcedBoundsCenter = ctx.DeserializeVector3();
            forcedBoundsSize = ctx.DeserializeVector3();
            mask = ctx.reader.ReadInt32();
            int num = ctx.reader.ReadInt32();
            tagMask = new List<string>(num);
            for (int i = 0; i < num; i++)
            {
                tagMask.Add(ctx.reader.ReadString());
            }
            showMeshOutline = ctx.reader.ReadBoolean();
            showNodeConnections = ctx.reader.ReadBoolean();
            terrainSampleSize = ctx.reader.ReadInt32();
            walkableClimb = ctx.DeserializeFloat(walkableClimb);
            minRegionSize = ctx.DeserializeFloat(minRegionSize);
            tileSizeZ = ctx.DeserializeInt(tileSizeX);
            showMeshSurface = ctx.reader.ReadBoolean();
        }

        public override void SerializeExtraInfo(GraphSerializationContext ctx)
        {
            BinaryWriter writer = ctx.writer;
            if (tiles == null)
            {
                writer.Write(-1);
                return;
            }
            writer.Write(tileXCount);
            writer.Write(tileZCount);
            for (int i = 0; i < tileZCount; i++)
            {
                for (int j = 0; j < tileXCount; j++)
                {
                    NavmeshTile navmeshTile = tiles[j + i * tileXCount];
                    if (navmeshTile == null)
                    {
                        throw new Exception("NULL Tile");
                    }
                    writer.Write(navmeshTile.x);
                    writer.Write(navmeshTile.z);
                    if (navmeshTile.x == j && navmeshTile.z == i)
                    {
                        writer.Write(navmeshTile.w);
                        writer.Write(navmeshTile.d);
                        writer.Write(navmeshTile.tris.Length);
                        for (int k = 0; k < navmeshTile.tris.Length; k++)
                        {
                            writer.Write(navmeshTile.tris[k]);
                        }
                        writer.Write(navmeshTile.verts.Length);
                        for (int l = 0; l < navmeshTile.verts.Length; l++)
                        {
                            ctx.SerializeInt3(navmeshTile.verts[l]);
                        }
                        writer.Write(navmeshTile.nodes.Length);
                        for (int m = 0; m < navmeshTile.nodes.Length; m++)
                        {
                            navmeshTile.nodes[m].SerializeNode(ctx);
                        }
                    }
                }
            }
        }

        public override void DeserializeExtraInfo(GraphSerializationContext ctx)
        {
            BinaryReader reader = ctx.reader;
            tileXCount = reader.ReadInt32();
            if (tileXCount < 0)
            {
                return;
            }
            tileZCount = reader.ReadInt32();
            tiles = new NavmeshTile[tileXCount * tileZCount];
            TriangleMeshNode.SetNavmeshHolder((int)ctx.graphIndex, this);
            for (int i = 0; i < tileZCount; i++)
            {
                for (int j = 0; j < tileXCount; j++)
                {
                    int num = j + i * tileXCount;
                    int num2 = reader.ReadInt32();
                    if (num2 < 0)
                    {
                        throw new Exception("Invalid tile coordinates (x < 0)");
                    }
                    int num3 = reader.ReadInt32();
                    if (num3 < 0)
                    {
                        throw new Exception("Invalid tile coordinates (z < 0)");
                    }
                    if (num2 != j || num3 != i)
                    {
                        tiles[num] = tiles[num3 * tileXCount + num2];
                        continue;
                    }
                    NavmeshTile navmeshTile = new NavmeshTile();
                    navmeshTile.x = num2;
                    navmeshTile.z = num3;
                    navmeshTile.w = reader.ReadInt32();
                    navmeshTile.d = reader.ReadInt32();
                    navmeshTile.bbTree = ObjectPool<BBTree>.Claim();
                    tiles[num] = navmeshTile;
                    int num4 = reader.ReadInt32();
                    if (num4 % 3 != 0)
                    {
                        throw new Exception("Corrupt data. Triangle indices count must be divisable by 3. Got " + num4);
                    }
                    navmeshTile.tris = new int[num4];
                    for (int k = 0; k < navmeshTile.tris.Length; k++)
                    {
                        navmeshTile.tris[k] = reader.ReadInt32();
                    }
                    navmeshTile.verts = new Int3[reader.ReadInt32()];
                    for (int l = 0; l < navmeshTile.verts.Length; l++)
                    {
                        navmeshTile.verts[l] = ctx.DeserializeInt3();
                    }
                    int num5 = reader.ReadInt32();
                    navmeshTile.nodes = new TriangleMeshNode[num5];
                    num <<= 12;
                    for (int m = 0; m < navmeshTile.nodes.Length; m++)
                    {
                        TriangleMeshNode triangleMeshNode = new TriangleMeshNode(active);
                        navmeshTile.nodes[m] = triangleMeshNode;
                        triangleMeshNode.DeserializeNode(ctx);
                        triangleMeshNode.v0 = (navmeshTile.tris[m * 3] | num);
                        triangleMeshNode.v1 = (navmeshTile.tris[m * 3 + 1] | num);
                        triangleMeshNode.v2 = (navmeshTile.tris[m * 3 + 2] | num);
                        triangleMeshNode.UpdatePositionFromVertices();
                    }
                    navmeshTile.bbTree.RebuildFrom(navmeshTile.nodes);
                }
            }
        }
    }
}
