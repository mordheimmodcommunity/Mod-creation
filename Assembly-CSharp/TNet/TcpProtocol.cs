using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TNet
{
	public class TcpProtocol : Player
	{
		public enum Stage
		{
			NotConnected,
			Connecting,
			Verifying,
			Connected
		}

		public Stage stage;

		public IPEndPoint tcpEndPoint;

		public long lastReceivedTime;

		public long timeoutTime = 10000L;

		private Queue<Buffer> mIn = new Queue<Buffer>();

		private Queue<Buffer> mOut = new Queue<Buffer>();

		private byte[] mTemp = new byte[8192];

		private Buffer mReceiveBuffer;

		private int mExpected;

		private int mOffset;

		private Socket mSocket;

		private bool mNoDelay;

		private IPEndPoint mFallback;

		private List<Socket> mConnecting = new List<Socket>();

		private static Buffer mBuffer;

		public bool isConnected => stage == Stage.Connected;

		public bool isTryingToConnect => mConnecting.size != 0;

		public bool noDelay
		{
			get
			{
				return mNoDelay;
			}
			set
			{
				if (mNoDelay != value)
				{
					mNoDelay = value;
					mSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.Debug, mNoDelay);
				}
			}
		}

		public string address => (tcpEndPoint == null) ? "0.0.0.0:0" : tcpEndPoint.ToString();

		public void Connect(IPEndPoint externalIP)
		{
			Connect(externalIP, null);
		}

		public void Connect(IPEndPoint externalIP, IPEndPoint internalIP)
		{
			Disconnect();
			if (internalIP != null && Tools.GetSubnet(Tools.localAddress) == Tools.GetSubnet(internalIP.Address))
			{
				tcpEndPoint = internalIP;
				mFallback = externalIP;
			}
			else
			{
				tcpEndPoint = externalIP;
				mFallback = internalIP;
			}
			ConnectToTcpEndPoint();
		}

		private bool ConnectToTcpEndPoint()
		{
			//Discarded unreachable code: IL_008f
			if (tcpEndPoint != null)
			{
				stage = Stage.Connecting;
				try
				{
					lock (mConnecting)
					{
						mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
						mConnecting.Add(mSocket);
					}
					IAsyncResult parameter = mSocket.BeginConnect(tcpEndPoint, OnConnectResult, mSocket);
					Thread thread = new Thread(CancelConnect);
					thread.Start(parameter);
					return true;
				}
				catch (Exception ex)
				{
					Error(ex.Message);
				}
			}
			else
			{
				Error("Unable to resolve the specified address");
			}
			return false;
		}

		private bool ConnectToFallback()
		{
			tcpEndPoint = mFallback;
			mFallback = null;
			return tcpEndPoint != null && ConnectToTcpEndPoint();
		}

		private void CancelConnect(object obj)
		{
			IAsyncResult asyncResult = (IAsyncResult)obj;
			if (asyncResult != null && !asyncResult.AsyncWaitHandle.WaitOne(3000, exitContext: true))
			{
				try
				{
					Socket socket = (Socket)asyncResult.AsyncState;
					if (socket != null)
					{
						socket.Close();
						lock (mConnecting)
						{
							if (mConnecting.size > 0 && mConnecting[mConnecting.size - 1] == socket)
							{
								mSocket = null;
								if (!ConnectToFallback())
								{
									Error("Unable to connect");
									Close(notify: false);
								}
							}
							mConnecting.Remove(socket);
						}
					}
				}
				catch (Exception)
				{
				}
			}
		}

		private void OnConnectResult(IAsyncResult result)
		{
			Socket socket = (Socket)result.AsyncState;
			if (socket == null)
			{
				return;
			}
			if (mSocket != null && socket == mSocket)
			{
				bool flag = true;
				string error = "Failed to connect";
				try
				{
					socket.EndConnect(result);
				}
				catch (Exception ex)
				{
					if (socket == mSocket)
					{
						mSocket = null;
					}
					socket.Close();
					error = ex.Message;
					flag = false;
				}
				if (flag)
				{
					stage = Stage.Verifying;
					BinaryWriter binaryWriter = BeginSend(Packet.RequestID);
					binaryWriter.Write(11);
					binaryWriter.Write((!string.IsNullOrEmpty(name)) ? name : "Guest");
					binaryWriter.WriteObject(data);
					EndSend();
					StartReceiving();
				}
				else if (!ConnectToFallback())
				{
					Error(error);
					Close(notify: false);
				}
			}
			lock (mConnecting)
			{
				mConnecting.Remove(socket);
			}
		}

		public void Disconnect()
		{
			Disconnect(notify: false);
		}

		public void Disconnect(bool notify)
		{
			if (isConnected)
			{
				try
				{
					lock (mConnecting)
					{
						int num = mConnecting.size;
						while (num > 0)
						{
							Socket socket = mConnecting[--num];
							mConnecting.RemoveAt(num);
							socket?.Close();
						}
					}
					if (mSocket != null)
					{
						Close(notify || mSocket.Connected);
					}
				}
				catch (Exception)
				{
					lock (mConnecting)
					{
						mConnecting.Clear();
					}
					mSocket = null;
				}
			}
		}

		public void Close(bool notify)
		{
			stage = Stage.NotConnected;
			name = "Guest";
			data = null;
			if (mReceiveBuffer != null)
			{
				mReceiveBuffer.Recycle();
				mReceiveBuffer = null;
			}
			if (mSocket != null)
			{
				try
				{
					if (mSocket.Connected)
					{
						mSocket.Shutdown(SocketShutdown.Both);
					}
					mSocket.Close();
				}
				catch (Exception)
				{
				}
				mSocket = null;
				if (notify)
				{
					Buffer buffer = Buffer.Create();
					buffer.BeginPacket(Packet.Disconnect);
					buffer.EndTcpPacketWithOffset(4);
					lock (mIn)
					{
						mIn.Enqueue(buffer);
					}
				}
			}
		}

		public void Release()
		{
			Close(notify: false);
			Buffer.Recycle(mIn);
			Buffer.Recycle(mOut);
		}

		public BinaryWriter BeginSend(Packet type)
		{
			mBuffer = Buffer.Create(markAsUsed: false);
			return mBuffer.BeginPacket(type);
		}

		public BinaryWriter BeginSend(byte packetID)
		{
			mBuffer = Buffer.Create(markAsUsed: false);
			return mBuffer.BeginPacket(packetID);
		}

		public void EndSend()
		{
			mBuffer.EndPacket();
			SendTcpPacket(mBuffer);
			mBuffer = null;
		}

		public void SendTcpPacket(Buffer buffer)
		{
			buffer.MarkAsUsed();
			if (mSocket != null && mSocket.Connected)
			{
				buffer.BeginReading();
				lock (mOut)
				{
					mOut.Enqueue(buffer);
					if (mOut.Count == 1)
					{
						try
						{
							mSocket.BeginSend(buffer.buffer, buffer.position, buffer.size, SocketFlags.None, OnSend, buffer);
						}
						catch (Exception ex)
						{
							Error(ex.Message);
							Close(notify: false);
							Release();
						}
					}
				}
			}
			else
			{
				buffer.Recycle();
			}
		}

		private void OnSend(IAsyncResult result)
		{
			//Discarded unreachable code: IL_0039
			if (stage != 0)
			{
				int num;
				try
				{
					num = mSocket.EndSend(result);
				}
				catch (Exception ex)
				{
					num = 0;
					Close(notify: true);
					Error(ex.Message);
					return;
				}
				lock (mOut)
				{
					mOut.Dequeue().Recycle();
					if (num > 0 && mSocket != null && mSocket.Connected)
					{
						Buffer buffer = (mOut.Count != 0) ? mOut.Peek() : null;
						if (buffer != null)
						{
							try
							{
								mSocket.BeginSend(buffer.buffer, buffer.position, buffer.size, SocketFlags.None, OnSend, buffer);
							}
							catch (Exception ex2)
							{
								Error(ex2.Message);
								Close(notify: false);
							}
						}
					}
					else
					{
						Close(notify: true);
					}
				}
			}
		}

		public void StartReceiving()
		{
			StartReceiving(null);
		}

		public void StartReceiving(Socket socket)
		{
			if (socket != null)
			{
				Close(notify: false);
				mSocket = socket;
			}
			if (mSocket != null && mSocket.Connected)
			{
				stage = Stage.Verifying;
				lastReceivedTime = DateTime.Now.Ticks / 10000;
				tcpEndPoint = (IPEndPoint)mSocket.RemoteEndPoint;
				try
				{
					mSocket.BeginReceive(mTemp, 0, mTemp.Length, SocketFlags.None, OnReceive, null);
				}
				catch (Exception ex)
				{
					Error(ex.Message);
					Disconnect(notify: true);
				}
			}
		}

		public bool ReceivePacket(out Buffer buffer)
		{
			//Discarded unreachable code: IL_0031
			if (mIn.Count != 0)
			{
				lock (mIn)
				{
					buffer = mIn.Dequeue();
					return true;
				}
			}
			buffer = null;
			return false;
		}

		private void OnReceive(IAsyncResult result)
		{
			//Discarded unreachable code: IL_0039
			if (stage == Stage.NotConnected)
			{
				return;
			}
			int num = 0;
			try
			{
				num = mSocket.EndReceive(result);
			}
			catch (Exception ex)
			{
				Error(ex.Message);
				Disconnect(notify: true);
				return;
			}
			lastReceivedTime = DateTime.Now.Ticks / 10000;
			if (num == 0)
			{
				Close(notify: true);
			}
			else if (ProcessBuffer(num))
			{
				if (stage != 0)
				{
					try
					{
						mSocket.BeginReceive(mTemp, 0, mTemp.Length, SocketFlags.None, OnReceive, null);
					}
					catch (Exception ex2)
					{
						Error(ex2.Message);
						Close(notify: false);
					}
				}
			}
			else
			{
				Close(notify: true);
			}
		}

		private bool ProcessBuffer(int bytes)
		{
			if (mReceiveBuffer == null)
			{
				mReceiveBuffer = Buffer.Create();
				mReceiveBuffer.BeginWriting(append: false).Write(mTemp, 0, bytes);
			}
			else
			{
				mReceiveBuffer.BeginWriting(append: true).Write(mTemp, 0, bytes);
			}
			int num = mReceiveBuffer.size - mOffset;
			while (num >= 4)
			{
				if (mExpected == 0)
				{
					mExpected = mReceiveBuffer.PeekInt(mOffset);
					if (mExpected < 0 || mExpected > 16777216)
					{
						Close(notify: true);
						return false;
					}
				}
				num -= 4;
				if (num == mExpected)
				{
					mReceiveBuffer.BeginReading(mOffset + 4);
					lock (mIn)
					{
						mIn.Enqueue(mReceiveBuffer);
					}
					mReceiveBuffer = null;
					mExpected = 0;
					mOffset = 0;
					break;
				}
				if (num > mExpected)
				{
					int num2 = mExpected + 4;
					Buffer buffer = Buffer.Create();
					buffer.BeginWriting(append: false).Write(mReceiveBuffer.buffer, mOffset, num2);
					buffer.BeginReading(4);
					lock (mIn)
					{
						mIn.Enqueue(buffer);
					}
					num -= mExpected;
					mOffset += num2;
					mExpected = 0;
					continue;
				}
				break;
			}
			return true;
		}

		public void Error(string error)
		{
			Error(Buffer.Create(), error);
		}

		private void Error(Buffer buffer, string error)
		{
			buffer.BeginPacket(Packet.Error).Write(error);
			buffer.EndTcpPacketWithOffset(4);
			lock (mIn)
			{
				mIn.Enqueue(buffer);
			}
		}

		public bool VerifyRequestID(Buffer buffer, bool uniqueID)
		{
			BinaryReader binaryReader = buffer.BeginReading();
			Packet packet = (Packet)binaryReader.ReadByte();
			if (packet == Packet.RequestID)
			{
				if (binaryReader.ReadInt32() == 11)
				{
					id = (uniqueID ? Interlocked.Increment(ref Player.mPlayerCounter) : 0);
					name = binaryReader.ReadString();
					if (buffer.size > 1)
					{
						data = binaryReader.ReadObject();
					}
					else
					{
						data = null;
					}
					stage = Stage.Connected;
					BinaryWriter binaryWriter = BeginSend(Packet.ResponseID);
					binaryWriter.Write(11);
					binaryWriter.Write(id);
					EndSend();
					return true;
				}
				BinaryWriter binaryWriter2 = BeginSend(Packet.ResponseID);
				binaryWriter2.Write(11);
				binaryWriter2.Write(0);
				EndSend();
				Close(notify: false);
			}
			return false;
		}

		public bool VerifyResponseID(Packet packet, BinaryReader reader)
		{
			if (packet == Packet.ResponseID)
			{
				int num = reader.ReadInt32();
				if (num == 11)
				{
					id = reader.ReadInt32();
					stage = Stage.Connected;
					return true;
				}
				id = 0;
				Error("Version mismatch! Server is running protocol version " + num + " while you are on version " + 11);
				Close(notify: false);
				return false;
			}
			Error("Expected a response ID, got " + packet);
			Close(notify: false);
			return false;
		}
	}
}
