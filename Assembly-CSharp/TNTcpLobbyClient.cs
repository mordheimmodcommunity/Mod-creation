using System;
using System.IO;
using System.Net;
using TNet;
using UnityEngine;

public class TNTcpLobbyClient : TNLobbyClient
{
    private TcpProtocol mTcp = new TcpProtocol();

    private long mNextConnect;

    private IPEndPoint mRemoteAddress;

    private void OnEnable()
    {
        if (mRemoteAddress == null)
        {
            mRemoteAddress = ((!string.IsNullOrEmpty(remoteAddress)) ? Tools.ResolveEndPoint(remoteAddress, remotePort) : new IPEndPoint(IPAddress.Broadcast, remotePort));
            if (mRemoteAddress == null)
            {
                mTcp.Error("Invalid address: " + remoteAddress + ":" + remotePort);
            }
        }
    }

    protected override void OnDisable()
    {
        TNLobbyClient.isActive = false;
        mTcp.Disconnect();
        base.OnDisable();
        if (TNLobbyClient.onChange != null)
        {
            TNLobbyClient.onChange();
        }
    }

    private void Update()
    {
        bool flag = false;
        long num = DateTime.Now.Ticks / 10000;
        if (mRemoteAddress != null && mTcp.stage == TcpProtocol.Stage.NotConnected && mNextConnect < num)
        {
            mNextConnect = num + 5000;
            mTcp.Connect(mRemoteAddress);
        }
        TNet.Buffer buffer;
        while (mTcp.ReceivePacket(out buffer))
        {
            if (buffer.size > 0)
            {
                try
                {
                    BinaryReader binaryReader = buffer.BeginReading();
                    Packet packet = (Packet)binaryReader.ReadByte();
                    switch (packet)
                    {
                        case Packet.ResponseID:
                            if (mTcp.VerifyResponseID(packet, binaryReader))
                            {
                                TNLobbyClient.isActive = true;
                                mTcp.BeginSend(Packet.RequestServerList).Write((ushort)1);
                                mTcp.EndSend();
                            }
                            break;
                        case Packet.Disconnect:
                            TNLobbyClient.knownServers.Clear();
                            TNLobbyClient.isActive = false;
                            flag = true;
                            break;
                        case Packet.ResponseServerList:
                            TNLobbyClient.knownServers.ReadFrom(binaryReader, num);
                            flag = true;
                            break;
                        case Packet.Error:
                            TNLobbyClient.errorString = binaryReader.ReadString();
                            Debug.LogWarning(TNLobbyClient.errorString);
                            flag = true;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    TNLobbyClient.errorString = ex.Message;
                    Debug.LogWarning(ex.Message);
                    mTcp.Close(notify: false);
                }
            }
            buffer.Recycle();
        }
        if (flag && TNLobbyClient.onChange != null)
        {
            TNLobbyClient.onChange();
        }
    }
}
