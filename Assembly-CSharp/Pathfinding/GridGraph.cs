using Pathfinding.Serialization;
using Pathfinding.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    [JsonOptIn]
    public class GridGraph : NavGraph, IUpdatableGraph, IRaycastableGraph
    {
        public class TextureData
        {
            public enum ChannelUse
            {
                None,
                Penalty,
                Position,
                WalkablePenalty
            }

            public bool enabled;

            public Texture2D source;

            public float[] factors = new float[3];

            public ChannelUse[] channels = new ChannelUse[3];

            private Color32[] data;

            public void Initialize()
            {
                if (!enabled || !(source != null))
                {
                    return;
                }
                int num = 0;
                while (true)
                {
                    if (num < channels.Length)
                    {
                        if (channels[num] != 0)
                        {
                            break;
                        }
                        num++;
                        continue;
                    }
                    return;
                }
                try
                {
                    data = source.GetPixels32();
                }
                catch (UnityException ex)
                {
                    Debug.LogWarning(ex.ToString());
                    data = null;
                }
            }

            public void Apply(GridNode node, int x, int z)
            {
                if (enabled && data != null && x < source.width && z < source.height)
                {
                    Color32 color = data[z * source.width + x];
                    if (channels[0] != 0)
                    {
                        ApplyChannel(node, x, z, color.r, channels[0], factors[0]);
                    }
                    if (channels[1] != 0)
                    {
                        ApplyChannel(node, x, z, color.g, channels[1], factors[1]);
                    }
                    if (channels[2] != 0)
                    {
                        ApplyChannel(node, x, z, color.b, channels[2], factors[2]);
                    }
                }
            }

            private void ApplyChannel(GridNode node, int x, int z, int value, ChannelUse channelUse, float factor)
            {
                switch (channelUse)
                {
                    case ChannelUse.Penalty:
                        node.Penalty += (uint)Mathf.RoundToInt((float)value * factor);
                        break;
                    case ChannelUse.Position:
                        node.position = GridNode.GetGridGraph(node.GraphIndex).GraphPointToWorld(x, z, value);
                        break;
                    case ChannelUse.WalkablePenalty:
                        if (value == 0)
                        {
                            node.Walkable = false;
                        }
                        else
                        {
                            node.Penalty += (uint)Mathf.RoundToInt((float)(value - 1) * factor);
                        }
                        break;
                }
            }
        }

        public const int getNearestForceOverlap = 2;

        public int width;

        public int depth;

        [JsonMember]
        public float aspectRatio = 1f;

        [JsonMember]
        public float isometricAngle;

        [JsonMember]
        public bool uniformEdgeCosts;

        [JsonMember]
        public Vector3 rotation;

        [JsonMember]
        public Vector3 center;

        [JsonMember]
        public Vector2 unclampedSize;

        [JsonMember]
        public float nodeSize = 1f;

        [JsonMember]
        public GraphCollision collision;

        [JsonMember]
        public float maxClimb = 0.4f;

        [JsonMember]
        public int maxClimbAxis = 1;

        [JsonMember]
        public float maxSlope = 90f;

        [JsonMember]
        public int erodeIterations;

        [JsonMember]
        public bool erosionUseTags;

        [JsonMember]
        public int erosionFirstTag = 1;

        [JsonMember]
        public bool autoLinkGrids;

        [JsonMember]
        public float autoLinkDistLimit = 10f;

        [JsonMember]
        public NumNeighbours neighbours = NumNeighbours.Eight;

        [JsonMember]
        public bool cutCorners = true;

        [JsonMember]
        public float penaltyPositionOffset;

        [JsonMember]
        public bool penaltyPosition;

        [JsonMember]
        public float penaltyPositionFactor = 1f;

        [JsonMember]
        public bool penaltyAngle;

        [JsonMember]
        public float penaltyAngleFactor = 100f;

        [JsonMember]
        public float penaltyAnglePower = 1f;

        [JsonMember]
        public bool useJumpPointSearch;

        [JsonMember]
        public TextureData textureData = new TextureData();

        [NonSerialized]
        public readonly int[] neighbourOffsets = new int[8];

        [NonSerialized]
        public readonly uint[] neighbourCosts = new uint[8];

        [NonSerialized]
        public readonly int[] neighbourXOffsets = new int[8];

        [NonSerialized]
        public readonly int[] neighbourZOffsets = new int[8];

        internal static readonly int[] hexagonNeighbourIndices = new int[6]
        {
            0,
            1,
            2,
            3,
            5,
            7
        };

        public GridNode[] nodes;

        public virtual bool uniformWidthDepthGrid => true;

        public bool useRaycastNormal => Math.Abs(90f - maxSlope) > float.Epsilon;

        public Vector2 size
        {
            get;
            protected set;
        }

        public Matrix4x4 boundsMatrix
        {
            get;
            protected set;
        }

        public int Width
        {
            get
            {
                return width;
            }
            set
            {
                width = value;
            }
        }

        public int Depth
        {
            get
            {
                return depth;
            }
            set
            {
                depth = value;
            }
        }

        public GridGraph()
        {
            unclampedSize = new Vector2(10f, 10f);
            nodeSize = 1f;
            collision = new GraphCollision();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            RemoveGridGraphFromStatic();
        }

        private void RemoveGridGraphFromStatic()
        {
            GridNode.SetGridGraph(AstarPath.active.astarData.GetGraphIndex(this), null);
        }

        public override int CountNodes()
        {
            return nodes.Length;
        }

        public override void GetNodes(GraphNodeDelegateCancelable del)
        {
            if (nodes != null)
            {
                for (int i = 0; i < nodes.Length && del(nodes[i]); i++)
                {
                }
            }
        }

        public void RelocateNodes(Vector3 center, Quaternion rotation, float nodeSize, float aspectRatio = 1f, float isometricAngle = 0f)
        {
            Matrix4x4 matrix = base.matrix;
            this.center = center;
            this.rotation = rotation.eulerAngles;
            this.nodeSize = nodeSize;
            this.aspectRatio = aspectRatio;
            this.isometricAngle = isometricAngle;
            UpdateSizeFromWidthDepth();
            RelocateNodes(matrix, base.matrix);
        }

        public Int3 GraphPointToWorld(int x, int z, float height)
        {
            return (Int3)matrix.MultiplyPoint3x4(new Vector3((float)x + 0.5f, height, (float)z + 0.5f));
        }

        public uint GetConnectionCost(int dir)
        {
            return neighbourCosts[dir];
        }

        public GridNode GetNodeConnection(GridNode node, int dir)
        {
            if (!node.GetConnectionInternal(dir))
            {
                return null;
            }
            if (!node.EdgeNode)
            {
                return nodes[node.NodeInGridIndex + neighbourOffsets[dir]];
            }
            int nodeInGridIndex = node.NodeInGridIndex;
            int num = nodeInGridIndex / Width;
            int x = nodeInGridIndex - num * Width;
            return GetNodeConnection(nodeInGridIndex, x, num, dir);
        }

        public bool HasNodeConnection(GridNode node, int dir)
        {
            if (!node.GetConnectionInternal(dir))
            {
                return false;
            }
            if (!node.EdgeNode)
            {
                return true;
            }
            int nodeInGridIndex = node.NodeInGridIndex;
            int num = nodeInGridIndex / Width;
            int x = nodeInGridIndex - num * Width;
            return HasNodeConnection(nodeInGridIndex, x, num, dir);
        }

        public void SetNodeConnection(GridNode node, int dir, bool value)
        {
            int nodeInGridIndex = node.NodeInGridIndex;
            int num = nodeInGridIndex / Width;
            int x = nodeInGridIndex - num * Width;
            SetNodeConnection(nodeInGridIndex, x, num, dir, value);
        }

        private GridNode GetNodeConnection(int index, int x, int z, int dir)
        {
            if (!nodes[index].GetConnectionInternal(dir))
            {
                return null;
            }
            int num = x + neighbourXOffsets[dir];
            if (num < 0 || num >= Width)
            {
                return null;
            }
            int num2 = z + neighbourZOffsets[dir];
            if (num2 < 0 || num2 >= Depth)
            {
                return null;
            }
            int num3 = index + neighbourOffsets[dir];
            return nodes[num3];
        }

        public void SetNodeConnection(int index, int x, int z, int dir, bool value)
        {
            nodes[index].SetConnectionInternal(dir, value);
        }

        public bool HasNodeConnection(int index, int x, int z, int dir)
        {
            if (!nodes[index].GetConnectionInternal(dir))
            {
                return false;
            }
            int num = x + neighbourXOffsets[dir];
            if (num < 0 || num >= Width)
            {
                return false;
            }
            int num2 = z + neighbourZOffsets[dir];
            if (num2 < 0 || num2 >= Depth)
            {
                return false;
            }
            return true;
        }

        public void UpdateSizeFromWidthDepth()
        {
            unclampedSize = new Vector2(width, depth) * nodeSize;
            GenerateMatrix();
        }

        public void GenerateMatrix()
        {
            Vector2 size = unclampedSize;
            size.x *= Mathf.Sign(size.x);
            size.y *= Mathf.Sign(size.y);
            nodeSize = Mathf.Clamp(nodeSize, size.x / 1024f, float.PositiveInfinity);
            nodeSize = Mathf.Clamp(nodeSize, size.y / 1024f, float.PositiveInfinity);
            size.x = ((!(size.x < nodeSize)) ? size.x : nodeSize);
            size.y = ((!(size.y < nodeSize)) ? size.y : nodeSize);
            this.size = size;
            Matrix4x4 rhs = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 45f, 0f), Vector3.one);
            rhs = Matrix4x4.Scale(new Vector3(Mathf.Cos(MathF.PI / 180f * isometricAngle), 1f, 1f)) * rhs;
            rhs = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, -45f, 0f), Vector3.one) * rhs;
            this.boundsMatrix = Matrix4x4.TRS(center, Quaternion.Euler(rotation), new Vector3(aspectRatio, 1f, 1f)) * rhs;
            Vector2 size2 = this.size;
            width = Mathf.FloorToInt(size2.x / nodeSize);
            Vector2 size3 = this.size;
            depth = Mathf.FloorToInt(size3.y / nodeSize);
            Vector2 size4 = this.size;
            float a = size4.x / nodeSize;
            Vector2 size5 = this.size;
            if (Mathf.Approximately(a, Mathf.CeilToInt(size5.x / nodeSize)))
            {
                Vector2 size6 = this.size;
                width = Mathf.CeilToInt(size6.x / nodeSize);
            }
            Vector2 size7 = this.size;
            float a2 = size7.y / nodeSize;
            Vector2 size8 = this.size;
            if (Mathf.Approximately(a2, Mathf.CeilToInt(size8.y / nodeSize)))
            {
                Vector2 size9 = this.size;
                depth = Mathf.CeilToInt(size9.y / nodeSize);
            }
            Matrix4x4 boundsMatrix = this.boundsMatrix;
            Vector2 size10 = this.size;
            float x = size10.x;
            Vector2 size11 = this.size;
            Matrix4x4 matrix = Matrix4x4.TRS(boundsMatrix.MultiplyPoint3x4(-new Vector3(x, 0f, size11.y) * 0.5f), Quaternion.Euler(rotation), new Vector3(nodeSize * aspectRatio, 1f, nodeSize)) * rhs;
            SetMatrix(matrix);
        }

        public override NNInfoInternal GetNearest(Vector3 position, NNConstraint constraint, GraphNode hint)
        {
            if (nodes == null || depth * width != nodes.Length)
            {
                return default(NNInfoInternal);
            }
            position = inverseMatrix.MultiplyPoint3x4(position);
            float num = position.x - 0.5f;
            float num2 = position.z - 0.5f;
            int num3 = Mathf.Clamp(Mathf.RoundToInt(num), 0, width - 1);
            int num4 = Mathf.Clamp(Mathf.RoundToInt(num2), 0, depth - 1);
            NNInfoInternal result = new NNInfoInternal(nodes[num4 * width + num3]);
            Vector3 vector = inverseMatrix.MultiplyPoint3x4((Vector3)nodes[num4 * width + num3].position);
            float y = vector.y;
            result.clampedPosition = matrix.MultiplyPoint3x4(new Vector3(Mathf.Clamp(num, (float)num3 - 0.5f, (float)num3 + 0.5f) + 0.5f, y, Mathf.Clamp(num2, (float)num4 - 0.5f, (float)num4 + 0.5f) + 0.5f));
            return result;
        }

        public override NNInfoInternal GetNearestForce(Vector3 position, NNConstraint constraint)
        {
            if (nodes == null || depth * width != nodes.Length)
            {
                return default(NNInfoInternal);
            }
            Vector3 b = position;
            position = inverseMatrix.MultiplyPoint3x4(position);
            float num = position.x - 0.5f;
            float num2 = position.z - 0.5f;
            int num3 = Mathf.Clamp(Mathf.RoundToInt(num), 0, width - 1);
            int num4 = Mathf.Clamp(Mathf.RoundToInt(num2), 0, depth - 1);
            GridNode gridNode = nodes[num3 + num4 * width];
            GridNode gridNode2 = null;
            float num5 = float.PositiveInfinity;
            int num6 = 2;
            Vector3 clampedPosition = Vector3.zero;
            NNInfoInternal result = new NNInfoInternal(null);
            if (constraint.Suitable(gridNode))
            {
                gridNode2 = gridNode;
                num5 = ((Vector3)gridNode2.position - b).sqrMagnitude;
                Vector3 vector = inverseMatrix.MultiplyPoint3x4((Vector3)gridNode.position);
                float y = vector.y;
                clampedPosition = base.matrix.MultiplyPoint3x4(new Vector3(Mathf.Clamp(num, (float)num3 - 0.5f, (float)num3 + 0.5f) + 0.5f, y, Mathf.Clamp(num2, (float)num4 - 0.5f, (float)num4 + 0.5f) + 0.5f));
            }
            if (gridNode2 != null)
            {
                result.node = gridNode2;
                result.clampedPosition = clampedPosition;
                if (num6 == 0)
                {
                    return result;
                }
                num6--;
            }
            float num7 = (!constraint.constrainDistance) ? float.PositiveInfinity : AstarPath.active.maxNearestNodeDistance;
            float num8 = num7 * num7;
            int num9 = 1;
            while (true)
            {
                if (nodeSize * (float)num9 > num7)
                {
                    result.node = gridNode2;
                    result.clampedPosition = clampedPosition;
                    return result;
                }
                bool flag = false;
                int num10 = num4 + num9;
                int num11 = num10 * width;
                int i;
                for (i = num3 - num9; i <= num3 + num9; i++)
                {
                    if (i < 0 || num10 < 0 || i >= width || num10 >= depth)
                    {
                        continue;
                    }
                    flag = true;
                    if (constraint.Suitable(nodes[i + num11]))
                    {
                        float sqrMagnitude = ((Vector3)nodes[i + num11].position - b).sqrMagnitude;
                        if (sqrMagnitude < num5 && sqrMagnitude < num8)
                        {
                            num5 = sqrMagnitude;
                            gridNode2 = nodes[i + num11];
                            ref Matrix4x4 matrix = ref base.matrix;
                            float x = Mathf.Clamp(num, (float)i - 0.5f, (float)i + 0.5f) + 0.5f;
                            Vector3 vector2 = inverseMatrix.MultiplyPoint3x4((Vector3)gridNode2.position);
                            clampedPosition = matrix.MultiplyPoint3x4(new Vector3(x, vector2.y, Mathf.Clamp(num2, (float)num10 - 0.5f, (float)num10 + 0.5f) + 0.5f));
                        }
                    }
                }
                num10 = num4 - num9;
                num11 = num10 * width;
                for (i = num3 - num9; i <= num3 + num9; i++)
                {
                    if (i < 0 || num10 < 0 || i >= width || num10 >= depth)
                    {
                        continue;
                    }
                    flag = true;
                    if (constraint.Suitable(nodes[i + num11]))
                    {
                        float sqrMagnitude2 = ((Vector3)nodes[i + num11].position - b).sqrMagnitude;
                        if (sqrMagnitude2 < num5 && sqrMagnitude2 < num8)
                        {
                            num5 = sqrMagnitude2;
                            gridNode2 = nodes[i + num11];
                            ref Matrix4x4 matrix2 = ref base.matrix;
                            float x2 = Mathf.Clamp(num, (float)i - 0.5f, (float)i + 0.5f) + 0.5f;
                            Vector3 vector3 = inverseMatrix.MultiplyPoint3x4((Vector3)gridNode2.position);
                            clampedPosition = matrix2.MultiplyPoint3x4(new Vector3(x2, vector3.y, Mathf.Clamp(num2, (float)num10 - 0.5f, (float)num10 + 0.5f) + 0.5f));
                        }
                    }
                }
                i = num3 - num9;
                for (num10 = num4 - num9 + 1; num10 <= num4 + num9 - 1; num10++)
                {
                    if (i < 0 || num10 < 0 || i >= width || num10 >= depth)
                    {
                        continue;
                    }
                    flag = true;
                    if (constraint.Suitable(nodes[i + num10 * width]))
                    {
                        float sqrMagnitude3 = ((Vector3)nodes[i + num10 * width].position - b).sqrMagnitude;
                        if (sqrMagnitude3 < num5 && sqrMagnitude3 < num8)
                        {
                            num5 = sqrMagnitude3;
                            gridNode2 = nodes[i + num10 * width];
                            ref Matrix4x4 matrix3 = ref base.matrix;
                            float x3 = Mathf.Clamp(num, (float)i - 0.5f, (float)i + 0.5f) + 0.5f;
                            Vector3 vector4 = inverseMatrix.MultiplyPoint3x4((Vector3)gridNode2.position);
                            clampedPosition = matrix3.MultiplyPoint3x4(new Vector3(x3, vector4.y, Mathf.Clamp(num2, (float)num10 - 0.5f, (float)num10 + 0.5f) + 0.5f));
                        }
                    }
                }
                i = num3 + num9;
                for (num10 = num4 - num9 + 1; num10 <= num4 + num9 - 1; num10++)
                {
                    if (i < 0 || num10 < 0 || i >= width || num10 >= depth)
                    {
                        continue;
                    }
                    flag = true;
                    if (constraint.Suitable(nodes[i + num10 * width]))
                    {
                        float sqrMagnitude4 = ((Vector3)nodes[i + num10 * width].position - b).sqrMagnitude;
                        if (sqrMagnitude4 < num5 && sqrMagnitude4 < num8)
                        {
                            num5 = sqrMagnitude4;
                            gridNode2 = nodes[i + num10 * width];
                            ref Matrix4x4 matrix4 = ref base.matrix;
                            float x4 = Mathf.Clamp(num, (float)i - 0.5f, (float)i + 0.5f) + 0.5f;
                            Vector3 vector5 = inverseMatrix.MultiplyPoint3x4((Vector3)gridNode2.position);
                            clampedPosition = matrix4.MultiplyPoint3x4(new Vector3(x4, vector5.y, Mathf.Clamp(num2, (float)num10 - 0.5f, (float)num10 + 0.5f) + 0.5f));
                        }
                    }
                }
                if (gridNode2 != null)
                {
                    if (num6 == 0)
                    {
                        result.node = gridNode2;
                        result.clampedPosition = clampedPosition;
                        return result;
                    }
                    num6--;
                }
                if (!flag)
                {
                    break;
                }
                num9++;
            }
            result.node = gridNode2;
            result.clampedPosition = clampedPosition;
            return result;
        }

        public virtual void SetUpOffsetsAndCosts()
        {
            neighbourOffsets[0] = -width;
            neighbourOffsets[1] = 1;
            neighbourOffsets[2] = width;
            neighbourOffsets[3] = -1;
            neighbourOffsets[4] = -width + 1;
            neighbourOffsets[5] = width + 1;
            neighbourOffsets[6] = width - 1;
            neighbourOffsets[7] = -width - 1;
            uint num = (uint)Mathf.RoundToInt(nodeSize * 1000f);
            uint num2 = (uint)((!uniformEdgeCosts) ? Mathf.RoundToInt(nodeSize * Mathf.Sqrt(2f) * 1000f) : ((int)num));
            neighbourCosts[0] = num;
            neighbourCosts[1] = num;
            neighbourCosts[2] = num;
            neighbourCosts[3] = num;
            neighbourCosts[4] = num2;
            neighbourCosts[5] = num2;
            neighbourCosts[6] = num2;
            neighbourCosts[7] = num2;
            neighbourXOffsets[0] = 0;
            neighbourXOffsets[1] = 1;
            neighbourXOffsets[2] = 0;
            neighbourXOffsets[3] = -1;
            neighbourXOffsets[4] = 1;
            neighbourXOffsets[5] = 1;
            neighbourXOffsets[6] = -1;
            neighbourXOffsets[7] = -1;
            neighbourZOffsets[0] = -1;
            neighbourZOffsets[1] = 0;
            neighbourZOffsets[2] = 1;
            neighbourZOffsets[3] = 0;
            neighbourZOffsets[4] = -1;
            neighbourZOffsets[5] = 1;
            neighbourZOffsets[6] = 1;
            neighbourZOffsets[7] = -1;
        }

        public override IEnumerable<Progress> ScanInternal()
        {
            AstarPath.OnPostScan = (OnScanDelegate)Delegate.Combine(AstarPath.OnPostScan, new OnScanDelegate(OnPostScan));
            if (nodeSize <= 0f)
            {
                yield break;
            }
            GenerateMatrix();
            if (width > 1024 || depth > 1024)
            {
                Debug.LogError("One of the grid's sides is longer than 1024 nodes");
                yield break;
            }
            if (useJumpPointSearch)
            {
                Debug.LogError("Trying to use Jump Point Search, but support for it is not enabled. Please enable it in the inspector (Grid Graph settings).");
            }
            SetUpOffsetsAndCosts();
            int graphIndex = AstarPath.active.astarData.GetGraphIndex(this);
            GridNode.SetGridGraph(graphIndex, this);
            yield return new Progress(0.05f, "Creating nodes");
            nodes = new GridNode[width * depth];
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i] = new GridNode(active);
                nodes[i].GraphIndex = (uint)graphIndex;
            }
            if (collision == null)
            {
                collision = new GraphCollision();
            }
            collision.Initialize(matrix, nodeSize);
            textureData.Initialize();
            int progressCounter = 0;
            for (int z2 = 0; z2 < depth; z2++)
            {
                if (progressCounter >= 1000)
                {
                    progressCounter = 0;
                    yield return new Progress(Mathf.Lerp(0.1f, 0.7f, (float)z2 / (float)depth), "Calculating positions");
                }
                progressCounter += width;
                for (int x = 0; x < width; x++)
                {
                    GridNode node = nodes[z2 * width + x];
                    node.NodeInGridIndex = z2 * width + x;
                    UpdateNodePositionCollision(node, x, z2);
                    textureData.Apply(node, x, z2);
                }
            }
            for (int z = 0; z < depth; z++)
            {
                if (progressCounter >= 1000)
                {
                    progressCounter = 0;
                    yield return new Progress(Mathf.Lerp(0.1f, 0.7f, (float)z / (float)depth), "Calculating connections");
                }
                for (int x2 = 0; x2 < width; x2++)
                {
                    GridNode node2 = nodes[z * width + x2];
                    CalculateConnections(x2, z, node2);
                }
            }
            yield return new Progress(0.95f, "Calculating erosion");
            ErodeWalkableArea();
        }

        public virtual void UpdateNodePositionCollision(GridNode node, int x, int z, bool resetPenalty = true)
        {
            node.position = GraphPointToWorld(x, z, 0f);
            RaycastHit hit;
            bool walkable;
            Vector3 ob = collision.CheckHeight((Vector3)node.position, out hit, out walkable);
            node.position = (Int3)ob;
            if (resetPenalty)
            {
                node.Penalty = initialPenalty;
                if (penaltyPosition)
                {
                    node.Penalty += (uint)Mathf.RoundToInt(((float)node.position.y - penaltyPositionOffset) * penaltyPositionFactor);
                }
            }
            if (walkable && useRaycastNormal && collision.heightCheck && hit.normal != Vector3.zero)
            {
                float num = Vector3.Dot(hit.normal.normalized, collision.up);
                if (penaltyAngle && resetPenalty)
                {
                    node.Penalty += (uint)Mathf.RoundToInt((1f - Mathf.Pow(num, penaltyAnglePower)) * penaltyAngleFactor);
                }
                float num2 = Mathf.Cos(maxSlope * (MathF.PI / 180f));
                if (num < num2)
                {
                    walkable = false;
                }
            }
            node.Walkable = (walkable && collision.Check((Vector3)node.position));
            node.WalkableErosion = node.Walkable;
        }

        public virtual void ErodeWalkableArea()
        {
            ErodeWalkableArea(0, 0, Width, Depth);
        }

        private bool ErosionAnyFalseConnections(GridNode node)
        {
            if (neighbours == NumNeighbours.Six)
            {
                for (int i = 0; i < 6; i++)
                {
                    if (!HasNodeConnection(node, hexagonNeighbourIndices[i]))
                    {
                        return true;
                    }
                }
            }
            else
            {
                for (int j = 0; j < 4; j++)
                {
                    if (!HasNodeConnection(node, j))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public virtual void ErodeWalkableArea(int xmin, int zmin, int xmax, int zmax)
        {
            xmin = Mathf.Clamp(xmin, 0, Width);
            xmax = Mathf.Clamp(xmax, 0, Width);
            zmin = Mathf.Clamp(zmin, 0, Depth);
            zmax = Mathf.Clamp(zmax, 0, Depth);
            if (!erosionUseTags)
            {
                for (int i = 0; i < erodeIterations; i++)
                {
                    for (int j = zmin; j < zmax; j++)
                    {
                        for (int k = xmin; k < xmax; k++)
                        {
                            GridNode gridNode = nodes[j * Width + k];
                            if (gridNode.Walkable && ErosionAnyFalseConnections(gridNode))
                            {
                                gridNode.Walkable = false;
                            }
                        }
                    }
                    for (int l = zmin; l < zmax; l++)
                    {
                        for (int m = xmin; m < xmax; m++)
                        {
                            GridNode node = nodes[l * Width + m];
                            CalculateConnections(m, l, node);
                        }
                    }
                }
                return;
            }
            if (erodeIterations + erosionFirstTag > 31)
            {
                Debug.LogError("Too few tags available for " + erodeIterations + " erode iterations and starting with tag " + erosionFirstTag + " (erodeIterations+erosionFirstTag > 31)");
                return;
            }
            if (erosionFirstTag <= 0)
            {
                Debug.LogError("First erosion tag must be greater or equal to 1");
                return;
            }
            for (int n = 0; n < erodeIterations; n++)
            {
                for (int num = zmin; num < zmax; num++)
                {
                    for (int num2 = xmin; num2 < xmax; num2++)
                    {
                        GridNode gridNode2 = nodes[num * width + num2];
                        if (gridNode2.Walkable && gridNode2.Tag >= erosionFirstTag && gridNode2.Tag < erosionFirstTag + n)
                        {
                            if (neighbours == NumNeighbours.Six)
                            {
                                for (int num3 = 0; num3 < 6; num3++)
                                {
                                    GridNode nodeConnection = GetNodeConnection(gridNode2, hexagonNeighbourIndices[num3]);
                                    if (nodeConnection != null)
                                    {
                                        uint tag = nodeConnection.Tag;
                                        if (tag > erosionFirstTag + n || tag < erosionFirstTag)
                                        {
                                            nodeConnection.Tag = (uint)(erosionFirstTag + n);
                                        }
                                    }
                                }
                                continue;
                            }
                            for (int num4 = 0; num4 < 4; num4++)
                            {
                                GridNode nodeConnection2 = GetNodeConnection(gridNode2, num4);
                                if (nodeConnection2 != null)
                                {
                                    uint tag2 = nodeConnection2.Tag;
                                    if (tag2 > erosionFirstTag + n || tag2 < erosionFirstTag)
                                    {
                                        nodeConnection2.Tag = (uint)(erosionFirstTag + n);
                                    }
                                }
                            }
                        }
                        else if (gridNode2.Walkable && n == 0 && ErosionAnyFalseConnections(gridNode2))
                        {
                            gridNode2.Tag = (uint)(erosionFirstTag + n);
                        }
                    }
                }
            }
        }

        public virtual bool IsValidConnection(GridNode n1, GridNode n2)
        {
            if (!n1.Walkable || !n2.Walkable)
            {
                return false;
            }
            return maxClimb <= 0f || (float)Math.Abs(n1.position[maxClimbAxis] - n2.position[maxClimbAxis]) <= maxClimb * 1000f;
        }

        public static void CalculateConnections(GridNode node)
        {
            GridGraph gridGraph = AstarData.GetGraph(node) as GridGraph;
            if (gridGraph != null)
            {
                int nodeInGridIndex = node.NodeInGridIndex;
                int x = nodeInGridIndex % gridGraph.width;
                int z = nodeInGridIndex / gridGraph.width;
                gridGraph.CalculateConnections(x, z, node);
            }
        }

        [Obsolete("CalculateConnections no longer takes a node array, it just uses the one on the graph")]
        public virtual void CalculateConnections(GridNode[] nodes, int x, int z, GridNode node)
        {
            CalculateConnections(x, z, node);
        }

        public virtual void CalculateConnections(int x, int z, GridNode node)
        {
            if (!node.Walkable)
            {
                node.ResetConnectionsInternal();
                return;
            }
            int nodeInGridIndex = node.NodeInGridIndex;
            if (neighbours == NumNeighbours.Four || neighbours == NumNeighbours.Eight)
            {
                int num = 0;
                for (int i = 0; i < 4; i++)
                {
                    int num2 = x + neighbourXOffsets[i];
                    int num3 = z + neighbourZOffsets[i];
                    if ((num2 >= 0 && num3 >= 0) & (num2 < width) & (num3 < depth))
                    {
                        GridNode n = nodes[nodeInGridIndex + neighbourOffsets[i]];
                        if (IsValidConnection(node, n))
                        {
                            num |= 1 << (i & 0x1F);
                        }
                    }
                }
                int num4 = 0;
                if (neighbours == NumNeighbours.Eight)
                {
                    if (cutCorners)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            if ((((num >> j) | (num >> j + 1) | (num >> j + 1 - 4)) & 1) == 0)
                            {
                                continue;
                            }
                            int num5 = j + 4;
                            int num6 = x + neighbourXOffsets[num5];
                            int num7 = z + neighbourZOffsets[num5];
                            if ((num6 >= 0 && num7 >= 0) & (num6 < width) & (num7 < depth))
                            {
                                GridNode n2 = nodes[nodeInGridIndex + neighbourOffsets[num5]];
                                if (IsValidConnection(node, n2))
                                {
                                    num4 |= 1 << (num5 & 0x1F);
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            if (((num >> k) & 1) != 0 && (((num >> k + 1) | (num >> k + 1 - 4)) & 1) != 0)
                            {
                                GridNode n3 = nodes[nodeInGridIndex + neighbourOffsets[k + 4]];
                                if (IsValidConnection(node, n3))
                                {
                                    num4 |= 1 << ((k + 4) & 0x1F);
                                }
                            }
                        }
                    }
                }
                node.SetAllConnectionInternal(num | num4);
                return;
            }
            node.ResetConnectionsInternal();
            for (int l = 0; l < hexagonNeighbourIndices.Length; l++)
            {
                int num8 = hexagonNeighbourIndices[l];
                int num9 = x + neighbourXOffsets[num8];
                int num10 = z + neighbourZOffsets[num8];
                if ((num9 >= 0 && num10 >= 0) & (num9 < width) & (num10 < depth))
                {
                    GridNode n4 = nodes[nodeInGridIndex + neighbourOffsets[num8]];
                    node.SetConnectionInternal(num8, IsValidConnection(node, n4));
                }
            }
        }

        public void OnPostScan(AstarPath script)
        {
            AstarPath.OnPostScan = (OnScanDelegate)Delegate.Remove(AstarPath.OnPostScan, new OnScanDelegate(OnPostScan));
            if (!autoLinkGrids || autoLinkDistLimit <= 0f)
            {
                return;
            }
            throw new NotSupportedException();
        }

        public override void OnDrawGizmos(bool drawNodes)
        {
            Gizmos.matrix = boundsMatrix;
            Gizmos.color = Color.white;
            Vector3 zero = Vector3.zero;
            Vector2 size = this.size;
            float x = size.x;
            Vector2 size2 = this.size;
            Gizmos.DrawWireCube(zero, new Vector3(x, 0f, size2.y));
            Gizmos.matrix = Matrix4x4.identity;
            if (!drawNodes || nodes == null || nodes.Length != width * depth)
            {
                return;
            }
            PathHandler debugPathData = AstarPath.active.debugPathData;
            bool flag = AstarPath.active.showSearchTree && debugPathData != null;
            for (int i = 0; i < depth; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    GridNode gridNode = nodes[i * width + j];
                    if (!gridNode.Walkable)
                    {
                        continue;
                    }
                    Gizmos.color = NodeColor(gridNode, debugPathData);
                    Vector3 from = (Vector3)gridNode.position;
                    if (flag)
                    {
                        if (NavGraph.InSearchTree(gridNode, AstarPath.active.debugPath))
                        {
                            PathNode pathNode = debugPathData.GetPathNode(gridNode);
                            if (pathNode != null && pathNode.parent != null)
                            {
                                Gizmos.DrawLine(from, (Vector3)pathNode.parent.node.position);
                            }
                        }
                        continue;
                    }
                    for (int k = 0; k < 8; k++)
                    {
                        if (gridNode.GetConnectionInternal(k))
                        {
                            GridNode gridNode2 = nodes[gridNode.NodeInGridIndex + neighbourOffsets[k]];
                            Gizmos.DrawLine(from, (Vector3)gridNode2.position);
                        }
                    }
                    if (gridNode.connections != null)
                    {
                        for (int l = 0; l < gridNode.connections.Length; l++)
                        {
                            GraphNode graphNode = gridNode.connections[l];
                            Gizmos.DrawLine(from, (Vector3)graphNode.position);
                        }
                    }
                }
            }
        }

        protected static void GetBoundsMinMax(Bounds b, Matrix4x4 matrix, out Vector3 min, out Vector3 max)
        {
            Vector3[] array = new Vector3[8];
            ref Vector3 reference = ref array[0];
            Vector3 a = b.center;
            Vector3 extents = b.extents;
            float x = extents.x;
            Vector3 extents2 = b.extents;
            float y = extents2.y;
            Vector3 extents3 = b.extents;
            reference = matrix.MultiplyPoint3x4(a + new Vector3(x, y, extents3.z));
            ref Vector3 reference2 = ref array[1];
            Vector3 a2 = b.center;
            Vector3 extents4 = b.extents;
            float x2 = extents4.x;
            Vector3 extents5 = b.extents;
            float y2 = extents5.y;
            Vector3 extents6 = b.extents;
            reference2 = matrix.MultiplyPoint3x4(a2 + new Vector3(x2, y2, 0f - extents6.z));
            ref Vector3 reference3 = ref array[2];
            Vector3 a3 = b.center;
            Vector3 extents7 = b.extents;
            float x3 = extents7.x;
            Vector3 extents8 = b.extents;
            float y3 = 0f - extents8.y;
            Vector3 extents9 = b.extents;
            reference3 = matrix.MultiplyPoint3x4(a3 + new Vector3(x3, y3, extents9.z));
            ref Vector3 reference4 = ref array[3];
            Vector3 a4 = b.center;
            Vector3 extents10 = b.extents;
            float x4 = extents10.x;
            Vector3 extents11 = b.extents;
            float y4 = 0f - extents11.y;
            Vector3 extents12 = b.extents;
            reference4 = matrix.MultiplyPoint3x4(a4 + new Vector3(x4, y4, 0f - extents12.z));
            ref Vector3 reference5 = ref array[4];
            Vector3 a5 = b.center;
            Vector3 extents13 = b.extents;
            float x5 = 0f - extents13.x;
            Vector3 extents14 = b.extents;
            float y5 = extents14.y;
            Vector3 extents15 = b.extents;
            reference5 = matrix.MultiplyPoint3x4(a5 + new Vector3(x5, y5, extents15.z));
            ref Vector3 reference6 = ref array[5];
            Vector3 a6 = b.center;
            Vector3 extents16 = b.extents;
            float x6 = 0f - extents16.x;
            Vector3 extents17 = b.extents;
            float y6 = extents17.y;
            Vector3 extents18 = b.extents;
            reference6 = matrix.MultiplyPoint3x4(a6 + new Vector3(x6, y6, 0f - extents18.z));
            ref Vector3 reference7 = ref array[6];
            Vector3 a7 = b.center;
            Vector3 extents19 = b.extents;
            float x7 = 0f - extents19.x;
            Vector3 extents20 = b.extents;
            float y7 = 0f - extents20.y;
            Vector3 extents21 = b.extents;
            reference7 = matrix.MultiplyPoint3x4(a7 + new Vector3(x7, y7, extents21.z));
            ref Vector3 reference8 = ref array[7];
            Vector3 a8 = b.center;
            Vector3 extents22 = b.extents;
            float x8 = 0f - extents22.x;
            Vector3 extents23 = b.extents;
            float y8 = 0f - extents23.y;
            Vector3 extents24 = b.extents;
            reference8 = matrix.MultiplyPoint3x4(a8 + new Vector3(x8, y8, 0f - extents24.z));
            min = array[0];
            max = array[0];
            for (int i = 1; i < 8; i++)
            {
                min = Vector3.Min(min, array[i]);
                max = Vector3.Max(max, array[i]);
            }
        }

        public List<GraphNode> GetNodesInArea(Bounds b)
        {
            return GetNodesInArea(b, null);
        }

        public List<GraphNode> GetNodesInArea(GraphUpdateShape shape)
        {
            return GetNodesInArea(shape.GetBounds(), shape);
        }

        private List<GraphNode> GetNodesInArea(Bounds b, GraphUpdateShape shape)
        {
            if (nodes == null || width * depth != nodes.Length)
            {
                return null;
            }
            List<GraphNode> list = ListPool<GraphNode>.Claim();
            GetBoundsMinMax(b, inverseMatrix, out Vector3 min, out Vector3 max);
            int xmin = Mathf.RoundToInt(min.x - 0.5f);
            int xmax = Mathf.RoundToInt(max.x - 0.5f);
            int ymin = Mathf.RoundToInt(min.z - 0.5f);
            int ymax = Mathf.RoundToInt(max.z - 0.5f);
            IntRect a = new IntRect(xmin, ymin, xmax, ymax);
            IntRect b2 = new IntRect(0, 0, width - 1, depth - 1);
            IntRect intRect = IntRect.Intersection(a, b2);
            for (int i = intRect.xmin; i <= intRect.xmax; i++)
            {
                for (int j = intRect.ymin; j <= intRect.ymax; j++)
                {
                    int num = j * width + i;
                    GraphNode graphNode = nodes[num];
                    if (b.Contains((Vector3)graphNode.position) && (shape == null || shape.Contains((Vector3)graphNode.position)))
                    {
                        list.Add(graphNode);
                    }
                }
            }
            return list;
        }

        public GraphUpdateThreading CanUpdateAsync(GraphUpdateObject o)
        {
            return GraphUpdateThreading.UnityThread;
        }

        public void UpdateAreaInit(GraphUpdateObject o)
        {
        }

        public void UpdateAreaPost(GraphUpdateObject o)
        {
        }

        public void UpdateArea(GraphUpdateObject o)
        {
            if (nodes == null || nodes.Length != width * depth)
            {
                Debug.LogWarning("The Grid Graph is not scanned, cannot update area ");
                return;
            }
            Bounds bounds = o.bounds;
            GetBoundsMinMax(bounds, inverseMatrix, out Vector3 min, out Vector3 max);
            int xmin = Mathf.RoundToInt(min.x - 0.5f);
            int xmax = Mathf.RoundToInt(max.x - 0.5f);
            int ymin = Mathf.RoundToInt(min.z - 0.5f);
            int ymax = Mathf.RoundToInt(max.z - 0.5f);
            IntRect intRect = new IntRect(xmin, ymin, xmax, ymax);
            IntRect intRect2 = intRect;
            IntRect b = new IntRect(0, 0, width - 1, depth - 1);
            IntRect intRect3 = intRect;
            int num = o.updateErosion ? erodeIterations : 0;
            bool flag = o.updatePhysics || o.modifyWalkability;
            if (o.updatePhysics && !o.modifyWalkability && collision.collisionCheck)
            {
                Vector3 a = new Vector3(collision.diameter, 0f, collision.diameter) * 0.5f;
                min -= a * 1.02f;
                max += a * 1.02f;
                intRect3 = new IntRect(Mathf.RoundToInt(min.x - 0.5f), Mathf.RoundToInt(min.z - 0.5f), Mathf.RoundToInt(max.x - 0.5f), Mathf.RoundToInt(max.z - 0.5f));
                intRect2 = IntRect.Union(intRect3, intRect2);
            }
            if (flag || num > 0)
            {
                intRect2 = intRect2.Expand(num + 1);
            }
            IntRect intRect4 = IntRect.Intersection(intRect2, b);
            for (int i = intRect4.xmin; i <= intRect4.xmax; i++)
            {
                for (int j = intRect4.ymin; j <= intRect4.ymax; j++)
                {
                    o.WillUpdateNode(nodes[j * width + i]);
                }
            }
            if (o.updatePhysics && !o.modifyWalkability)
            {
                collision.Initialize(matrix, nodeSize);
                intRect4 = IntRect.Intersection(intRect3, b);
                for (int k = intRect4.xmin; k <= intRect4.xmax; k++)
                {
                    for (int l = intRect4.ymin; l <= intRect4.ymax; l++)
                    {
                        int num2 = l * width + k;
                        GridNode node = nodes[num2];
                        UpdateNodePositionCollision(node, k, l, o.resetPenaltyOnPhysics);
                    }
                }
            }
            intRect4 = IntRect.Intersection(intRect, b);
            for (int m = intRect4.xmin; m <= intRect4.xmax; m++)
            {
                for (int n = intRect4.ymin; n <= intRect4.ymax; n++)
                {
                    int num3 = n * width + m;
                    GridNode gridNode = nodes[num3];
                    if (flag)
                    {
                        gridNode.Walkable = gridNode.WalkableErosion;
                        if (o.bounds.Contains((Vector3)gridNode.position))
                        {
                            o.Apply(gridNode);
                        }
                        gridNode.WalkableErosion = gridNode.Walkable;
                    }
                    else if (o.bounds.Contains((Vector3)gridNode.position))
                    {
                        o.Apply(gridNode);
                    }
                }
            }
            if (flag && num == 0)
            {
                intRect4 = IntRect.Intersection(intRect2, b);
                for (int num4 = intRect4.xmin; num4 <= intRect4.xmax; num4++)
                {
                    for (int num5 = intRect4.ymin; num5 <= intRect4.ymax; num5++)
                    {
                        int num6 = num5 * width + num4;
                        GridNode node2 = nodes[num6];
                        CalculateConnections(num4, num5, node2);
                    }
                }
            }
            else
            {
                if (!flag || num <= 0)
                {
                    return;
                }
                IntRect a2 = IntRect.Union(intRect, intRect3).Expand(num);
                IntRect a3 = a2.Expand(num);
                a2 = IntRect.Intersection(a2, b);
                a3 = IntRect.Intersection(a3, b);
                for (int num7 = a3.xmin; num7 <= a3.xmax; num7++)
                {
                    for (int num8 = a3.ymin; num8 <= a3.ymax; num8++)
                    {
                        int num9 = num8 * width + num7;
                        GridNode gridNode2 = nodes[num9];
                        bool walkable = gridNode2.Walkable;
                        gridNode2.Walkable = gridNode2.WalkableErosion;
                        if (!a2.Contains(num7, num8))
                        {
                            gridNode2.TmpWalkable = walkable;
                        }
                    }
                }
                for (int num10 = a3.xmin; num10 <= a3.xmax; num10++)
                {
                    for (int num11 = a3.ymin; num11 <= a3.ymax; num11++)
                    {
                        int num12 = num11 * width + num10;
                        GridNode node3 = nodes[num12];
                        CalculateConnections(num10, num11, node3);
                    }
                }
                ErodeWalkableArea(a3.xmin, a3.ymin, a3.xmax + 1, a3.ymax + 1);
                for (int num13 = a3.xmin; num13 <= a3.xmax; num13++)
                {
                    for (int num14 = a3.ymin; num14 <= a3.ymax; num14++)
                    {
                        if (!a2.Contains(num13, num14))
                        {
                            int num15 = num14 * width + num13;
                            GridNode gridNode3 = nodes[num15];
                            gridNode3.Walkable = gridNode3.TmpWalkable;
                        }
                    }
                }
                for (int num16 = a3.xmin; num16 <= a3.xmax; num16++)
                {
                    for (int num17 = a3.ymin; num17 <= a3.ymax; num17++)
                    {
                        int num18 = num17 * width + num16;
                        GridNode node4 = nodes[num18];
                        CalculateConnections(num16, num17, node4);
                    }
                }
            }
        }

        public bool Linecast(Vector3 _a, Vector3 _b)
        {
            GraphHitInfo hit;
            return Linecast(_a, _b, null, out hit);
        }

        public bool Linecast(Vector3 _a, Vector3 _b, GraphNode hint)
        {
            GraphHitInfo hit;
            return Linecast(_a, _b, hint, out hit);
        }

        public bool Linecast(Vector3 _a, Vector3 _b, GraphNode hint, out GraphHitInfo hit)
        {
            return Linecast(_a, _b, hint, out hit, null);
        }

        protected static float CrossMagnitude(Vector2 a, Vector2 b)
        {
            return a.x * b.y - b.x * a.y;
        }

        protected virtual GridNodeBase GetNeighbourAlongDirection(GridNodeBase node, int direction)
        {
            GridNode gridNode = node as GridNode;
            if (gridNode.GetConnectionInternal(direction))
            {
                return nodes[gridNode.NodeInGridIndex + neighbourOffsets[direction]];
            }
            return null;
        }

        protected bool ClipLineSegmentToBounds(Vector3 a, Vector3 b, out Vector3 outA, out Vector3 outB)
        {
            if (a.x < 0f || a.z < 0f || a.x > (float)width || a.z > (float)depth || b.x < 0f || b.z < 0f || b.x > (float)width || b.z > (float)depth)
            {
                Vector3 vector = new Vector3(0f, 0f, 0f);
                Vector3 vector2 = new Vector3(0f, 0f, depth);
                Vector3 vector3 = new Vector3(width, 0f, depth);
                Vector3 vector4 = new Vector3(width, 0f, 0f);
                int num = 0;
                Vector3 vector5 = VectorMath.SegmentIntersectionPointXZ(a, b, vector, vector2, out bool intersects);
                if (intersects)
                {
                    num++;
                    if (!VectorMath.RightOrColinearXZ(vector, vector2, a))
                    {
                        a = vector5;
                    }
                    else
                    {
                        b = vector5;
                    }
                }
                vector5 = VectorMath.SegmentIntersectionPointXZ(a, b, vector2, vector3, out intersects);
                if (intersects)
                {
                    num++;
                    if (!VectorMath.RightOrColinearXZ(vector2, vector3, a))
                    {
                        a = vector5;
                    }
                    else
                    {
                        b = vector5;
                    }
                }
                vector5 = VectorMath.SegmentIntersectionPointXZ(a, b, vector3, vector4, out intersects);
                if (intersects)
                {
                    num++;
                    if (!VectorMath.RightOrColinearXZ(vector3, vector4, a))
                    {
                        a = vector5;
                    }
                    else
                    {
                        b = vector5;
                    }
                }
                vector5 = VectorMath.SegmentIntersectionPointXZ(a, b, vector4, vector, out intersects);
                if (intersects)
                {
                    num++;
                    if (!VectorMath.RightOrColinearXZ(vector4, vector, a))
                    {
                        a = vector5;
                    }
                    else
                    {
                        b = vector5;
                    }
                }
                if (num == 0)
                {
                    outA = Vector3.zero;
                    outB = Vector3.zero;
                    return false;
                }
            }
            outA = a;
            outB = b;
            return true;
        }

        public bool Linecast(Vector3 _a, Vector3 _b, GraphNode hint, out GraphHitInfo hit, List<GraphNode> trace)
        {
            hit = default(GraphHitInfo);
            hit.origin = _a;
            Vector3 outA = inverseMatrix.MultiplyPoint3x4(_a);
            Vector3 outB = inverseMatrix.MultiplyPoint3x4(_b);
            if (!ClipLineSegmentToBounds(outA, outB, out outA, out outB))
            {
                return false;
            }
            NNInfoInternal nearest = GetNearest(matrix.MultiplyPoint3x4(outA), NNConstraint.None);
            GridNodeBase gridNodeBase = nearest.node as GridNodeBase;
            NNInfoInternal nearest2 = GetNearest(matrix.MultiplyPoint3x4(outB), NNConstraint.None);
            GridNodeBase gridNodeBase2 = nearest2.node as GridNodeBase;
            if (!gridNodeBase.Walkable)
            {
                hit.node = gridNodeBase;
                hit.point = matrix.MultiplyPoint3x4(outA);
                hit.tangentOrigin = hit.point;
                return true;
            }
            Vector2 vector = new Vector2(outA.x, outA.z);
            Vector2 vector2 = new Vector2(outB.x, outB.z);
            vector -= Vector2.one * 0.5f;
            vector2 -= Vector2.one * 0.5f;
            if (gridNodeBase == null || gridNodeBase2 == null)
            {
                hit.node = null;
                hit.point = _a;
                return true;
            }
            Vector2 a = vector2 - vector;
            Int2 @int = new Int2((int)Mathf.Sign(a.x), (int)Mathf.Sign(a.y));
            float num = CrossMagnitude(a, new Vector2(@int.x, @int.y)) * 0.5f;
            int num2;
            int num3;
            if (a.y >= 0f)
            {
                if (a.x >= 0f)
                {
                    num2 = 1;
                    num3 = 2;
                }
                else
                {
                    num2 = 2;
                    num3 = 3;
                }
            }
            else if (a.x < 0f)
            {
                num2 = 3;
                num3 = 0;
            }
            else
            {
                num2 = 0;
                num3 = 1;
            }
            GridNodeBase gridNodeBase3 = gridNodeBase;
            while (gridNodeBase3.NodeInGridIndex != gridNodeBase2.NodeInGridIndex)
            {
                trace?.Add(gridNodeBase3);
                Vector2 a2 = new Vector2(gridNodeBase3.NodeInGridIndex % width, gridNodeBase3.NodeInGridIndex / width);
                float num4 = CrossMagnitude(a, a2 - vector);
                float num5 = num4 + num;
                int num6 = (!(num5 < 0f)) ? num2 : num3;
                GridNodeBase neighbourAlongDirection = GetNeighbourAlongDirection(gridNodeBase3, num6);
                if (neighbourAlongDirection != null)
                {
                    gridNodeBase3 = neighbourAlongDirection;
                    continue;
                }
                Vector2 vector3 = a2 + new Vector2(neighbourXOffsets[num6], neighbourZOffsets[num6]) * 0.5f;
                Vector2 b = (neighbourXOffsets[num6] == 0) ? new Vector2(1f, 0f) : new Vector2(0f, 1f);
                Vector2 vector4 = VectorMath.LineIntersectionPoint(vector3, vector3 + b, vector, vector2);
                Vector3 vector5 = inverseMatrix.MultiplyPoint3x4((Vector3)gridNodeBase3.position);
                Vector3 v = new Vector3(vector4.x + 0.5f, vector5.y, vector4.y + 0.5f);
                Vector3 v2 = new Vector3(vector3.x + 0.5f, vector5.y, vector3.y + 0.5f);
                hit.point = matrix.MultiplyPoint3x4(v);
                hit.tangentOrigin = matrix.MultiplyPoint3x4(v2);
                hit.tangent = matrix.MultiplyVector(new Vector3(b.x, 0f, b.y));
                hit.node = gridNodeBase3;
                return true;
            }
            trace?.Add(gridNodeBase3);
            if (gridNodeBase3 == gridNodeBase2)
            {
                return false;
            }
            hit.point = (Vector3)gridNodeBase3.position;
            hit.tangentOrigin = hit.point;
            return true;
        }

        public bool SnappedLinecast(Vector3 a, Vector3 b, GraphNode hint, out GraphHitInfo hit)
        {
            NNInfoInternal nearest = GetNearest(a, NNConstraint.None);
            Vector3 a2 = (Vector3)nearest.node.position;
            NNInfoInternal nearest2 = GetNearest(b, NNConstraint.None);
            return Linecast(a2, (Vector3)nearest2.node.position, hint, out hit);
        }

        public bool CheckConnection(GridNode node, int dir)
        {
            if (neighbours == NumNeighbours.Eight || neighbours == NumNeighbours.Six || dir < 4)
            {
                return HasNodeConnection(node, dir);
            }
            int num = (dir - 4 - 1) & 3;
            int num2 = (dir - 4 + 1) & 3;
            if (!HasNodeConnection(node, num) || !HasNodeConnection(node, num2))
            {
                return false;
            }
            GridNode gridNode = nodes[node.NodeInGridIndex + neighbourOffsets[num]];
            GridNode gridNode2 = nodes[node.NodeInGridIndex + neighbourOffsets[num2]];
            if (!gridNode.Walkable || !gridNode2.Walkable)
            {
                return false;
            }
            if (!HasNodeConnection(gridNode2, num) || !HasNodeConnection(gridNode, num2))
            {
                return false;
            }
            return true;
        }

        public override void SerializeExtraInfo(GraphSerializationContext ctx)
        {
            if (nodes == null)
            {
                ctx.writer.Write(-1);
                return;
            }
            ctx.writer.Write(nodes.Length);
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i].SerializeNode(ctx);
            }
        }

        public override void DeserializeExtraInfo(GraphSerializationContext ctx)
        {
            int num = ctx.reader.ReadInt32();
            if (num == -1)
            {
                nodes = null;
                return;
            }
            nodes = new GridNode[num];
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i] = new GridNode(active);
                nodes[i].DeserializeNode(ctx);
            }
        }

        public override void DeserializeSettingsCompatibility(GraphSerializationContext ctx)
        {
            base.DeserializeSettingsCompatibility(ctx);
            aspectRatio = ctx.reader.ReadSingle();
            rotation = ctx.DeserializeVector3();
            center = ctx.DeserializeVector3();
            unclampedSize = ctx.DeserializeVector3();
            nodeSize = ctx.reader.ReadSingle();
            collision.DeserializeSettingsCompatibility(ctx);
            maxClimb = ctx.reader.ReadSingle();
            maxClimbAxis = ctx.reader.ReadInt32();
            maxSlope = ctx.reader.ReadSingle();
            erodeIterations = ctx.reader.ReadInt32();
            erosionUseTags = ctx.reader.ReadBoolean();
            erosionFirstTag = ctx.reader.ReadInt32();
            autoLinkGrids = ctx.reader.ReadBoolean();
            neighbours = (NumNeighbours)ctx.reader.ReadInt32();
            cutCorners = ctx.reader.ReadBoolean();
            penaltyPosition = ctx.reader.ReadBoolean();
            penaltyPositionFactor = ctx.reader.ReadSingle();
            penaltyAngle = ctx.reader.ReadBoolean();
            penaltyAngleFactor = ctx.reader.ReadSingle();
            penaltyAnglePower = ctx.reader.ReadSingle();
            isometricAngle = ctx.reader.ReadSingle();
            uniformEdgeCosts = ctx.reader.ReadBoolean();
            useJumpPointSearch = ctx.reader.ReadBoolean();
        }

        public override void PostDeserialization()
        {
            GenerateMatrix();
            SetUpOffsetsAndCosts();
            if (nodes == null || nodes.Length == 0)
            {
                return;
            }
            if (width * depth != nodes.Length)
            {
                Debug.LogError("Node data did not match with bounds data. Probably a change to the bounds/width/depth data was made after scanning the graph just prior to saving it. Nodes will be discarded");
                nodes = new GridNode[0];
                return;
            }
            GridNode.SetGridGraph(AstarPath.active.astarData.GetGraphIndex(this), this);
            for (int i = 0; i < depth; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    GridNode gridNode = nodes[i * width + j];
                    if (gridNode == null)
                    {
                        Debug.LogError("Deserialization Error : Couldn't cast the node to the appropriate type - GridGenerator");
                        return;
                    }
                    gridNode.NodeInGridIndex = i * width + j;
                }
            }
        }
    }
}
