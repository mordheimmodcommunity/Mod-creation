using Pathfinding.Serialization;
using Pathfinding.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
	[JsonOptIn]
	public class PointGraph : NavGraph, IUpdatableGraph
	{
		[JsonMember]
		public Transform root;

		[JsonMember]
		public string searchTag;

		[JsonMember]
		public float maxDistance;

		[JsonMember]
		public Vector3 limits;

		[JsonMember]
		public bool raycast = true;

		[JsonMember]
		public bool use2DPhysics;

		[JsonMember]
		public bool thickRaycast;

		[JsonMember]
		public float thickRaycastRadius = 1f;

		[JsonMember]
		public bool recursive = true;

		[JsonMember]
		public LayerMask mask;

		[JsonMember]
		public bool optimizeForSparseGraph;

		private PointKDTree lookupTree = new PointKDTree();

		public PointNode[] nodes;

		public int nodeCount
		{
			get;
			private set;
		}

		public override int CountNodes()
		{
			return nodeCount;
		}

		public override void GetNodes(GraphNodeDelegateCancelable del)
		{
			if (nodes != null)
			{
				int nodeCount = this.nodeCount;
				for (int i = 0; i < nodeCount && del(nodes[i]); i++)
				{
				}
			}
		}

		public override NNInfoInternal GetNearest(Vector3 position, NNConstraint constraint, GraphNode hint)
		{
			return GetNearestForce(position, null);
		}

		public override NNInfoInternal GetNearestForce(Vector3 position, NNConstraint constraint)
		{
			if (nodes == null)
			{
				return default(NNInfoInternal);
			}
			if (optimizeForSparseGraph)
			{
				return new NNInfoInternal(lookupTree.GetNearest((Int3)position, constraint));
			}
			float num = (constraint != null && !constraint.constrainDistance) ? float.PositiveInfinity : AstarPath.active.maxNearestNodeDistanceSqr;
			NNInfoInternal result = new NNInfoInternal(null);
			float num2 = float.PositiveInfinity;
			float num3 = float.PositiveInfinity;
			for (int i = 0; i < nodeCount; i++)
			{
				PointNode pointNode = nodes[i];
				float sqrMagnitude = (position - (Vector3)pointNode.position).sqrMagnitude;
				if (sqrMagnitude < num2)
				{
					num2 = sqrMagnitude;
					result.node = pointNode;
				}
				if (sqrMagnitude < num3 && sqrMagnitude < num && (constraint == null || constraint.Suitable(pointNode)))
				{
					num3 = sqrMagnitude;
					result.constrainedNode = pointNode;
				}
			}
			result.UpdateInfo();
			return result;
		}

		public PointNode AddNode(Int3 position)
		{
			return AddNode(new PointNode(active), position);
		}

		public T AddNode<T>(T node, Int3 position) where T : PointNode
		{
			if (nodes == null || nodeCount == nodes.Length)
			{
				PointNode[] array = new PointNode[(nodes == null) ? 4 : Math.Max(nodes.Length + 4, nodes.Length * 2)];
				for (int i = 0; i < nodeCount; i++)
				{
					array[i] = nodes[i];
				}
				nodes = array;
			}
			node.SetPosition(position);
			node.GraphIndex = graphIndex;
			node.Walkable = true;
			nodes[nodeCount] = node;
			nodeCount++;
			AddToLookup(node);
			return node;
		}

		protected static int CountChildren(Transform tr)
		{
			int num = 0;
			foreach (Transform item in tr)
			{
				num++;
				num += CountChildren(item);
			}
			return num;
		}

		protected void AddChildren(ref int c, Transform tr)
		{
			foreach (Transform item in tr)
			{
				nodes[c].SetPosition((Int3)item.position);
				nodes[c].Walkable = true;
				nodes[c].gameObject = item.gameObject;
				c++;
				AddChildren(ref c, item);
			}
		}

		public void RebuildNodeLookup()
		{
			if (!optimizeForSparseGraph || nodes == null)
			{
				lookupTree = new PointKDTree();
			}
			else
			{
				lookupTree.Rebuild(nodes, 0, nodeCount);
			}
		}

		private void AddToLookup(PointNode node)
		{
			lookupTree.Add(node);
		}

		public override IEnumerable<Progress> ScanInternal()
		{
			yield return new Progress(0f, "Searching for GameObjects");
			if (root == null)
			{
				GameObject[] gos = (searchTag == null) ? null : GameObject.FindGameObjectsWithTag(searchTag);
				if (gos == null)
				{
					nodes = new PointNode[0];
					nodeCount = 0;
					yield break;
				}
				yield return new Progress(0.1f, "Creating nodes");
				nodes = new PointNode[gos.Length];
				nodeCount = nodes.Length;
				for (int m = 0; m < nodes.Length; m++)
				{
					nodes[m] = new PointNode(active);
				}
				for (int i2 = 0; i2 < gos.Length; i2++)
				{
					nodes[i2].SetPosition((Int3)gos[i2].transform.position);
					nodes[i2].Walkable = true;
					nodes[i2].gameObject = gos[i2].gameObject;
				}
			}
			else if (!recursive)
			{
				nodes = new PointNode[root.childCount];
				nodeCount = nodes.Length;
				for (int n = 0; n < nodes.Length; n++)
				{
					nodes[n] = new PointNode(active);
				}
				int c = 0;
				foreach (Transform child in root)
				{
					nodes[c].SetPosition((Int3)child.position);
					nodes[c].Walkable = true;
					nodes[c].gameObject = child.gameObject;
					c++;
				}
			}
			else
			{
				nodes = new PointNode[CountChildren(root)];
				nodeCount = nodes.Length;
				for (int l = 0; l < nodes.Length; l++)
				{
					nodes[l] = new PointNode(active);
				}
				int startID = 0;
				AddChildren(ref startID, root);
			}
			if (optimizeForSparseGraph)
			{
				yield return new Progress(0.15f, "Building node lookup");
				RebuildNodeLookup();
			}
			if (!(maxDistance >= 0f))
			{
				yield break;
			}
			List<PointNode> connections = new List<PointNode>();
			List<uint> costs = new List<uint>();
			List<GraphNode> candidateConnections = new List<GraphNode>();
			long maxPossibleSqrRange2;
			if (maxDistance == 0f && (limits.x == 0f || limits.y == 0f || limits.z == 0f))
			{
				maxPossibleSqrRange2 = long.MaxValue;
			}
			else
			{
				maxPossibleSqrRange2 = (long)(Mathf.Max(limits.x, Mathf.Max(limits.y, Mathf.Max(limits.z, maxDistance))) * 1000f) + 1;
				maxPossibleSqrRange2 *= maxPossibleSqrRange2;
			}
			for (int k = 0; k < nodes.Length; k++)
			{
				if (k % 512 == 0)
				{
					yield return new Progress(Mathf.Lerp(0.15f, 1f, (float)k / (float)nodes.Length), "Connecting nodes");
				}
				connections.Clear();
				costs.Clear();
				PointNode node = nodes[k];
				if (optimizeForSparseGraph)
				{
					candidateConnections.Clear();
					lookupTree.GetInRange(node.position, maxPossibleSqrRange2, candidateConnections);
					Console.WriteLine(k + " " + candidateConnections.Count);
					for (int j = 0; j < candidateConnections.Count; j++)
					{
						PointNode other2 = candidateConnections[j] as PointNode;
						if (other2 != node && IsValidConnection(node, other2, out float dist2))
						{
							connections.Add(other2);
							costs.Add((uint)Mathf.RoundToInt(dist2 * 1000f));
						}
					}
				}
				else
				{
					for (int i = 0; i < nodes.Length; i++)
					{
						if (k != i)
						{
							PointNode other = nodes[i];
							if (IsValidConnection(node, other, out float dist))
							{
								connections.Add(other);
								costs.Add((uint)Mathf.RoundToInt(dist * 1000f));
							}
						}
					}
				}
				node.connections = connections.ToArray();
				node.connectionCosts = costs.ToArray();
			}
		}

		public virtual bool IsValidConnection(GraphNode a, GraphNode b, out float dist)
		{
			dist = 0f;
			if (!a.Walkable || !b.Walkable)
			{
				return false;
			}
			Vector3 vector = (Vector3)(b.position - a.position);
			if ((!Mathf.Approximately(limits.x, 0f) && Mathf.Abs(vector.x) > limits.x) || (!Mathf.Approximately(limits.y, 0f) && Mathf.Abs(vector.y) > limits.y) || (!Mathf.Approximately(limits.z, 0f) && Mathf.Abs(vector.z) > limits.z))
			{
				return false;
			}
			dist = vector.magnitude;
			if (maxDistance == 0f || dist < maxDistance)
			{
				if (raycast)
				{
					Ray ray = new Ray((Vector3)a.position, vector);
					Ray ray2 = new Ray((Vector3)b.position, -vector);
					if (use2DPhysics)
					{
						if (thickRaycast)
						{
							return !Physics2D.CircleCast(ray.origin, thickRaycastRadius, ray.direction, dist, mask) && !Physics2D.CircleCast(ray2.origin, thickRaycastRadius, ray2.direction, dist, mask);
						}
						return !Physics2D.Linecast((Vector3)a.position, (Vector3)b.position, mask) && !Physics2D.Linecast((Vector3)b.position, (Vector3)a.position, mask);
					}
					if (thickRaycast)
					{
						return !Physics.SphereCast(ray, thickRaycastRadius, dist, mask) && !Physics.SphereCast(ray2, thickRaycastRadius, dist, mask);
					}
					return !Physics.Linecast((Vector3)a.position, (Vector3)b.position, mask) && !Physics.Linecast((Vector3)b.position, (Vector3)a.position, mask);
				}
				return true;
			}
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

		public void UpdateArea(GraphUpdateObject guo)
		{
			if (nodes == null)
			{
				return;
			}
			for (int i = 0; i < nodeCount; i++)
			{
				if (guo.bounds.Contains((Vector3)nodes[i].position))
				{
					guo.WillUpdateNode(nodes[i]);
					guo.Apply(nodes[i]);
				}
			}
			if (!guo.updatePhysics)
			{
				return;
			}
			Bounds bounds = guo.bounds;
			if (thickRaycast)
			{
				bounds.Expand(thickRaycastRadius * 2f);
			}
			List<GraphNode> list = ListPool<GraphNode>.Claim();
			List<uint> list2 = ListPool<uint>.Claim();
			for (int j = 0; j < nodeCount; j++)
			{
				PointNode pointNode = nodes[j];
				Vector3 a = (Vector3)pointNode.position;
				List<GraphNode> list3 = null;
				List<uint> list4 = null;
				for (int k = 0; k < nodeCount; k++)
				{
					if (k == j)
					{
						continue;
					}
					Vector3 b = (Vector3)nodes[k].position;
					if (!VectorMath.SegmentIntersectsBounds(bounds, a, b))
					{
						continue;
					}
					PointNode pointNode2 = nodes[k];
					bool flag = pointNode.ContainsConnection(pointNode2);
					float dist;
					bool flag2 = IsValidConnection(pointNode, pointNode2, out dist);
					if (!flag && flag2)
					{
						if (list3 == null)
						{
							list.Clear();
							list2.Clear();
							list3 = list;
							list4 = list2;
							list3.AddRange(pointNode.connections);
							list4.AddRange(pointNode.connectionCosts);
						}
						uint item = (uint)Mathf.RoundToInt(dist * 1000f);
						list3.Add(pointNode2);
						list4.Add(item);
					}
					else if (flag && !flag2)
					{
						if (list3 == null)
						{
							list.Clear();
							list2.Clear();
							list3 = list;
							list4 = list2;
							list3.AddRange(pointNode.connections);
							list4.AddRange(pointNode.connectionCosts);
						}
						int num = list3.IndexOf(pointNode2);
						if (num != -1)
						{
							list3.RemoveAt(num);
							list4.RemoveAt(num);
						}
					}
				}
				if (list3 != null)
				{
					pointNode.connections = list3.ToArray();
					pointNode.connectionCosts = list4.ToArray();
				}
			}
			ListPool<GraphNode>.Release(list);
			ListPool<uint>.Release(list2);
		}

		public override void PostDeserialization()
		{
			RebuildNodeLookup();
		}

		public override void RelocateNodes(Matrix4x4 oldMatrix, Matrix4x4 newMatrix)
		{
			base.RelocateNodes(oldMatrix, newMatrix);
			RebuildNodeLookup();
		}

		public override void DeserializeSettingsCompatibility(GraphSerializationContext ctx)
		{
			base.DeserializeSettingsCompatibility(ctx);
			root = (ctx.DeserializeUnityObject() as Transform);
			searchTag = ctx.reader.ReadString();
			maxDistance = ctx.reader.ReadSingle();
			limits = ctx.DeserializeVector3();
			raycast = ctx.reader.ReadBoolean();
			use2DPhysics = ctx.reader.ReadBoolean();
			thickRaycast = ctx.reader.ReadBoolean();
			thickRaycastRadius = ctx.reader.ReadSingle();
			recursive = ctx.reader.ReadBoolean();
			ctx.reader.ReadBoolean();
			mask = ctx.reader.ReadInt32();
			optimizeForSparseGraph = ctx.reader.ReadBoolean();
			ctx.reader.ReadBoolean();
		}

		public override void SerializeExtraInfo(GraphSerializationContext ctx)
		{
			if (nodes == null)
			{
				ctx.writer.Write(-1);
			}
			ctx.writer.Write(nodeCount);
			for (int i = 0; i < nodeCount; i++)
			{
				if (nodes[i] == null)
				{
					ctx.writer.Write(-1);
					continue;
				}
				ctx.writer.Write(0);
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
			nodes = new PointNode[num];
			nodeCount = num;
			for (int i = 0; i < nodes.Length; i++)
			{
				if (ctx.reader.ReadInt32() != -1)
				{
					nodes[i] = new PointNode(active);
					nodes[i].DeserializeNode(ctx);
				}
			}
		}
	}
}
