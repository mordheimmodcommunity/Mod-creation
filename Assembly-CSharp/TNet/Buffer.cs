using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace TNet
{
    public class Buffer
    {
        private static List<Buffer> mPool = new List<Buffer>();

        private MemoryStream mStream;

        private BinaryWriter mWriter;

        private BinaryReader mReader;

        private int mCounter;

        private int mSize;

        private bool mWriting;

        private bool mInPool;

        public int size => (int)((!mWriting) ? (mSize - (int)mStream.Position) : mStream.Position);

        public int position
        {
            get
            {
                return (int)mStream.Position;
            }
            set
            {
                mStream.Seek(value, SeekOrigin.Begin);
            }
        }

        public MemoryStream stream => mStream;

        public byte[] buffer => mStream.GetBuffer();

        public static int recycleQueue => mPool.size;

        private Buffer()
        {
            mStream = new MemoryStream();
            mWriter = new BinaryWriter(mStream);
            mReader = new BinaryReader(mStream);
        }

        ~Buffer()
        {
            mStream.Dispose();
        }

        public static Buffer Create()
        {
            return Create(markAsUsed: true);
        }

        public static Buffer Create(bool markAsUsed)
        {
            Buffer buffer = null;
            if (mPool.size == 0)
            {
                buffer = new Buffer();
            }
            else
            {
                lock (mPool)
                {
                    if (mPool.size != 0)
                    {
                        buffer = mPool.Pop();
                        buffer.mInPool = false;
                    }
                    else
                    {
                        buffer = new Buffer();
                    }
                }
            }
            buffer.mCounter = (markAsUsed ? 1 : 0);
            return buffer;
        }

        public bool Recycle()
        {
            return Recycle(checkUsedFlag: true);
        }

        public bool Recycle(bool checkUsedFlag)
        {
            if (!mInPool && (!checkUsedFlag || MarkAsUnused()))
            {
                mInPool = true;
                lock (mPool)
                {
                    Clear();
                    mPool.Add(this);
                }
                return true;
            }
            return false;
        }

        public static void Recycle(Queue<Buffer> list)
        {
            lock (mPool)
            {
                while (list.Count != 0)
                {
                    Buffer buffer = list.Dequeue();
                    buffer.Clear();
                    mPool.Add(buffer);
                }
            }
        }

        public static void Recycle(Queue<Datagram> list)
        {
            lock (mPool)
            {
                while (list.Count != 0)
                {
                    Datagram datagram = list.Dequeue();
                    Buffer buffer = datagram.buffer;
                    buffer.Clear();
                    mPool.Add(buffer);
                }
            }
        }

        public static void Recycle(List<Buffer> list)
        {
            lock (mPool)
            {
                for (int i = 0; i < list.size; i++)
                {
                    Buffer buffer = list[i];
                    buffer.Clear();
                    mPool.Add(buffer);
                }
                list.Clear();
            }
        }

        public static void Recycle(List<Datagram> list)
        {
            lock (mPool)
            {
                for (int i = 0; i < list.size; i++)
                {
                    Datagram datagram = list[i];
                    Buffer buffer = datagram.buffer;
                    buffer.Clear();
                    mPool.Add(buffer);
                }
                list.Clear();
            }
        }

        public void MarkAsUsed()
        {
            Interlocked.Increment(ref mCounter);
        }

        public bool MarkAsUnused()
        {
            if (Interlocked.Decrement(ref mCounter) > 0)
            {
                return false;
            }
            mSize = 0;
            mStream.Seek(0L, SeekOrigin.Begin);
            mWriting = true;
            return true;
        }

        public void Clear()
        {
            mCounter = 0;
            mSize = 0;
            if (mStream.Capacity > 1024)
            {
                mStream.SetLength(256L);
            }
            mStream.Seek(0L, SeekOrigin.Begin);
            mWriting = true;
        }

        public void CopyTo(Buffer target)
        {
            BinaryWriter binaryWriter = target.BeginWriting(append: false);
            int size = this.size;
            if (size > 0)
            {
                binaryWriter.Write(buffer, position, size);
            }
            target.EndWriting();
        }

        public void Dispose()
        {
            mStream.Dispose();
        }

        public BinaryWriter BeginWriting(bool append)
        {
            if (!append || !mWriting)
            {
                mStream.Seek(0L, SeekOrigin.Begin);
                mSize = 0;
            }
            mWriting = true;
            return mWriter;
        }

        public BinaryWriter BeginWriting(int startOffset)
        {
            mStream.Seek(startOffset, SeekOrigin.Begin);
            mWriting = true;
            return mWriter;
        }

        public int EndWriting()
        {
            if (mWriting)
            {
                mSize = position;
                mStream.Seek(0L, SeekOrigin.Begin);
                mWriting = false;
            }
            return mSize;
        }

        public BinaryReader BeginReading()
        {
            if (mWriting)
            {
                mWriting = false;
                mSize = (int)mStream.Position;
                mStream.Seek(0L, SeekOrigin.Begin);
            }
            return mReader;
        }

        public BinaryReader BeginReading(int startOffset)
        {
            if (mWriting)
            {
                mWriting = false;
                mSize = (int)mStream.Position;
            }
            mStream.Seek(startOffset, SeekOrigin.Begin);
            return mReader;
        }

        public int PeekByte(int offset)
        {
            long position = mStream.Position;
            if (offset + 1 > position)
            {
                return -1;
            }
            mStream.Seek(offset, SeekOrigin.Begin);
            int result = mReader.ReadByte();
            mStream.Seek(position, SeekOrigin.Begin);
            return result;
        }

        public int PeekInt(int offset)
        {
            long position = mStream.Position;
            if (offset + 4 > position)
            {
                return -1;
            }
            mStream.Seek(offset, SeekOrigin.Begin);
            int result = mReader.ReadInt32();
            mStream.Seek(position, SeekOrigin.Begin);
            return result;
        }

        public BinaryWriter BeginPacket(byte packetID)
        {
            BinaryWriter binaryWriter = BeginWriting(append: false);
            binaryWriter.Write(0);
            binaryWriter.Write(packetID);
            return binaryWriter;
        }

        public BinaryWriter BeginPacket(Packet packet)
        {
            BinaryWriter binaryWriter = BeginWriting(append: false);
            binaryWriter.Write(0);
            binaryWriter.Write((byte)packet);
            return binaryWriter;
        }

        public BinaryWriter BeginPacket(Packet packet, int startOffset)
        {
            BinaryWriter binaryWriter = BeginWriting(startOffset);
            binaryWriter.Write(0);
            binaryWriter.Write((byte)packet);
            return binaryWriter;
        }

        public int EndPacket()
        {
            if (mWriting)
            {
                mSize = position;
                mStream.Seek(0L, SeekOrigin.Begin);
                mWriter.Write(mSize - 4);
                mStream.Seek(0L, SeekOrigin.Begin);
                mWriting = false;
            }
            return mSize;
        }

        public int EndTcpPacketStartingAt(int startOffset)
        {
            if (mWriting)
            {
                mSize = position;
                mStream.Seek(startOffset, SeekOrigin.Begin);
                mWriter.Write(mSize - 4 - startOffset);
                mStream.Seek(0L, SeekOrigin.Begin);
                mWriting = false;
            }
            return mSize;
        }

        public int EndTcpPacketWithOffset(int offset)
        {
            if (mWriting)
            {
                mSize = position;
                mStream.Seek(0L, SeekOrigin.Begin);
                mWriter.Write(mSize - 4);
                mStream.Seek(offset, SeekOrigin.Begin);
                mWriting = false;
            }
            return mSize;
        }
    }
}
