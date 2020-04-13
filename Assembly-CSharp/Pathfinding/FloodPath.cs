using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class FloodPath : Path
    {
        public Vector3 originalStartPoint;

        public Vector3 startPoint;

        public GraphNode startNode;

        public bool saveParents = true;

        protected Dictionary<GraphNode, GraphNode> parents;

        public override bool FloodingPath => true;

        public bool HasPathTo(GraphNode node)
        {
            return parents != null && parents.ContainsKey(node);
        }

        public GraphNode GetParent(GraphNode node)
        {
            return parents[node];
        }

        public static FloodPath Construct(Vector3 start, OnPathDelegate callback = null)
        {
            FloodPath path = PathPool.GetPath<FloodPath>();
            path.Setup(start, callback);
            return path;
        }

        public static FloodPath Construct(GraphNode start, OnPathDelegate callback = null)
        {
            if (start == null)
            {
                throw new ArgumentNullException("start");
            }
            FloodPath path = PathPool.GetPath<FloodPath>();
            path.Setup(start, callback);
            return path;
        }

        protected void Setup(Vector3 start, OnPathDelegate callback)
        {
            base.callback = callback;
            originalStartPoint = start;
            startPoint = start;
            heuristic = Heuristic.None;
        }

        protected void Setup(GraphNode start, OnPathDelegate callback)
        {
            base.callback = callback;
            originalStartPoint = (Vector3)start.position;
            startNode = start;
            startPoint = (Vector3)start.position;
            heuristic = Heuristic.None;
        }

        public override void Reset()
        {
            base.Reset();
            originalStartPoint = Vector3.zero;
            startPoint = Vector3.zero;
            startNode = null;
            parents = new Dictionary<GraphNode, GraphNode>();
            saveParents = true;
        }

        public override void Prepare()
        {
            if (startNode == null)
            {
                nnConstraint.tags = enabledTags;
                NNInfo nearest = AstarPath.active.GetNearest(originalStartPoint, nnConstraint);
                startPoint = nearest.position;
                startNode = nearest.node;
            }
            else
            {
                startPoint = (Vector3)startNode.position;
            }
            if (startNode == null)
            {
                Error();
            }
            else if (!startNode.Walkable)
            {
                Error();
            }
        }

        public override void Initialize()
        {
            PathNode pathNode = base.pathHandler.GetPathNode(startNode);
            pathNode.node = startNode;
            pathNode.pathID = base.pathHandler.PathID;
            pathNode.parent = null;
            pathNode.cost = 0u;
            pathNode.G = GetTraversalCost(startNode);
            pathNode.H = CalculateHScore(startNode);
            parents[startNode] = null;
            startNode.Open(this, pathNode, base.pathHandler);
            searchedNodes++;
            if (base.pathHandler.heap.isEmpty)
            {
                base.CompleteState = PathCompleteState.Complete;
            }
            currentR = base.pathHandler.heap.Remove();
        }

        public override void CalculateStep(long targetTick)
        {
            int num = 0;
            while (true)
            {
                if (base.CompleteState != 0)
                {
                    return;
                }
                searchedNodes++;
                currentR.node.Open(this, currentR, base.pathHandler);
                if (saveParents)
                {
                    parents[currentR.node] = currentR.parent.node;
                }
                if (base.pathHandler.heap.isEmpty)
                {
                    base.CompleteState = PathCompleteState.Complete;
                    return;
                }
                currentR = base.pathHandler.heap.Remove();
                if (num > 500)
                {
                    if (DateTime.UtcNow.Ticks >= targetTick)
                    {
                        return;
                    }
                    num = 0;
                    if (searchedNodes > 1000000)
                    {
                        break;
                    }
                }
                num++;
            }
            throw new Exception("Probable infinite loop. Over 1,000,000 nodes searched");
        }
    }
}
