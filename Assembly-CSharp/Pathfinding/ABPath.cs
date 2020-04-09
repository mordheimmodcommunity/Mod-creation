using System;
using System.Text;
using UnityEngine;

namespace Pathfinding
{
	public class ABPath : Path
	{
		public bool recalcStartEndCosts = true;

		public GraphNode startNode;

		public GraphNode endNode;

		public GraphNode startHint;

		public GraphNode endHint;

		public Vector3 originalStartPoint;

		public Vector3 originalEndPoint;

		public Vector3 startPoint;

		public Vector3 endPoint;

		public Int3 startIntPoint;

		public bool calculatePartial;

		protected PathNode partialBestTarget;

		protected int[] endNodeCosts;

		private GridNode gridSpecialCaseNode;

		protected virtual bool hasEndPoint => true;

		public static ABPath Construct(Vector3 start, Vector3 end, OnPathDelegate callback = null)
		{
			ABPath path = PathPool.GetPath<ABPath>();
			path.Setup(start, end, callback);
			return path;
		}

		protected void Setup(Vector3 start, Vector3 end, OnPathDelegate callbackDelegate)
		{
			callback = callbackDelegate;
			UpdateStartEnd(start, end);
		}

		protected void UpdateStartEnd(Vector3 start, Vector3 end)
		{
			originalStartPoint = start;
			originalEndPoint = end;
			startPoint = start;
			endPoint = end;
			startIntPoint = (Int3)start;
			hTarget = (Int3)end;
		}

		public override uint GetConnectionSpecialCost(GraphNode a, GraphNode b, uint currentCost)
		{
			if (startNode != null && endNode != null)
			{
				if (a == startNode)
				{
					return (uint)((double)(startIntPoint - ((b != endNode) ? b.position : hTarget)).costMagnitude * ((double)currentCost * 1.0 / (double)(a.position - b.position).costMagnitude));
				}
				if (b == startNode)
				{
					return (uint)((double)(startIntPoint - ((a != endNode) ? a.position : hTarget)).costMagnitude * ((double)currentCost * 1.0 / (double)(a.position - b.position).costMagnitude));
				}
				if (a == endNode)
				{
					return (uint)((double)(hTarget - b.position).costMagnitude * ((double)currentCost * 1.0 / (double)(a.position - b.position).costMagnitude));
				}
				if (b == endNode)
				{
					return (uint)((double)(hTarget - a.position).costMagnitude * ((double)currentCost * 1.0 / (double)(a.position - b.position).costMagnitude));
				}
			}
			else
			{
				if (a == startNode)
				{
					return (uint)((double)(startIntPoint - b.position).costMagnitude * ((double)currentCost * 1.0 / (double)(a.position - b.position).costMagnitude));
				}
				if (b == startNode)
				{
					return (uint)((double)(startIntPoint - a.position).costMagnitude * ((double)currentCost * 1.0 / (double)(a.position - b.position).costMagnitude));
				}
			}
			return currentCost;
		}

		public override void Reset()
		{
			base.Reset();
			startNode = null;
			endNode = null;
			startHint = null;
			endHint = null;
			originalStartPoint = Vector3.zero;
			originalEndPoint = Vector3.zero;
			startPoint = Vector3.zero;
			endPoint = Vector3.zero;
			calculatePartial = false;
			partialBestTarget = null;
			startIntPoint = default(Int3);
			hTarget = default(Int3);
			endNodeCosts = null;
			gridSpecialCaseNode = null;
		}

		protected virtual bool EndPointGridGraphSpecialCase(GraphNode closestWalkableEndNode)
		{
			GridNode gridNode = closestWalkableEndNode as GridNode;
			if (gridNode != null)
			{
				GridGraph gridGraph = GridNode.GetGridGraph(gridNode.GraphIndex);
				NNInfo nearest = AstarPath.active.GetNearest(originalEndPoint, NNConstraint.None, endHint);
				GridNode gridNode2 = nearest.node as GridNode;
				if (gridNode != gridNode2 && gridNode2 != null && gridNode.GraphIndex == gridNode2.GraphIndex)
				{
					int num = gridNode.NodeInGridIndex % gridGraph.width;
					int num2 = gridNode.NodeInGridIndex / gridGraph.width;
					int num3 = gridNode2.NodeInGridIndex % gridGraph.width;
					int num4 = gridNode2.NodeInGridIndex / gridGraph.width;
					bool flag = false;
					switch (gridGraph.neighbours)
					{
					case NumNeighbours.Four:
						if ((num == num3 && Math.Abs(num2 - num4) == 1) || (num2 == num4 && Math.Abs(num - num3) == 1))
						{
							flag = true;
						}
						break;
					case NumNeighbours.Eight:
						if (Math.Abs(num - num3) <= 1 && Math.Abs(num2 - num4) <= 1)
						{
							flag = true;
						}
						break;
					case NumNeighbours.Six:
					{
						for (int i = 0; i < 6; i++)
						{
							int num5 = num3 + gridGraph.neighbourXOffsets[GridGraph.hexagonNeighbourIndices[i]];
							int num6 = num4 + gridGraph.neighbourZOffsets[GridGraph.hexagonNeighbourIndices[i]];
							if (num == num5 && num2 == num6)
							{
								flag = true;
								break;
							}
						}
						break;
					}
					default:
						throw new Exception("Unhandled NumNeighbours");
					}
					if (flag)
					{
						SetFlagOnSurroundingGridNodes(gridNode2, 1, flagState: true);
						endPoint = (Vector3)gridNode2.position;
						hTarget = gridNode2.position;
						endNode = gridNode2;
						hTargetNode = endNode;
						gridSpecialCaseNode = gridNode2;
						return true;
					}
				}
			}
			return false;
		}

