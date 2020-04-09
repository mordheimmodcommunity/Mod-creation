using System.IO;
using System.Net;

namespace TNet
{
	public class ServerList
	{
		public class Entry
		{
			public string name;

			public int playerCount;

			public IPEndPoint internalAddress;

			public IPEndPoint externalAddress;

			public long recordTime;

			public object data;

			public void WriteTo(BinaryWriter writer)
			{
				writer.Write(name);
				writer.Write((ushort)playerCount);
				Tools.Serialize(writer, internalAddress);
				Tools.Serialize(writer, externalAddress);
			}

			public void ReadFrom(BinaryReader reader)
			{
				name = reader.ReadString();
				playerCount = reader.ReadUInt16();
				Tools.Serialize(reader, out internalAddress);
				Tools.Serialize(reader, out externalAddress);
			}
		}

		public List<Entry> list = new List<Entry>();

		public Entry Add(string name, int playerCount, IPEndPoint internalAddress, IPEndPoint externalAddress, long time)
		{
			//Discarded unreachable code: IL_00c3
			lock (list)
			{
				for (int i = 0; i < list.size; i++)
				{
					Entry entry = list[i];
					if (entry.internalAddress.Equals(internalAddress) && entry.externalAddress.Equals(externalAddress))
					{
						entry.name = name;
						entry.playerCount = playerCount;
						entry.recordTime = time;
						list[i] = entry;
						return entry;
					}
				}
				Entry entry2 = new Entry();
				entry2.name = name;
				entry2.playerCount = playerCount;
				entry2.internalAddress = internalAddress;
				entry2.externalAddress = externalAddress;
				entry2.recordTime = time;
				list.Add(entry2);
				return entry2;
			}
		}

		public Entry Add(Entry newEntry, long time)
		{
			lock (list)
			{
				for (int i = 0; i < list.size; i++)
				{
					Entry entry = list[i];
					if (entry.internalAddress.Equals(newEntry.internalAddress) && entry.externalAddress.Equals(newEntry.externalAddress))
					{
						entry.name = newEntry.name;
						entry.playerCount = newEntry.playerCount;
						entry.recordTime = time;
						return entry;
					}
				}
				newEntry.recordTime = time;
				list.Add(newEntry);
				return newEntry;
			}
		}

		public bool Remove(IPEndPoint internalAddress, IPEndPoint externalAddress)
		{
			lock (list)
			{
				for (int i = 0; i < list.size; i++)
				{
					Entry entry = list[i];
					if (entry.internalAddress.Equals(internalAddress) && entry.externalAddress.Equals(externalAddress))
					{
						list.RemoveAt(i);
						return true;
					}
				}
			}
			return false;
		}

		public bool Cleanup(long time)
		{
			time -= 7000;
			bool result = false;
			lock (list)
			{
				int num = 0;
				while (num < list.size)
				{
					Entry entry = list[num];
					if (entry.recordTime < time)
					{
						result = true;
						list.RemoveAt(num);
					}
					else
					{
						num++;
					}
				}
				return result;
			}
		}

		public void Clear()
		{
			lock (list)
			{
				list.Clear();
			}
		}

		public void WriteTo(BinaryWriter writer)
		{
			writer.Write((ushort)1);
			lock (list)
			{
				writer.Write((ushort)list.size);
				for (int i = 0; i < list.size; i++)
				{
					list[i].WriteTo(writer);
				}
			}
		}

		public void ReadFrom(BinaryReader reader, long time)
		{
			if (reader.ReadUInt16() == 1)
			{
				lock (list)
				{
					int num = reader.ReadUInt16();
					for (int i = 0; i < num; i++)
					{
						Entry entry = new Entry();
						entry.ReadFrom(reader);
						AddInternal(entry, time);
					}
				}
			}
		}

		private void AddInternal(Entry newEntry, long time)
		{
			for (int i = 0; i < list.size; i++)
			{
				Entry entry = list[i];
				if (entry.internalAddress.Equals(newEntry.internalAddress) && entry.externalAddress.Equals(newEntry.externalAddress))
				{
					entry.name = newEntry.name;
					entry.playerCount = newEntry.playerCount;
					entry.recordTime = time;
					return;
				}
			}
			newEntry.recordTime = time;
			list.Add(newEntry);
		}
	}
}
