using System;
using System.Collections.Generic;

namespace Pathfinding.Serialization
{
	public class GraphMeta
	{
		public Version version;

		public int graphs;

		public List<string> guids;

		public List<string> typeNames;

		public Type GetGraphType(int i)
		{
			if (string.IsNullOrEmpty(typeNames[i]))
			{
				return null;
			}
			Type type = typeof(AstarPath).Assembly.GetType(typeNames[i]);
			if (!object.Equals(type, null))
			{
				return type;
			}
			throw new Exception("No graph of type '" + typeNames[i] + "' could be created, type does not exist");
		}
	}
}
