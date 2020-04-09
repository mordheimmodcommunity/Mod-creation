using System;
using UnityEngine;

namespace Pathfinding
{
	public class FloodPathTracer : ABPath
	{
		protected FloodPath flood;

		protected override bool hasEndPoint => false;

		public static FloodPathTracer Construct(Vector3 start, FloodPath flood, OnPathDelegate callback = null)
		{
			FloodPathTracer path = PathPool.GetPath<FloodPathTracer>();
			path.Setup(start, flood, callback);
			return path;
		}

		protected void Setup(Vector3 start, FloodPath flood, OnPathDelegate callback)
		{
			this.flood = flood;
			if (flood == null || flood.GetState() < PathState.Returned)
			{
				throw new ArgumentException("You must supply a calculated FloodPath to the 'flood' argument");
			}
			Setup(start, flood.originalStartPoint, callback);
			nnConstraint = new FloodPathConstraint(flood);
		}

		public override void Reset()
		{
			base.Reset();
			flood = null;
		}

		public override void Initialize()
		{
			if (startNode != null && flood.HasPathTo(startNode))
			{
				Trace(startNode);
				base.CompleteState = PathCompleteState.Complete;
			}
			else
			{
				Error();
			}
		}

		public override void CalculateStep(long targetTick)
		{
			if (!IsDone())
			{
				Error();
			}
		}

		public void Trace(GraphNode from)
		{
			GraphNode graphNode = from;
			int num = 0;
			do
			{
				if (graphNode != null)
				{
					path.Add(graphNode);
					vectorPath.Add((Vector3)graphNode.position);
					graphNode = flood.GetParent(graphNode);
					num++;
					continue;
				}
				return;
			}
			while (num <= 1024);
			Debug.LogWarning("Inifinity loop? >1024 node path. Remove this message if you really have that long paths (FloodPathTracer.cs, Trace function)");
		}
	}
}
