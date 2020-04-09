using Pathfinding.Serialization;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
	public abstract class GraphNode
	{
		private const int FlagsWalkableOffset = 0;

		private const uint FlagsWalkableMask = 1u;

		private const int FlagsAreaOffset = 1;

		private const uint FlagsAreaMask = 262142u;

		private const int FlagsGraphOffset = 24;

		private const uint FlagsGraphMask = 4278190080u;

		public const uint MaxAreaIndex = 131071u;

		public const uint MaxGraphIndex = 255u;

		private const int FlagsTagOffset = 19;

		private const uint FlagsTagMask = 16252928u;

		private int nodeIndex;

		protected uint flags;

		public Int3 position;

		public bool Destroyed => nodeIndex == -1;

		public int NodeIndex => nodeIndex;

		public uint Flags
		{
			get
			{
				return flags;
			}
			set
			{
				flags = value;
			}
		}

		public uint Penalty
		{
			get
			{
				return 0u;
			}
			set
			{
			}
		}

		public bool Walkable
		{
			get
			{
				return (flags & 1) != 0;
			}
			set
			{
				flags = (uint)(((int)flags & -2) | (value ? 1 : 0));
			}
		}

		public uint Area
		{
			get
			{
				return (flags & 0x3FFFE) >> 1;
			}
			set
			{
				flags = (uint)(((int)flags & -262143) | (int)(value << 1));
			}
		}

		public uint GraphIndex
		{
			get
			{
				return (uint)((int)flags & -16777216) >> 24;
			}
			set
			{
				flags = ((flags & 0xFFFFFF) | (value << 24));
			}
		}

		public uint Tag
		{
			get
			{
				return (flags & 0xF80000) >> 19;
			}
			set
			{
				flags = (uint)(((int)flags & -16252929) | (int)(value << 19));
			}
		}

		protected GraphNode(AstarPath astar)
		{
			if (!object.ReferenceEquals(astar, null))
			{
				nodeIndex = astar.GetNewNodeIndex();
				astar.InitializeNode(this);
				return;
			}
			throw new Exception("No active AstarPath object to bind to");
		}

		internal void Destroy()
		{
			if (!Destroyed)
			{
				ClearConnections(alsoReverse: true);
				if (AstarPath.active != null)
				{
					AstarPath.active.DestroyNode(this);
				}
				nodeIndex = -1;
			}
		}

		public void UpdateG(Path path, PathNode pathNode)
		{
			pathNode.G = pathNode.parent.G + pathNode.cost + path.GetTraversalCost(this);
		}

		public virtual void UpdateRecursiveG(Path path, PathNode pathNode, PathHandler handler)
		{
			UpdateG(path, pathNode);
			handler.heap.Add(pathNode);
			GetConnections(delegate(GraphNode other)
			{
				PathNode pathNode2 = handler.GetPathNode(other);
				if (pathNode2.parent == pathNode && pathNode2.pathID == handler.PathID)
				{
					other.UpdateRecursiveG(path, pathNode2, handler);
				}
			});
		}

		public virtual void FloodFill(Stack<GraphNode> stack, uint region)
		{
			GetConnections(delegate(GraphNode other)
			{
				if (other.Area != region)
				{
					other.Area = region;
					stack.Push(other);
				}
			});
		}

		public abstract void GetConnections(GraphNodeDelegate del);

		public abstract void AddConnection(GraphNode node, uint cost);

		public abstract void RemoveConnection(GraphNode node);

		public abstract void ClearConnections(bool alsoReverse);

		public virtual bool ContainsConnection(GraphNode node)
		{
			bool contains = false;
			GetConnections(delegate(GraphNode neighbour)
			{
				contains |= (neighbour == node);
			});
			return contains;
		}

		public virtual void RecalculateConnectionCosts()
		{
		}

		public virtual bool GetPortal(GraphNode other, List<Vector3> left, List<Vector3> right, bool backwards)
		{
			return false;
		}

		public abstract void Open(Path path, PathNode pathNode, PathHandler handler);

		public virtual float SurfaceArea()
		{
			return 0f;
		}

		public virtual Vector3 RandomPointOnSurface()
		{
			return (Vector3)position;
		}

		public virtual void SerializeNode(GraphSerializationContext ctx)
		{
			ctx.writer.Write(Penalty);
			ctx.writer.Write(Flags);
		}

		public virtual void DeserializeNode(GraphSerializationContext ctx)
		{
			Penalty = ctx.reader.ReadUInt32();
			Flags = ctx.reader.ReadUInt32();
			GraphIndex = ctx.graphIndex;
		}

		public virtual void SerializeReferences(GraphSerializationContext ctx)
		{
		}

		public virtual void DeserializeReferences(GraphSerializationContext ctx)
		{
		}
	}
}
