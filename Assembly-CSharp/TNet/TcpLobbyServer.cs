using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TNet
{
	public class TcpLobbyServer : LobbyServer
	{
		private ServerList mList = new ServerList();

		private long mTime;

		private long mLastChange;

		private List<TcpProtocol> mTcp = new List<TcpProtocol>();

		private TcpListener mListener;

		private int mPort;

		private Thread mThread;

		private bool mInstantUpdates = true;

		private Buffer mBuffer;

		public int instantUpdatesClientLimit = 50;

		public override int port => mPort;

		public override bool isActive => mListener != null;

		public override bool Start(int listenPort)
		{
			//Discarded unreachable code: IL_0038
			Stop();
			try
			{
				mListener = new TcpListener(IPAddress.Any, listenPort);
				mListener.Start(50);
				mPort = listenPort;
			}
			catch (Exception)
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
			if (mListener != null)
			{
				mListener.Stop();
				mListener = null;
			}
			mList.Clear();
		}

		private BinaryWriter BeginSend(Packet type)
		{
			mBuffer = Buffer.Create();
			return mBuffer.BeginPacket(type);
		}

		private void EndSend(TcpProtocol tc)
		{
			mBuffer.EndPacket();
			tc.SendTcpPacket(mBuffer);
			mBuffer.Recycle();
			mBuffer = null;
		}

		private void ThreadFunction()
		{
			while (true)
			{
				mTime = DateTime.Now.Ticks / 10000;
				while (mListener != null && mListener.Pending())
				{
					TcpProtocol tcpProtocol = new TcpProtocol();
					tcpProtocol.StartReceiving(mListener.AcceptSocket());
					mTcp.Add(tcpProtocol);
				}
				Buffer buffer = null;
				for (int i = 0; i < mTcp.size; i++)
				{
					TcpProtocol tcpProtocol2 = mTcp[i];
					while (tcpProtocol2.ReceivePacket(out buffer))
					{
						try
						{
							if (!ProcessPacket(buffer, tcpProtocol2))
							{
								RemoveServer(tcpProtocol2);
								tcpProtocol2.Disconnect();
							}
						}
						catch (Exception)
						{
							RemoveServer(tcpProtocol2);
							tcpProtocol2.Disconnect();
						}
						if (buffer != null)
						{
							buffer.Recycle();
							buffer = null;
						}
					}
				}
				if (mTcp.size > instantUpdatesClientLimit)
				{
					mInstantUpdates = false;
				}
				for (int j = 0; j < mTcp.size; j++)
				{
					TcpProtocol tcpProtocol3 = mTcp[j];
					if (tcpProtocol3.stage != TcpProtocol.Stage.Connected || tcpProtocol3.data == null || !(tcpProtocol3.data is long))
					{
						continue;
					}
					long num = (long)tcpProtocol3.data;
					if (num == 0L || (num < mLastChange && (mInstantUpdates || num + 4000 <= mTime)))
					{
						if (buffer == null)
						{
							buffer = Buffer.Create();
							BinaryWriter writer = buffer.BeginPacket(Packet.ResponseServerList);
							mList.WriteTo(writer);
							buffer.EndPacket();
						}
						tcpProtocol3.SendTcpPacket(buffer);
						tcpProtocol3.data = mTime;
					}
				}
				if (buffer != null)
				{
					buffer.Recycle();
					buffer = null;
				}
				Thread.Sleep(1);
			}
		}

		private bool ProcessPacket(Buffer buffer, TcpProtocol tc)
		{
			if (tc.stage == TcpProtocol.Stage.Verifying)
			{
				if (tc.VerifyRequestID(buffer, uniqueID: false))
				{
					return true;
				}
				return false;
			}
			BinaryReader binaryReader = buffer.BeginReading();
			switch (binaryReader.ReadByte())
			{
			case 4:
				BeginSend(Packet.ResponsePing);
				EndSend(tc);
				break;
			case 47:
				if (binaryReader.ReadUInt16() != 1)
				{
					return false;
				}
				tc.data = 0L;
				return true;
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
					entry.externalAddress = tc.tcpEndPoint;
				}
				mList.Add(entry, mTime).data = tc;
				mLastChange = mTime;
				return true;
			}
			case 46:
			{
				if (binaryReader.ReadUInt16() != 1)
				{
					return false;
				}
				Tools.Serialize(binaryReader, out IPEndPoint ip);
				Tools.Serialize(binaryReader, out IPEndPoint ip2);
				if (ip2.Address.Equals(IPAddress.None))
				{
					ip2 = tc.tcpEndPoint;
				}
				RemoveServer(ip, ip2);
				return true;
			}
			case 2:
				RemoveServer(tc);
				mTcp.Remove(tc);
				return true;
			case 16:
			{
				string fileName = binaryReader.ReadString();
				byte[] data = binaryReader.ReadBytes(binaryReader.ReadInt32());
				SaveFile(fileName, data);
				break;
			}
			case 17:
			{
				string text = binaryReader.ReadString();
				byte[] array = LoadFile(text);
				BinaryWriter binaryWriter = BeginSend(Packet.ResponseLoadFile);
				binaryWriter.Write(text);
				if (array != null)
				{
					binaryWriter.Write(array.Length);
					binaryWriter.Write(array);
				}
				else
				{
					binaryWriter.Write(0);
				}
				EndSend(tc);
				break;
			}
			case 18:
				DeleteFile(binaryReader.ReadString());
				break;
			case 1:
				return false;
			}
			return false;
		}

		private bool RemoveServer(Player player)
		{
			bool result = false;
			lock (mList.list)
			{
				int num = mList.list.size;
				while (num > 0)
				{
					ServerList.Entry entry = mList.list[--num];
					if (entry.data == player)
					{
						mList.list.RemoveAt(num);
						mLastChange = mTime;
						result = true;
					}
				}
				return result;
			}
		}

		public override void AddServer(string name, int playerCount, IPEndPoint internalAddress, IPEndPoint externalAddress)
		{
			mList.Add(name, playerCount, internalAddress, externalAddress, mTime);
			mLastChange = mTime;
		}

		public override void RemoveServer(IPEndPoint internalAddress, IPEndPoint externalAddress)
		{
			if (mList.Remove(internalAddress, externalAddress))
			{
				mLastChange = mTime;
			}
		}
	}
}
