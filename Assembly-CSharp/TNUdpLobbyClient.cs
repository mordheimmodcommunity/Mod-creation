using System;
using System.IO;
using System.Net;
using TNet;
using UnityEngine;

public class TNUdpLobbyClient : TNLobbyClient
{
	private UdpProtocol mUdp = new UdpProtocol();

	private TNet.Buffer mRequest;

	private long mNextSend;

	private IPEndPoint mRemoteAddress;

	private bool mReEnable;

	private void Awake()
	{
	}

	private void OnEnable()
	{
		if (mRequest == null)
		{
			mRequest = TNet.Buffer.Create();
			mRequest.BeginPacket(Packet.RequestServerList).Write((ushort)1);
			mRequest.EndPacket();
		}
		if (mRemoteAddress == null)
		{
			mRemoteAddress = ((!string.IsNullOrEmpty(remoteAddress)) ? Tools.ResolveEndPoint(remoteAddress, remotePort) : new IPEndPoint(IPAddress.Broadcast, remotePort));
			if (mRemoteAddress == null)
			{
				mUdp.Error(new IPEndPoint(IPAddress.Loopback, mUdp.listeningPort), "Invalid address: " + remoteAddress + ":" + remotePort);
			}
		}
		if (!mUdp.Start(Tools.randomPort))
		{
			mUdp.Start(Tools.randomPort);
		}
	}

	protected override void OnDisable()
	{
		TNLobbyClient.isActive = false;
		base.OnDisable();
		try
		{
			mUdp.Stop();
			if (mRequest != null)
			{
				mRequest.Recycle();
				mRequest = null;
			}
			if (TNLobbyClient.onChange != null)
			{
				TNLobbyClient.onChange();
			}
		}
		catch (Exception)
		{
		}
	}

	private void OnApplicationPause(bool paused)
	{
		if (paused)
		{
			if (TNLobbyClient.isActive)
			{
				mReEnable = true;
				OnDisable();
			}
		}
		else if (mReEnable)
		{
			mReEnable = false;
			OnEnable();
		}
	}

	private void Update()
	{
		bool flag = false;
		long num = DateTime.Now.Ticks / 10000;
		TNet.Buffer buffer;
		IPEndPoint source;
		while (mUdp != null && mUdp.ReceivePacket(out buffer, out source))
		{
			if (buffer.size > 0)
			{
				try
				{
					BinaryReader binaryReader = buffer.BeginReading();
					switch (binaryReader.ReadByte())
					{
					case 48:
						TNLobbyClient.isActive = true;
						mNextSend = num + 3000;
						TNLobbyClient.knownServers.ReadFrom(binaryReader, num);
						TNLobbyClient.knownServers.Cleanup(num);
						flag = true;
						break;
					case 1:
						TNLobbyClient.errorString = binaryReader.ReadString();
						Debug.LogWarning(TNLobbyClient.errorString);
						flag = true;
						break;
					}
				}
				catch (Exception)
				{
				}
			}
			buffer.Recycle();
		}
		if (TNLobbyClient.knownServers.Cleanup(num))
		{
			flag = true;
		}
		if (flag && TNLobbyClient.onChange != null)
		{
			TNLobbyClient.onChange();
		}
		else if (mNextSend < num && mUdp != null)
		{
			mNextSend = num + 3000;
			mUdp.Send(mRequest, mRemoteAddress);
		}
	}
}
