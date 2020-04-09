using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace TNet
{
	public class GameClient
	{
		public delegate void OnPing(IPEndPoint ip, int milliSeconds);

		public delegate void OnError(string message);

		public delegate void OnConnect(bool success, string message);

		public delegate void OnDisconnect();

		public delegate void OnJoinChannel(bool success, string message);

		public delegate void OnLeftChannel();

		public delegate void OnLoadLevel(string levelName);

		public delegate void OnPlayerJoined(Player p);

		public delegate void OnPlayerLeft(Player p);

		public delegate void OnPlayerSync(Player p);

		public delegate void OnRenamePlayer(Player p, string previous);

		public delegate void OnSetHost(bool hosting);

		public delegate void OnSetChannelData(string data);

		public delegate void OnCreate(int creator, int index, uint objID, BinaryReader reader);

		public delegate void OnDestroy(uint objID);

		public delegate void OnForwardedPacket(BinaryReader reader);

		public delegate void OnPacket(Packet response, BinaryReader reader, IPEndPoint source);

		public delegate void OnLoadFile(string filename, byte[] data);

		public Dictionary<byte, OnPacket> packetHandlers = new Dictionary<byte, OnPacket>();

		public OnPing onPing;

		public OnError onError;

		public OnConnect onConnect;

		public OnDisconnect onDisconnect;

		public OnJoinChannel onJoinChannel;

		public OnLeftChannel onLeftChannel;

		public OnLoadLevel onLoadLevel;

		public OnPlayerJoined onPlayerJoined;

		public OnPlayerLeft onPlayerLeft;

		public OnPlayerSync onPlayerSync;

		public OnRenamePlayer onRenamePlayer;

		public OnSetHost onSetHost;

		public OnSetChannelData onSetChannelData;

		public OnCreate onCreate;

		public OnDestroy onDestroy;

		public OnForwardedPacket onForwardedPacket;

		public List<Player> players = new List<Player>();

		public bool isActive = true;

		private Dictionary<int, Player> mDictionary = new Dictionary<int, Player>();

		private TcpProtocol mTcp = new TcpProtocol();

		private UdpProtocol mUdp = new UdpProtocol();

		private bool mUdpIsUsable;

		private int mHost;

		private int mChannelID;

		private long mTime;

		private long mPingTime;

		private int mPing;

		private bool mCanPing;

		private bool mIsInChannel;

		private string mData = string.Empty;

		private List<OnLoadFile> mLoadFiles = new List<OnLoadFile>();

		private IPEndPoint mServerUdpEndPoint;

		private IPEndPoint mPacketSource;

		private static Buffer mBuffer;

		public int channelID => mChannelID;

		public int hostID => (!mTcp.isConnected) ? mTcp.id : mHost;

		public bool isConnected => mTcp.isConnected;

		public bool isTryingToConnect => mTcp.isTryingToConnect;

		public bool isHosting => !mIsInChannel || mHost == mTcp.id;

		public bool isInChannel => mIsInChannel;

		public int listeningPort => mUdp.listeningPort;

		public IPEndPoint packetSource => (mPacketSource == null) ? mTcp.tcpEndPoint : mPacketSource;

		public string channelData
		{
			get
			{
				return (!isInChannel) ? string.Empty : mData;
			}
			set
			{
				if (isHosting && isInChannel && !mData.Equals(value))
				{
					mData = value;
					BeginSend(Packet.RequestSetChannelData).Write(value);
					EndSend();
				}
			}
		}

		public bool noDelay
		{
			get
			{
				return mTcp.noDelay;
			}
			set
			{
				if (mTcp.noDelay != value)
				{
					mTcp.noDelay = value;
					BeginSend(Packet.RequestNoDelay).Write(value);
					EndSend();
				}
			}
		}

		public int ping => isConnected ? mPing : 0;

		public bool canUseUDP => mUdp.isActive && mServerUdpEndPoint != null;

		public Player player => mTcp;

		public int playerID => mTcp.id;

		public string playerName
		{
			get
			{
				return mTcp.name;
			}
			set
			{
				if (mTcp.name != value)
				{
					if (isConnected)
					{
						BinaryWriter binaryWriter = BeginSend(Packet.RequestSetName);
						binaryWriter.Write(value);
						EndSend();
					}
					else
					{
						mTcp.name = value;
					}
				}
			}
		}

		public object playerData
		{
			get
			{
				return mTcp.data;
			}
			set
			{
				mTcp.data = value;
				SyncPlayerData();
			}
		}

		public void SyncPlayerData()
		{
			if (isConnected)
			{
				BinaryWriter binaryWriter = BeginSend(Packet.SyncPlayerData);
				binaryWriter.Write(mTcp.id);
				binaryWriter.WriteObject(mTcp.data);
				EndSend();
			}
		}

		public Player GetPlayer(int id)
		{
			if (id == mTcp.id)
			{
				return mTcp;
			}
			if (isConnected)
			{
				Player value = null;
				mDictionary.TryGetValue(id, out value);
				return value;
			}
			return null;
		}

		public BinaryWriter BeginSend(Packet type)
		{
			mBuffer = Buffer.Create();
			return mBuffer.BeginPacket(type);
		}

		public BinaryWriter BeginSend(byte packetID)
		{
			mBuffer = Buffer.Create();
			return mBuffer.BeginPacket(packetID);
		}

		public void EndSend()
		{
			if (mBuffer != null)
			{
				mBuffer.EndPacket();
				mTcp.SendTcpPacket(mBuffer);
				mBuffer.Recycle();
				mBuffer = null;
			}
		}

		public void EndSend(bool reliable)
		{
			mBuffer.EndPacket();
			if (reliable || !mUdpIsUsable || mServerUdpEndPoint == null || !mUdp.isActive)
			{
				mTcp.SendTcpPacket(mBuffer);
			}
			else
			{
				mUdp.Send(mBuffer, mServerUdpEndPoint);
			}
			mBuffer.Recycle();
			mBuffer = null;
		}

		public void EndSend(int port)
		{
			mBuffer.EndPacket();
			mUdp.Broadcast(mBuffer, port);
			mBuffer.Recycle();
			mBuffer = null;
		}

		public void EndSend(IPEndPoint target)
		{
			mBuffer.EndPacket();
			mUdp.Send(mBuffer, target);
			mBuffer.Recycle();
			mBuffer = null;
		}

		public void Connect(IPEndPoint externalIP, IPEndPoint internalIP)
		{
			Disconnect();
			mTcp.Connect(externalIP, internalIP);
		}

		public void Disconnect()
		{
			mTcp.Disconnect();
		}

		public bool StartUDP(int udpPort)
		{
			if (mUdp.Start(udpPort))
			{
				if (isConnected)
				{
					BeginSend(Packet.RequestSetUDP).Write((ushort)udpPort);
					EndSend();
				}
				return true;
			}
			return false;
		}

		public void StopUDP()
		{
			if (mUdp.isActive)
			{
				if (isConnected)
				{
					BeginSend(Packet.RequestSetUDP).Write((ushort)0);
					EndSend();
				}
				mUdp.Stop();
				mUdpIsUsable = false;
			}
		}

		public void JoinChannel(int channelID, string levelName, bool persistent, int playerLimit, string password)
		{
			if (isConnected)
			{
				BinaryWriter binaryWriter = BeginSend(Packet.RequestJoinChannel);
				binaryWriter.Write(channelID);
				binaryWriter.Write((!string.IsNullOrEmpty(password)) ? password : string.Empty);
				binaryWriter.Write((!string.IsNullOrEmpty(levelName)) ? levelName : string.Empty);
				binaryWriter.Write(persistent);
				binaryWriter.Write((ushort)playerLimit);
				EndSend();
			}
		}

		public void CloseChannel()
		{
			if (isConnected && isInChannel)
			{
				BeginSend(Packet.RequestCloseChannel);
				EndSend();
			}
		}

		public void LeaveChannel()
		{
			if (isConnected && isInChannel)
			{
				BeginSend(Packet.RequestLeaveChannel);
				EndSend();
			}
		}

		public void SetPlayerLimit(int max)
		{
			if (isConnected && isInChannel)
			{
				BeginSend(Packet.RequestSetPlayerLimit).Write((ushort)max);
				EndSend();
			}
		}

		public void LoadLevel(string levelName)
		{
			if (isConnected && isInChannel)
			{
				BeginSend(Packet.RequestLoadLevel).Write(levelName);
				EndSend();
			}
		}

		public void SetHost(Player player)
		{
			if (isConnected && isInChannel && isHosting)
			{
				BinaryWriter binaryWriter = BeginSend(Packet.RequestSetHost);
				binaryWriter.Write(player.id);
				EndSend();
			}
		}

		public void SetTimeout(int seconds)
		{
			if (isConnected)
			{
				BeginSend(Packet.RequestSetTimeout).Write(seconds);
				EndSend();
			}
		}

		public void Ping(IPEndPoint udpEndPoint, OnPing callback)
		{
			onPing = callback;
			mPingTime = DateTime.Now.Ticks / 10000;
			BeginSend(Packet.RequestPing);
			EndSend(udpEndPoint);
		}

		public void LoadFile(string filename, OnLoadFile callback)
		{
			mLoadFiles.Add(callback);
			BinaryWriter binaryWriter = BeginSend(Packet.RequestLoadFile);
			binaryWriter.Write(filename);
			EndSend();
		}

		public void SaveFile(string filename, byte[] data)
		{
			BinaryWriter binaryWriter = BeginSend(Packet.RequestSaveFile);
			binaryWriter.Write(filename);
			binaryWriter.Write(data.Length);
			binaryWriter.Write(data);
			EndSend();
		}

		public void ProcessPackets()
		{
			mTime = DateTime.Now.Ticks / 10000;
			if (mTcp.isConnected && mCanPing && mPingTime + 4000 < mTime)
			{
				mCanPing = false;
				mPingTime = mTime;
				BeginSend(Packet.RequestPing);
				EndSend();
			}
			Buffer buffer = null;
			bool flag = true;
			IPEndPoint source = null;
			while (flag && isActive && mUdp.ReceivePacket(out buffer, out source))
			{
				mUdpIsUsable = true;
				flag = ProcessPacket(buffer, source);
				buffer.Recycle();
			}
			while (flag && isActive && mTcp.ReceivePacket(out buffer))
			{
				flag = ProcessPacket(buffer, null);
				buffer.Recycle();
			}
		}

		private bool ProcessPacket(Buffer buffer, IPEndPoint ip)
		{
			mPacketSource = ip;
			BinaryReader binaryReader = buffer.BeginReading();
			if (buffer.size == 0)
			{
				return true;
			}
			int num = binaryReader.ReadByte();
			Packet packet = (Packet)num;
			if (packet == Packet.ResponseID || mTcp.stage == TcpProtocol.Stage.Verifying)
			{
				if (mTcp.VerifyResponseID(packet, binaryReader))
				{
					if (mUdp.isActive)
					{
						BeginSend(Packet.RequestSetUDP).Write((ushort)mUdp.listeningPort);
						EndSend();
					}
					mCanPing = true;
					if (onConnect != null)
					{
						onConnect(success: true, null);
					}
				}
				return true;
			}
			if (packetHandlers.TryGetValue((byte)packet, out OnPacket value) && value != null)
			{
				value(packet, binaryReader, ip);
				return true;
			}
			switch (packet)
			{
			case Packet.ForwardToAll:
			case Packet.ForwardToAllSaved:
			case Packet.ForwardToOthers:
			case Packet.ForwardToOthersSaved:
			case Packet.ForwardToHost:
			case Packet.Broadcast:
				if (onForwardedPacket != null)
				{
					onForwardedPacket(binaryReader);
				}
				break;
			case Packet.ForwardToPlayer:
				binaryReader.ReadInt32();
				if (onForwardedPacket != null)
				{
					onForwardedPacket(binaryReader);
				}
				break;
			case Packet.SyncPlayerData:
			{
				Player player3 = GetPlayer(binaryReader.ReadInt32());
				if (player3 != null)
				{
					player3.data = binaryReader.ReadObject();
					if (onPlayerSync != null)
					{
						onPlayerSync(player3);
					}
				}
				break;
			}
			case Packet.ResponsePing:
			{
				int milliSeconds = (int)(mTime - mPingTime);
				if (ip != null)
				{
					if (onPing != null && ip != null)
					{
						onPing(ip, milliSeconds);
					}
				}
				else
				{
					mCanPing = true;
					mPing = milliSeconds;
				}
				break;
			}
			case Packet.ResponseSetUDP:
			{
				ushort num3 = binaryReader.ReadUInt16();
				if (num3 != 0)
				{
					IPAddress address = new IPAddress(mTcp.tcpEndPoint.Address.GetAddressBytes());
					mServerUdpEndPoint = new IPEndPoint(address, num3);
					if (mUdp.isActive)
					{
						mBuffer = Buffer.Create();
						mBuffer.BeginPacket(Packet.RequestActivateUDP).Write(playerID);
						mBuffer.EndPacket();
						mUdp.Send(mBuffer, mServerUdpEndPoint);
						mBuffer.Recycle();
						mBuffer = null;
					}
				}
				else
				{
					mServerUdpEndPoint = null;
				}
				break;
			}
			case Packet.ResponseJoiningChannel:
			{
				mIsInChannel = true;
				mDictionary.Clear();
				players.Clear();
				mChannelID = binaryReader.ReadInt32();
				int num2 = binaryReader.ReadInt16();
				for (int i = 0; i < num2; i++)
				{
					Player player2 = new Player();
					player2.id = binaryReader.ReadInt32();
					player2.name = binaryReader.ReadString();
					player2.data = binaryReader.ReadObject();
					mDictionary.Add(player2.id, player2);
					players.Add(player2);
				}
				break;
			}
			case Packet.ResponseLoadLevel:
				if (onLoadLevel != null)
				{
					onLoadLevel(binaryReader.ReadString());
				}
				return false;
			case Packet.ResponsePlayerLeft:
			{
				Player player = GetPlayer(binaryReader.ReadInt32());
				if (player != null)
				{
					mDictionary.Remove(player.id);
				}
				players.Remove(player);
				if (onPlayerLeft != null)
				{
					onPlayerLeft(player);
				}
				break;
			}
			case Packet.ResponsePlayerJoined:
			{
				Player player5 = new Player();
				player5.id = binaryReader.ReadInt32();
				player5.name = binaryReader.ReadString();
				player5.data = binaryReader.ReadObject();
				mDictionary.Add(player5.id, player5);
				players.Add(player5);
				if (onPlayerJoined != null)
				{
					onPlayerJoined(player5);
				}
				break;
			}
			case Packet.ResponseSetHost:
				mHost = binaryReader.ReadInt32();
				if (onSetHost != null)
				{
					onSetHost(isHosting);
				}
				break;
			case Packet.ResponseSetChannelData:
				mData = binaryReader.ReadString();
				if (onSetChannelData != null)
				{
					onSetChannelData(mData);
				}
				break;
			case Packet.ResponseJoinChannel:
				mIsInChannel = binaryReader.ReadBoolean();
				if (onJoinChannel != null)
				{
					onJoinChannel(mIsInChannel, (!mIsInChannel) ? binaryReader.ReadString() : null);
				}
				break;
			case Packet.ResponseLeaveChannel:
				mData = string.Empty;
				mChannelID = 0;
				mIsInChannel = false;
				mDictionary.Clear();
				players.Clear();
				if (onLeftChannel != null)
				{
					onLeftChannel();
				}
				break;
			case Packet.ResponseRenamePlayer:
			{
				Player player4 = GetPlayer(binaryReader.ReadInt32());
				string name = player4.name;
				if (player4 != null)
				{
					player4.name = binaryReader.ReadString();
				}
				if (onRenamePlayer != null)
				{
					onRenamePlayer(player4, name);
				}
				break;
			}
			case Packet.ResponseCreate:
				if (onCreate != null)
				{
					int creator = binaryReader.ReadInt32();
					ushort index = binaryReader.ReadUInt16();
					uint objID = binaryReader.ReadUInt32();
					onCreate(creator, index, objID, binaryReader);
				}
				break;
			case Packet.ResponseDestroy:
				if (onDestroy != null)
				{
					int num4 = binaryReader.ReadUInt16();
					for (int j = 0; j < num4; j++)
					{
						onDestroy(binaryReader.ReadUInt32());
					}
				}
				break;
			case Packet.Error:
				if (mTcp.stage != TcpProtocol.Stage.Connected && onConnect != null)
				{
					onConnect(success: false, binaryReader.ReadString());
				}
				else if (onError != null)
				{
					onError(binaryReader.ReadString());
				}
				break;
			case Packet.Disconnect:
				mData = string.Empty;
				if (isInChannel && onLeftChannel != null)
				{
					onLeftChannel();
				}
				players.Clear();
				mDictionary.Clear();
				mTcp.Close(notify: false);
				mLoadFiles.Clear();
				if (onDisconnect != null)
				{
					onDisconnect();
				}
				break;
			case Packet.ResponseLoadFile:
			{
				string filename = binaryReader.ReadString();
				int count = binaryReader.ReadInt32();
				byte[] data = binaryReader.ReadBytes(count);
				mLoadFiles.Pop()?.Invoke(filename, data);
				break;
			}
			}
			return true;
		}
	}
}
