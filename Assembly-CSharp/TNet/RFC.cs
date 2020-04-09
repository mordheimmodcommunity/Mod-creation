using System;

namespace TNet
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public sealed class RFC : Attribute
	{
		public byte id;

		public RFC()
		{
		}

		public RFC(byte rid)
		{
			id = rid;
		}
	}
}
