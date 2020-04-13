using System.IO;

namespace TNet
{
    public class Channel
    {
        public class RFC
        {
            public uint uid;

            public string functionName;

            public Buffer buffer;

            public uint objectID => uid >> 8;

            public uint functionID => uid & 0xFF;
        }

        public class CreatedObject
        {
            public int playerID;

            public ushort objectID;

            public uint uniqueID;

            public byte type;

            public Buffer buffer;
        }

        public int id;

        public string password = string.Empty;

        public string level = string.Empty;

        public string data = string.Empty;

        public bool persistent;

        public bool closed;

        public ushort playerLimit = ushort.MaxValue;

        public List<TcpPlayer> players = new List<TcpPlayer>();

        public List<RFC> rfcs = new List<RFC>();

        public List<CreatedObject> created = new List<CreatedObject>();

        public List<uint> destroyed = new List<uint>();

        public uint objectCounter = 16777215u;

        public TcpPlayer host;

        public bool hasData => rfcs.size > 0 || created.size > 0 || destroyed.size > 0;

        public bool isOpen => !closed && players.size < playerLimit;

        public void Reset()
        {
            for (int i = 0; i < rfcs.size; i++)
            {
                rfcs[i].buffer.Recycle();
            }
            for (int j = 0; j < created.size; j++)
            {
                created[j].buffer.Recycle();
            }
            rfcs.Clear();
            created.Clear();
            destroyed.Clear();
            objectCounter = 16777215u;
        }

        public void RemovePlayer(TcpPlayer p, List<uint> destroyedObjects)
        {
            destroyedObjects.Clear();
            if (p == host)
            {
                host = null;
            }
            if (!players.Remove(p))
            {
                return;
            }
            int num = created.size;
            while (num > 0)
            {
                CreatedObject createdObject = created[--num];
                if (createdObject.type == 2 && createdObject.playerID == p.id)
                {
                    if (createdObject.buffer != null)
                    {
                        createdObject.buffer.Recycle();
                    }
                    created.RemoveAt(num);
                    destroyedObjects.Add(createdObject.uniqueID);
                    DestroyObjectRFCs(createdObject.uniqueID);
                }
            }
            if ((persistent && playerLimit >= 1) || players.size != 0)
            {
                return;
            }
            closed = true;
            for (int i = 0; i < rfcs.size; i++)
            {
                RFC rFC = rfcs[i];
                if (rFC.buffer != null)
                {
                    rFC.buffer.Recycle();
                }
            }
            rfcs.Clear();
        }

        public bool DestroyObject(uint uniqueID)
        {
            if (!destroyed.Contains(uniqueID))
            {
                for (int i = 0; i < created.size; i++)
                {
                    CreatedObject createdObject = created[i];
                    if (createdObject.uniqueID == uniqueID)
                    {
                        if (createdObject.buffer != null)
                        {
                            createdObject.buffer.Recycle();
                        }
                        created.RemoveAt(i);
                        DestroyObjectRFCs(uniqueID);
                        return true;
                    }
                }
                destroyed.Add(uniqueID);
                DestroyObjectRFCs(uniqueID);
                return true;
            }
            return false;
        }

        public void DestroyObjectRFCs(uint objectID)
        {
            int num = 0;
            while (num < rfcs.size)
            {
                RFC rFC = rfcs[num];
                if (rFC.objectID == objectID)
                {
                    rfcs.RemoveAt(num);
                    rFC.buffer.Recycle();
                }
                else
                {
                    num++;
                }
            }
        }

        public void CreateRFC(uint inID, string funcName, Buffer buffer)
        {
            if (closed || buffer == null)
            {
                return;
            }
            buffer.MarkAsUsed();
            for (int i = 0; i < rfcs.size; i++)
            {
                RFC rFC = rfcs[i];
                if (rFC.uid == inID && rFC.functionName == funcName)
                {
                    if (rFC.buffer != null)
                    {
                        rFC.buffer.Recycle();
                    }
                    rFC.buffer = buffer;
                    return;
                }
            }
            RFC rFC2 = new RFC();
            rFC2.uid = inID;
            rFC2.buffer = buffer;
            rFC2.functionName = funcName;
            rfcs.Add(rFC2);
        }

