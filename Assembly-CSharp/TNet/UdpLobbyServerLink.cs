using System;
using System.IO;
using System.Net;
using System.Threading;

namespace TNet
{
	public class UdpLobbyServerLink : LobbyServerLink
	{
		private UdpProtocol mUdp;

		private IPEndPoint mRemoteAddress;

		private long mNextSend;

		public override bool isActive => mUdp != null && mUdp.isActive;

		public UdpLobbyServerLink(IPEndPoint address)
			: base(null)
		{
			mRemoteAddress = address;
		}

		~UdpLobbyServerLink()
		{
			if (mUdp != null)
			{
				mUdp.Stop();
				mUdp = null;
			}
		}

		public override void Start()
		{
			base.Start();
			if (mUdp == null)
			{
				mUdp = new UdpProtocol();
				mUdp.Start();
			}
		}

		public override void SendUpdate(GameServer server)
		{
			if (!mShutdown)
			{
				mNextSend = 0L;
				mGameServer = server;
				if (mThread == null)
				{
					mThread = new Thread(ThreadFunction);
					mThread.Start();
				}
			}
		}

		private void ThreadFunction()
		{
			mInternal = new IPEndPoint(Tools.localAddress, mGameServer.tcpPort);
			mExternal = new IPEndPoint(Tools.externalAddress, mGameServer.tcpPort);
			while (true)
			{
				long num = DateTime.Now.Ticks / 10000;
				if (mShutdown)
				{
					break;
				}
				if (mNextSend < num && mGameServer != null)
				{
					mNextSend = num + 3000;
					Buffer buffer = Buffer.Create();
					BinaryWriter binaryWriter = buffer.BeginPacket(Packet.RequestAddServer);
					binaryWriter.Write((ushort)1);
					binaryWriter.Write(mGameServer.name);
					binaryWriter.Write((short)mGameServer.playerCount);
					Tools.Serialize(binaryWriter, mInternal);
					Tools.Serialize(binaryWriter, mExternal);
					buffer.EndPacket();
					mUdp.Send(buffer, mRemoteAddress);
					buffer.Recycle();
				}
				Thread.Sleep(10);
			}
			Buffer buffer2 = Buffer.Create();
			BinaryWriter binaryWriter2 = buffer2.BeginPacket(Packet.RequestRemoveServer);
			binaryWriter2.Write((ushort)1);
			Tools.Serialize(binaryWriter2, mInternal);
			Tools.Serialize(binaryWriter2, mExternal);
			buffer2.EndPacket();
			mUdp.Send(buffer2, mRemoteAddress);
			buffer2.Recycle();
			mThread = null;
		}
	}
}
