using Steamworks;
using System.Collections;
using UnityEngine;

public class Matchmaking
{
    private Lobby lobby;

    private CallResult<LobbyCreated_t> steamCbLobbyCreated;

    private CallResult<LobbyEnter_t> steamCbLobbyEnter;

    private CallResult<LobbyMatchList_t> steamCbLobbyMatchList;

    private Callback<PersonaStateChange_t> StateChangeCb;

    private Callback<LobbyDataUpdate_t> LobbyDataUpdateCb;

    private Callback<LobbyChatUpdate_t> LobbyChatUpdateCb;

    private Callback<LobbyKicked_t> LobbyKickedCb;

    private Callback<GameLobbyJoinRequested_t> LobbyJoinRequestCb;

    private CSteamID pendingLobbyJoinId;

    private Coroutine pendingLobbyInvite;

    public Matchmaking()
    {
        StateChangeCb = Callback<PersonaStateChange_t>.Create(OnPersonaStateChange);
        LobbyDataUpdateCb = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);
        LobbyChatUpdateCb = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
        LobbyKickedCb = Callback<LobbyKicked_t>.Create(OnLobbyKicked);
        LobbyJoinRequestCb = Callback<GameLobbyJoinRequested_t>.Create(OnLobbyJoinRequest);
        steamCbLobbyCreated = new CallResult<LobbyCreated_t>();
        steamCbLobbyEnter = new CallResult<LobbyEnter_t>();
        steamCbLobbyMatchList = new CallResult<LobbyMatchList_t>();
    }

    public void CreateLobby(string name, ELobbyType type)
    {
        if (!steamCbLobbyCreated.IsActive())
        {
            PandoraDebug.LogInfo("Create Lobby = " + type, "HEPHAESTUS-STEAMWORKS");
            if (type != 0)
            {
                SteamAPICall_t hAPICall = SteamMatchmaking.CreateLobby((type == ELobbyType.k_ELobbyTypeInvisible) ? ELobbyType.k_ELobbyTypeFriendsOnly : type, 2);
                steamCbLobbyCreated.Set(hAPICall, OnLobbyCreated);
                lobby = new Lobby();
                lobby.name = name;
                lobby.SetPrivacy(type);
            }
            else
            {
                lobby = new Lobby();
                lobby.name = name;
                lobby.SetPrivacy(type);
                lobby.id = 0uL;
                lobby.version = "1.4.4.4";
                PandoraSingleton<Hephaestus>.Instance.UpdateLobby(lobby);
                PandoraSingleton<Hephaestus>.Instance.OnCreateLobby(lobby.id, success: true);
            }
        }
    }

    private void OnLobbyCreated(LobbyCreated_t callback, bool failure)
    {
        PandoraDebug.LogInfo("OnLobbyCreated Result = " + callback.m_eResult + " FAIL = " + failure + "LobbyId = " + lobby.id, "HEPHAESTUS-STEAMWORKS");
        if (callback.m_eResult == EResult.k_EResultOK)
        {
            lobby.id = callback.m_ulSteamIDLobby;
            lobby.hostId = (ulong)SteamUser.GetSteamID();
            lobby.version = "1.4.4.4";
            lobby.mapName = 0;
            lobby.warbandId = 0;
            lobby.isExhibition = true;
            if (string.IsNullOrEmpty(lobby.name))
            {
                lobby.name = SteamFriends.GetPersonaName() + "'s lobby";
            }
            SetLobbyData(null, null);
            PandoraDebug.LogInfo("OnLobbyCreated = " + lobby.id + " name = " + lobby.name, "HEPHAESTUS-STEAMWORKS");
            SteamMatchmaking.SetLobbyMemberLimit((CSteamID)lobby.id, 2);
            PandoraSingleton<Hephaestus>.Instance.UpdateLobby(lobby);
            PandoraSingleton<Hephaestus>.Instance.OnCreateLobby(lobby.id, success: true);
        }
        else
        {
            PandoraDebug.LogInfo("OnLobbyCreated Not OK. forget lobby", "HEPHAESTUS-STEAMWORKS");
            PandoraSingleton<Hephaestus>.Instance.OnCreateLobby(lobby.id, success: false);
        }
    }

    public void LeaveLobby()
    {
        if (lobby != null)
        {
            PandoraDebug.LogInfo("Left Lobby = " + lobby.id, "HEPHAESTUS-STEAMWORKS");
            SteamMatchmaking.LeaveLobby((CSteamID)lobby.id);
            if (SteamMatchmaking.GetNumLobbyMembers((CSteamID)lobby.id) == 0)
            {
                PandoraSingleton<Hephaestus>.Instance.RemoveLobby(lobby.id);
            }
            lobby = null;
        }
    }

    public void JoinLobby(ulong lobbyId)
    {
        if (!steamCbLobbyEnter.IsActive())
        {
            PandoraDebug.LogInfo("Join Lobby " + lobbyId, "HEPHAESTUS-STEAMWORKS");
            pendingLobbyJoinId = (CSteamID)lobbyId;
            SteamAPICall_t hAPICall = SteamMatchmaking.JoinLobby(pendingLobbyJoinId);
            steamCbLobbyEnter.Set(hAPICall, OnLobbyEntered);
        }
    }

    public void CancelJoinLobby()
    {
        if (steamCbLobbyEnter.IsActive())
        {
            steamCbLobbyEnter.Cancel();
        }
        SteamMatchmaking.LeaveLobby(pendingLobbyJoinId);
    }

    private void OnLobbyEntered(LobbyEnter_t callback, bool failure)
    {
        Hephaestus.LobbyConnexionResult lobbyConnexionResult;
        switch (callback.m_EChatRoomEnterResponse)
        {
            case 1u:
                lobbyConnexionResult = Hephaestus.LobbyConnexionResult.SUCCESS;
                break;
            case 6u:
                lobbyConnexionResult = Hephaestus.LobbyConnexionResult.BANNED;
                break;
            case 8u:
                lobbyConnexionResult = Hephaestus.LobbyConnexionResult.CLAN_DISABLED;
                break;
            case 9u:
                lobbyConnexionResult = Hephaestus.LobbyConnexionResult.COMMUNITY_BANNED;
                break;
            case 7u:
                lobbyConnexionResult = Hephaestus.LobbyConnexionResult.LIMITED_USER;
                break;
            case 10u:
                lobbyConnexionResult = Hephaestus.LobbyConnexionResult.MEMBER_BLOCKED_YOU;
                break;
            case 3u:
                lobbyConnexionResult = Hephaestus.LobbyConnexionResult.NOT_ALLOWED;
                break;
            case 11u:
                lobbyConnexionResult = Hephaestus.LobbyConnexionResult.BLOCKED_A_MEMBER;
                break;
            case 2u:
                lobbyConnexionResult = Hephaestus.LobbyConnexionResult.DOES_NOT_EXIST;
                break;
            case 4u:
                lobbyConnexionResult = Hephaestus.LobbyConnexionResult.FULL;
                break;
            case 5u:
                lobbyConnexionResult = Hephaestus.LobbyConnexionResult.UNEXPECTED_ERROR;
                break;
            default:
                lobbyConnexionResult = Hephaestus.LobbyConnexionResult.UNEXPECTED_ERROR;
                break;
        }
        if (lobbyConnexionResult == Hephaestus.LobbyConnexionResult.SUCCESS)
        {
            lobby = new Lobby();
            lobby.id = callback.m_ulSteamIDLobby;
            string lobbyData = SteamMatchmaking.GetLobbyData((CSteamID)callback.m_ulSteamIDLobby, "version");
            if (lobbyData != "1.4.4.4")
            {
                lobbyConnexionResult = Hephaestus.LobbyConnexionResult.VERSION_MISMATCH;
                PandoraSingleton<Hephaestus>.Instance.LeaveLobby();
            }
        }
        if (lobbyConnexionResult == Hephaestus.LobbyConnexionResult.SUCCESS)
        {
            PandoraDebug.LogInfo("Lobby Enter Successful", "HEPHAESTUS-STEAMWORKS");
            PandoraSingleton<Hephaestus>.Instance.OnJoinLobby((ulong)SteamMatchmaking.GetLobbyOwner((CSteamID)callback.m_ulSteamIDLobby), lobbyConnexionResult);
        }
        else
        {
            PandoraDebug.LogInfo("Lobby Enter Unsuccessful, probably full!", "HEPHAESTUS-STEAMWORKS");
            PandoraSingleton<Hephaestus>.Instance.OnJoinLobby(0uL, lobbyConnexionResult);
        }
    }

    public void SetLobbyData(string key, string value)
    {
        if (lobby == null || lobby.id == 0L)
        {
            return;
        }
        if (!string.IsNullOrEmpty(key))
        {
            switch (key)
            {
                case "privacy":
                    lobby.privacy = (Hephaestus.LobbyPrivacy)int.Parse(value);
                    break;
                case "name":
                    lobby.name = value;
                    break;
                case "version":
                    lobby.version = value;
                    break;
                case "map":
                    lobby.mapName = int.Parse(value);
                    break;
                case "warband":
                    lobby.warbandId = int.Parse(value);
                    break;
                case "exhibition":
                    lobby.isExhibition = bool.Parse(value);
                    break;
                case "rating_min":
                    lobby.ratingMin = int.Parse(value);
                    break;
                case "rating_max":
                    lobby.ratingMax = int.Parse(value);
                    break;
                default:
                    PandoraDebug.LogWarning("Setting Unknown key in lobby data:" + key, "HEPHAESTUS-STEAMWORKS");
                    break;
            }
        }
        int privacy = (int)lobby.privacy;
        SetSingleLobbyDataKeyValue("privacy", privacy.ToString());
        SetSingleLobbyDataKeyValue("name", lobby.name);
        SetSingleLobbyDataKeyValue("version", lobby.version);
        SetSingleLobbyDataKeyValue("map", lobby.mapName.ToLowerString());
        SetSingleLobbyDataKeyValue("warband", lobby.warbandId.ToLowerString());
        SetSingleLobbyDataKeyValue("exhibition", lobby.isExhibition.ToLowerString());
        SetSingleLobbyDataKeyValue("rating_min", lobby.ratingMin.ToLowerString());
        SetSingleLobbyDataKeyValue("rating_max", lobby.ratingMax.ToLowerString());
    }

    private void SetSingleLobbyDataKeyValue(string key, string value)
    {
        if (!SteamMatchmaking.SetLobbyData((CSteamID)lobby.id, key, value))
        {
            PandoraDebug.LogWarning("Unable to set Lobby Data:" + key + " to value:" + value, "HEPHAESTUS-STEAMWORKS");
        }
    }

    public void SetLobbyJoinable(bool joinable)
    {
        if (lobby != null)
        {
            PandoraDebug.LogDebug("Setting Lobby Joinable = " + joinable);
            SteamMatchmaking.SetLobbyJoinable((CSteamID)lobby.id, joinable);
        }
    }

    public void SearchLobbies()
    {
        PandoraDebug.LogInfo("Call Search Lobbies", "HEPHAESTUS-STEAMWORKS");
        SteamAPICall_t hAPICall = SteamMatchmaking.RequestLobbyList();
        steamCbLobbyMatchList.Set(hAPICall, OnLobbyMatchList);
    }

    private void OnPersonaStateChange(PersonaStateChange_t callback)
    {
    }

    private void OnLobbyDataUpdate(LobbyDataUpdate_t callback)
    {
        PandoraDebug.LogInfo("Update Lobby data begin", "HEPHAESTUS-STEAMWORKS");
        if (callback.m_bSuccess > 0)
        {
            int lobbyMemberLimit = SteamMatchmaking.GetLobbyMemberLimit((CSteamID)callback.m_ulSteamIDLobby);
            int numLobbyMembers = SteamMatchmaking.GetNumLobbyMembers((CSteamID)callback.m_ulSteamIDLobby);
            if (numLobbyMembers < lobbyMemberLimit || lobbyMemberLimit != 0)
            {
                Lobby lobby = new Lobby();
                lobby.id = callback.m_ulSteamIDLobby;
                int lobbyDataCount = SteamMatchmaking.GetLobbyDataCount((CSteamID)lobby.id);
                string lobbyData = SteamMatchmaking.GetLobbyData((CSteamID)lobby.id, "privacy");
                string lobbyData2 = SteamMatchmaking.GetLobbyData((CSteamID)lobby.id, "name");
                string lobbyData3 = SteamMatchmaking.GetLobbyData((CSteamID)lobby.id, "map");
                string lobbyData4 = SteamMatchmaking.GetLobbyData((CSteamID)lobby.id, "version");
                string lobbyData5 = SteamMatchmaking.GetLobbyData((CSteamID)lobby.id, "warband");
                string lobbyData6 = SteamMatchmaking.GetLobbyData((CSteamID)lobby.id, "exhibition");
                string lobbyData7 = SteamMatchmaking.GetLobbyData((CSteamID)lobby.id, "rating_min");
                string lobbyData8 = SteamMatchmaking.GetLobbyData((CSteamID)lobby.id, "rating_max");
                ulong num = (ulong)SteamMatchmaking.GetLobbyOwner((CSteamID)lobby.id);
                if (!string.IsNullOrEmpty(lobbyData))
                {
                    lobby.privacy = (Hephaestus.LobbyPrivacy)int.Parse(lobbyData);
                }
                lobby.hostId = num;
                lobby.name = lobbyData2;
                lobby.version = lobbyData4;
                if (!string.IsNullOrEmpty(lobbyData6))
                {
                    lobby.isExhibition = bool.Parse(lobbyData6);
                }
                if (!string.IsNullOrEmpty(lobbyData7))
                {
                    lobby.ratingMin = int.Parse(lobbyData7);
                }
                if (!string.IsNullOrEmpty(lobbyData8))
                {
                    lobby.ratingMax = int.Parse(lobbyData8);
                }
                PandoraDebug.LogInfo("Update Lobby Id = " + lobby.id + " lobbyId of cb:" + callback.m_ulSteamIDLobby + " paramCount:" + lobbyDataCount + " name = " + lobbyData2 + " host Id = " + num + " map = " + lobbyData3 + " warband = " + lobbyData5, "HEPHAESTUS-STEAMWORKS");
                if (!string.IsNullOrEmpty(lobbyData3))
                {
                    lobby.mapName = int.Parse(lobbyData3);
                }
                if (!string.IsNullOrEmpty(lobbyData5))
                {
                    lobby.warbandId = int.Parse(lobbyData5);
                }
                PandoraSingleton<Hephaestus>.Instance.UpdateLobby(lobby);
            }
        }
        else
        {
            PandoraDebug.LogInfo("FAIL Update Lobby Id", "HEPHAESTUS-STEAMWORKS");
        }
    }

    private void OnLobbyJoinRequest(GameLobbyJoinRequested_t callback)
    {
        PandoraDebug.LogDebug("OnLobbyJoinRequest LobbyId = " + callback.m_steamIDLobby, "HEPHAESTUS-STEAMWORKS");
        OnLobbyJoinRequest(callback.m_steamIDLobby);
    }

    public void OnLobbyJoinRequest(CSteamID lobbyId)
    {
        SteamMatchmaking.RequestLobbyData(lobbyId);
        if (pendingLobbyInvite != null)
        {
            PandoraSingleton<Hephaestus>.Instance.StopCoroutine(pendingLobbyInvite);
        }
        pendingLobbyInvite = PandoraSingleton<Hephaestus>.Instance.StartCoroutine(AsyncLobbyJoin(lobbyId));
    }

    private IEnumerator AsyncLobbyJoin(CSteamID lobbyId)
    {
        string exhibition = string.Empty;
        while (string.IsNullOrEmpty(exhibition))
        {
            exhibition = SteamMatchmaking.GetLobbyData(lobbyId, "exhibition");
            yield return null;
        }
        string lobbyName = SteamMatchmaking.GetLobbyData(lobbyId, "name");
        bool isExhibition = bool.Parse(exhibition);
        PandoraSingleton<Hephaestus>.Instance.ReceiveInvite((ulong)lobbyId, lobbyName, isExhibition, 0, 5000);
    }

    private void OnLobbyChatUpdate(LobbyChatUpdate_t callback)
    {
        if (PandoraSingleton<Hephaestus>.Instance.Lobby != null && callback.m_ulSteamIDLobby == PandoraSingleton<Hephaestus>.Instance.Lobby.id)
        {
            PandoraDebug.LogInfo("OnLobbyChatUpdate lobby = " + PandoraSingleton<Hephaestus>.Instance.Lobby.id + " Callback lobby = " + callback.m_ulSteamIDLobby + " Message = " + (EChatMemberStateChange)callback.m_rgfChatMemberStateChange, "HEPHAESTUS-STEAMWORKS");
            uint rgfChatMemberStateChange = callback.m_rgfChatMemberStateChange;
            if ((rgfChatMemberStateChange & 1) != 0)
            {
                PandoraDebug.LogInfo(callback.m_ulSteamIDUserChanged + " has entered the Lobby", "HEPHAESTUS-STEAMWORKS");
                PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.SKIRMISH_LOBBY_JOINED, callback.m_ulSteamIDUserChanged);
            }
            if ((rgfChatMemberStateChange & 2) != 0)
            {
                PandoraDebug.LogInfo(callback.m_ulSteamIDUserChanged + " has Left the Lobby", "HEPHAESTUS-STEAMWORKS");
                PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.SKIRMISH_LOBBY_LEFT, callback.m_ulSteamIDUserChanged);
            }
            if ((rgfChatMemberStateChange & 4) != 0)
            {
                PandoraDebug.LogInfo(callback.m_ulSteamIDUserChanged + " has Disconnected from steam", "HEPHAESTUS-STEAMWORKS");
                PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.SKIRMISH_LOBBY_LEFT, callback.m_ulSteamIDUserChanged);
            }
            if ((rgfChatMemberStateChange & 8) != 0)
            {
                PandoraDebug.LogInfo(callback.m_ulSteamIDUserChanged + " was kicked by" + callback.m_ulSteamIDMakingChange, "HEPHAESTUS-STEAMWORKS");
                PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.SKIRMISH_LOBBY_LEFT, callback.m_ulSteamIDUserChanged);
            }
            if ((rgfChatMemberStateChange & 0x10) != 0)
            {
                PandoraDebug.LogInfo(callback.m_ulSteamIDUserChanged + " was banned by" + callback.m_ulSteamIDMakingChange, "HEPHAESTUS-STEAMWORKS");
                PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.SKIRMISH_LOBBY_LEFT, callback.m_ulSteamIDUserChanged);
            }
        }
        if (SteamMatchmaking.GetNumLobbyMembers((CSteamID)callback.m_ulSteamIDLobby) == 0)
        {
            PandoraSingleton<Hephaestus>.Instance.RemoveLobby(callback.m_ulSteamIDLobby);
        }
    }

    private void OnLobbyKicked(LobbyKicked_t callback)
    {
        if (lobby != null)
        {
            lobby = null;
            PandoraSingleton<Hephaestus>.Instance.OnKickFromLobby();
        }
    }

    private void OnLobbyMatchList(LobbyMatchList_t callback, bool failure)
    {
        PandoraDebug.LogInfo("OnLobbyMatchList " + ((!failure) ? "SUCCESS" : "FAIL"), "HEPHAESTUS-STEAMWORKS");
        uint num = callback.m_nLobbiesMatching;
        PandoraDebug.LogInfo("OnLobbyMatchList num lobbies = " + num, "HEPHAESTUS-STEAMWORKS");
        for (int i = 0; i < num; i++)
        {
            CSteamID lobbyByIndex = SteamMatchmaking.GetLobbyByIndex(i);
            PandoraDebug.LogInfo("Lobby Id = " + (ulong)lobbyByIndex, "HEPHAESTUS-STEAMWORKS");
            string lobbyData = SteamMatchmaking.GetLobbyData(lobbyByIndex, "name");
            SteamMatchmaking.RequestLobbyData(lobbyByIndex);
        }
        int friendCount = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
        for (int j = 0; j < friendCount; j++)
        {
            FriendGameInfo_t pFriendGameInfo = default(FriendGameInfo_t);
            CSteamID friendByIndex = SteamFriends.GetFriendByIndex(j, EFriendFlags.k_EFriendFlagImmediate);
            if (!SteamFriends.GetFriendGamePlayed(friendByIndex, out pFriendGameInfo) || !pFriendGameInfo.m_steamIDLobby.IsValid() || (int)(ulong)pFriendGameInfo.m_gameID != 276810)
            {
                continue;
            }
            PandoraDebug.LogInfo("Found Friend = " + friendByIndex + " Limit = ", "HEPHAESTUS-STEAMWORKS");
            bool flag = false;
            for (int k = 0; k < num; k++)
            {
                if (flag)
                {
                    break;
                }
                if (SteamMatchmaking.GetLobbyByIndex(k) == pFriendGameInfo.m_steamIDLobby)
                {
                    flag = true;
                }
            }
            if (!flag)
            {
                SteamMatchmaking.RequestLobbyData(pFriendGameInfo.m_steamIDLobby);
                num++;
                string friendPersonaName = SteamFriends.GetFriendPersonaName(friendByIndex);
                PandoraDebug.LogInfo("Found Friend Requesting Data = " + friendByIndex + " Name = " + friendPersonaName + " Lobby = " + pFriendGameInfo.m_steamIDLobby, "HEPHAESTUS-STEAMWORKS");
            }
        }
        PandoraSingleton<Hephaestus>.Instance.OnSearchLobbies(num);
    }
}