        public void DeleteRFC(uint inID, string funcName)
        {
            for (int i = 0; i < rfcs.size; i++)
            {
                RFC rFC = rfcs[i];
                if (rFC.uid == inID && rFC.functionName == funcName)
                {
                    rfcs.RemoveAt(i);
                    rFC.buffer.Recycle();
                }
            }
        }

        public void SaveTo(BinaryWriter writer)
        {
            writer.Write(11);
            writer.Write(level);
            writer.Write(data);
            writer.Write(objectCounter);
            writer.Write(password);
            writer.Write(persistent);
            writer.Write(playerLimit);
            writer.Write(rfcs.size);
            for (int i = 0; i < rfcs.size; i++)
            {
                RFC rFC = rfcs[i];
                writer.Write(rFC.uid);
                if (rFC.functionID == 0)
                {
                    writer.Write(rFC.functionName);
                }
                writer.Write(rFC.buffer.size);
                if (rFC.buffer.size > 0)
                {
                    rFC.buffer.BeginReading();
                    writer.Write(rFC.buffer.buffer, rFC.buffer.position, rFC.buffer.size);
                }
            }
            writer.Write(created.size);
            for (int j = 0; j < created.size; j++)
            {
                CreatedObject createdObject = created[j];
                writer.Write(createdObject.playerID);
                writer.Write(createdObject.uniqueID);
                writer.Write(createdObject.objectID);
                writer.Write(createdObject.buffer.size);
                if (createdObject.buffer.size > 0)
                {
                    createdObject.buffer.BeginReading();
                    writer.Write(createdObject.buffer.buffer, createdObject.buffer.position, createdObject.buffer.size);
                }
            }
            writer.Write(destroyed.size);
            for (int k = 0; k < destroyed.size; k++)
            {
                writer.Write(destroyed[k]);
            }
        }

        public bool LoadFrom(BinaryReader reader)
        {
            int num = reader.ReadInt32();
            if (num != 11)
            {
                return false;
            }
            for (int i = 0; i < rfcs.size; i++)
            {
                RFC rFC = rfcs[i];
                if (rFC.buffer != null)
                {
                    rFC.buffer.Recycle();
                }
            }
            rfcs.Clear();
            created.Clear();
            destroyed.Clear();
            level = reader.ReadString();
            data = reader.ReadString();
            objectCounter = reader.ReadUInt32();
            password = reader.ReadString();
            persistent = reader.ReadBoolean();
            playerLimit = reader.ReadUInt16();
            int num2 = reader.ReadInt32();
            for (int j = 0; j < num2; j++)
            {
                RFC rFC2 = new RFC();
                rFC2.uid = reader.ReadUInt32();
                if (rFC2.functionID == 0)
                {
                    rFC2.functionName = reader.ReadString();
                }
                Buffer buffer = Buffer.Create();
                buffer.BeginWriting(append: false).Write(reader.ReadBytes(reader.ReadInt32()));
                rFC2.buffer = buffer;
                rfcs.Add(rFC2);
            }
            num2 = reader.ReadInt32();
            for (int k = 0; k < num2; k++)
            {
                CreatedObject createdObject = new CreatedObject();
                createdObject.playerID = reader.ReadInt32();
                createdObject.uniqueID = reader.ReadUInt32();
                createdObject.objectID = reader.ReadUInt16();
                Buffer buffer2 = Buffer.Create();
                buffer2.BeginWriting(append: false).Write(reader.ReadBytes(reader.ReadInt32()));
                createdObject.buffer = buffer2;
                created.Add(createdObject);
            }
            num2 = reader.ReadInt32();
            for (int l = 0; l < num2; l++)
            {
                destroyed.Add(reader.ReadUInt32());
            }
            return true;
        }
    }
}
