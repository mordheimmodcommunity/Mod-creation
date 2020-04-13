using Pathfinding.Serialization;
using Pathfinding.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    [Serializable]
    [JsonOptIn]
    public class NavMeshGraph : NavGraph, IUpdatableGraph, IRaycastableGraph, INavmesh, INavmeshHolder
    {
        [JsonMember]
        public Mesh sourceMesh;

        [JsonMember]
        public Vector3 offset;

        [JsonMember]
        public Vector3 rotation;

        [JsonMember]
        public float scale = 1f;

        [JsonMember]
        public bool accurateNearestNode = true;

        public TriangleMeshNode[] nodes;

        private BBTree _bbTree;

        [NonSerialized]
        private Int3[] _vertices;

        [NonSerialized]
        private Vector3[] originalVertices;

        [NonSerialized]
        public int[] triangles;

        public TriangleMeshNode[] TriNodes => nodes;

        public BBTree bbTree
        {
            get
            {
                return _bbTree;
            }
            set
            {
                _bbTree = value;
            }
        }

        public Int3[] vertices
        {
            get
            {
                return _vertices;
            }
            set
            {
                _vertices = value;
            }
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

        public override void OnDestroy()
        {
            base.OnDestroy();
            TriangleMeshNode.SetNavmeshHolder(active.astarData.GetGraphIndex(this), null);
        }

        public Int3 GetVertex(int index)
        {
            return vertices[index];
        }

        public int GetVertexArrayIndex(int index)
        {
            return index;
        }

        public void GetTileCoordinates(int tileIndex, out int x, out int z)
        {
            x = (z = 0);
        }

        public void GenerateMatrix()
        {
            SetMatrix(Matrix4x4.TRS(offset, Quaternion.Euler(rotation), new Vector3(scale, scale, scale)));
        }

        public override void RelocateNodes(Matrix4x4 oldMatrix, Matrix4x4 newMatrix)
        {
            if (vertices == null || vertices.Length == 0 || originalVertices == null || originalVertices.Length != vertices.Length)
            {
                return;
            }
            for (int i = 0; i < _vertices.Length; i++)
            {
                _vertices[i] = (Int3)newMatrix.MultiplyPoint3x4(originalVertices[i]);
            }
            for (int j = 0; j < nodes.Length; j++)
            {
                TriangleMeshNode triangleMeshNode = nodes[j];
                triangleMeshNode.UpdatePositionFromVertices();
                if (triangleMeshNode.connections != null)
                {
                    for (int k = 0; k < triangleMeshNode.connections.Length; k++)
                    {
                        triangleMeshNode.connectionCosts[k] = (uint)(triangleMeshNode.position - triangleMeshNode.connections[k].position).costMagnitude;
                    }
                }
            }
            SetMatrix(newMatrix);
            RebuildBBTree(this);
        }

        public static NNInfoInternal GetNearest(NavMeshGraph graph, GraphNode[] nodes, Vector3 position, NNConstraint constraint, bool accurateNearestNode)
        {
            if (nodes == null || nodes.Length == 0)
            {
                Debug.LogError("NavGraph hasn't been generated yet or does not contain any nodes");
                return default(NNInfoInternal);
            }
            if (constraint == null)
            {
                constraint = NNConstraint.None;
            }
            Int3[] vertices = graph.vertices;
            if (graph.bbTree == null)
            {
                return GetNearestForce(graph, graph, position, constraint, accurateNearestNode);
            }
            float num = (graph.bbTree.Size.width + graph.bbTree.Size.height) * 0.5f * 0.02f;
            NNInfoInternal result = graph.bbTree.QueryCircle(position, num, constraint);
            if (result.node == null)
            {
                for (int i = 1; i <= 8; i++)
                {
                    result = graph.bbTree.QueryCircle(position, (float)(i * i) * num, constraint);
                    if (result.node != null || (float)((i - 1) * (i - 1)) * num > AstarPath.active.maxNearestNodeDistance * 2f)
                    {
                        break;
                    }
                }
            }
            if (result.node != null)
            {
                result.clampedPosition = ClosestPointOnNode(result.node as TriangleMeshNode, vertices, position);
            }
            if (result.constrainedNode != null)
            {
                if (constraint.constrainDistance && ((Vector3)result.constrainedNode.position - position).sqrMagnitude > AstarPath.active.maxNearestNodeDistanceSqr)
                {
                    result.constrainedNode = null;
                }
                else
                {
                    result.constClampedPosition = ClosestPointOnNode(result.constrainedNode as TriangleMeshNode, vertices, position);
                }
            }
            return result;
        }

        public override NNInfoInternal GetNearest(Vector3 position, NNConstraint constraint, GraphNode hint)
        {
            return GetNearest(this, nodes, position, constraint, accurateNearestNode);
        }

        public override NNInfoInternal GetNearestForce(Vector3 position, NNConstraint constraint)
        {
            return GetNearestForce(this, this, position, constraint, accurateNearestNode);
        }

        public static NNInfoInternal GetNearestForce(NavGraph graph, INavmeshHolder navmesh, Vector3 position, NNConstraint constraint, bool accurateNearestNode)
        {
            NNInfoInternal nearestForceBoth = GetNearestForceBoth(graph, navmesh, position, constraint, accurateNearestNode);
            nearestForceBoth.node = nearestForceBoth.constrainedNode;
            nearestForceBoth.clampedPosition = nearestForceBoth.constClampedPosition;
            return nearestForceBoth;
        }

        public static NNInfoInternal GetNearestForceBoth(NavGraph graph, INavmeshHolder navmesh, Vector3 position, NNConstraint constraint, bool accurateNearestNode)
        {
            Int3 pos = (Int3)position;
            float minDist = -1f;
            GraphNode minNode = null;
            float minConstDist = -1f;
            GraphNode minConstNode = null;
            float maxDistSqr = (!constraint.constrainDistance) ? float.PositiveInfinity : AstarPath.active.maxNearestNodeDistanceSqr;
            GraphNodeDelegateCancelable del = delegate (GraphNode _node)
            {
                TriangleMeshNode triangleMeshNode3 = _node as TriangleMeshNode;
                if (accurateNearestNode)
                {
                    Vector3 b = triangleMeshNode3.ClosestPointOnNode(position);
                    float sqrMagnitude = ((Vector3)pos - b).sqrMagnitude;
                    if (minNode == null || sqrMagnitude < minDist)
                    {
                        minDist = sqrMagnitude;
                        minNode = triangleMeshNode3;
                    }
                    if (sqrMagnitude < maxDistSqr && constraint.Suitable(triangleMeshNode3) && (minConstNode == null || sqrMagnitude < minConstDist))
                    {
                        minConstDist = sqrMagnitude;
                        minConstNode = triangleMeshNode3;
                    }
                }
                else if (!triangleMeshNode3.ContainsPoint((Int3)position))
                {
                    float sqrMagnitude2 = (triangleMeshNode3.position - pos).sqrMagnitude;
                    if (minNode == null || sqrMagnitude2 < minDist)
                    {
                        minDist = sqrMagnitude2;
                        minNode = triangleMeshNode3;
                    }
                    if (sqrMagnitude2 < maxDistSqr && constraint.Suitable(triangleMeshNode3) && (minConstNode == null || sqrMagnitude2 < minConstDist))
                    {
                        minConstDist = sqrMagnitude2;
                        minConstNode = triangleMeshNode3;
                    }
                }
                else
                {
                    int num = Math.Abs(triangleMeshNode3.position.y - pos.y);
                    if (minNode == null || (float)num < minDist)
                    {
                        minDist = num;
                        minNode = triangleMeshNode3;
                    }
                    if ((float)num < maxDistSqr && constraint.Suitable(triangleMeshNode3) && (minConstNode == null || (float)num < minConstDist))
                    {
                        minConstDist = num;
                        minConstNode = triangleMeshNode3;
                    }
                }
                return true;
            };
            graph.GetNodes(del);
            NNInfoInternal result = new NNInfoInternal(minNode);
            if (result.node != null)
            {
                TriangleMeshNode triangleMeshNode = result.node as TriangleMeshNode;
                Vector3 vector = result.clampedPosition = triangleMeshNode.ClosestPointOnNode(position);
            }
            result.constrainedNode = minConstNode;
            if (result.constrainedNode != null)
            {
                TriangleMeshNode triangleMeshNode2 = result.constrainedNode as TriangleMeshNode;
                Vector3 vector2 = result.constClampedPosition = triangleMeshNode2.ClosestPointOnNode(position);
            }
            return result;
        }

        public bool Linecast(Vector3 origin, Vector3 end)
        {
            NNInfoInternal nearest = GetNearest(origin, NNConstraint.None);
            return Linecast(origin, end, nearest.node);
        }

        public bool Linecast(Vector3 origin, Vector3 end, GraphNode hint, out GraphHitInfo hit)
        {
            return Linecast(this, origin, end, hint, out hit, null);
        }

        public bool Linecast(Vector3 origin, Vector3 end, GraphNode hint)
        {
            GraphHitInfo hit;
            return Linecast(this, origin, end, hint, out hit, null);
        }

        public bool Linecast(Vector3 origin, Vector3 end, GraphNode hint, out GraphHitInfo hit, List<GraphNode> trace)
        {
            return Linecast(this, origin, end, hint, out hit, trace);
        }

        public static bool Linecast(INavmesh graph, Vector3 tmp_origin, Vector3 tmp_end, GraphNode hint, out GraphHitInfo hit)
        {
            return Linecast(graph, tmp_origin, tmp_end, hint, out hit, null);
        }

        public static bool Linecast(INavmesh graph, Vector3 tmp_origin, Vector3 tmp_end, GraphNode hint, out GraphHitInfo hit, List<GraphNode> trace)
        {
            Int3 @int = (Int3)tmp_end;
            Int3 int2 = (Int3)tmp_origin;
            hit = default(GraphHitInfo);
            if (float.IsNaN(tmp_origin.x + tmp_origin.y + tmp_origin.z))
            {
                throw new ArgumentException("origin is NaN");
            }
            if (float.IsNaN(tmp_end.x + tmp_end.y + tmp_end.z))
            {
                throw new ArgumentException("end is NaN");
            }
            TriangleMeshNode triangleMeshNode = hint as TriangleMeshNode;
            if (triangleMeshNode == null)
            {
                NNInfoInternal nearest = (graph as NavGraph).GetNearest(tmp_origin, NNConstraint.None);
                triangleMeshNode = (nearest.node as TriangleMeshNode);
                if (triangleMeshNode == null)
                {
                    Debug.LogError("Could not find a valid node to start from");
                    hit.point = tmp_origin;
                    return true;
                }
            }
            if (int2 == @int)
            {
                hit.node = triangleMeshNode;
                return false;
            }
            int2 = (Int3)triangleMeshNode.ClosestPointOnNode((Vector3)int2);
            hit.origin = (Vector3)int2;
            if (!triangleMeshNode.Walkable)
            {
                hit.point = (Vector3)int2;
                hit.tangentOrigin = (Vector3)int2;
                return true;
            }
            List<Vector3> list = ListPool<Vector3>.Claim();
            List<Vector3> list2 = ListPool<Vector3>.Claim();
            int num = 0;
            while (true)
            {
                num++;
                if (num > 2000)
                {
                    Debug.LogError("Linecast was stuck in infinite loop. Breaking.");
                    ListPool<Vector3>.Release(list);
                    ListPool<Vector3>.Release(list2);
                    return true;
                }
                TriangleMeshNode triangleMeshNode2 = null;
                trace?.Add(triangleMeshNode);
                if (triangleMeshNode.ContainsPoint(@int))
                {
                    ListPool<Vector3>.Release(list);
                    ListPool<Vector3>.Release(list2);
                    return false;
                }
                for (int i = 0; i < triangleMeshNode.connections.Length; i++)
                {
                    if (triangleMeshNode.connections[i].GraphIndex != triangleMeshNode.GraphIndex)
                    {
                        continue;
                    }
                    list.Clear();
                    list2.Clear();
                    if (triangleMeshNode.GetPortal(triangleMeshNode.connections[i], list, list2, backwards: false))
                    {
                        Vector3 vector = list[0];
                        Vector3 vector2 = list2[0];
                        if ((VectorMath.RightXZ(vector, vector2, hit.origin) || !VectorMath.RightXZ(vector, vector2, tmp_end)) && VectorMath.LineIntersectionFactorXZ(vector, vector2, hit.origin, tmp_end, out float factor, out float factor2) && !(factor2 < 0f) && factor >= 0f && factor <= 1f)
                        {
                            triangleMeshNode2 = (triangleMeshNode.connections[i] as TriangleMeshNode);
                            break;
                        }
                    }
                }
                if (triangleMeshNode2 == null)
                {
                    break;
                }
                triangleMeshNode = triangleMeshNode2;
            }
            int vertexCount = triangleMeshNode.GetVertexCount();
            for (int j = 0; j < vertexCount; j++)
            {
                Vector3 vector3 = (Vector3)triangleMeshNode.GetVertex(j);
                Vector3 vector4 = (Vector3)triangleMeshNode.GetVertex((j + 1) % vertexCount);
                if ((VectorMath.RightXZ(vector3, vector4, hit.origin) || !VectorMath.RightXZ(vector3, vector4, tmp_end)) && VectorMath.LineIntersectionFactorXZ(vector3, vector4, hit.origin, tmp_end, out float factor3, out float factor4) && !(factor4 < 0f) && factor3 >= 0f && factor3 <= 1f)
                {
                    Vector3 vector5 = hit.point = vector3 + (vector4 - vector3) * factor3;
                    hit.node = triangleMeshNode;
                    hit.tangent = vector4 - vector3;
                    hit.tangentOrigin = vector3;
                    ListPool<Vector3>.Release(list);
                    ListPool<Vector3>.Release(list2);
                    return true;
                }
            }
            Debug.LogWarning("Linecast failing because point not inside node, and line does not hit any edges of it");
            ListPool<Vector3>.Release(list);
            ListPool<Vector3>.Release(list2);
            return false;
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
            UpdateArea(o, this);
        }

        public static void UpdateArea(GraphUpdateObject o, INavmesh graph)
        {
            Bounds bounds = o.bounds;
            Vector3 min = bounds.min;
            float x = min.x;
            Vector3 min2 = bounds.min;
            float z = min2.z;
            Vector3 max = bounds.max;
            float x2 = max.x;
            Vector3 max2 = bounds.max;
            Rect r = Rect.MinMaxRect(x, z, x2, max2.z);
            Vector3 min3 = bounds.min;
            int xmin = Mathf.FloorToInt(min3.x * 1000f);
            Vector3 min4 = bounds.min;
            int ymin2 = Mathf.FloorToInt(min4.z * 1000f);
            Vector3 max3 = bounds.max;
            int xmax = Mathf.FloorToInt(max3.x * 1000f);
            Vector3 max4 = bounds.max;
            IntRect r2 = new IntRect(xmin, ymin2, xmax, Mathf.FloorToInt(max4.z * 1000f));
            Int3 a = new Int3(r2.xmin, 0, r2.ymin);
            Int3 b = new Int3(r2.xmin, 0, r2.ymax);
            Int3 c = new Int3(r2.xmax, 0, r2.ymin);
            Int3 d = new Int3(r2.xmax, 0, r2.ymax);
            Int3 @int = (Int3)bounds.min;
            int ymin = @int.y;
            Int3 int2 = (Int3)bounds.max;
            int ymax = int2.y;
            graph.GetNodes(delegate (GraphNode _node)
            {
                TriangleMeshNode triangleMeshNode = _node as TriangleMeshNode;
                bool flag = false;
                int num = 0;
                int num2 = 0;
                int num3 = 0;
                int num4 = 0;
                for (int i = 0; i < 3; i++)
                {
                    Int3 vertex = triangleMeshNode.GetVertex(i);
                    Vector3 vector = (Vector3)vertex;
                    if (r2.Contains(vertex.x, vertex.z))
                    {
                        flag = true;
                        break;
                    }
                    if (vector.x < r.xMin)
                    {
                        num++;
                    }
                    if (vector.x > r.xMax)
                    {
                        num2++;
                    }
                    if (vector.z < r.yMin)
                    {
                        num3++;
                    }
                    if (vector.z > r.yMax)
                    {
                        num4++;
                    }
                }
                if (!flag && (num == 3 || num2 == 3 || num3 == 3 || num4 == 3))
                {
                    return true;
                }
                for (int j = 0; j < 3; j++)
                {
                    int i2 = (j <= 1) ? (j + 1) : 0;
                    Int3 vertex2 = triangleMeshNode.GetVertex(j);
                    Int3 vertex3 = triangleMeshNode.GetVertex(i2);
                    if (VectorMath.SegmentsIntersectXZ(a, b, vertex2, vertex3))
                    {
                        flag = true;
                        break;
                    }
                    if (VectorMath.SegmentsIntersectXZ(a, c, vertex2, vertex3))
                    {
                        flag = true;
                        break;
                    }
                    if (VectorMath.SegmentsIntersectXZ(c, d, vertex2, vertex3))
                    {
                        flag = true;
                        break;
                    }
                    if (VectorMath.SegmentsIntersectXZ(d, b, vertex2, vertex3))
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag || triangleMeshNode.ContainsPoint(a) || triangleMeshNode.ContainsPoint(b) || triangleMeshNode.ContainsPoint(c) || triangleMeshNode.ContainsPoint(d))
                {
                    flag = true;
                }
                if (!flag)
                {
                    return true;
                }
                int num5 = 0;
                int num6 = 0;
                for (int k = 0; k < 3; k++)
                {
                    Int3 vertex4 = triangleMeshNode.GetVertex(k);
                    if (vertex4.y < ymin)
                    {
                        num6++;
                    }
                    if (vertex4.y > ymax)
                    {
                        num5++;
                    }
                }
                if (num6 == 3 || num5 == 3)
                {
                    return true;
                }
                o.WillUpdateNode(triangleMeshNode);
                o.Apply(triangleMeshNode);
                return true;
            });
        }

        private static Vector3 ClosestPointOnNode(TriangleMeshNode node, Int3[] vertices, Vector3 pos)
        {
            return Polygon.ClosestPointOnTriangle((Vector3)vertices[node.v0], (Vector3)vertices[node.v1], (Vector3)vertices[node.v2], pos);
        }

        [Obsolete("Use TriangleMeshNode.ContainsPoint instead")]
        public bool ContainsPoint(TriangleMeshNode node, Vector3 pos)
        {
            if (VectorMath.IsClockwiseXZ((Vector3)vertices[node.v0], (Vector3)vertices[node.v1], pos) && VectorMath.IsClockwiseXZ((Vector3)vertices[node.v1], (Vector3)vertices[node.v2], pos) && VectorMath.IsClockwiseXZ((Vector3)vertices[node.v2], (Vector3)vertices[node.v0], pos))
            {
                return true;
            }
            return false;
        }

        [Obsolete("Use TriangleMeshNode.ContainsPoint instead")]
        public static bool ContainsPoint(TriangleMeshNode node, Vector3 pos, Int3[] vertices)
        {
            if (!VectorMath.IsClockwiseMarginXZ((Vector3)vertices[node.v0], (Vector3)vertices[node.v1], (Vector3)vertices[node.v2]))
            {
                Debug.LogError("Noes!");
            }
            if (VectorMath.IsClockwiseMarginXZ((Vector3)vertices[node.v0], (Vector3)vertices[node.v1], pos) && VectorMath.IsClockwiseMarginXZ((Vector3)vertices[node.v1], (Vector3)vertices[node.v2], pos) && VectorMath.IsClockwiseMarginXZ((Vector3)vertices[node.v2], (Vector3)vertices[node.v0], pos))
            {
                return true;
            }
            return false;
        }

        public void ScanInternal(string objMeshPath)
        {
            Mesh x = ObjImporter.ImportFile(objMeshPath);
            if (x == null)
            {
                Debug.LogError("Couldn't read .obj file at '" + objMeshPath + "'");
                return;
            }
            sourceMesh = x;
            IEnumerator<Progress> enumerator = ScanInternal().GetEnumerator();
            while (enumerator.MoveNext())
            {
            }
        }

        public override IEnumerable<Progress> ScanInternal()
        {
            ScanInternal(delegate
            {
            });
            yield break;
        }

        public void ScanInternal(OnScanStatus statusCallback)
        {
            if (!(sourceMesh == null))
            {
                GenerateMatrix();
                Vector3[] vertices = sourceMesh.vertices;
                triangles = sourceMesh.triangles;
                TriangleMeshNode.SetNavmeshHolder(active.astarData.GetGraphIndex(this), this);
                GenerateNodes(vertices, triangles, out originalVertices, out _vertices);
            }
        }

        private void GenerateNodes(Vector3[] vectorVertices, int[] triangles, out Vector3[] originalVertices, out Int3[] vertices)
        {
            if (vectorVertices.Length == 0 || triangles.Length == 0)
            {
                originalVertices = vectorVertices;
                vertices = new Int3[0];
                nodes = new TriangleMeshNode[0];
                return;
            }
            vertices = new Int3[vectorVertices.Length];
            int num = 0;
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = (Int3)matrix.MultiplyPoint3x4(vectorVertices[i]);
            }
            Dictionary<Int3, int> dictionary = new Dictionary<Int3, int>();
            int[] array = new int[vertices.Length];
            for (int j = 0; j < vertices.Length; j++)
            {
                if (!dictionary.ContainsKey(vertices[j]))
                {
                    array[num] = j;
                    dictionary.Add(vertices[j], num);
                    num++;
                }
            }
            for (int k = 0; k < triangles.Length; k++)
            {
                Int3 key = vertices[triangles[k]];
                triangles[k] = dictionary[key];
            }
            Int3[] array2 = vertices;
            vertices = new Int3[num];
            originalVertices = new Vector3[num];
            for (int l = 0; l < num; l++)
            {
                vertices[l] = array2[array[l]];
                originalVertices[l] = vectorVertices[array[l]];
            }
            nodes = new TriangleMeshNode[triangles.Length / 3];
            int graphIndex = active.astarData.GetGraphIndex(this);
            for (int m = 0; m < nodes.Length; m++)
            {
                nodes[m] = new TriangleMeshNode(active);
                TriangleMeshNode triangleMeshNode = nodes[m];
                triangleMeshNode.GraphIndex = (uint)graphIndex;
                triangleMeshNode.Penalty = initialPenalty;
                triangleMeshNode.Walkable = true;
                triangleMeshNode.v0 = triangles[m * 3];
                triangleMeshNode.v1 = triangles[m * 3 + 1];
                triangleMeshNode.v2 = triangles[m * 3 + 2];
                if (!VectorMath.IsClockwiseXZ(vertices[triangleMeshNode.v0], vertices[triangleMeshNode.v1], vertices[triangleMeshNode.v2]))
                {
                    int v = triangleMeshNode.v0;
                    triangleMeshNode.v0 = triangleMeshNode.v2;
                    triangleMeshNode.v2 = v;
                }
                if (VectorMath.IsColinearXZ(vertices[triangleMeshNode.v0], vertices[triangleMeshNode.v1], vertices[triangleMeshNode.v2]))
                {
                    Debug.DrawLine((Vector3)vertices[triangleMeshNode.v0], (Vector3)vertices[triangleMeshNode.v1], Color.red);
                    Debug.DrawLine((Vector3)vertices[triangleMeshNode.v1], (Vector3)vertices[triangleMeshNode.v2], Color.red);
                    Debug.DrawLine((Vector3)vertices[triangleMeshNode.v2], (Vector3)vertices[triangleMeshNode.v0], Color.red);
                }
                triangleMeshNode.UpdatePositionFromVertices();
            }
            Dictionary<Int2, TriangleMeshNode> dictionary2 = new Dictionary<Int2, TriangleMeshNode>();
            int n = 0;
            int num2 = 0;
            for (; n < triangles.Length; n += 3)
            {
                dictionary2[new Int2(triangles[n], triangles[n + 1])] = nodes[num2];
                dictionary2[new Int2(triangles[n + 1], triangles[n + 2])] = nodes[num2];
                dictionary2[new Int2(triangles[n + 2], triangles[n])] = nodes[num2];
                num2++;
            }
            List<MeshNode> list = new List<MeshNode>();
            List<uint> list2 = new List<uint>();
            int num3 = 0;
            int num4 = 0;
            for (; num3 < triangles.Length; num3 += 3)
            {
                list.Clear();
                list2.Clear();
                TriangleMeshNode triangleMeshNode2 = nodes[num4];
                for (int num5 = 0; num5 < 3; num5++)
                {
                    if (dictionary2.TryGetValue(new Int2(triangles[num3 + (num5 + 1) % 3], triangles[num3 + num5]), out TriangleMeshNode value))
                    {
                        list.Add(value);
                        list2.Add((uint)(triangleMeshNode2.position - value.position).costMagnitude);
                    }
                }
                triangleMeshNode2.connections = list.ToArray();
                triangleMeshNode2.connectionCosts = list2.ToArray();
                num4++;
            }
            RebuildBBTree(this);
        }

        public static void RebuildBBTree(NavMeshGraph graph)
        {
            BBTree bbTree = graph.bbTree;
            bbTree = (bbTree ?? new BBTree());
            bbTree.RebuildFrom(graph.nodes);
            graph.bbTree = bbTree;
        }

        public void PostProcess()
        {
        }

        public override void OnDrawGizmos(bool drawNodes)
        {
            if (!drawNodes)
            {
                return;
            }
            Matrix4x4 matrix = base.matrix;
            GenerateMatrix();
            if (nodes == null)
            {
            }
            if (nodes == null)
            {
                return;
            }
            if (matrix != base.matrix)
            {
                RelocateNodes(matrix, base.matrix);
            }
            PathHandler debugPathData = AstarPath.active.debugPathData;
            for (int i = 0; i < nodes.Length; i++)
            {
                TriangleMeshNode triangleMeshNode = nodes[i];
                Gizmos.color = NodeColor(triangleMeshNode, AstarPath.active.debugPathData);
                if (triangleMeshNode.Walkable)
                {
                    if (AstarPath.active.showSearchTree && debugPathData != null && debugPathData.GetPathNode(triangleMeshNode).parent != null)
                    {
                        Gizmos.DrawLine((Vector3)triangleMeshNode.position, (Vector3)debugPathData.GetPathNode(triangleMeshNode).parent.node.position);
                    }
                    else
                    {
                        for (int j = 0; j < triangleMeshNode.connections.Length; j++)
                        {
                            Gizmos.DrawLine((Vector3)triangleMeshNode.position, Vector3.Lerp((Vector3)triangleMeshNode.position, (Vector3)triangleMeshNode.connections[j].position, 0.45f));
                        }
                    }
                    Gizmos.color = AstarColor.MeshEdgeColor;
                }
                else
                {
                    Gizmos.color = AstarColor.UnwalkableNode;
                }
                Gizmos.DrawLine((Vector3)vertices[triangleMeshNode.v0], (Vector3)vertices[triangleMeshNode.v1]);
                Gizmos.DrawLine((Vector3)vertices[triangleMeshNode.v1], (Vector3)vertices[triangleMeshNode.v2]);
                Gizmos.DrawLine((Vector3)vertices[triangleMeshNode.v2], (Vector3)vertices[triangleMeshNode.v0]);
            }
        }

        public override void DeserializeExtraInfo(GraphSerializationContext ctx)
        {
            uint graphIndex = ctx.graphIndex;
            TriangleMeshNode.SetNavmeshHolder((int)graphIndex, this);
            int num = ctx.reader.ReadInt32();
            int num2 = ctx.reader.ReadInt32();
            if (num == -1)
            {
                nodes = new TriangleMeshNode[0];
                _vertices = new Int3[0];
                originalVertices = new Vector3[0];
                return;
            }
            nodes = new TriangleMeshNode[num];
            _vertices = new Int3[num2];
            originalVertices = new Vector3[num2];
            for (int i = 0; i < num2; i++)
            {
                _vertices[i] = ctx.DeserializeInt3();
                originalVertices[i] = ctx.DeserializeVector3();
            }
            bbTree = new BBTree();
            for (int j = 0; j < num; j++)
            {
                nodes[j] = new TriangleMeshNode(active);
                TriangleMeshNode triangleMeshNode = nodes[j];
                triangleMeshNode.DeserializeNode(ctx);
                triangleMeshNode.UpdatePositionFromVertices();
            }
            bbTree.RebuildFrom(nodes);
        }

        public override void SerializeExtraInfo(GraphSerializationContext ctx)
        {
            if (nodes == null || originalVertices == null || _vertices == null || originalVertices.Length != _vertices.Length)
            {
                ctx.writer.Write(-1);
                ctx.writer.Write(-1);
                return;
            }
            ctx.writer.Write(nodes.Length);
            ctx.writer.Write(_vertices.Length);
            for (int i = 0; i < _vertices.Length; i++)
            {
                ctx.SerializeInt3(_vertices[i]);
                ctx.SerializeVector3(originalVertices[i]);
            }
            for (int j = 0; j < nodes.Length; j++)
            {
                nodes[j].SerializeNode(ctx);
            }
        }

        public override void DeserializeSettingsCompatibility(GraphSerializationContext ctx)
        {
            base.DeserializeSettingsCompatibility(ctx);
            sourceMesh = (ctx.DeserializeUnityObject() as Mesh);
            offset = ctx.DeserializeVector3();
            rotation = ctx.DeserializeVector3();
            scale = ctx.reader.ReadSingle();
            accurateNearestNode = ctx.reader.ReadBoolean();
        }
    }
}
