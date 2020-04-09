using Steamworks;

public class Networking
{
	private Callback<P2PSessionRequest_t> P2PSessionRequestCb;

	private Callback<P2PSessionConnectFail_t> P2PSessionConnectFailCb;

	private Hephaestus.OnDataReceivedCallback dataReceivedCallback;

	public Networking()
	{
		P2PSessionRequestCb = Callback<P2PSessionRequest_t>.Create(OnP2PSessionRequest);
		P2PSessionConnectFailCb = Callback<P2PSessionConnectFail_t>.Create(OnP2PSessionConnectFail);
	}

	public void SetDataReceivedCallback(Hephaestus.OnDataReceivedCallback cb)
	{
		dataReceivedCallback = cb;
	}

	public void CloseP2PSessionWithUser(ulong steamID)
	{
		SteamNetworking.CloseP2PSessionWithUser((CSteamID)steamID);
	}

	private void OnP2PSessionRequest(P2PSessionRequest_t callback)
	{
		PandoraDebug.LogDebug("OnP2PSessionRequest lobby is ?" + PandoraSingleton<Hephaestus>.Instance.Lobby != null, "HEPHAESTUS-STEAMWORKS");
		if (PandoraSingleton<Hephaestus>.Instance.Lobby == null)
		{
			return;
		}
		int numLobbyMembers = SteamMatchmaking.GetNumLobbyMembers((CSteamID)PandoraSingleton<Hephaestus>.Instance.Lobby.id);
		if (numLobbyMembers != 2)
		{
			PandoraDebug.LogDebug("OnP2PSessionRequest need 2 members in lobby", "HEPHAESTUS-STEAMWORKS");
			return;
		}
		bool flag = false;
		for (int i = 0; i < numLobbyMembers; i++)
		{
			if (SteamMatchmaking.GetLobbyMemberByIndex((CSteamID)PandoraSingleton<Hephaestus>.Instance.Lobby.id, i) == callback.m_steamIDRemote)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			PandoraDebug.LogDebug("OnP2PSessionRequest Accepting Session", "HEPHAESTUS-STEAMWORKS");
			SteamNetworking.AcceptP2PSessionWithUser(callback.m_steamIDRemote);
		}
		else
		{
			PandoraDebug.LogDebug("OnP2PSessionRequest occured outside of lobby/game", "HEPHAESTUS-STEAMWORKS");
		}
	}

	private void OnP2PSessionConnectFail(P2PSessionConnectFail_t callback)
	{
		PandoraDebug.LogWarning("OnP2PSessionConnectFail Error = " + (EP2PSessionError)callback.m_eP2PSessionError);
		PandoraSingleton<Hermes>.Instance.ConnectionError((ulong)callback.m_steamIDRemote, ((EP2PSessionError)callback.m_eP2PSessionError).ToLowerString());
	}

	public void GetConnectionState(ulong steamID)
	{
		P2PSessionState_t pConnectionState = default(P2PSessionState_t);
		if (SteamNetworking.GetP2PSessionState((CSteamID)steamID, out pConnectionState))
		{
			PandoraDebug.LogDebug("Current P2P State to " + steamID + " Is Connecting = " + pConnectionState.m_bConnecting + " Is Connection Acvite = " + pConnectionState.m_bConnectionActive + " Is Using Relay = " + pConnectionState.m_bUsingRelay + " Is Remote IP = " + pConnectionState.m_nRemoteIP + " Is Remote Port = " + pConnectionState.m_nRemotePort);
		}
	}

	public void Send(bool reliable, CSteamID steamID, byte[] data)
	{
		if (reliable)
		{
			SteamNetworking.SendP2PPacket(steamID, data, (uint)data.Length, EP2PSend.k_EP2PSendReliable);
		}
		else
		{
			SteamNetworking.SendP2PPacket(steamID, data, (uint)data.Length, EP2PSend.k_EP2PSendUnreliable);
		}
	}

	public void ReadPackets()
	{
		uint pcubMsgSize = 0u;
		while (SteamNetworking.IsP2PPacketAvailable(out pcubMsgSize))
		{
			byte[] array = new byte[pcubMsgSize];
			uint pcubMsgSize2 = 0u;
			if (SteamNetworking.ReadP2PPacket(array, pcubMsgSize, out pcubMsgSize2, out CSteamID psteamIDRemote))
			{
				dataReceivedCallback((ulong)psteamIDRemote, array);
			}
		}
	}
}
