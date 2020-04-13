using Pathfinding.Serialization;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class GridNode : GridNodeBase
    {
        private const int GridFlagsConnectionOffset = 0;

        private const int GridFlagsConnectionBit0 = 1;

        private const int GridFlagsConnectionMask = 255;

        private const int GridFlagsEdgeNodeOffset = 10;

        private const int GridFlagsEdgeNodeMask = 1024;

        private static GridGraph[] _gridGraphs = new GridGraph[0];

        internal ushort InternalGridFlags
        {
            get
            {
                return gridFlags;
            }
            set
            {
                gridFlags = value;
            }
        }

        public bool EdgeNode
        {
            get
            {
                return (gridFlags & 0x400) != 0;
            }
            set
            {
                gridFlags = (ushort)((gridFlags & -1025) | (value ? 1024 : 0));
            }
        }

        public int XCoordinateInGrid => nodeInGridIndex % GetGridGraph(base.GraphIndex).width;

        public int ZCoordinateInGrid => nodeInGridIndex / GetGridGraph(base.GraphIndex).width;

        public GridNode(AstarPath astar)
            : base(astar)
        {
        }

        public static GridGraph GetGridGraph(uint graphIndex)
        {
            return _gridGraphs[graphIndex];
        }

        public static void SetGridGraph(int graphIndex, GridGraph graph)
        {
            if (_gridGraphs.Length <= graphIndex)
            {
                GridGraph[] array = new GridGraph[graphIndex + 1];
                for (int i = 0; i < _gridGraphs.Length; i++)
                {
                    array[i] = _gridGraphs[i];
                }
                _gridGraphs = array;
            }
            _gridGraphs[graphIndex] = graph;
        }

        public bool GetConnectionInternal(int dir)
        {
            return ((gridFlags >> dir) & 1) != 0;
        }

        public void SetConnectionInternal(int dir, bool value)
        {
            gridFlags = (ushort)((gridFlags & ~(1 << dir)) | ((value ? 1 : 0) << dir));
        }

        public void SetAllConnectionInternal(int connections)
        {
            gridFlags = (ushort)((gridFlags & -256) | connections);
        }

        public void ResetConnectionsInternal()
        {
            gridFlags = (ushort)(gridFlags & -256);
        }

        public override void ClearConnections(bool alsoReverse)
        {
            if (alsoReverse)
            {
                GridGraph gridGraph = GetGridGraph(base.GraphIndex);
                for (int i = 0; i < 8; i++)
                {
                    gridGraph.GetNodeConnection(this, i)?.SetConnectionInternal((i >= 4) ? 7 : ((i + 2) % 4), value: false);
                }
            }
            ResetConnectionsInternal();
            base.ClearConnections(alsoReverse);
        }

        public override void GetConnections(GraphNodeDelegate del)
        {
            GridGraph gridGraph = GetGridGraph(base.GraphIndex);
            int[] neighbourOffsets = gridGraph.neighbourOffsets;
            GridNode[] nodes = gridGraph.nodes;
            for (int i = 0; i < 8; i++)
            {
                if (GetConnectionInternal(i))
                {
                    GridNode gridNode = nodes[nodeInGridIndex + neighbourOffsets[i]];
                    if (gridNode != null)
                    {
                        del(gridNode);
                    }
                }
            }
            base.GetConnections(del);
        }

        public Vector3 ClosestPointOnNode(Vector3 p)
        {
            GridGraph gridGraph = GetGridGraph(base.GraphIndex);
            p = gridGraph.inverseMatrix.MultiplyPoint3x4(p);
            float value = (float)position.x - 0.5f;
            float value2 = (float)position.z - 0.5f;
            int num = nodeInGridIndex % gridGraph.width;
            int num2 = nodeInGridIndex / gridGraph.width;
            Vector3 vector = gridGraph.inverseMatrix.MultiplyPoint3x4(p);
            float y = vector.y;
            Vector3 v = new Vector3(Mathf.Clamp(value, (float)num - 0.5f, (float)num + 0.5f) + 0.5f, y, Mathf.Clamp(value2, (float)num2 - 0.5f, (float)num2 + 0.5f) + 0.5f);
            return gridGraph.matrix.MultiplyPoint3x4(v);
        }

        public override bool GetPortal(GraphNode other, List<Vector3> left, List<Vector3> right, bool backwards)
        {
            if (backwards)
            {
                return true;
            }
            GridGraph gridGraph = GetGridGraph(base.GraphIndex);
            int[] neighbourOffsets = gridGraph.neighbourOffsets;
            GridNode[] nodes = gridGraph.nodes;
            for (int i = 0; i < 4; i++)
            {
                if (GetConnectionInternal(i) && other == nodes[nodeInGridIndex + neighbourOffsets[i]])
                {
                    Vector3 a = (Vector3)(position + other.position) * 0.5f;
                    Vector3 b = Vector3.Cross(gridGraph.collision.up, (Vector3)(other.position - position));
                    b.Normalize();
                    b *= gridGraph.nodeSize * 0.5f;
                    left.Add(a - b);
                    right.Add(a + b);
                    return true;
                }
            }
            for (int j = 4; j < 8; j++)
            {
                if (!GetConnectionInternal(j) || other != nodes[nodeInGridIndex + neighbourOffsets[j]])
                {
                    continue;
                }
                bool flag = false;
                bool flag2 = false;
                if (GetConnectionInternal(j - 4))
                {
                    GridNode gridNode = nodes[nodeInGridIndex + neighbourOffsets[j - 4]];
                    if (gridNode.Walkable && gridNode.GetConnectionInternal((j - 4 + 1) % 4))
                    {
                        flag = true;
                    }
                }
                if (GetConnectionInternal((j - 4 + 1) % 4))
                {
                    GridNode gridNode2 = nodes[nodeInGridIndex + neighbourOffsets[(j - 4 + 1) % 4]];
                    if (gridNode2.Walkable && gridNode2.GetConnectionInternal(j - 4))
                    {
                        flag2 = true;
                    }
                }
                Vector3 a2 = (Vector3)(position + other.position) * 0.5f;
                Vector3 vector = Vector3.Cross(gridGraph.collision.up, (Vector3)(other.position - position));
                vector.Normalize();
                vector *= gridGraph.nodeSize * 1.4142f;
                left.Add(a2 - ((!flag2) ? Vector3.zero : vector));
                right.Add(a2 + ((!flag) ? Vector3.zero : vector));
                return true;
            }
            return false;
        }

        public override void FloodFill(Stack<GraphNode> stack, uint region)
        {
            GridGraph gridGraph = GetGridGraph(base.GraphIndex);
            int[] neighbourOffsets = gridGraph.neighbourOffsets;
            GridNode[] nodes = gridGraph.nodes;
            for (int i = 0; i < 8; i++)
            {
                if (GetConnectionInternal(i))
                {
                    GridNode gridNode = nodes[nodeInGridIndex + neighbourOffsets[i]];
                    if (gridNode != null && gridNode.Area != region)
                    {
                        gridNode.Area = region;
                        stack.Push(gridNode);
                    }
                }
            }
            base.FloodFill(stack, region);
        }

        public override bool ContainsConnection(GraphNode node)
        {
            GridGraph gridGraph = GetGridGraph(base.GraphIndex);
            int[] neighbourOffsets = gridGraph.neighbourOffsets;
            GridNode[] nodes = gridGraph.nodes;
            for (int i = 0; i < 8; i++)
            {
                if (GetConnectionInternal(i))
                {
                    GridNode gridNode = nodes[nodeInGridIndex + neighbourOffsets[i]];
                    if (gridNode == node)
                    {
                        return true;
                    }
                }
            }
            return base.ContainsConnection(node);
        }

        public override void UpdateRecursiveG(Path path, PathNode pathNode, PathHandler handler)
        {
            GridGraph gridGraph = GetGridGraph(base.GraphIndex);
            int[] neighbourOffsets = gridGraph.neighbourOffsets;
            GridNode[] nodes = gridGraph.nodes;
            UpdateG(path, pathNode);
            handler.heap.Add(pathNode);
            ushort pathID = handler.PathID;
            for (int i = 0; i < 8; i++)
            {
                if (GetConnectionInternal(i))
                {
                    GridNode gridNode = nodes[nodeInGridIndex + neighbourOffsets[i]];
                    PathNode pathNode2 = handler.GetPathNode(gridNode);
                    if (pathNode2.parent == pathNode && pathNode2.pathID == pathID)
                    {
                        gridNode.UpdateRecursiveG(path, pathNode2, handler);
                    }
                }
            }
            base.UpdateRecursiveG(path, pathNode, handler);
        }

        public override void Open(Path path, PathNode pathNode, PathHandler handler)
        {
            GridGraph gridGraph = GetGridGraph(base.GraphIndex);
            ushort pathID = handler.PathID;
            int[] neighbourOffsets = gridGraph.neighbourOffsets;
            uint[] neighbourCosts = gridGraph.neighbourCosts;
            GridNode[] nodes = gridGraph.nodes;
            for (int i = 0; i < 8; i++)
            {
                if (!GetConnectionInternal(i))
                {
                    continue;
                }
                GridNode gridNode = nodes[nodeInGridIndex + neighbourOffsets[i]];
                if (path.CanTraverse(gridNode))
                {
                    PathNode pathNode2 = handler.GetPathNode(gridNode);
                    uint num = neighbourCosts[i];
                    if (pathNode2.pathID != pathID)
                    {
                        pathNode2.parent = pathNode;
                        pathNode2.pathID = pathID;
                        pathNode2.cost = num;
                        pathNode2.H = path.CalculateHScore(gridNode);
                        gridNode.UpdateG(path, pathNode2);
                        handler.heap.Add(pathNode2);
                    }
                    else if (pathNode.G + num + path.GetTraversalCost(gridNode) < pathNode2.G)
                    {
                        pathNode2.cost = num;
                        pathNode2.parent = pathNode;
                        gridNode.UpdateRecursiveG(path, pathNode2, handler);
                    }
                    else if (pathNode2.G + num + path.GetTraversalCost(this) < pathNode.G)
                    {
                        pathNode.parent = pathNode2;
                        pathNode.cost = num;
                        UpdateRecursiveG(path, pathNode, handler);
                    }
                }
            }
            base.Open(path, pathNode, handler);
        }

        public override void SerializeNode(GraphSerializationContext ctx)
        {
            base.SerializeNode(ctx);
            ctx.SerializeInt3(position);
            ctx.writer.Write(gridFlags);
        }

        public override void DeserializeNode(GraphSerializationContext ctx)
        {
            base.DeserializeNode(ctx);
            position = ctx.DeserializeInt3();
            gridFlags = ctx.reader.ReadUInt16();
        }
    }
}
