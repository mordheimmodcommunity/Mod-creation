using System;
using System.IO;
using System.Net;
using System.Threading;

namespace TNet
{
    public class UdpLobbyServer : LobbyServer
    {
        private ServerList mList = new ServerList();

        private long mTime;

        private UdpProtocol mUdp;

        private Thread mThread;

        private Buffer mBuffer;

        public override int port => mUdp.isActive ? mUdp.listeningPort : 0;

        public override bool isActive => mUdp != null && mUdp.isActive;

        public override bool Start(int listenPort)
        {
            Stop();
            mUdp = new UdpProtocol();
            if (!mUdp.Start(listenPort))
            {
                return false;
            }
            mThread = new Thread(ThreadFunction);
            mThread.Start();
            return true;
        }

        public override void Stop()
        {
            if (mThread != null)
            {
                mThread.Abort();
                mThread = null;
            }
            if (mUdp != null)
            {
                mUdp.Stop();
                mUdp = null;
            }
            mList.Clear();
        }

        private void ThreadFunction()
        {
            while (true)
            {
                mTime = DateTime.Now.Ticks / 10000;
                mList.Cleanup(mTime);
                Buffer buffer;
                IPEndPoint source;
                while (mUdp != null && mUdp.listeningPort != 0 && mUdp.ReceivePacket(out buffer, out source))
                {
                    try
                    {
                        ProcessPacket(buffer, source);
                    }
                    catch (Exception)
                    {
                    }
                    if (buffer != null)
                    {
                        buffer.Recycle();
                        buffer = null;
                    }
                }
                Thread.Sleep(1);
            }
        }

        private bool ProcessPacket(Buffer buffer, IPEndPoint ip)
        {
            BinaryReader binaryReader = buffer.BeginReading();
            switch (binaryReader.ReadByte())
            {
                case 4:
                    BeginSend(Packet.ResponsePing);
                    EndSend(ip);
                    break;
                case 45:
                    {
                        if (binaryReader.ReadUInt16() != 1)
                        {
                            return false;
                        }
                        ServerList.Entry entry = new ServerList.Entry();
                        entry.ReadFrom(binaryReader);
                        if (entry.externalAddress.Address.Equals(IPAddress.None))
                        {
                            entry.externalAddress = ip;
                        }
                        mList.Add(entry, mTime);
                        return true;
                    }
                case 46:
                    {
                        if (binaryReader.ReadUInt16() != 1)
                        {
                            return false;
                        }
                        Tools.Serialize(binaryReader, out IPEndPoint ip2);
                        Tools.Serialize(binaryReader, out IPEndPoint ip3);
                        if (ip3.Address.Equals(IPAddress.None))
                        {
                            ip3 = ip;
                        }
                        RemoveServer(ip2, ip3);
                        return true;
                    }
                case 47:
                    if (binaryReader.ReadUInt16() != 1)
                    {
                        return false;
                    }
                    mList.WriteTo(BeginSend(Packet.ResponseServerList));
                    EndSend(ip);
                    return true;
            }
            return false;
        }

        public override void AddServer(string name, int playerCount, IPEndPoint internalAddress, IPEndPoint externalAddress)
        {
            mList.Add(name, playerCount, internalAddress, externalAddress, mTime);
        }

        public override void RemoveServer(IPEndPoint internalAddress, IPEndPoint externalAddress)
        {
            mList.Remove(internalAddress, externalAddress);
        }

        private BinaryWriter BeginSend(Packet packet)
        {
            mBuffer = Buffer.Create();
            return mBuffer.BeginPacket(packet);
        }

        private void EndSend(IPEndPoint ip)
        {
            mBuffer.EndPacket();
            mUdp.Send(mBuffer, ip);
            mBuffer.Recycle();
            mBuffer = null;
        }
    }
}
