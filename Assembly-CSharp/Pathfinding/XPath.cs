using System;
using UnityEngine;

namespace Pathfinding
{
	public class XPath : ABPath
	{
		public PathEndingCondition endingCondition;

		public new static XPath Construct(Vector3 start, Vector3 end, OnPathDelegate callback = null)
		{
			XPath path = PathPool.GetPath<XPath>();
			path.Setup(start, end, callback);
			path.endingCondition = new ABPathEndingCondition(path);
			return path;
		}

		public override void Reset()
		{
			base.Reset();
			endingCondition = null;
		}

		protected override bool EndPointGridGraphSpecialCase(GraphNode endNode)
		{
			return false;
		}

		protected override void CompletePathIfStartIsValidTarget()
		{
			PathNode pathNode = base.pathHandler.GetPathNode(startNode);
			if (endingCondition.TargetFound(pathNode))
			{
				ChangeEndNode(startNode);
				Trace(pathNode);
				base.CompleteState = PathCompleteState.Complete;
			}
		}

		private void ChangeEndNode(GraphNode target)
		{
			if (endNode != null && endNode != startNode)
			{
				PathNode pathNode = base.pathHandler.GetPathNode(endNode);
				bool flag2 = pathNode.flag2 = false;
				pathNode.flag1 = flag2;
			}
			endNode = target;
			endPoint = (Vector3)target.position;
		}

		public override void CalculateStep(long targetTick)
		{
			int num = 0;
			while (base.CompleteState == PathCompleteState.NotCalculated)
			{
				searchedNodes++;
				if (endingCondition.TargetFound(currentR))
				{
					base.CompleteState = PathCompleteState.Complete;
					break;
				}
				currentR.node.Open(this, currentR, base.pathHandler);
				if (base.pathHandler.heap.isEmpty)
				{
					Error();
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
						throw new Exception("Probable infinite loop. Over 1,000,000 nodes searched");
					}
				}
				num++;
			}
			if (base.CompleteState == PathCompleteState.Complete)
			{
				ChangeEndNode(currentR.node);
				Trace(currentR);
			}
		}
	}
}