		private void SetFlagOnSurroundingGridNodes(GridNode gridNode, int flag, bool flagState)
		{
			GridGraph gridGraph = GridNode.GetGridGraph(gridNode.GraphIndex);
			int num = (gridGraph.neighbours == NumNeighbours.Four) ? 4 : ((gridGraph.neighbours != NumNeighbours.Eight) ? 6 : 8);
			int num2 = gridNode.NodeInGridIndex % gridGraph.width;
			int num3 = gridNode.NodeInGridIndex / gridGraph.width;
			if (flag != 1 && flag != 2)
			{
				throw new ArgumentOutOfRangeException("flag");
			}
			for (int i = 0; i < num; i++)
			{
				int num4;
				int num5;
				if (gridGraph.neighbours == NumNeighbours.Six)
				{
					num4 = num2 + gridGraph.neighbourXOffsets[GridGraph.hexagonNeighbourIndices[i]];
					num5 = num3 + gridGraph.neighbourZOffsets[GridGraph.hexagonNeighbourIndices[i]];
				}
				else
				{
					num4 = num2 + gridGraph.neighbourXOffsets[i];
					num5 = num3 + gridGraph.neighbourZOffsets[i];
				}
				if (num4 >= 0 && num5 >= 0 && num4 < gridGraph.width && num5 < gridGraph.depth)
				{
					GridNode node = gridGraph.nodes[num5 * gridGraph.width + num4];
					PathNode pathNode = base.pathHandler.GetPathNode(node);
					if (flag == 1)
					{
						pathNode.flag1 = flagState;
					}
					else
					{
						pathNode.flag2 = flagState;
					}
				}
			}
		}

		public override void Prepare()
		{
			nnConstraint.tags = enabledTags;
			NNInfo nearest = AstarPath.active.GetNearest(startPoint, nnConstraint, startHint);
			(nnConstraint as PathNNConstraint)?.SetStart(nearest.node);
			startPoint = nearest.position;
			startIntPoint = (Int3)startPoint;
			startNode = nearest.node;
			if (startNode == null)
			{
				Error();
			}
			else if (!startNode.Walkable)
			{
				Error();
			}
			else if (hasEndPoint)
			{
				NNInfo nearest2 = AstarPath.active.GetNearest(endPoint, nnConstraint, endHint);
				endPoint = nearest2.position;
				endNode = nearest2.node;
				if (startNode == null && endNode == null)
				{
					Error();
				}
				else if (endNode == null)
				{
					Error();
				}
				else if (!endNode.Walkable)
				{
					Error();
				}
				else if (startNode.Area != endNode.Area)
				{
					Error();
				}
				else if (!EndPointGridGraphSpecialCase(nearest2.node))
				{
					hTarget = (Int3)endPoint;
					hTargetNode = endNode;
					base.pathHandler.GetPathNode(endNode).flag1 = true;
				}
			}
		}

		protected virtual void CompletePathIfStartIsValidTarget()
		{
			if (hasEndPoint && base.pathHandler.GetPathNode(startNode).flag1)
			{
				CompleteWith(startNode);
				Trace(base.pathHandler.GetPathNode(startNode));
			}
		}

		public override void Initialize()
		{
			if (startNode != null)
			{
				base.pathHandler.GetPathNode(startNode).flag2 = true;
			}
			if (endNode != null)
			{
				base.pathHandler.GetPathNode(endNode).flag2 = true;
			}
			PathNode pathNode = base.pathHandler.GetPathNode(startNode);
			pathNode.node = startNode;
			pathNode.pathID = base.pathHandler.PathID;
			pathNode.parent = null;
			pathNode.cost = 0u;
			pathNode.G = GetTraversalCost(startNode);
			pathNode.H = CalculateHScore(startNode);
			CompletePathIfStartIsValidTarget();
			if (base.CompleteState == PathCompleteState.Complete)
			{
				return;
			}
			startNode.Open(this, pathNode, base.pathHandler);
			searchedNodes++;
			partialBestTarget = pathNode;
			if (base.pathHandler.heap.isEmpty)
			{
				if (!calculatePartial)
				{
					Error();
					return;
				}
				base.CompleteState = PathCompleteState.Partial;
				Trace(partialBestTarget);
			}
			currentR = base.pathHandler.heap.Remove();
		}

