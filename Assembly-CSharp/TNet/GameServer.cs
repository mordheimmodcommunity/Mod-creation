using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TNet
{
    public class GameServer : FileServer
    {
        public delegate void OnCustomPacket(TcpPlayer player, Buffer buffer, BinaryReader reader, Packet request, bool reliable);

        public delegate void OnPlayerAction(Player p);

        public delegate void OnShutdown();

        public const ushort gameID = 1;

        public OnCustomPacket onCustomPacket;

        public OnPlayerAction onPlayerConnect;

        public OnPlayerAction onPlayerDisconnect;

        public OnShutdown onShutdown;

        public string name = "Game Server";

        public LobbyServerLink lobbyLink;

        private List<TcpPlayer> mPlayers = new List<TcpPlayer>();

        private Dictionary<int, TcpPlayer> mDictionaryID = new Dictionary<int, TcpPlayer>();

        private Dictionary<IPEndPoint, TcpPlayer> mDictionaryEP = new Dictionary<IPEndPoint, TcpPlayer>();

        private List<Channel> mChannels = new List<Channel>();

        private Random mRandom = new Random();

        private Buffer mBuffer;

        private TcpListener mListener;

        private Thread mThread;

        private int mListenerPort;

        private long mTime;

        private UdpProtocol mUdp = new UdpProtocol();

        private bool mAllowUdp;

        private List<uint> mTemp = new List<uint>();

        public bool isActive => mThread != null;

        public bool isListening => mListener != null;

        public int tcpPort => (mListener != null) ? mListenerPort : 0;

        public int udpPort => mUdp.listeningPort;

        public int playerCount => isActive ? mPlayers.size : 0;

        public bool Start(int tcpPort)
        {
            return Start(tcpPort, 0);
        }

        public bool Start(int tcpPort, int udpPort)
        {
            //Discarded unreachable code: IL_0044
            Stop();
            try
            {
                mListenerPort = tcpPort;
                mListener = new TcpListener(IPAddress.Any, tcpPort);
                mListener.Start(50);
            }
            catch (Exception ex)
            {
                Error(ex.Message);
                return false;
            }
            if (!mUdp.Start(udpPort))
            {
                Error("Unable to listen to UDP port " + udpPort);
                Stop();
                return false;
            }
            mAllowUdp = (udpPort > 0);
            if (lobbyLink != null)
            {
                lobbyLink.Start();
                lobbyLink.SendUpdate(this);
            }
            mThread = new Thread(ThreadFunction);
            mThread.Start();
            return true;
        }

        public void Stop()
        {
            if (lobbyLink != null)
            {
                lobbyLink.Stop();
            }
            mAllowUdp = false;
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
            mUdp.Stop();
            int num = mPlayers.size;
            while (num > 0)
            {
                RemovePlayer(mPlayers[--num]);
            }
            mChannels.Clear();
        }

        public void MakePrivate()
        {
            mListenerPort = 0;
        }

        private void ThreadFunction()
        {
            while (true)
            {
                bool flag = false;
                mTime = DateTime.Now.Ticks / 10000;
                if (mListenerPort == 0)
                {
                    if (mListener != null)
                    {
                        mListener.Stop();
                        mListener = null;
                        if (lobbyLink != null)
                        {
                            lobbyLink.Stop();
                        }
                        if (onShutdown != null)
                        {
                            onShutdown();
                        }
                    }
                }
                else
                {
                    while (mListener != null && mListener.Pending())
                    {
                        AddPlayer(mListener.AcceptSocket());
                    }
                }
                Buffer buffer;
                IPEndPoint source;
                while (mUdp.listeningPort != 0 && mUdp.ReceivePacket(out buffer, out source))
                {
                    if (buffer.size > 0)
                    {
                        TcpPlayer player = GetPlayer(source);
                        if (player != null)
                        {
                            if (!player.udpIsUsable)
                            {
                                player.udpIsUsable = true;
                            }
                            try
                            {
                                if (ProcessPlayerPacket(buffer, player, reliable: false))
                                {
                                    flag = true;
                                }
                            }
                            catch (Exception ex)
                            {
                                Error(ex.Message);
                                RemovePlayer(player);
                            }
                        }
                        else if (buffer.size > 4)
                        {
                            try
                            {
                                BinaryReader binaryReader = buffer.BeginReading();
                                switch (binaryReader.ReadByte())
                                {
                                    case 51:
                                        {
                                            int id = binaryReader.ReadInt32();
                                            player = GetPlayer(id);
                                            if (player != null && player.udpEndPoint != null && player.udpEndPoint.Address == source.Address)
                                            {
                                                player.udpEndPoint = source;
                                                player.udpIsUsable = true;
                                                mUdp.SendEmptyPacket(player.udpEndPoint);
                                            }
                                            break;
                                        }
                                    case 4:
                                        BeginSend(Packet.ResponsePing);
                                        EndSend(source);
                                        break;
                                }
                            }
                            catch (Exception ex2)
                            {
                                Error(ex2.Message);
                                RemovePlayer(player);
                            }
                        }
                    }
                    buffer.Recycle();
                }
                int num = 0;
                while (num < mPlayers.size)
                {
                    TcpPlayer tcpPlayer = mPlayers[num];
                    for (int i = 0; i < 100; i++)
                    {
                        if (!tcpPlayer.ReceivePacket(out buffer))
                        {
                            break;
                        }
                        if (buffer.size > 0)
                        {
                            try
                            {
                                if (ProcessPlayerPacket(buffer, tcpPlayer, reliable: true))
                                {
                                    flag = true;
                                }
                            }
                            catch (Exception ex3)
                            {
                                Error(ex3.Message);
                                RemovePlayer(tcpPlayer);
                            }
                        }
                        buffer.Recycle();
                    }
                    if (tcpPlayer.stage == TcpProtocol.Stage.Connected)
                    {
                        if (tcpPlayer.timeoutTime > 0 && tcpPlayer.lastReceivedTime + tcpPlayer.timeoutTime < mTime)
                        {
                            RemovePlayer(tcpPlayer);
                            continue;
                        }
                    }
                    else if (tcpPlayer.lastReceivedTime + 2000 < mTime)
                    {
                        RemovePlayer(tcpPlayer);
                        continue;
                    }
                    num++;
                }
                if (!flag)
                {
                    Thread.Sleep(1);
                }
            }
        }

        private void Error(TcpPlayer p, string error)
        {
        }

        private TcpPlayer AddPlayer(Socket socket)
        {
            TcpPlayer tcpPlayer = new TcpPlayer();
            tcpPlayer.StartReceiving(socket);
            mPlayers.Add(tcpPlayer);
            return tcpPlayer;
        }

        private void RemovePlayer(TcpPlayer p)
        {
            if (p == null)
            {
                return;
            }
            SendLeaveChannel(p, notify: false);
            p.Release();
            mPlayers.Remove(p);
            if (p.udpEndPoint != null)
            {
                mDictionaryEP.Remove(p.udpEndPoint);
                p.udpEndPoint = null;
                p.udpIsUsable = false;
            }
            if (p.id == 0)
            {
                return;
            }
            if (mDictionaryID.Remove(p.id))
            {
                if (lobbyLink != null)
                {
                    lobbyLink.SendUpdate(this);
                }
                if (onPlayerDisconnect != null)
                {
                    onPlayerDisconnect(p);
                }
            }
            p.id = 0;
        }

        private TcpPlayer GetPlayer(int id)
        {
            TcpPlayer value = null;
            mDictionaryID.TryGetValue(id, out value);
            return value;
        }

        private TcpPlayer GetPlayer(IPEndPoint ip)
        {
            TcpPlayer value = null;
            mDictionaryEP.TryGetValue(ip, out value);
            return value;
        }

        private void SetPlayerUdpEndPoint(TcpPlayer player, IPEndPoint udp)
        {
            if (player.udpEndPoint != null)
            {
                mDictionaryEP.Remove(player.udpEndPoint);
            }
            player.udpEndPoint = udp;
            player.udpIsUsable = false;
            if (udp != null)
            {
                mDictionaryEP[udp] = player;
            }
        }

        private Channel CreateChannel(int channelID, out bool isNew)
        {
            Channel channel;
            for (int i = 0; i < mChannels.size; i++)
            {
                channel = mChannels[i];
                if (channel.id == channelID)
                {
                    isNew = false;
                    if (channel.closed)
                    {
                        return null;
                    }
                    return channel;
                }
            }
            channel = new Channel();
            channel.id = channelID;
            mChannels.Add(channel);
            isNew = true;
            return channel;
        }

        private bool ChannelExists(int id)
        {
            for (int i = 0; i < mChannels.size; i++)
            {
                if (mChannels[i].id == id)
                {
                    return true;
                }
            }
            return false;
        }

        private BinaryWriter BeginSend(Packet type)
        {
            mBuffer = Buffer.Create();
            return mBuffer.BeginPacket(type);
        }

        private void EndSend(IPEndPoint ip)
        {
            mBuffer.EndPacket();
            mUdp.Send(mBuffer, ip);
            mBuffer.Recycle();
            mBuffer = null;
        }

        private void EndSend(bool reliable, TcpPlayer player)
        {
            mBuffer.EndPacket();
            if (mBuffer.size > 1024)
            {
                reliable = true;
            }
            if (reliable || !player.udpIsUsable || player.udpEndPoint == null || !mAllowUdp)
            {
                player.SendTcpPacket(mBuffer);
            }
            else
            {
                mUdp.Send(mBuffer, player.udpEndPoint);
            }
            mBuffer.Recycle();
            mBuffer = null;
        }

        private void EndSend(bool reliable, Channel channel, TcpPlayer exclude)
        {
            mBuffer.EndPacket();
            if (mBuffer.size > 1024)
            {
                reliable = true;
            }
            for (int i = 0; i < channel.players.size; i++)
            {
                TcpPlayer tcpPlayer = channel.players[i];
                if (tcpPlayer.stage == TcpProtocol.Stage.Connected && tcpPlayer != exclude)
                {
                    if (reliable || !tcpPlayer.udpIsUsable || tcpPlayer.udpEndPoint == null || !mAllowUdp)
                    {
                        tcpPlayer.SendTcpPacket(mBuffer);
                    }
                    else
                    {
                        mUdp.Send(mBuffer, tcpPlayer.udpEndPoint);
                    }
                }
            }
            mBuffer.Recycle();
            mBuffer = null;
        }

        private void EndSend(bool reliable)
        {
            mBuffer.EndPacket();
            if (mBuffer.size > 1024)
            {
                reliable = true;
            }
            for (int i = 0; i < mChannels.size; i++)
            {
                Channel channel = mChannels[i];
                for (int j = 0; j < channel.players.size; j++)
                {
                    TcpPlayer tcpPlayer = channel.players[j];
                    if (tcpPlayer.stage == TcpProtocol.Stage.Connected)
                    {
                        if (reliable || !tcpPlayer.udpIsUsable || tcpPlayer.udpEndPoint == null || !mAllowUdp)
                        {
                            tcpPlayer.SendTcpPacket(mBuffer);
                        }
                        else
                        {
                            mUdp.Send(mBuffer, tcpPlayer.udpEndPoint);
                        }
                    }
                }
            }
            mBuffer.Recycle();
            mBuffer = null;
        }

        private void SendToChannel(bool reliable, Channel channel, Buffer buffer)
        {
            mBuffer.MarkAsUsed();
            if (mBuffer.size > 1024)
            {
                reliable = true;
            }
            for (int i = 0; i < channel.players.size; i++)
            {
                TcpPlayer tcpPlayer = channel.players[i];
                if (tcpPlayer.stage == TcpProtocol.Stage.Connected)
                {
                    if (reliable || !tcpPlayer.udpIsUsable || tcpPlayer.udpEndPoint == null || !mAllowUdp)
                    {
                        tcpPlayer.SendTcpPacket(mBuffer);
                    }
                    else
                    {
                        mUdp.Send(mBuffer, tcpPlayer.udpEndPoint);
                    }
                }
            }
            mBuffer.Recycle();
        }

        private void SendSetHost(TcpPlayer player)
        {
            if (player.channel != null && player.channel.host != player)
            {
                player.channel.host = player;
                BinaryWriter binaryWriter = BeginSend(Packet.ResponseSetHost);
                binaryWriter.Write(player.id);
                EndSend(reliable: true, player.channel, null);
            }
        }

        private void SendLeaveChannel(TcpPlayer player, bool notify)
        {
            Channel channel = player.channel;
            if (channel == null)
            {
                return;
            }
            channel.RemovePlayer(player, mTemp);
            player.channel = null;
            if (channel.players.size > 0)
            {
                BinaryWriter binaryWriter;
                if (mTemp.size > 0)
                {
                    binaryWriter = BeginSend(Packet.ResponseDestroy);
                    binaryWriter.Write((ushort)mTemp.size);
                    for (int i = 0; i < mTemp.size; i++)
                    {
                        binaryWriter.Write(mTemp[i]);
                    }
                    EndSend(reliable: true, channel, null);
                }
                if (channel.host == null)
                {
                    SendSetHost(channel.players[0]);
                }
                binaryWriter = BeginSend(Packet.ResponsePlayerLeft);
                binaryWriter.Write(player.id);
                EndSend(reliable: true, channel, null);
            }
            else if (!channel.persistent)
            {
                mChannels.Remove(channel);
            }
            if (notify && player.isConnected)
            {
                BeginSend(Packet.ResponseLeaveChannel);
                EndSend(reliable: true, player);
            }
        }

        private void SendJoinChannel(TcpPlayer player, Channel channel)
        {
            if (player.channel == null || player.channel != channel)
            {
                player.channel = channel;
                player.FinishJoiningChannel();
                BinaryWriter binaryWriter = BeginSend(Packet.ResponsePlayerJoined);
                binaryWriter.Write(player.id);
                binaryWriter.Write((!string.IsNullOrEmpty(player.name)) ? player.name : "Guest");
                binaryWriter.WriteObject(player.data);
                EndSend(reliable: true, channel, null);
                channel.players.Add(player);
            }
        }

        private bool ProcessPlayerPacket(Buffer buffer, TcpPlayer player, bool reliable)
        {
            if (player.stage == TcpProtocol.Stage.Verifying)
            {
                if (player.VerifyRequestID(buffer, uniqueID: true))
                {
                    mDictionaryID.Add(player.id, player);
                    if (lobbyLink != null)
                    {
                        lobbyLink.SendUpdate(this);
                    }
                    if (onPlayerConnect != null)
                    {
                        onPlayerConnect(player);
                    }
                    return true;
                }
                RemovePlayer(player);
                return false;
            }
            BinaryReader binaryReader = buffer.BeginReading();
            Packet packet = (Packet)binaryReader.ReadByte();
            switch (packet)
            {
                case Packet.Error:
                    Error(player, binaryReader.ReadString());
                    break;
                case Packet.Disconnect:
                    RemovePlayer(player);
                    break;
                case Packet.RequestPing:
                    BeginSend(Packet.ResponsePing);
                    EndSend(reliable: true, player);
                    break;
                case Packet.RequestSetUDP:
                    {
                        int num3 = binaryReader.ReadUInt16();
                        if (num3 != 0 && mUdp.isActive)
                        {
                            IPAddress address = new IPAddress(player.tcpEndPoint.Address.GetAddressBytes());
                            SetPlayerUdpEndPoint(player, new IPEndPoint(address, num3));
                        }
                        else
                        {
                            SetPlayerUdpEndPoint(player, null);
                        }
                        ushort value = (ushort)(mUdp.isActive ? ((ushort)mUdp.listeningPort) : 0);
                        BeginSend(Packet.ResponseSetUDP).Write(value);
                        EndSend(reliable: true, player);
                        if (player.udpEndPoint != null)
                        {
                            mUdp.SendEmptyPacket(player.udpEndPoint);
                        }
                        break;
                    }
                case Packet.RequestActivateUDP:
                    player.udpIsUsable = true;
                    if (player.udpEndPoint != null)
                    {
                        mUdp.SendEmptyPacket(player.udpEndPoint);
                    }
                    break;
                case Packet.RequestJoinChannel:
                    {
                        int num = binaryReader.ReadInt32();
                        string text2 = binaryReader.ReadString();
                        string text3 = binaryReader.ReadString();
                        bool persistent = binaryReader.ReadBoolean();
                        ushort playerLimit = binaryReader.ReadUInt16();
                        if (num == -2)
                        {
                            bool flag = string.IsNullOrEmpty(text3);
                            num = -1;
                            for (int j = 0; j < mChannels.size; j++)
                            {
                                Channel channel = mChannels[j];
                                if (channel.isOpen && (flag || text3.Equals(channel.level)) && (string.IsNullOrEmpty(channel.password) || channel.password == text2))
                                {
                                    num = channel.id;
                                    break;
                                }
                            }
                            if (flag && num == -1)
                            {
                                BinaryWriter binaryWriter3 = BeginSend(Packet.ResponseJoinChannel);
                                binaryWriter3.Write(value: false);
                                binaryWriter3.Write("No suitable channels found");
                                EndSend(reliable: true, player);
                                break;
                            }
                        }
                        if (num == -1)
                        {
                            num = mRandom.Next(100000000);
                            for (int k = 0; k < 1000; k++)
                            {
                                if (!ChannelExists(num))
                                {
                                    break;
                                }
                                num = mRandom.Next(100000000);
                            }
                        }
                        if (player.channel == null || player.channel.id != num)
                        {
                            bool isNew;
                            Channel channel2 = CreateChannel(num, out isNew);
                            if (channel2 == null || !channel2.isOpen)
                            {
                                BinaryWriter binaryWriter4 = BeginSend(Packet.ResponseJoinChannel);
                                binaryWriter4.Write(value: false);
                                binaryWriter4.Write("The requested channel is closed");
                                EndSend(reliable: true, player);
                            }
                            else if (isNew)
                            {
                                channel2.password = text2;
                                channel2.persistent = persistent;
                                channel2.level = text3;
                                channel2.playerLimit = playerLimit;
                                SendLeaveChannel(player, notify: false);
                                SendJoinChannel(player, channel2);
                            }
                            else if (string.IsNullOrEmpty(channel2.password) || channel2.password == text2)
                            {
                                SendLeaveChannel(player, notify: false);
                                SendJoinChannel(player, channel2);
                            }
                            else
                            {
                                BinaryWriter binaryWriter5 = BeginSend(Packet.ResponseJoinChannel);
                                binaryWriter5.Write(value: false);
                                binaryWriter5.Write("Wrong password");
                                EndSend(reliable: true, player);
                            }
                        }
                        break;
                    }
                case Packet.RequestSetName:
                    {
                        player.name = binaryReader.ReadString();
                        BinaryWriter binaryWriter2 = BeginSend(Packet.ResponseRenamePlayer);
                        binaryWriter2.Write(player.id);
                        binaryWriter2.Write(player.name);
                        if (player.channel != null)
                        {
                            EndSend(reliable: true, player.channel, null);
                        }
                        else
                        {
                            EndSend(reliable: true, player);
                        }
                        break;
                    }
                case Packet.SyncPlayerData:
                    {
                        int position = buffer.position - 5;
                        TcpPlayer player3 = GetPlayer(binaryReader.ReadInt32());
                        if (player3 == null)
                        {
                            break;
                        }
                        if (buffer.size > 1)
                        {
                            player3.data = binaryReader.ReadObject();
                        }
                        else
                        {
                            player3.data = null;
                        }
                        if (player3.channel != null)
                        {
                            buffer.position = position;
                            for (int l = 0; l < player3.channel.players.size; l++)
                            {
                                TcpPlayer tcpPlayer2 = player3.channel.players[l];
                                if (tcpPlayer2 != player)
                                {
                                    if (reliable || !tcpPlayer2.udpIsUsable || tcpPlayer2.udpEndPoint == null || !mAllowUdp)
                                    {
                                        tcpPlayer2.SendTcpPacket(buffer);
                                    }
                                    else
                                    {
                                        mUdp.Send(buffer, tcpPlayer2.udpEndPoint);
                                    }
                                }
                            }
                        }
                        else if (player3 != player)
                        {
                            buffer.position = position;
                            player3.SendTcpPacket(buffer);
                        }
                        break;
                    }
                case Packet.RequestSaveFile:
                    {
                        string fileName = binaryReader.ReadString();
                        byte[] data = binaryReader.ReadBytes(binaryReader.ReadInt32());
                        SaveFile(fileName, data);
                        break;
                    }
                case Packet.RequestLoadFile:
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
                        EndSend(reliable: true, player);
                        break;
                    }
                case Packet.RequestDeleteFile:
                    DeleteFile(binaryReader.ReadString());
                    break;
                case Packet.RequestNoDelay:
                    player.noDelay = binaryReader.ReadBoolean();
                    break;
                case Packet.RequestChannelList:
                    {
                        BinaryWriter binaryWriter6 = BeginSend(Packet.ResponseChannelList);
                        int num2 = 0;
                        for (int m = 0; m < mChannels.size; m++)
                        {
                            if (!mChannels[m].closed)
                            {
                                num2++;
                            }
                        }
                        binaryWriter6.Write(num2);
                        for (int n = 0; n < mChannels.size; n++)
                        {
                            Channel channel3 = mChannels[n];
                            if (!channel3.closed)
                            {
                                binaryWriter6.Write(channel3.id);
                                binaryWriter6.Write((ushort)channel3.players.size);
                                binaryWriter6.Write(channel3.playerLimit);
                                binaryWriter6.Write(!string.IsNullOrEmpty(channel3.password));
                                binaryWriter6.Write(channel3.persistent);
                                binaryWriter6.Write(channel3.level);
                                binaryWriter6.Write(channel3.data);
                            }
                        }
                        EndSend(reliable: true, player);
                        break;
                    }
                case Packet.RequestSetTimeout:
                    player.timeoutTime = binaryReader.ReadInt32() * 1000;
                    break;
                case Packet.ForwardToPlayer:
                    {
                        TcpPlayer player2 = GetPlayer(binaryReader.ReadInt32());
                        if (player2 != null && player2.isConnected)
                        {
                            buffer.position -= 9;
                            player2.SendTcpPacket(buffer);
                        }
                        break;
                    }
                case Packet.Broadcast:
                    {
                        buffer.position -= 5;
                        for (int i = 0; i < mPlayers.size; i++)
                        {
                            TcpPlayer tcpPlayer = mPlayers[i];
                            if (reliable || !tcpPlayer.udpIsUsable || tcpPlayer.udpEndPoint == null || !mAllowUdp)
                            {
                                tcpPlayer.SendTcpPacket(buffer);
                            }
                            else
                            {
                                mUdp.Send(buffer, tcpPlayer.udpEndPoint);
                            }
                        }
                        break;
                    }
                default:
                    if (player.channel != null && packet <= Packet.ForwardToPlayerBuffered)
                    {
                        if (packet >= Packet.ForwardToAll)
                        {
                            ProcessForwardPacket(player, buffer, binaryReader, packet, reliable);
                        }
                        else
                        {
                            ProcessChannelPacket(player, buffer, binaryReader, packet);
                        }
                    }
                    else if (onCustomPacket != null)
                    {
                        onCustomPacket(player, buffer, binaryReader, packet, reliable);
                    }
                    break;
                case Packet.Empty:
                    break;
            }
            return true;
        }

        private void ProcessForwardPacket(TcpPlayer player, Buffer buffer, BinaryReader reader, Packet request, bool reliable)
        {
            if (!mUdp.isActive || buffer.size > 1024)
            {
                reliable = true;
            }
            object obj;
            switch (request)
            {
                case Packet.ForwardToHost:
                    buffer.position -= 5;
                    if (reliable || !player.udpIsUsable || player.channel.host.udpEndPoint == null || !mAllowUdp)
                    {
                        player.channel.host.SendTcpPacket(buffer);
                    }
                    else
                    {
                        mUdp.Send(buffer, player.channel.host.udpEndPoint);
                    }
                    return;
                case Packet.ForwardToPlayerBuffered:
                    {
                        int position = buffer.position - 5;
                        TcpPlayer player2 = GetPlayer(reader.ReadInt32());
                        uint num = reader.ReadUInt32();
                        string funcName = ((num & 0xFF) != 0) ? null : reader.ReadString();
                        buffer.position = position;
                        player.channel.CreateRFC(num, funcName, buffer);
                        if (player2 != null && player2.isConnected)
                        {
                            if (reliable || !player2.udpIsUsable || player2.udpEndPoint == null || !mAllowUdp)
                            {
                                player2.SendTcpPacket(buffer);
                            }
                            else
                            {
                                mUdp.Send(buffer, player2.udpEndPoint);
                            }
                        }
                        return;
                    }
                case Packet.ForwardToOthers:
                case Packet.ForwardToOthersSaved:
                    obj = player;
                    break;
                default:
                    obj = null;
                    break;
            }
            TcpPlayer tcpPlayer = (TcpPlayer)obj;
            int position2 = buffer.position - 5;
            if (request == Packet.ForwardToAllSaved || request == Packet.ForwardToOthersSaved)
            {
                uint num2 = reader.ReadUInt32();
                string funcName2 = ((num2 & 0xFF) != 0) ? null : reader.ReadString();
                buffer.position = position2;
                player.channel.CreateRFC(num2, funcName2, buffer);
            }
            else
            {
                buffer.position = position2;
            }
            for (int i = 0; i < player.channel.players.size; i++)
            {
                TcpPlayer tcpPlayer2 = player.channel.players[i];
                if (tcpPlayer2 != tcpPlayer)
                {
                    if (reliable || !tcpPlayer2.udpIsUsable || tcpPlayer2.udpEndPoint == null || !mAllowUdp)
                    {
                        tcpPlayer2.SendTcpPacket(buffer);
                    }
                    else
                    {
                        mUdp.Send(buffer, tcpPlayer2.udpEndPoint);
                    }
                }
            }
        }

        private void ProcessChannelPacket(TcpPlayer player, Buffer buffer, BinaryReader reader, Packet request)
        {
            switch (request)
            {
                case Packet.RequestSetName:
                case Packet.RequestSaveFile:
                case Packet.RequestLoadFile:
                case Packet.RequestDeleteFile:
                case Packet.RequestNoDelay:
                    break;
                case Packet.RequestCreate:
                    {
                        ushort num2 = reader.ReadUInt16();
                        byte b = reader.ReadByte();
                        uint num3 = 0u;
                        if (b != 0)
                        {
                            num3 = --player.channel.objectCounter;
                            if (num3 < 32768)
                            {
                                player.channel.objectCounter = 16777215u;
                                num3 = 16777215u;
                            }
                            Channel.CreatedObject createdObject = new Channel.CreatedObject();
                            createdObject.playerID = player.id;
                            createdObject.objectID = num2;
                            createdObject.uniqueID = num3;
                            createdObject.type = b;
                            if (buffer.size > 0)
                            {
                                createdObject.buffer = buffer;
                                buffer.MarkAsUsed();
                            }
                            player.channel.created.Add(createdObject);
                        }
                        BinaryWriter binaryWriter2 = BeginSend(Packet.ResponseCreate);
                        binaryWriter2.Write(player.id);
                        binaryWriter2.Write(num2);
                        binaryWriter2.Write(num3);
                        if (buffer.size > 0)
                        {
                            binaryWriter2.Write(buffer.buffer, buffer.position, buffer.size);
                        }
                        EndSend(reliable: true, player.channel, null);
                        break;
                    }
                case Packet.RequestDestroy:
                    {
                        uint num4 = reader.ReadUInt32();
                        if (player.channel.DestroyObject(num4))
                        {
                            BinaryWriter binaryWriter3 = BeginSend(Packet.ResponseDestroy);
                            binaryWriter3.Write((ushort)1);
                            binaryWriter3.Write(num4);
                            EndSend(reliable: true, player.channel, null);
                        }
                        break;
                    }
                case Packet.RequestLoadLevel:
                    if (player.channel.host == player)
                    {
                        player.channel.Reset();
                        player.channel.level = reader.ReadString();
                        BinaryWriter binaryWriter4 = BeginSend(Packet.ResponseLoadLevel);
                        binaryWriter4.Write((!string.IsNullOrEmpty(player.channel.level)) ? player.channel.level : string.Empty);
                        EndSend(reliable: true, player.channel, null);
                    }
                    break;
                case Packet.RequestSetHost:
                    if (player.channel.host == player)
                    {
                        TcpPlayer player2 = GetPlayer(reader.ReadInt32());
                        if (player2 != null && player2.channel == player.channel)
                        {
                            SendSetHost(player2);
                        }
                    }
                    break;
                case Packet.RequestLeaveChannel:
                    SendLeaveChannel(player, notify: true);
                    break;
                case Packet.RequestCloseChannel:
                    player.channel.persistent = false;
                    player.channel.closed = true;
                    break;
                case Packet.RequestSetPlayerLimit:
                    player.channel.playerLimit = reader.ReadUInt16();
                    break;
                case Packet.RequestRemoveRFC:
                    {
                        uint num = reader.ReadUInt32();
                        string funcName = ((num & 0xFF) != 0) ? null : reader.ReadString();
                        player.channel.DeleteRFC(num, funcName);
                        break;
                    }
                case Packet.RequestSetChannelData:
                    {
                        player.channel.data = reader.ReadString();
                        BinaryWriter binaryWriter = BeginSend(Packet.ResponseSetChannelData);
                        binaryWriter.Write(player.channel.data);
                        EndSend(reliable: true, player.channel, null);
                        break;
                    }
            }
        }

        public void SaveTo(string fileName)
        {
            if (mListener == null)
            {
                return;
            }
            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
            binaryWriter.Write(0);
            int num = 0;
            for (int i = 0; i < mChannels.size; i++)
            {
                Channel channel = mChannels[i];
                if (!channel.closed && channel.persistent && channel.hasData)
                {
                    binaryWriter.Write(channel.id);
                    channel.SaveTo(binaryWriter);
                    num++;
                }
            }
            if (num > 0)
            {
                memoryStream.Seek(0L, SeekOrigin.Begin);
                binaryWriter.Write(num);
            }
            Tools.WriteFile(fileName, memoryStream.ToArray());
            memoryStream.Close();
        }

        public bool LoadFrom(string fileName)
        {
            //Discarded unreachable code: IL_008a
            byte[] array = Tools.ReadFile(fileName);
            if (array == null)
            {
                return false;
            }
            MemoryStream input = new MemoryStream(array);
            try
            {
                BinaryReader binaryReader = new BinaryReader(input);
                int num = binaryReader.ReadInt32();
                for (int i = 0; i < num; i++)
                {
                    int channelID = binaryReader.ReadInt32();
                    bool isNew;
                    Channel channel = CreateChannel(channelID, out isNew);
                    if (isNew)
                    {
                        channel.LoadFrom(binaryReader);
                    }
                }
            }
            catch (Exception ex)
            {
                Error("Loading from " + fileName + ": " + ex.Message);
                return false;
            }
            return true;
        }
    }
}
