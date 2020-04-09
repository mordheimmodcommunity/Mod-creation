using System;
using System.Reflection;
using System.Runtime.Serialization;

public sealed class VersionDeserializationBinderEdge : SerializationBinder
{
	public override Type BindToType(string assemblyName, string typeName)
	{
		if (!string.IsNullOrEmpty(assemblyName) && !string.IsNullOrEmpty(typeName))
		{
			Type type = null;
			assemblyName = Assembly.GetExecutingAssembly().FullName;
			return Type.GetType($"{typeName}, {assemblyName}");
		}
		return null;
	}
}
