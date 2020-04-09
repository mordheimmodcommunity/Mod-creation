using System;

namespace TNet
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public sealed class RCC : Attribute
	{
		public byte id;

		public RCC(byte rid)
		{
			id = rid;
		}
	}
}
