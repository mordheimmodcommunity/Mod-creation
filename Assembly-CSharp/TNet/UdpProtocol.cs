using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace TNet
{
	public class UdpProtocol
	{
		public static bool useMulticasting = true;

		public static IPAddress defaultNetworkInterface = null;

		private int mPort = -1;

		private Socket mSocket;

		private bool mMulticast = true;

		private byte[] mTemp = new byte[8192];

		private EndPoint mEndPoint;

		private static EndPoint mDefaultEndPoint;

		private static IPAddress multicastIP = IPAddress.Parse("224.168.100.17");

		private IPEndPoint mMulticastEndPoint = new IPEndPoint(multicastIP, 0);

		private IPEndPoint mBroadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, 0);

		protected Queue<Datagram> mIn = new Queue<Datagram>();

		protected Queue<Datagram> mOut = new Queue<Datagram>();

		public bool isActive => mPort != -1;

		public int listeningPort => (mPort > 0) ? mPort : 0;

		public bool Start()
		{
			return Start(0);
		}

		public bool Start(int port)
		{
			//Discarded unreachable code: IL_013a
			Stop();
			mPort = port;
			mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			mSocket.MulticastLoopback = true;
			mMulticast = useMulticasting;
			if (useMulticasting)
			{
				List<IPAddress> localAddresses = Tools.localAddresses;
				foreach (IPAddress item in localAddresses)
				{
					MulticastOption optionValue = new MulticastOption(multicastIP, item);
					mSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, optionValue);
				}
			}
			else
			{
				mSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
			}
			if (mPort == 0)
			{
				return true;
			}
			try
			{
				IPAddress address = defaultNetworkInterface ?? IPAddress.Any;
				mEndPoint = new IPEndPoint(address, 0);
				mDefaultEndPoint = new IPEndPoint(address, 0);
				mSocket.Bind(new IPEndPoint(address, mPort));
				mSocket.BeginReceiveFrom(mTemp, 0, mTemp.Length, SocketFlags.None, ref mEndPoint, OnReceive, null);
			}
			catch (Exception)
			{
				Stop();
				return false;
			}
			return true;
		}

		public void Stop()
		{
			mPort = -1;
			if (mSocket != null)
			{
				mSocket.Close();
				mSocket = null;
			}
			Buffer.Recycle(mIn);
			Buffer.Recycle(mOut);
		}

		private void OnReceive(IAsyncResult result)
		{
			if (isActive)
			{
				int num = 0;
				try
				{
					num = mSocket.EndReceiveFrom(result, ref mEndPoint);
				}
				catch (Exception ex)
				{
					Error(new IPEndPoint(Tools.localAddress, 0), ex.Message);
				}
				if (num > 4)
				{
					Buffer buffer = Buffer.Create();
					buffer.BeginWriting(append: false).Write(mTemp, 0, num);
					buffer.BeginReading(4);
					Datagram item = default(Datagram);
					item.buffer = buffer;
					item.ip = (IPEndPoint)mEndPoint;
					lock (mIn)
					{
						mIn.Enqueue(item);
					}
				}
				if (mSocket != null)
				{
					mEndPoint = mDefaultEndPoint;
					mSocket.BeginReceiveFrom(mTemp, 0, mTemp.Length, SocketFlags.None, ref mEndPoint, OnReceive, null);
				}
			}
		}

		public bool ReceivePacket(out Buffer buffer, out IPEndPoint source)
		{
			//Discarded unreachable code: IL_005e
			if (mPort == 0)
			{
				Stop();
				throw new InvalidOperationException("You must specify a non-zero port to UdpProtocol.Start() before you can receive data.");
			}
			if (mIn.Count != 0)
			{
				lock (mIn)
				{
					Datagram datagram = mIn.Dequeue();
					buffer = datagram.buffer;
					source = datagram.ip;
					return true;
				}
			}
			buffer = null;
			source = null;
			return false;
		}

		public void SendEmptyPacket(IPEndPoint ip)
		{
			Buffer buffer = Buffer.Create(markAsUsed: false);
			buffer.BeginPacket(Packet.Empty);
			buffer.EndPacket();
			Send(buffer, ip);
		}

		public void Broadcast(Buffer buffer, int port)
		{
			if (buffer != null)
			{
				buffer.MarkAsUsed();
				IPEndPoint iPEndPoint = (!mMulticast) ? mBroadcastEndPoint : mMulticastEndPoint;
				iPEndPoint.Port = port;
				try
				{
					mSocket.SendTo(buffer.buffer, buffer.position, buffer.size, SocketFlags.None, iPEndPoint);
				}
				catch (Exception ex)
				{
					Error(null, ex.Message);
				}
				buffer.Recycle();
			}
		}

		public void Send(Buffer buffer, IPEndPoint ip)
		{
			if (ip.Address.Equals(IPAddress.Broadcast))
			{
				Broadcast(buffer, ip.Port);
				return;
			}
			buffer.MarkAsUsed();
			if (mSocket != null)
			{
				buffer.BeginReading();
				lock (mOut)
				{
					Datagram item = default(Datagram);
					item.buffer = buffer;
					item.ip = ip;
					mOut.Enqueue(item);
					if (mOut.Count == 1)
					{
						mSocket.BeginSendTo(buffer.buffer, buffer.position, buffer.size, SocketFlags.None, ip, OnSend, null);
					}
				}
				return;
			}
			buffer.Recycle();
			throw new InvalidOperationException("The socket is null. Did you forget to call UdpProtocol.Start()?");
		}

		private void OnSend(IAsyncResult result)
		{
			if (isActive)
			{
				int num = 0;
				try
				{
					num = mSocket.EndSendTo(result);
				}
				catch (Exception ex)
				{
					num = 1;
					Debug.Log("[TNet] " + ex.Message);
				}
				lock (mOut)
				{
					Datagram datagram = mOut.Dequeue();
					datagram.buffer.Recycle();
					if (num > 0 && mSocket != null && mOut.Count != 0)
					{
						Datagram datagram2 = mOut.Peek();
						mSocket.BeginSendTo(datagram2.buffer.buffer, datagram2.buffer.position, datagram2.buffer.size, SocketFlags.None, datagram2.ip, OnSend, null);
					}
				}
			}
		}

		public void Error(IPEndPoint ip, string error)
		{
			Buffer buffer = Buffer.Create();
			buffer.BeginPacket(Packet.Error).Write(error);
			buffer.EndTcpPacketWithOffset(4);
			Datagram item = default(Datagram);
			item.buffer = buffer;
			item.ip = ip;
			lock (mIn)
			{
				mIn.Enqueue(item);
			}
		}
	}
}
