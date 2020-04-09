using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Pathfinding.Serialization.Zip
{
	public class ZipFile
	{
		public Encoding AlternateEncoding;

		public ZipOption AlternateEncodingUsage;

		private Dictionary<string, ZipEntry> dict = new Dictionary<string, ZipEntry>();

		public ZipEntry this[string index]
		{
			get
			{
				dict.TryGetValue(index, out ZipEntry value);
				return value;
			}
		}

		public void AddEntry(string name, byte[] bytes)
		{
			dict[name] = new ZipEntry(name, bytes);
		}

		public bool ContainsEntry(string name)
		{
			return dict.ContainsKey(name);
		}

		public void Save(Stream stream)
		{
			BinaryWriter binaryWriter = new BinaryWriter(stream);
			binaryWriter.Write(dict.Count);
			foreach (KeyValuePair<string, ZipEntry> item in dict)
			{
				binaryWriter.Write(item.Key);
				binaryWriter.Write(item.Value.bytes.Length);
				binaryWriter.Write(item.Value.bytes);
			}
		}

		public static ZipFile Read(Stream stream)
		{
			ZipFile zipFile = new ZipFile();
			BinaryReader binaryReader = new BinaryReader(stream);
			int num = binaryReader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				string text = binaryReader.ReadString();
				int count = binaryReader.ReadInt32();
				byte[] bytes = binaryReader.ReadBytes(count);
				zipFile.dict[text] = new ZipEntry(text, bytes);
			}
			return zipFile;
		}

		public void Dispose()
		{
		}
	}
}
