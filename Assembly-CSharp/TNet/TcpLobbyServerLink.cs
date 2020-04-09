using System;
using System.IO;
using System.Net;
using System.Threading;

namespace TNet
{
	public class TcpLobbyServerLink : LobbyServerLink
	{
		private TcpProtocol mTcp;

		private IPEndPoint mRemoteAddress;

		private long mNextConnect;

		private bool mWasConnected;

		public override bool isActive => mTcp.isConnected;

		public TcpLobbyServerLink(IPEndPoint address)
			: base(null)
		{
			mRemoteAddress = address;
		}

		public override void Start()
		{
			base.Start();
			if (mTcp == null)
			{
				mTcp = new TcpProtocol();
				mTcp.name = "Link";
			}
			mNextConnect = 0L;
		}

		public override void SendUpdate(GameServer server)
		{
			if (!mShutdown)
			{
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
				if (mGameServer != null && !mTcp.isConnected && mNextConnect < num)
				{
					mNextConnect = num + 15000;
					mTcp.Connect(mRemoteAddress);
				}
				Buffer buffer;
				while (mTcp.ReceivePacket(out buffer))
				{
					BinaryReader binaryReader = buffer.BeginReading();
					Packet packet = (Packet)binaryReader.ReadByte();
					if (mTcp.stage == TcpProtocol.Stage.Verifying)
					{
						if (!mTcp.VerifyResponseID(packet, binaryReader))
						{
							mThread = null;
							return;
						}
						mWasConnected = true;
					}
					else if (packet == Packet.Error)
					{
						mNextConnect = ((!mWasConnected) ? (num + 30000) : 0);
					}
					buffer.Recycle();
				}
				if (mGameServer != null && mTcp.isConnected)
				{
					BinaryWriter binaryWriter = mTcp.BeginSend(Packet.RequestAddServer);
					binaryWriter.Write((ushort)1);
					binaryWriter.Write(mGameServer.name);
					binaryWriter.Write((short)mGameServer.playerCount);
					Tools.Serialize(binaryWriter, mInternal);
					Tools.Serialize(binaryWriter, mExternal);
					mTcp.EndSend();
					mGameServer = null;
				}
				Thread.Sleep(10);
			}
			mTcp.Disconnect();
			mThread = null;
		}
	}
}
