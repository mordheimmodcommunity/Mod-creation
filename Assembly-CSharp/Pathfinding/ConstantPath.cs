using Pathfinding.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class ConstantPath : Path
    {
        public GraphNode startNode;

        public Vector3 startPoint;

        public Vector3 originalStartPoint;

        public List<GraphNode> allNodes;

        public PathEndingCondition endingCondition;

        public override bool FloodingPath => true;

        public static ConstantPath Construct(Vector3 start, int maxGScore, OnPathDelegate callback = null)
        {
            ConstantPath path = PathPool.GetPath<ConstantPath>();
            path.Setup(start, maxGScore, callback);
            return path;
        }

        protected void Setup(Vector3 start, int maxGScore, OnPathDelegate callback)
        {
            base.callback = callback;
            startPoint = start;
            originalStartPoint = startPoint;
            endingCondition = new EndingConditionDistance(this, maxGScore);
        }

        public override void OnEnterPool()
        {
            base.OnEnterPool();
            if (allNodes != null)
            {
                ListPool<GraphNode>.Release(allNodes);
            }
        }

        public override void Reset()
        {
            base.Reset();
            allNodes = ListPool<GraphNode>.Claim();
            endingCondition = null;
            originalStartPoint = Vector3.zero;
            startPoint = Vector3.zero;
            startNode = null;
            heuristic = Heuristic.None;
        }

        public override void Prepare()
        {
            nnConstraint.tags = enabledTags;
            NNInfo nearest = AstarPath.active.GetNearest(startPoint, nnConstraint);
            startNode = nearest.node;
            if (startNode == null)
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
            startNode.Open(this, pathNode, base.pathHandler);
            searchedNodes++;
            pathNode.flag1 = true;
            allNodes.Add(startNode);
            if (base.pathHandler.heap.isEmpty)
            {
                base.CompleteState = PathCompleteState.Complete;
            }
            else
            {
                currentR = base.pathHandler.heap.Remove();
            }
        }

        public override void Cleanup()
        {
            int count = allNodes.Count;
            for (int i = 0; i < count; i++)
            {
                base.pathHandler.GetPathNode(allNodes[i]).flag1 = false;
            }
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
                if (endingCondition.TargetFound(currentR))
                {
                    base.CompleteState = PathCompleteState.Complete;
                    return;
                }
                if (!currentR.flag1)
                {
                    allNodes.Add(currentR.node);
                    currentR.flag1 = true;
                }
                currentR.node.Open(this, currentR, base.pathHandler);
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