		public override void Cleanup()
		{
			if (startNode != null)
			{
				PathNode pathNode = base.pathHandler.GetPathNode(startNode);
				pathNode.flag1 = false;
				pathNode.flag2 = false;
			}
			if (endNode != null)
			{
				PathNode pathNode2 = base.pathHandler.GetPathNode(endNode);
				pathNode2.flag1 = false;
				pathNode2.flag2 = false;
			}
			if (gridSpecialCaseNode != null)
			{
				PathNode pathNode3 = base.pathHandler.GetPathNode(gridSpecialCaseNode);
				pathNode3.flag1 = false;
				pathNode3.flag2 = false;
				SetFlagOnSurroundingGridNodes(gridSpecialCaseNode, 1, flagState: false);
				SetFlagOnSurroundingGridNodes(gridSpecialCaseNode, 2, flagState: false);
			}
		}

		private void CompleteWith(GraphNode node)
		{
			if (endNode != node)
			{
				GridNode gridNode = node as GridNode;
				if (gridNode == null)
				{
					throw new Exception("Some path is not cleaning up the flag1 field. This is a bug.");
				}
				endPoint = gridNode.ClosestPointOnNode(originalEndPoint);
				endNode = node;
			}
			base.CompleteState = PathCompleteState.Complete;
		}

		public override void CalculateStep(long targetTick)
		{
			int num = 0;
			while (base.CompleteState == PathCompleteState.NotCalculated)
			{
				searchedNodes++;
				if (currentR.flag1)
				{
					CompleteWith(currentR.node);
					break;
				}
				if (currentR.H < partialBestTarget.H)
				{
					partialBestTarget = currentR;
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
				Trace(currentR);
			}
			else if (calculatePartial && partialBestTarget != null)
			{
				base.CompleteState = PathCompleteState.Partial;
				Trace(partialBestTarget);
			}
		}

		public void ResetCosts(Path p)
		{
		}

		public override string DebugString(PathLog logMode)
		{
			if (logMode == PathLog.None || (!base.error && logMode == PathLog.OnlyErrors))
			{
				return string.Empty;
			}
			StringBuilder stringBuilder = new StringBuilder();
			DebugStringPrefix(logMode, stringBuilder);
			if (!base.error && logMode == PathLog.Heavy)
			{
				stringBuilder.Append("\nSearch Iterations " + searchIterations);
				if (hasEndPoint && endNode != null)
				{
					PathNode pathNode = base.pathHandler.GetPathNode(endNode);
					stringBuilder.Append("\nEnd Node\n\tG: ");
					stringBuilder.Append(pathNode.G);
					stringBuilder.Append("\n\tH: ");
					stringBuilder.Append(pathNode.H);
					stringBuilder.Append("\n\tF: ");
					stringBuilder.Append(pathNode.F);
					stringBuilder.Append("\n\tPoint: ");
					stringBuilder.Append(endPoint.ToString());
					stringBuilder.Append("\n\tGraph: ");
					stringBuilder.Append(endNode.GraphIndex);
				}
				stringBuilder.Append("\nStart Node");
				stringBuilder.Append("\n\tPoint: ");
				stringBuilder.Append(startPoint.ToString());
				stringBuilder.Append("\n\tGraph: ");
				if (startNode != null)
				{
					stringBuilder.Append(startNode.GraphIndex);
				}
				else
				{
					stringBuilder.Append("< null startNode >");
				}
			}
			DebugStringSuffix(logMode, stringBuilder);
			return stringBuilder.ToString();
		}

		public Vector3 GetMovementVector(Vector3 point)
		{
			if (vectorPath == null || vectorPath.Count == 0)
			{
				return Vector3.zero;
			}
			if (vectorPath.Count == 1)
			{
				return vectorPath[0] - point;
			}
			float num = float.PositiveInfinity;
			int num2 = 0;
			for (int i = 0; i < vectorPath.Count - 1; i++)
			{
				Vector3 a = VectorMath.ClosestPointOnSegment(vectorPath[i], vectorPath[i + 1], point);
				float sqrMagnitude = (a - point).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					num2 = i;
				}
			}
			return vectorPath[num2 + 1] - point;
		}
	}
}
