using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Hephaestus : PandoraSingleton<Hephaestus>
{
	public enum RestrictionId
	{
		CHAT,
		UGC,
		VOICE_CHAT,
		PROFILE_VIEWING
	}

	public enum DlcId
	{
		GLOBADIER,
		SMUGGLER,
		PRIEST_OF_ULRIC,
		DOOMWEAVER,
		WITCH_HUNTERS,
		UNDEAD
	}

	public enum RichPresenceId
	{
		MAIN_MENU,
		HIDEOUT,
		LOBBY_EXHIBITION,
		LOBBY_CONTEST,
		CAMPAIGN_MISSION,
		TUTORIAL_MISSION,
		PROC_MISSION,
		EXHIBITION_AI,
		EXHIBITION_PLAYER,
		CONTEST
	}

	public enum UserPictureSize
	{
		SMALL,
		MEDIUM,
		LARGE,
		EXTRA_LARGE
	}

	public enum LobbyPrivacy
	{
		PRIVATE,
		FRIENDS,
		PUBLIC,
		OFFLINE,
		COUNT
	}

	public enum LobbyConnexionResult
	{
		SUCCESS,
		DOES_NOT_EXIST,
		NOT_ALLOWED,
		BLOCKED_A_MEMBER,
		MEMBER_BLOCKED_YOU,
		LIMITED_USER,
		COMMUNITY_BANNED,
		CLAN_DISABLED,
		BANNED,
		FULL,
		UNEXPECTED_ERROR,
		VERSION_MISMATCH,
		KICKED
	}

	public enum TrophyId
	{
		STORY_SKAVEN_1,
		STORY_MERC_1,
		STORY_POSSESSED_1,
		STORY_SISTERS_1,
		STORY_ALL_1,
		STORY_SKAVEN_2,
		STORY_MERC_2,
		STORY_POSSESSED_2,
		STORY_SISTERS_2,
		STORY_ALL_2,
		SKAVEN_RANK_10,
		MERC_RANK_10,
		POSSESSED_RANK_10,
		SISTERS_RANK_10,
		ALL_RANK_10,
		LEADER_RANK_10_1,
		LEADER_RANK_10_2,
		LEADER_RANK_10_3,
		HERO_RANK_10_1,
		HERO_RANK_10_2,
		HERO_RANK_10_3,
		HENCHMEN_RANK_10_1,
		HENCHMEN_RANK_10_2,
		HENCHMEN_RANK_10_3,
		IMPRESSIVE_RANK_10_1,
		IMPRESSIVE_RANK_10_2,
		IMPRESSIVE_RANK_10_3,
		LEADER_NO_INJURY,
		HERO_NO_INJURY,
		HENCHMEN_NO_INJURY,
		IMPRESSIVE_NO_INJURY,
		SHIPMENT_1,
		SHIPMENT_2,
		SHIPMENT_3,
		WYRDSTONES,
		WYRDSTONE_GOLD,
		WYRDSTONE_WEIGHT,
		SHOP_BUY,
		NORMAL_EQUIP,
		GOOD_EQUIP,
		BEST_EQUIP,
		ENCHANT_EQUIP_1,
		ENCHANT_EQUIP_2,
		RENAME,
		YEAR_1,
		YEAR_5,
		GAME_OVER,
		HIRE_1,
		HIRE_2,
		HIRE_3,
		HIRE_4,
		HIRE_5,
		RECIPIES,
		MULTIPLE_INJURED,
		INJURED_FIRE,
		TREATMENT_NOT_PAID,
		UPKEEP_NOT_PAID,
		MULTIPLE_INJURIES,
		MUTATION_1,
		MUTATION_2,
		RANGE_9M,
		WIN_ALONE,
		WIN_CRIPPLED,
		ONE_SHOT,
		TUTO_1,
		TUTO_2,
		ALTF4,
		WITCH_HUNTERS_10,
		STORY_WITCH_HUNTERS_1,
		STORY_WITCH_HUNTERS_2,
		UNDEAD_10,
		STORY_UNDEAD_1,
		STORY_UNDEAD_2
	}

	public enum StatId
	{
		LEADER_RANK_10,
		HERO_RANK_10,
		HENCHMEN_RANK_10,
		IMPRESSIVE_RANK_10,
		SHIPMENTS,
		WYRDSTONE_SELL,
		WYRDSTONE_GATHER,
		SHOP_GOLD,
		HIRED_WARRIORS,
		UNLOCKED_RECIPES,
		MUTATIONS,
		STUNNED_OOAS,
		OPENED_CHESTS,
		IMPRESSIVE_OOAS,
		STUN_ENEMIES,
		CRITICALS,
		SPELLS_CAST,
		SPELLS_CURSES,
		MY_TOTAL_OOA,
		LOOT_ENEMIES,
		TRAPS,
		MULTI_WINS,
		MULTI_PLAY,
		ENEMIES_OOA,
		GOLD_EARNED,
		MY_TOTAL_INJURIES
	}

	public delegate void OnLobbyCreatedCallback(ulong lobbyId, bool success);

	public delegate void OnLobbyEnteredCallback(ulong lobbyId, bool success);

	public delegate void OnSearchLobbiesCallback();

	public delegate void OnJoinLobbyCallback(LobbyConnexionResult result);

	public delegate void OnSaveDataRefreshed();

	public delegate void OnFileWriteCallback(bool success);

	public delegate void OnFileReadCallback(byte[] data, bool success);

	public delegate void OnFileDeleteCallback(bool success);

	public delegate void OnDataReceivedCallback(ulong fromId, byte[] data);

	public delegate void OnNumberOfPlayersCallback(int number);

	public delegate void OnDLCBoughtCallback();

	public delegate void OnVirtualKeyboardCallback(bool success, string newString);

	public delegate bool WaitDelegate();

	private const float SAVE_DISPLAY_TIME = 2f;

	private IHephaestus client;

	private OnLobbyCreatedCallback lobbyCreatedCb;

	private OnLobbyEnteredCallback lobbyEnteredCb;

	private OnSearchLobbiesCallback searchLobbiesCb;

	private OnJoinLobbyCallback joinLobbyCb;

	private OnSaveDataRefreshed saveRefreshCb;

	private OnFileWriteCallback fileWriteCb;

	private OnFileReadCallback fileReadCb;

	private OnFileDeleteCallback fileDeleteCb;

	private OnNumberOfPlayersCallback nbPlayersCb;

	private OnDLCBoughtCallback dlcBoughtCb;

	private OnVirtualKeyboardCallback vkCallback;

	private UnityAction OnEngagedCallback;

	private UnityAction<Sprite> OnPlayerPictureLoaded;

	private uint numLobbies;

	private Canvas SaveUI;

	private float saveTimer;

	private readonly List<DlcId> dlcNeeded = new List<DlcId>();

	private readonly List<string> dlcNeededLoc = new List<string>();

	private bool isJoiningInvite;

	public Lobby joiningLobby;

	private bool isJoiningLobbyExhibition;

	public Lobby Lobby
	{
		get;
		private set;
	}

	public List<Lobby> Lobbies
	{
		get;
		private set;
	}

	private void Awake()
	{
		Init();
		SaveUI = GetComponent<Canvas>();
		SaveUI.enabled = false;
	}

	public void Init()
	{
		client = new SteamManager();
	}

	public bool ClientLoaded()
	{
		return true;
	}

	private void OnDestroy()
	{
		client.OnDestroy();
	}

	public IEnumerator InitializeClient()
	{
		Lobby = null;
		Lobbies = new List<Lobby>();
		yield return null;
		StopCoroutine(client.Init());
		yield return StartCoroutine(client.Init());
	}

	public bool IsInitialized()
	{
		return client.IsInitialized();
	}

	public void Reset()
	{
		client.Reset();
	}

	public bool UpdateLobby(Lobby lobby)
	{
		bool result = true;
		if (lobby.version != "1.4.4.4" || (lobby.privacy == LobbyPrivacy.FRIENDS && lobby.hostId == 0L))
		{
			PandoraDebug.LogInfo("removing lobby because it is invalid " + lobby.version + " " + lobby.privacy + " " + lobby.hostId);
			RemoveLobby(lobby.id);
			result = false;
		}
		else
		{
			bool flag = false;
			for (int i = 0; i < Lobbies.Count; i++)
			{
				if (Lobbies[i].id == lobby.id)
				{
					PandoraDebug.LogInfo("found lobby " + Lobbies[i].id);
					Lobbies[i].name = lobby.name;
					Lobbies[i].privacy = lobby.privacy;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				PandoraDebug.LogInfo("adding lobby " + lobby.id);
				Lobbies.Add(lobby);
			}
		}
		if (Lobbies.Count == numLobbies && searchLobbiesCb != null)
		{
			searchLobbiesCb();
			searchLobbiesCb = null;
		}
		return result;
	}

	public void RemoveLobby(ulong lobbyId, bool check = false)
	{
		uint num = numLobbies;
		for (int num2 = Lobbies.Count - 1; num2 >= 0; num2--)
		{
			if (Lobbies[num2].id == lobbyId)
			{
				Lobbies.RemoveAt(num2);
				numLobbies--;
			}
		}
		if (num == numLobbies && numLobbies != 0)
		{
			numLobbies--;
		}
		if (check && Lobbies.Count == numLobbies && searchLobbiesCb != null)
		{
			searchLobbiesCb();
			searchLobbiesCb = null;
		}
	}

	public void OnKickFromLobby()
	{
		PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.SKIRMISH_LOBBY_KICKED);
	}

	public bool IsOnline()
	{
		return client.IsOnline();
	}

	public string GetOfflineReason()
	{
		return client.GetOfflineReason();
	}

	public void CreateLobby(string name, LobbyPrivacy privacy, OnLobbyCreatedCallback callback)
	{
		Lobby = null;
		lobbyCreatedCb = callback;
		if (!IsOnline())
		{
			privacy = LobbyPrivacy.OFFLINE;
		}
		client.CreateLobby(name, privacy);
	}

	public void OnCreateLobby(ulong lobbyId, bool success)
	{
		if (success)
		{
			for (int i = 0; i < Lobbies.Count; i++)
			{
				if (Lobbies[i].id == lobbyId)
				{
					Lobby = Lobbies[i];
				}
			}
		}
		lobbyCreatedCb(lobbyId, success);
		lobbyCreatedCb = null;
	}

	public void LeaveLobby()
	{
		if (Lobby != null)
		{
			if (PandoraSingleton<Hermes>.Instance.IsHost())
			{
				client.SetLobbyJoinable(joinable: false);
			}
			client.LeaveLobby();
			Lobby = null;
		}
	}

	public void JoinLobby(ulong lobbyId, OnJoinLobbyCallback callback, Hermes.OnConnectedCallback hermesCb)
	{
		PandoraSingleton<Hermes>.Instance.connectedCallback = hermesCb;
		joinLobbyCb = callback;
		for (int i = 0; i < Lobbies.Count; i++)
		{
			if (Lobbies[i].id == lobbyId)
			{
				Lobby = Lobbies[i];
				break;
			}
		}
		if (Lobby == null)
		{
			Lobby = new Lobby();
			Lobby.id = lobbyId;
		}
		client.JoinLobby(lobbyId);
	}

	public void CancelJoinLobby()
	{
		PandoraSingleton<Hermes>.instance.connectedCallback = null;
		client.CancelJoinLobby();
	}

	public void OnJoinLobby(ulong ownerId, LobbyConnexionResult connexionResult)
	{
		if (Lobby != null)
		{
			Lobby.hostId = ownerId;
		}
		if (joinLobbyCb != null)
		{
			joinLobbyCb(connexionResult);
			joinLobbyCb = null;
		}
	}

	public void SetLobbyData(string key, string value)
	{
		client.SetLobbyData(key, value);
	}

	public void SetLobbyJoinable(bool joinable)
	{
		client.SetLobbyJoinable(joinable);
	}

	public void SearchLobbies(OnSearchLobbiesCallback callback)
	{
		searchLobbiesCb = callback;
		Lobbies.Clear();
		if (IsOnline())
		{
			client.SearchLobbies();
		}
	}

	public void OnSearchLobbies(uint lobbiesCount)
	{
		PandoraDebug.LogInfo("OnSearchLobbies to call callback", "Hephaestus");
		numLobbies = lobbiesCount;
		if (numLobbies == 0 && searchLobbiesCb != null)
		{
			searchLobbiesCb();
			searchLobbiesCb = null;
		}
	}

	public void InviteFriends()
	{
		if (Lobby != null)
		{
			client.InviteFriends();
		}
	}

	public void InitVoiceChat()
	{
		client.InitVoiceChat();
	}

	public void OpenStore(DlcId appId)
	{
		client.OpenStore(appId);
	}

	public void OpenCommunity()
	{
		client.OpenCommunity();
	}

	public string GetUserName()
	{
		return client.GetUserName();
	}

	public string GetOpponentUserName()
	{
		return client.GetOpponentUserName();
	}

	public ulong GetUserId()
	{
		return client.GetUserId();
	}

	public void DisplayOtherPlayerProfile()
	{
		client.DisplayOtherPlayerProfile();
	}

	public void CanReceiveMessages(Action<bool> cb)
	{
		client.CanReceiveMessages(cb);
	}

	public void RefreshSaveData(OnSaveDataRefreshed cb)
	{
		saveRefreshCb = cb;
		client.RefreshSaveInfo();
	}

	public void OnRefreshSaveDataDone()
	{
		PandoraDebug.LogInfo("Save Data Refreshed", "HEPHAESTUS");
		if (saveRefreshCb != null)
		{
			saveRefreshCb();
			saveRefreshCb = null;
		}
	}

	public void FileWrite(string fileName, byte[] data, OnFileWriteCallback callback)
	{
		fileWriteCb = callback;
		client.FileWrite(fileName, data);
		DisplaySaveLogo();
	}

	public void OnFileWrite(bool success)
	{
		fileWriteCb(success);
	}

	public void FileRead(string fileName, OnFileReadCallback callback)
	{
		fileReadCb = callback;
		client.FileRead(fileName);
	}

	public void OnFileRead(byte[] data, bool success)
	{
		PandoraDebug.LogInfo("On File Read " + success, "HEPHAESTUS");
		if (fileReadCb != null)
		{
			fileReadCb(data, success);
		}
	}

	public void FileDelete(string fileName, OnFileDeleteCallback callback)
	{
		fileDeleteCb = callback;
		client.FileDelete(fileName);
	}

	public void OnFileDelete(bool success)
	{
		if (fileDeleteCb != null)
		{
			fileDeleteCb(success);
		}
	}

	public bool FileExists(string fileName)
	{
		return client.FileExists(fileName);
	}

	public DateTime GetFileTimeStamp(string fileName)
	{
		return new DateTime(client.GetFileTimeStamp(fileName), DateTimeKind.Local);
	}

	public void DisconnectFromUser(ulong userId)
	{
		client.DisconnectFromUser(userId);
	}

	public void ResetNetwork()
	{
		client.ResetNetwork();
	}

	public void SetDataReceivedCallback(OnDataReceivedCallback cb)
	{
		if (client != null)
		{
			client.SetDataReceivedCallback(cb);
		}
	}

	public void SendData(bool reliable, ulong toid, byte[] data)
	{
		client.Send(reliable, toid, data);
	}

	public void InitDefaultLocale()
	{
		client.GetDefaultLocale(delegate(SupportedLanguage lang)
		{
			PandoraDebug.LogDebug("Changing Language to : " + lang.ToString(), "HEPHAESTUS");
			PandoraSingleton<LocalizationManager>.Instance.SetLanguage(lang, force: true);
			PandoraSingleton<GameManager>.Instance.Options.language = (int)lang;
		});
	}

	public List<SupportedLanguage> GetAvailableLanguages()
	{
		return client.GetAvailableLanguages();
	}

	private void Update()
	{
		if (client != null)
		{
			client.Update();
		}
		if (saveTimer > 0f)
		{
			SaveUI.enabled = true;
			saveTimer -= Time.smoothDeltaTime;
			if (saveTimer <= 0f)
			{
				SaveUI.enabled = false;
			}
		}
	}

	private void DisplaySaveLogo()
	{
		saveTimer = 2f;
	}

	public void IncrementStat(StatId stat, int increment)
	{
		client.IncrementStat(stat, increment);
	}

	public void UnlockAchievement(TrophyId achievement)
	{
		client.UnlockAchievement(achievement);
	}

	public bool IsAchievementUnlocked(TrophyId achievement)
	{
		return client.IsAchievementUnlocked(achievement);
	}

	public void UpdateGameProgress()
	{
		client.UpdateGameProgress();
	}

	public void RequestNumberOfCurrentPlayers(OnNumberOfPlayersCallback callback)
	{
		nbPlayersCb = callback;
		client.RequestNumberOfCurrentPlayers();
	}

	public void OnNumberOfCurrentPlayersReceived(int number)
	{
		if (nbPlayersCb != null)
		{
			nbPlayersCb(number);
		}
	}

	public bool OwnsDLC(DlcId dlcId)
	{
		return client.OwnsDLC(dlcId);
	}

	public void SetDLCBoughtCb(OnDLCBoughtCallback cb)
	{
		dlcBoughtCb = cb;
	}

	public void OnDLCBought()
	{
		if (dlcBoughtCb != null)
		{
			dlcBoughtCb();
		}
	}

	public bool ShowVirtualKeyboard(string title, string oldText, uint maxChar, bool multiLine, OnVirtualKeyboardCallback vkCb, bool validateString = true)
	{
		if (client.ShowVirtualKeyboard(multiLine, title, maxChar, oldText, validateString))
		{
			vkCallback = vkCb;
			return true;
		}
		return false;
	}

	public void OnVirtualKeyboardCB(bool success, string str)
	{
		if (vkCallback != null)
		{
			vkCallback(success, str);
			vkCallback = null;
		}
	}

	public void LockUserEngagement()
	{
		client.LockUserEngagement();
	}

	public void EngageUser(UnityAction cb)
	{
		OnEngagedCallback = cb;
		client.EngageUser();
	}

	public void OnUserEngaged()
	{
		if (OnEngagedCallback != null)
		{
			UnityAction onEngagedCallback = OnEngagedCallback;
			OnEngagedCallback = null;
			onEngagedCallback();
		}
	}

	public void SetRichPresence(RichPresenceId presId, bool active)
	{
		client.SetRichPresence(presId, active);
	}

	public void GetUserPicture(UserPictureSize size, UnityAction<Sprite> pictureLoaded)
	{
		OnPlayerPictureLoaded = pictureLoaded;
		client.GetUserPicture(size);
	}

	public void PlayerPictureLoaded(Sprite sprite)
	{
		if (OnPlayerPictureLoaded != null)
		{
			OnPlayerPictureLoaded(sprite);
		}
	}

	public bool IsPrivilegeRestricted(RestrictionId restrictionId)
	{
		return client.IsPrivilegeRestricted(restrictionId);
	}

	public void CheckNetworkServicesAvailability(Action<bool, string> callback)
	{
		client.CheckNetworkServicesAvailability(callback);
	}

	public void Delay(WaitDelegate whileCondition, Action cb)
	{
		StartCoroutine(Delayer(whileCondition, cb));
	}

	private IEnumerator Delayer(WaitDelegate whileCondition, Action cb)
	{
		while (whileCondition())
		{
			yield return null;
		}
		cb();
	}

	public void MultiplayerRoundStart()
	{
		client.MultiplayerRoundStart();
	}

	public void MultiplayerRoundEnd()
	{
		client.MultiplayerRoundEnd();
	}

	public bool ValidateWarbandDLC(WarbandSave warband)
	{
		dlcNeeded.Clear();
		dlcNeededLoc.Clear();
		switch (warband.id)
		{
		case 17:
			if (!PandoraSingleton<Hephaestus>.Instance.OwnsDLC(DlcId.WITCH_HUNTERS))
			{
				dlcNeeded.Add(DlcId.WITCH_HUNTERS);
				dlcNeededLoc.Add("Witch Hunters");
			}
			break;
		case 18:
			if (!PandoraSingleton<Hephaestus>.Instance.OwnsDLC(DlcId.UNDEAD))
			{
				dlcNeeded.Add(DlcId.UNDEAD);
				dlcNeededLoc.Add("Undead");
			}
			break;
		}
		for (int i = 0; i < warband.units.Count; i++)
		{
			UnitSave unitSave = warband.units[i];
			switch (unitSave.stats.id)
			{
			case 99:
				if (!PandoraSingleton<Hephaestus>.Instance.OwnsDLC(DlcId.SMUGGLER) && !dlcNeeded.Contains(DlcId.SMUGGLER))
				{
					dlcNeeded.Add(DlcId.SMUGGLER);
					dlcNeededLoc.Add("Smuggler");
				}
				break;
			case 98:
				if (!PandoraSingleton<Hephaestus>.Instance.OwnsDLC(DlcId.GLOBADIER) && !dlcNeeded.Contains(DlcId.GLOBADIER))
				{
					dlcNeeded.Add(DlcId.GLOBADIER);
					dlcNeededLoc.Add("Globadier");
				}
				break;
			case 102:
				if (!PandoraSingleton<Hephaestus>.Instance.OwnsDLC(DlcId.DOOMWEAVER) && !dlcNeeded.Contains(DlcId.DOOMWEAVER))
				{
					dlcNeeded.Add(DlcId.DOOMWEAVER);
					dlcNeededLoc.Add("Doomweaver");
				}
				break;
			case 101:
				if (!PandoraSingleton<Hephaestus>.Instance.OwnsDLC(DlcId.PRIEST_OF_ULRIC) && !dlcNeeded.Contains(DlcId.PRIEST_OF_ULRIC))
				{
					dlcNeeded.Add(DlcId.PRIEST_OF_ULRIC);
					dlcNeededLoc.Add("Wolf-Priest of Ulric");
				}
				break;
			}
		}
		if (dlcNeededLoc.Count > 0)
		{
			PandoraSingleton<GameManager>.Instance.ShowSystemPopup(GameManager.SystemPopupId.DLC, "com_wb_title_dlc", "popup_dlc_needed_desc", string.Join(", ", dlcNeededLoc.ToArray()), null);
		}
		return dlcNeededLoc.Count == 0;
	}

	public void JoinInvite()
	{
		if (joiningLobby != null)
		{
			Lobby = joiningLobby;
			ResetInvite();
			PandoraSingleton<SkirmishManager>.Instance.JoinLobby(Lobby.id, Lobby.name, Lobby.isExhibition, Lobby.ratingMin, Lobby.ratingMax);
		}
	}

	public void CheckPendingInvite()
	{
		client.CheckPendingInvite();
	}

	public void ReceiveInvite(Lobby lobby)
	{
		PandoraDebug.LogDebug("Receive invite " + lobby.name + " " + lobby.isExhibition);
		isJoiningInvite = true;
		joiningLobby = lobby;
		PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.SKIRMISH_INVITE_ACCEPTED);
	}

	public void ReceiveInvite(ulong lobbyId, string lobbyName, bool exhibition, int ratingMin, int ratingMax)
	{
		Lobby lobby = new Lobby();
		lobby.id = lobbyId;
		lobby.isExhibition = exhibition;
		lobby.name = lobbyName;
		lobby.ratingMin = ratingMin;
		lobby.ratingMax = ratingMax;
		ReceiveInvite(lobby);
	}

	public void ResetInvite()
	{
		isJoiningInvite = false;
		joiningLobby = null;
	}

	public bool IsJoiningInvite()
	{
		return isJoiningInvite;
	}

	public Lobby GetJoiningLobby()
	{
		return joiningLobby;
	}

	public bool IsPlayTogether()
	{
		return client.IsPlayTogether();
	}

	public bool IsPlayTogetherPassive()
	{
		return client.IsPlayTogetherPassive();
	}

	public void ResetPlayTogether(bool setPassive)
	{
		client.ResetPlayTogether(setPassive);
	}

	public void OnResume()
	{
		if (PandoraSingleton<HideoutManager>.Exists())
		{
			if (PandoraSingleton<HideoutManager>.Instance.StateMachine.GetActiveStateId() == 3 || PandoraSingleton<HideoutManager>.Instance.StateMachine.GetActiveStateId() == 4)
			{
				PandoraSingleton<HideoutManager>.Instance.StateMachine.ChangeState(3);
			}
		}
		else if (PandoraSingleton<MissionManager>.Exists() && PandoraSingleton<MissionManager>.Instance.GetPlayersCount() == 2)
		{
			PandoraSingleton<MissionManager>.Instance.OnConnectionLost(isResume: true);
		}
	}
}
