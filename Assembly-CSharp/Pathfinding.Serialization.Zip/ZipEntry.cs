using System.IO;

namespace Pathfinding.Serialization.Zip
{
	public class ZipEntry
	{
		internal string name;

		internal byte[] bytes;

		public ZipEntry(string name, byte[] bytes)
		{
			this.name = name;
			this.bytes = bytes;
		}

		public void Extract(Stream stream)
		{
			stream.Write(bytes, 0, bytes.Length);
		}
	}
}
