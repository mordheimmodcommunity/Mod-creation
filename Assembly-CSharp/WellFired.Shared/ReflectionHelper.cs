using System;
using System.Collections;
using System.Reflection;

namespace WellFired.Shared
{
	public class ReflectionHelper : IReflectionHelper
	{
		public bool IsAssignableFrom(Type first, Type second)
		{
			return first.IsAssignableFrom(second);
		}

		public bool IsEnum(Type type)
		{
			return type.IsEnum;
		}

		private IEnumerable GetBaseTypes(Type type)
		{
			yield return type;
			Type baseType = type.BaseType;
			if ((object)baseType != null)
			{
				foreach (object baseType2 in GetBaseTypes(baseType))
				{
					yield return baseType2;
				}
			}
		}

		public PropertyInfo GetProperty(Type type, string name)
		{
			return type.GetProperty(name);
		}

		public PropertyInfo GetNonPublicInstanceProperty(Type type, string name)
		{
			return type.GetProperty(name, BindingFlags.Instance | BindingFlags.NonPublic);
		}

		public MethodInfo GetMethod(Type type, string name)
		{
			return type.GetMethod(name);
		}

		public MethodInfo GetNonPublicStaticMethod(Type type, string name)
		{
			return type.GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic);
		}

		public FieldInfo GetField(Type type, string name)
		{
			return type.GetField(name);
		}

		public FieldInfo GetNonPublicInstanceField(Type type, string name)
		{
			return type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
		}

		public bool IsValueType(Type type)
		{
			return type.IsValueType;
		}
	}
}
