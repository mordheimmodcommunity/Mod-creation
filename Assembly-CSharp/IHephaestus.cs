using System;
using System.Collections;
using System.Collections.Generic;

public interface IHephaestus
{
	void OnDestroy();

	void Update();

	bool IsInitialized();

	IEnumerator Init();

	void Reset();

	bool IsOnline();

	string GetOfflineReason();

	void CreateLobby(string name, Hephaestus.LobbyPrivacy privacy);

	void LeaveLobby();

	void JoinLobby(ulong lobbyId);

	void CancelJoinLobby();

	void SetLobbyData(string key, string value);

	void SetLobbyJoinable(bool joinable);

	void InviteFriends();

	void SearchLobbies();

	void OpenStore(Hephaestus.DlcId DLCId);

	void OpenCommunity();

	string GetUserName();

	ulong GetUserId();

	void DisplayOtherPlayerProfile();

	void RefreshSaveInfo();

	long GetFileTimeStamp(string fileName);

	bool FileExists(string fileName);

	void FileRead(string fileName);

	void FileWrite(string fileName, byte[] data);

	void FileDelete(string fileName);

	void DisconnectFromUser(ulong id);

	void ResetNetwork();

	void SetDataReceivedCallback(Hephaestus.OnDataReceivedCallback cb);

	void Send(bool reliable, ulong uId, byte[] data);

	List<SupportedLanguage> GetAvailableLanguages();

	void GetDefaultLocale(Action<SupportedLanguage> callback);

	void IncrementStat(Hephaestus.StatId stat, int increment);

	void UnlockAchievement(Hephaestus.TrophyId achievement);

	bool IsAchievementUnlocked(Hephaestus.TrophyId achievement);

	void UpdateGameProgress();

	void RequestNumberOfCurrentPlayers();

	bool OwnsDLC(Hephaestus.DlcId dlcId);

	bool ShowVirtualKeyboard(bool multiLine, string title, uint maxChar, string oldText, bool validateString = true);

	void LockUserEngagement();

	void EngageUser();

	void SetRichPresence(Hephaestus.RichPresenceId presId, bool active);

	void GetUserPicture(Hephaestus.UserPictureSize sizeId);

	bool IsPrivilegeRestricted(Hephaestus.RestrictionId restrictionId);

	void CheckNetworkServicesAvailability(Action<bool, string> callback);

	void MultiplayerRoundStart();

	void MultiplayerRoundEnd();

	void CheckPendingInvite();

	string GetOpponentUserName();

	bool IsPlayTogether();

	void CanReceiveMessages(Action<bool> cb);

	void ResetPlayTogether(bool setPassive);

	bool IsPlayTogetherPassive();

	void InitVoiceChat();
}
