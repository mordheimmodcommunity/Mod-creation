using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

internal class SteamManager : IHephaestus
{
	public const uint GAME_ID = 276810u;

	public const uint ALL_LANGS = 276822u;

	public const uint GLOBADIER_DLC_ID = 434040u;

	public const uint SMUGGLER_DLC_ID = 434041u;

	public const uint PRIEST_OF_ULRICH_DLC_ID = 450810u;

	public const uint DOOMWEAVER_DLC_ID = 450811u;

	public const uint WITCH_HUNTERS_DLC_ID = 450812u;

	public const uint UNDEAD_DLC_ID = 534990u;

	private Matchmaking matchmaking;

	private Networking networking;

	private bool shouldUploadStats;

	private bool requestedStats;

	public string opponentName;

	private SteamAPIWarningMessageHook_t SteamAPIWarningMessageHook;

	private List<SupportedLanguage> availableLangs = new List<SupportedLanguage>();

	private CallResult<NumberOfCurrentPlayers_t> steamCbNumberOfPlayers;

	private Callback<GamepadTextInputDismissed_t> steamCbVKClosed;

	private string oldInputText;

	public bool Initialized
	{
		get;
		private set;
	}

	public SteamManager()
	{
		//Discarded unreachable code: IL_0065
		try
		{
			if (SteamAPI.RestartAppIfNecessary((AppId_t)276810u))
			{
				PandoraDebug.LogError("Steam is not started... start in now!\n", "HEPHAESTUS-STEAMWORKS");
				Application.Quit();
				return;
			}
		}
		catch (DllNotFoundException arg)
		{
			PandoraDebug.LogError("[Steamworks] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + arg, "HEPHAESTUS-STEAMWORKS");
			Application.Quit();
			return;
		}
		Initialized = SteamAPI.Init();
		if (!Initialized)
		{
			PandoraDebug.LogError("SteamAPI_Init() failed", "HEPHAESTUS-STEAMWORKS");
			Application.Quit();
			return;
		}
		SteamAPIWarningMessageHook = SteamAPIDebugTextHook;
		SteamClient.SetWarningMessageHook(SteamAPIWarningMessageHook);
		SteamUtils.SetOverlayNotificationPosition(ENotificationPosition.k_EPositionTopRight);
		if (!SteamApps.BIsSubscribed())
		{
			PandoraDebug.LogError("Steam user must own the game in order to play this game (SteamApps.BIsSubscribed() returned false).\n", "HEPHAESTUS-STEAMWORKS");
			Application.Quit();
		}
		matchmaking = new Matchmaking();
		networking = new Networking();
		new Callback<UserStatsReceived_t>(OnSteamUserStatsReceived);
		new Callback<UserStatsStored_t>(OnSteamUserStatsStored);
		new Callback<UserAchievementStored_t>(OnSteamAchievementStored);
		new Callback<DlcInstalled_t>(OnDLCInstalled);
		steamCbNumberOfPlayers = new CallResult<NumberOfCurrentPlayers_t>();
		steamCbVKClosed = Callback<GamepadTextInputDismissed_t>.Create(OnVirtualKeyboardClosed);
		SteamApps.GetAvailableGameLanguages();
		GetAvailableLanguages();
		ControllerHandle_t[] array = new ControllerHandle_t[25];
		int connectedControllers = SteamController.GetConnectedControllers(array);
		for (int i = 0; i < connectedControllers; i++)
		{
			PandoraDebug.LogInfo("FOUND CONTROLLER " + array[i].m_ControllerHandle, "HEPHAESTUS-STEAMWORKS");
		}
	}

	void IHephaestus.CreateLobby(string name, Hephaestus.LobbyPrivacy privacy)
	{
		ELobbyType type = ELobbyType.k_ELobbyTypePublic;
		switch (privacy)
		{
		case Hephaestus.LobbyPrivacy.PUBLIC:
			type = ELobbyType.k_ELobbyTypePublic;
			break;
		case Hephaestus.LobbyPrivacy.FRIENDS:
			type = ELobbyType.k_ELobbyTypeFriendsOnly;
			break;
		case Hephaestus.LobbyPrivacy.PRIVATE:
			type = ELobbyType.k_ELobbyTypeInvisible;
			break;
		case Hephaestus.LobbyPrivacy.OFFLINE:
			type = ELobbyType.k_ELobbyTypePrivate;
			break;
		}
		matchmaking.CreateLobby(name, type);
	}

	private static void SteamAPIDebugTextHook(int nSeverity, StringBuilder pchDebugText)
	{
		Debug.LogWarning(pchDebugText);
	}

	public void OnDestroy()
	{
		if (Initialized)
		{
			PandoraDebug.LogInfo("OnDestroy(), ShuttingDown SteamAPI!", "HEPHAESTUS-STEAMWORKS");
			LeaveLobby();
			SteamAPI.Shutdown();
		}
	}

	public IEnumerator Init()
	{
		PandoraSingleton<GameManager>.Instance.StartCoroutine(ProcessCommandLineArguments());
		yield return null;
	}

	public void Reset()
	{
	}

	public bool IsInitialized()
	{
		return Initialized;
	}

	private IEnumerator ProcessCommandLineArguments()
	{
		string[] args = Environment.GetCommandLineArgs();
		while (!PandoraSingleton<Hephaestus>.Instance.IsInitialized())
		{
			yield return null;
		}
		if (args.Length <= 1)
		{
			yield break;
		}
		for (int i = 1; i < args.Length; i++)
		{
			if (args[i].Contains("+connect_lobby"))
			{
				ulong lobbyId = ulong.Parse(args[i + 1]);
				string exhibition = null;
				while (string.IsNullOrEmpty(exhibition))
				{
					exhibition = SteamMatchmaking.GetLobbyData((CSteamID)lobbyId, "exhibition");
					yield return null;
				}
				string lobbyName = SteamMatchmaking.GetLobbyData((CSteamID)lobbyId, "name");
				bool isExhibition = bool.Parse(exhibition);
				PandoraSingleton<Hephaestus>.Instance.ReceiveInvite(lobbyId, lobbyName, isExhibition, 0, 5000);
			}
		}
	}

	public void GetDefaultLocale(Action<SupportedLanguage> callback)
	{
		if (GetAvailableLanguages()[0] == SupportedLanguage.ruRU)
		{
			callback(SupportedLanguage.ruRU);
			return;
		}
		if (GetAvailableLanguages()[0] == SupportedLanguage.plPL)
		{
			callback(SupportedLanguage.plPL);
			return;
		}
		string currentGameLanguage = SteamApps.GetCurrentGameLanguage();
		PandoraDebug.LogDebug("Steam default language : " + currentGameLanguage, "STEAM MANAGER");
		SupportedLanguage obj = SupportedLanguage.enUS;
		switch (currentGameLanguage)
		{
		case "english":
			obj = SupportedLanguage.enUS;
			break;
		case "french":
			obj = SupportedLanguage.frFR;
			break;
		case "german":
			obj = SupportedLanguage.deDE;
			break;
		case "spanish":
			obj = SupportedLanguage.esES;
			break;
		case "italian":
			obj = SupportedLanguage.itIT;
			break;
		case "polish":
			obj = SupportedLanguage.plPL;
			break;
		case "russian":
			obj = SupportedLanguage.ruRU;
			break;
		}
		callback(obj);
	}

	public List<SupportedLanguage> GetAvailableLanguages()
	{
		if (availableLangs.Count == 0)
		{
			DepotId_t[] array = new DepotId_t[25];
			uint installedDepots = SteamApps.GetInstalledDepots((AppId_t)276810u, array, 25u);
			for (int i = 0; i < installedDepots; i++)
			{
				PandoraDebug.LogDebug("Steam depot : " + array[i].m_DepotId, "STEAM MANAGER");
				if (array[i].m_DepotId == 276822)
				{
					for (int j = 0; j < 7; j++)
					{
						availableLangs.Add((SupportedLanguage)j);
					}
					return availableLangs;
				}
			}
			string currentGameLanguage = SteamApps.GetCurrentGameLanguage();
			PandoraDebug.LogDebug("Steam current languages : " + currentGameLanguage, "STEAM MANAGER");
			switch (currentGameLanguage)
			{
			case "english":
				availableLangs.Add(SupportedLanguage.enUS);
				break;
			case "french":
				availableLangs.Add(SupportedLanguage.frFR);
				break;
			case "german":
				availableLangs.Add(SupportedLanguage.deDE);
				break;
			case "spanish":
				availableLangs.Add(SupportedLanguage.esES);
				break;
			case "italian":
				availableLangs.Add(SupportedLanguage.itIT);
				break;
			case "polish":
				availableLangs.Add(SupportedLanguage.plPL);
				break;
			case "russian":
				availableLangs.Add(SupportedLanguage.ruRU);
				break;
			}
			if (availableLangs.Count == 0)
			{
				for (int k = 0; k < 7; k++)
				{
					availableLangs.Add((SupportedLanguage)k);
				}
			}
		}
		return availableLangs;
	}

	public bool IsOnline()
	{
		return SteamFriends.GetPersonaState() != EPersonaState.k_EPersonaStateOffline;
	}

	public string GetOfflineReason()
	{
		return string.Empty;
	}

	public void LeaveLobby()
	{
		matchmaking.LeaveLobby();
	}

	public void JoinLobby(ulong lobbyId)
	{
		matchmaking.JoinLobby(lobbyId);
	}

	public void CancelJoinLobby()
	{
		matchmaking.CancelJoinLobby();
	}

	public void SetLobbyData(string key, string value)
	{
		matchmaking.SetLobbyData(key, value);
	}

	public void SetLobbyJoinable(bool joinable)
	{
		matchmaking.SetLobbyJoinable(joinable);
	}

	public void InviteFriends()
	{
		PandoraDebug.LogInfo("InviteFriends", "HEPHAESTUS-STEAMWORKS");
		SteamFriends.ActivateGameOverlay("LobbyInvite");
	}

	public void SearchLobbies()
	{
		if (SteamUser.BLoggedOn())
		{
			matchmaking.SearchLobbies();
		}
	}

	public void OpenStore(Hephaestus.DlcId dlcId)
	{
		if (SteamUtils.IsOverlayEnabled())
		{
			switch (dlcId)
			{
			case Hephaestus.DlcId.GLOBADIER:
				SteamFriends.ActivateGameOverlayToStore((AppId_t)434040u, EOverlayToStoreFlag.k_EOverlayToStoreFlag_None);
				break;
			case Hephaestus.DlcId.SMUGGLER:
				SteamFriends.ActivateGameOverlayToStore((AppId_t)434041u, EOverlayToStoreFlag.k_EOverlayToStoreFlag_None);
				break;
			case Hephaestus.DlcId.PRIEST_OF_ULRIC:
				SteamFriends.ActivateGameOverlayToStore((AppId_t)450810u, EOverlayToStoreFlag.k_EOverlayToStoreFlag_None);
				break;
			case Hephaestus.DlcId.DOOMWEAVER:
				SteamFriends.ActivateGameOverlayToStore((AppId_t)450811u, EOverlayToStoreFlag.k_EOverlayToStoreFlag_None);
				break;
			case Hephaestus.DlcId.WITCH_HUNTERS:
				SteamFriends.ActivateGameOverlayToStore((AppId_t)450812u, EOverlayToStoreFlag.k_EOverlayToStoreFlag_None);
				break;
			case Hephaestus.DlcId.UNDEAD:
				SteamFriends.ActivateGameOverlayToStore((AppId_t)534990u, EOverlayToStoreFlag.k_EOverlayToStoreFlag_None);
				break;
			}
		}
	}

	public void OpenCommunity()
	{
		if (SteamUtils.IsOverlayEnabled())
		{
			SteamFriends.ActivateGameOverlayToWebPage("http://steamcommunity.com/app/276810");
		}
	}

	public string GetUserName()
	{
		return SteamFriends.GetPersonaName();
	}

	public ulong GetUserId()
	{
		return (ulong)SteamUser.GetSteamID();
	}

	public string GetOpponentUserName()
	{
		if (PandoraSingleton<MissionStartData>.Exists() && PandoraSingleton<MissionStartData>.Instance.FightingWarbands.Count > 1)
		{
			return PandoraSingleton<MissionStartData>.Instance.FightingWarbands[1].PlayerName;
		}
		return PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_title_player") + " 2";
	}

	public void DisplayOtherPlayerProfile()
	{
	}

	public bool FileExists(string fileName)
	{
		if (SteamRemoteStorage.FileExists(fileName))
		{
			return SteamRemoteStorage.GetFileSize(fileName) > 0;
		}
		return false;
	}

	public long GetFileTimeStamp(string fileName)
	{
		return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(SteamRemoteStorage.GetFileTimestamp(fileName)).Ticks;
	}

	public void RefreshSaveInfo()
	{
		PandoraSingleton<Hephaestus>.Instance.OnRefreshSaveDataDone();
	}

	public void FileWrite(string fileName, byte[] data)
	{
		PandoraSingleton<Hephaestus>.Instance.OnFileWrite(SteamRemoteStorage.FileWrite(fileName, data, data.Length));
	}

	public void FileDelete(string fileName)
	{
		PandoraSingleton<Hephaestus>.Instance.OnFileDelete(SteamRemoteStorage.FileDelete(fileName));
	}

	public void FileRead(string fileName)
	{
		byte[] array = null;
		int num = 0;
		int num2 = 0;
		if (SteamRemoteStorage.FileExists(fileName))
		{
			num = SteamRemoteStorage.GetFileSize(fileName);
			array = new byte[num];
			num2 = SteamRemoteStorage.FileRead(fileName, array, num);
		}
		PandoraSingleton<Hephaestus>.Instance.OnFileRead(array, num == num2);
	}

	public void DisconnectFromUser(ulong steamID)
	{
		networking.CloseP2PSessionWithUser(steamID);
	}

	public void ResetNetwork()
	{
	}

	public void Send(bool reliable, ulong steamID, byte[] data)
	{
		networking.Send(reliable, (CSteamID)steamID, data);
	}

	public void SetDataReceivedCallback(Hephaestus.OnDataReceivedCallback cb)
	{
		networking.SetDataReceivedCallback(cb);
	}

	public void Update()
	{
		if (!requestedStats)
		{
			PandoraDebug.LogDebug("RequestCurrentStats", "STEAM MANAGER");
			requestedStats = SteamUserStats.RequestCurrentStats();
		}
		SteamAPI.RunCallbacks();
		StoreStatsIfNecessary();
		networking.ReadPackets();
	}

	public void UnlockAchievement(Hephaestus.TrophyId achievement)
	{
		SteamUserStats.SetAchievement(achievement.ToLowerString());
		shouldUploadStats = true;
	}

	public void IncrementStat(Hephaestus.StatId stat, int increment)
	{
		int pData = 0;
		SteamUserStats.GetStat(stat.ToLowerString(), out pData);
		SteamUserStats.SetStat(stat.ToLowerString(), pData + increment);
		shouldUploadStats = true;
	}

	public bool IsAchievementUnlocked(Hephaestus.TrophyId achievement)
	{
		bool pbAchieved = false;
		SteamUserStats.GetAchievement(achievement.ToLowerString(), out pbAchieved);
		return pbAchieved;
	}

	private void StoreStatsIfNecessary()
	{
		if (shouldUploadStats)
		{
			bool flag = SteamUserStats.StoreStats();
			shouldUploadStats = !flag;
		}
	}

	private void OnSteamUserStatsReceived(UserStatsReceived_t pCallback)
	{
		PandoraDebug.LogInfo("OnSteamUserStatsReceived", "STEAM MANAGER");
	}

	private void OnSteamUserStatsStored(UserStatsStored_t pCallback)
	{
		PandoraDebug.LogInfo("OnSteamUserStatsStored", "STEAM MANAGER");
		if (pCallback.m_nGameID == 276810)
		{
			if (pCallback.m_eResult == EResult.k_EResultOK)
			{
				PandoraDebug.LogInfo("OnSteamUserStatsStored - Success", "STEAM MANAGER");
			}
			else if (pCallback.m_eResult == EResult.k_EResultInvalidParam)
			{
				PandoraDebug.LogInfo("OnSteamUserStatsStored - Some stats failed to validate", "STEAM MANAGER");
				UserStatsReceived_t pCallback2 = default(UserStatsReceived_t);
				pCallback2.m_eResult = EResult.k_EResultOK;
				pCallback2.m_nGameID = 276810uL;
				OnSteamUserStatsReceived(pCallback2);
			}
		}
		else
		{
			PandoraDebug.LogInfo("OnSteamUserStatsStored - Received event for wrong game_id", "STEAM MANAGER");
		}
	}

	private void OnSteamAchievementStored(UserAchievementStored_t pCallback)
	{
		PandoraDebug.LogInfo("OnSteamAchievementStored", "STEAM MANAGER");
		if (pCallback.m_nGameID != 276810)
		{
		}
	}

	public void RequestNumberOfCurrentPlayers()
	{
		SteamAPICall_t numberOfCurrentPlayers = SteamUserStats.GetNumberOfCurrentPlayers();
		steamCbNumberOfPlayers.Set(numberOfCurrentPlayers, OnNumberOfCurrentPlayers);
	}

	private void OnNumberOfCurrentPlayers(NumberOfCurrentPlayers_t data, bool failure)
	{
		int number = (data.m_bSuccess == 1) ? data.m_cPlayers : 0;
		PandoraSingleton<Hephaestus>.Instance.OnNumberOfCurrentPlayersReceived(number);
	}

	private void OnDLCInstalled(DlcInstalled_t pCallback)
	{
		PandoraDebug.LogInfo("New DLC bought " + pCallback.m_nAppID, "STEAM MANAGER");
		PandoraSingleton<Hephaestus>.Instance.OnDLCBought();
	}

	public bool OwnsDLC(Hephaestus.DlcId dlcId)
	{
		switch (dlcId)
		{
		case Hephaestus.DlcId.GLOBADIER:
			return SteamApps.BIsDlcInstalled((AppId_t)434040u);
		case Hephaestus.DlcId.SMUGGLER:
			return SteamApps.BIsDlcInstalled((AppId_t)434041u);
		case Hephaestus.DlcId.PRIEST_OF_ULRIC:
			return SteamApps.BIsDlcInstalled((AppId_t)450810u);
		case Hephaestus.DlcId.DOOMWEAVER:
			return SteamApps.BIsDlcInstalled((AppId_t)450811u);
		case Hephaestus.DlcId.WITCH_HUNTERS:
			return SteamApps.BIsDlcInstalled((AppId_t)450812u);
		case Hephaestus.DlcId.UNDEAD:
			return SteamApps.BIsDlcInstalled((AppId_t)534990u);
		default:
			return false;
		}
	}

	public bool IsDLCExists(Hephaestus.DlcId dlcId)
	{
		return true;
	}

	public bool ShowVirtualKeyboard(bool multiLine, string title, uint maxChar, string oldText, bool validateString = true)
	{
		oldInputText = oldText;
		EGamepadTextInputLineMode eLineInputMode = EGamepadTextInputLineMode.k_EGamepadTextInputLineModeSingleLine;
		if (multiLine)
		{
			eLineInputMode = EGamepadTextInputLineMode.k_EGamepadTextInputLineModeMultipleLines;
		}
		return SteamUtils.ShowGamepadTextInput(EGamepadTextInputMode.k_EGamepadTextInputModeNormal, eLineInputMode, title, maxChar, oldText);
	}

	private void OnVirtualKeyboardClosed(GamepadTextInputDismissed_t callback)
	{
		if (callback.m_bSubmitted)
		{
			uint enteredGamepadTextLength = SteamUtils.GetEnteredGamepadTextLength();
			if (enteredGamepadTextLength == 0)
			{
				PandoraSingleton<Hephaestus>.Instance.OnVirtualKeyboardCB(success: false, oldInputText);
				return;
			}
			if (SteamUtils.GetEnteredGamepadTextInput(out string pchText, enteredGamepadTextLength + 1))
			{
				PandoraSingleton<Hephaestus>.Instance.OnVirtualKeyboardCB(success: true, pchText);
				return;
			}
		}
		PandoraSingleton<Hephaestus>.Instance.OnVirtualKeyboardCB(success: false, oldInputText);
	}

	public void EngageUser()
	{
	}

	public void LockUserEngagement()
	{
	}

	public void SetRichPresence(Hephaestus.RichPresenceId presId, bool active)
	{
	}

	public void GetUserPicture(Hephaestus.UserPictureSize sizeId)
	{
	}

	public void UpdateGameProgress()
	{
	}

	public bool IsChatRestricted()
	{
		return false;
	}

	public void CheckNetworkServicesAvailability(Action<bool> callback)
	{
		callback(obj: true);
	}

	public bool IsPrivilegeRestricted(Hephaestus.RestrictionId restrictionId)
	{
		return false;
	}

	public void CheckNetworkServicesAvailability(Action<bool, string> callback)
	{
		callback(arg1: true, null);
	}

	public void MultiplayerRoundStart()
	{
	}

	public void MultiplayerRoundEnd()
	{
	}

	public void CheckPendingInvite()
	{
		throw new NotImplementedException();
	}

	public void CanReceiveMessages(Action<bool> cb)
	{
		cb?.Invoke(obj: true);
	}

	public bool IsPlayTogether()
	{
		return false;
	}

	public bool IsPlayTogetherPassive()
	{
		return false;
	}

	public void ResetPlayTogether(bool setPassive)
	{
	}

	public void InitVoiceChat()
	{
	}
}
