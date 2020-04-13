using Pathfinding.Serialization;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class LevelGridNode : GridNodeBase
    {
        public const int NoConnection = 255;

        public const int ConnectionMask = 255;

        private const int ConnectionStride = 8;

        public const int MaxLayerCount = 255;

        private static LayerGridGraph[] _gridGraphs = new LayerGridGraph[0];

        protected uint gridConnections;

        protected static LayerGridGraph[] gridGraphs;

        public LevelGridNode(AstarPath astar)
            : base(astar)
        {
        }

        public static LayerGridGraph GetGridGraph(uint graphIndex)
        {
            return _gridGraphs[graphIndex];
        }

        public static void SetGridGraph(int graphIndex, LayerGridGraph graph)
        {
            if (_gridGraphs.Length <= graphIndex)
            {
                LayerGridGraph[] array = new LayerGridGraph[graphIndex + 1];
                for (int i = 0; i < _gridGraphs.Length; i++)
                {
                    array[i] = _gridGraphs[i];
                }
                _gridGraphs = array;
            }
            _gridGraphs[graphIndex] = graph;
        }

        public void ResetAllGridConnections()
        {
            gridConnections = uint.MaxValue;
        }

        public bool HasAnyGridConnections()
        {
            return gridConnections != uint.MaxValue;
        }

        public void SetPosition(Int3 position)
        {
            base.position = position;
        }

        public override void ClearConnections(bool alsoReverse)
        {
            if (alsoReverse)
            {
                LayerGridGraph gridGraph = GetGridGraph(base.GraphIndex);
                int[] neighbourOffsets = gridGraph.neighbourOffsets;
                LevelGridNode[] nodes = gridGraph.nodes;
                for (int i = 0; i < 4; i++)
                {
                    int connectionValue = GetConnectionValue(i);
                    if (connectionValue != 255)
                    {
                        nodes[base.NodeInGridIndex + neighbourOffsets[i] + gridGraph.lastScannedWidth * gridGraph.lastScannedDepth * connectionValue]?.SetConnectionValue((i + 2) % 4, 255);
                    }
                }
            }
            ResetAllGridConnections();
            base.ClearConnections(alsoReverse);
        }

        public override void GetConnections(GraphNodeDelegate del)
        {
            int nodeInGridIndex = base.NodeInGridIndex;
            LayerGridGraph gridGraph = GetGridGraph(base.GraphIndex);
            int[] neighbourOffsets = gridGraph.neighbourOffsets;
            LevelGridNode[] nodes = gridGraph.nodes;
            for (int i = 0; i < 4; i++)
            {
                int connectionValue = GetConnectionValue(i);
                if (connectionValue != 255)
                {
                    LevelGridNode levelGridNode = nodes[nodeInGridIndex + neighbourOffsets[i] + gridGraph.lastScannedWidth * gridGraph.lastScannedDepth * connectionValue];
                    if (levelGridNode != null)
                    {
                        del(levelGridNode);
                    }
                }
            }
            base.GetConnections(del);
        }

        public override void FloodFill(Stack<GraphNode> stack, uint region)
        {
            int nodeInGridIndex = base.NodeInGridIndex;
            LayerGridGraph gridGraph = GetGridGraph(base.GraphIndex);
            int[] neighbourOffsets = gridGraph.neighbourOffsets;
            LevelGridNode[] nodes = gridGraph.nodes;
            for (int i = 0; i < 4; i++)
            {
                int connectionValue = GetConnectionValue(i);
                if (connectionValue != 255)
                {
                    LevelGridNode levelGridNode = nodes[nodeInGridIndex + neighbourOffsets[i] + gridGraph.lastScannedWidth * gridGraph.lastScannedDepth * connectionValue];
                    if (levelGridNode != null && levelGridNode.Area != region)
                    {
                        levelGridNode.Area = region;
                        stack.Push(levelGridNode);
                    }
                }
            }
            base.FloodFill(stack, region);
        }

        public bool GetConnection(int i)
        {
            return ((gridConnections >> i * 8) & 0xFF) != 255;
        }

        public void SetConnectionValue(int dir, int value)
        {
            gridConnections = (uint)(((int)gridConnections & ~(255 << dir * 8)) | (value << dir * 8));
        }

        public int GetConnectionValue(int dir)
        {
            return (int)((gridConnections >> dir * 8) & 0xFF);
        }

        public override bool GetPortal(GraphNode other, List<Vector3> left, List<Vector3> right, bool backwards)
        {
            if (backwards)
            {
                return true;
            }
            LayerGridGraph gridGraph = GetGridGraph(base.GraphIndex);
            int[] neighbourOffsets = gridGraph.neighbourOffsets;
            LevelGridNode[] nodes = gridGraph.nodes;
            int nodeInGridIndex = base.NodeInGridIndex;
            for (int i = 0; i < 4; i++)
            {
                int connectionValue = GetConnectionValue(i);
                if (connectionValue != 255 && other == nodes[nodeInGridIndex + neighbourOffsets[i] + gridGraph.lastScannedWidth * gridGraph.lastScannedDepth * connectionValue])
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
            return false;
        }

        public override void UpdateRecursiveG(Path path, PathNode pathNode, PathHandler handler)
        {
            handler.heap.Add(pathNode);
            UpdateG(path, pathNode);
            LayerGridGraph gridGraph = GetGridGraph(base.GraphIndex);
            int[] neighbourOffsets = gridGraph.neighbourOffsets;
            LevelGridNode[] nodes = gridGraph.nodes;
            int nodeInGridIndex = base.NodeInGridIndex;
            for (int i = 0; i < 4; i++)
            {
                int connectionValue = GetConnectionValue(i);
                if (connectionValue != 255)
                {
                    LevelGridNode levelGridNode = nodes[nodeInGridIndex + neighbourOffsets[i] + gridGraph.lastScannedWidth * gridGraph.lastScannedDepth * connectionValue];
                    PathNode pathNode2 = handler.GetPathNode(levelGridNode);
                    if (pathNode2 != null && pathNode2.parent == pathNode && pathNode2.pathID == handler.PathID)
                    {
                        levelGridNode.UpdateRecursiveG(path, pathNode2, handler);
                    }
                }
            }
            base.UpdateRecursiveG(path, pathNode, handler);
        }

        public override void Open(Path path, PathNode pathNode, PathHandler handler)
        {
            LayerGridGraph gridGraph = GetGridGraph(base.GraphIndex);
            int[] neighbourOffsets = gridGraph.neighbourOffsets;
            uint[] neighbourCosts = gridGraph.neighbourCosts;
            LevelGridNode[] nodes = gridGraph.nodes;
            int nodeInGridIndex = base.NodeInGridIndex;
            for (int i = 0; i < 4; i++)
            {
                int connectionValue = GetConnectionValue(i);
                if (connectionValue == 255)
                {
                    continue;
                }
                GraphNode graphNode = nodes[nodeInGridIndex + neighbourOffsets[i] + gridGraph.lastScannedWidth * gridGraph.lastScannedDepth * connectionValue];
                if (!path.CanTraverse(graphNode))
                {
                    continue;
                }
                PathNode pathNode2 = handler.GetPathNode(graphNode);
                if (pathNode2.pathID != handler.PathID)
                {
                    pathNode2.parent = pathNode;
                    pathNode2.pathID = handler.PathID;
                    pathNode2.cost = neighbourCosts[i];
                    pathNode2.H = path.CalculateHScore(graphNode);
                    graphNode.UpdateG(path, pathNode2);
                    handler.heap.Add(pathNode2);
                    continue;
                }
                uint num = neighbourCosts[i];
                if (pathNode.G + num + path.GetTraversalCost(graphNode) < pathNode2.G)
                {
                    pathNode2.cost = num;
                    pathNode2.parent = pathNode;
                    graphNode.UpdateRecursiveG(path, pathNode2, handler);
                }
                else if (pathNode2.G + num + path.GetTraversalCost(this) < pathNode.G)
                {
                    pathNode.parent = pathNode2;
                    pathNode.cost = num;
                    UpdateRecursiveG(path, pathNode, handler);
                }
            }
            base.Open(path, pathNode, handler);
        }

        public override void SerializeNode(GraphSerializationContext ctx)
        {
            base.SerializeNode(ctx);
            ctx.SerializeInt3(position);
            ctx.writer.Write(gridFlags);
            ctx.writer.Write(gridConnections);
        }

        public override void DeserializeNode(GraphSerializationContext ctx)
        {
            base.DeserializeNode(ctx);
            position = ctx.DeserializeInt3();
            gridFlags = ctx.reader.ReadUInt16();
            gridConnections = ctx.reader.ReadUInt32();
        }
    }
}
