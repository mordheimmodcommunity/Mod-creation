using Pathfinding.Serialization;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
	public abstract class MeshNode : GraphNode
	{
		public GraphNode[] connections;

		public uint[] connectionCosts;

		protected MeshNode(AstarPath astar)
			: base(astar)
		{
		}

		public abstract Int3 GetVertex(int i);

		public abstract int GetVertexCount();

		public abstract Vector3 ClosestPointOnNode(Vector3 p);

		public abstract Vector3 ClosestPointOnNodeXZ(Vector3 p);

		public override void ClearConnections(bool alsoReverse)
		{
			if (alsoReverse && connections != null)
			{
				for (int i = 0; i < connections.Length; i++)
				{
					connections[i].RemoveConnection(this);
				}
			}
			connections = null;
			connectionCosts = null;
		}

		public override void GetConnections(GraphNodeDelegate del)
		{
			if (connections != null)
			{
				for (int i = 0; i < connections.Length; i++)
				{
					del(connections[i]);
				}
			}
		}

		public override void FloodFill(Stack<GraphNode> stack, uint region)
		{
			if (connections == null)
			{
				return;
			}
			for (int i = 0; i < connections.Length; i++)
			{
				GraphNode graphNode = connections[i];
				if (graphNode != null && graphNode.Area != region)
				{
					graphNode.Area = region;
					stack.Push(graphNode);
				}
			}
		}

		public override bool ContainsConnection(GraphNode node)
		{
			for (int i = 0; i < connections.Length; i++)
			{
				if (connections[i] == node)
				{
					return true;
				}
			}
			return false;
		}

		public override void UpdateRecursiveG(Path path, PathNode pathNode, PathHandler handler)
		{
			UpdateG(path, pathNode);
			handler.heap.Add(pathNode);
			for (int i = 0; i < connections.Length; i++)
			{
				GraphNode graphNode = connections[i];
				PathNode pathNode2 = handler.GetPathNode(graphNode);
				if (pathNode2.parent == pathNode && pathNode2.pathID == handler.PathID)
				{
					graphNode.UpdateRecursiveG(path, pathNode2, handler);
				}
			}
		}

		public override void AddConnection(GraphNode node, uint cost)
		{
			if (node == null)
			{
				throw new ArgumentNullException();
			}
			if (connections != null)
			{
				for (int i = 0; i < connections.Length; i++)
				{
					if (connections[i] == node)
					{
						connectionCosts[i] = cost;
						return;
					}
				}
			}
			int num = (connections != null) ? connections.Length : 0;
			GraphNode[] array = new GraphNode[num + 1];
			uint[] array2 = new uint[num + 1];
			for (int j = 0; j < num; j++)
			{
				array[j] = connections[j];
				array2[j] = connectionCosts[j];
			}
			array[num] = node;
			array2[num] = cost;
			connections = array;
			connectionCosts = array2;
		}

		public override void RemoveConnection(GraphNode node)
		{
			if (connections == null)
			{
				return;
			}
			int num = 0;
			while (true)
			{
				if (num < connections.Length)
				{
					if (connections[num] == node)
					{
						break;
					}
					num++;
					continue;
				}
				return;
			}
			int num2 = connections.Length;
			GraphNode[] array = new GraphNode[num2 - 1];
			uint[] array2 = new uint[num2 - 1];
			for (int i = 0; i < num; i++)
			{
				array[i] = connections[i];
				array2[i] = connectionCosts[i];
			}
			for (int j = num + 1; j < num2; j++)
			{
				array[j - 1] = connections[j];
				array2[j - 1] = connectionCosts[j];
			}
			connections = array;
			connectionCosts = array2;
		}

		public virtual bool ContainsPoint(Int3 p)
		{
			bool flag = false;
			int vertexCount = GetVertexCount();
			int num = 0;
			for (int i = vertexCount - 1; num < vertexCount; i = num++)
			{
				Int3 vertex = GetVertex(num);
				if (vertex.z <= p.z)
				{
					int z = p.z;
					Int3 vertex2 = GetVertex(i);
					if (z < vertex2.z)
					{
						goto IL_0084;
					}
				}
				Int3 vertex3 = GetVertex(i);
				if (vertex3.z > p.z)
				{
					continue;
				}
				int z2 = p.z;
				Int3 vertex4 = GetVertex(num);
				if (z2 >= vertex4.z)
				{
					continue;
				}
				goto IL_0084;
				IL_0084:
				int x = p.x;
				Int3 vertex5 = GetVertex(i);
				int x2 = vertex5.x;
				Int3 vertex6 = GetVertex(num);
				int num2 = x2 - vertex6.x;
				int z3 = p.z;
				Int3 vertex7 = GetVertex(num);
				int num3 = num2 * (z3 - vertex7.z);
				Int3 vertex8 = GetVertex(i);
				int z4 = vertex8.z;
				Int3 vertex9 = GetVertex(num);
				int num4 = num3 / (z4 - vertex9.z);
				Int3 vertex10 = GetVertex(num);
				if (x < num4 + vertex10.x)
				{
					flag = !flag;
				}
			}
			return flag;
		}

		public override void SerializeReferences(GraphSerializationContext ctx)
		{
			if (connections == null)
			{
				ctx.writer.Write(-1);
				return;
			}
			ctx.writer.Write(connections.Length);
			for (int i = 0; i < connections.Length; i++)
			{
				ctx.SerializeNodeReference(connections[i]);
				ctx.writer.Write(connectionCosts[i]);
			}
		}

		public override void DeserializeReferences(GraphSerializationContext ctx)
		{
			int num = ctx.reader.ReadInt32();
			if (num == -1)
			{
				connections = null;
				connectionCosts = null;
				return;
			}
			connections = new GraphNode[num];
			connectionCosts = new uint[num];
			for (int i = 0; i < num; i++)
			{
				connections[i] = ctx.DeserializeNodeReference();
				connectionCosts[i] = ctx.reader.ReadUInt32();
			}
		}
	}
}
