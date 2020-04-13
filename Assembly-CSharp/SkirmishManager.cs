using System;
using System.Collections.Generic;
using UnityEngine;

public class SkirmishManager : PandoraSingleton<SkirmishManager>
{
    private const float MAX_LOBBY_CONNEXION_DELAY = 30f;

    public SkirmishCreatePopup createPopup;

    public SkirmishWarbandPopup warbandPopup;

    public ConfirmationPopupView messagePopup;

    public List<SkirmishMap> skirmishMaps = new List<SkirmishMap>();

    public bool ready;

    private Action cancelPopupCallback;

    private float lobbyJoinTimer;

    private bool joiningLobby;

    private AudioClip playerJoinSound;

    private AudioClip playerLeaveSound;

    public List<int> unitsPosition;

    private Coroutine getProcWarbandCoroutine;

    private void Start()
    {
        createPopup.gameObject.SetActive(value: false);
        warbandPopup.gameObject.SetActive(value: false);
        messagePopup.gameObject.SetActive(value: false);
        List<DeploymentScenarioMapLayoutData> list = PandoraSingleton<DataFactory>.Instance.InitData<DeploymentScenarioMapLayoutData>("skirmish", "1");
        skirmishMaps.Clear();
        List<MissionMapId> list2 = new List<MissionMapId>();
        foreach (DeploymentScenarioMapLayoutData item in list)
        {
            if (list2.IndexOf(item.MissionMapId, MissionMapIdComparer.Instance) == -1)
            {
                list2.Add(item.MissionMapId);
                SkirmishMap skirmishMap = new SkirmishMap();
                skirmishMap.mapData = PandoraSingleton<DataFactory>.Instance.InitData<MissionMapData>((int)item.MissionMapId);
                skirmishMaps.Add(skirmishMap);
            }
        }
        skirmishMaps.Sort(new SkirmishMapSorter());
        foreach (SkirmishMap skirmishMap2 in skirmishMaps)
        {
            skirmishMap2.layouts = PandoraSingleton<DataFactory>.Instance.InitData<MissionMapLayoutData>("fk_mission_map_id", ((int)skirmishMap2.mapData.Id).ToString());
            skirmishMap2.gameplays = PandoraSingleton<DataFactory>.Instance.InitData<MissionMapGameplayData>("fk_mission_map_id", ((int)skirmishMap2.mapData.Id).ToString());
            skirmishMap2.deployments = new List<DeploymentInfo>();
            foreach (DeploymentScenarioMapLayoutData item2 in list)
            {
                if (item2.MissionMapId == skirmishMap2.mapData.Id)
                {
                    DeploymentInfo deploymentInfo = new DeploymentInfo();
                    deploymentInfo.scenarioData = PandoraSingleton<DataFactory>.Instance.InitData<DeploymentScenarioData>((int)item2.DeploymentScenarioId);
                    List<DeploymentScenarioSlotData> list3 = PandoraSingleton<DataFactory>.Instance.InitData<DeploymentScenarioSlotData>("fk_deployment_scenario_id", ((int)item2.DeploymentScenarioId).ToString());
                    deploymentInfo.slots = new List<DeploymentData>();
                    foreach (DeploymentScenarioSlotData item3 in list3)
                    {
                        deploymentInfo.slots.Add(PandoraSingleton<DataFactory>.Instance.InitData<DeploymentData>((int)item3.DeploymentId));
                    }
                    skirmishMap2.deployments.Add(deploymentInfo);
                }
            }
        }
        PandoraSingleton<MissionStartData>.Instance.RegisterToHermes();
        PandoraSingleton<Pan>.Instance.GetSound("turn_begin", cache: true, delegate (AudioClip clip)
        {
            playerJoinSound = clip;
        });
        PandoraSingleton<Pan>.Instance.GetSound("turn_end", cache: true, delegate (AudioClip clip)
        {
            playerLeaveSound = clip;
        });
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.SKIRMISH_LOBBY_JOINED, OnLobbyEntered);
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.SKIRMISH_LOBBY_LEFT, OnLobbyLeft);
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.SKIRMISH_LOBBY_KICKED, LeaveLobby);
    }

    public void BuildUnitPosition()
    {
        Warband warband = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband;
        unitsPosition = new List<int>(new int[warband.Units.Count]);
        for (int i = 0; i < warband.Units.Count; i++)
        {
            unitsPosition[i] = warband.Units[i].UnitSave.warbandSlotIndex;
        }
    }

    private void OnDestroy()
    {
        PandoraSingleton<MissionStartData>.Instance.RemoveFromHermes();
    }

    private void Update()
    {
        if (joiningLobby && Time.time > lobbyJoinTimer)
        {
            CancelJoinLobby();
            messagePopup.Show("join_lobby_title_timed_out", "join_lobby_desc_timed_out", null);
        }
    }

    public void OnCreateGame(bool exhibition = true, Action onCancelCreateGame = null, bool silent = false)
    {
        cancelPopupCallback = onCancelCreateGame;
        if (exhibition)
        {
            BuildUnitPosition();
            int skirmishRating = PandoraSingleton<HideoutManager>.instance.WarbandCtrlr.GetSkirmishRating(unitsPosition);
            if (!silent)
            {
                createPopup.Show("menu_skirmish_create_game", (!PandoraSingleton<Hephaestus>.Instance.IsOnline()) ? "lobby_choose_privacy_and_name_offline" : "lobby_choose_privacy_and_name_online", allowOffline: true, skirmishRating, OnCreateExhibitionPopup);
                return;
            }
            string stringById = PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_name_default", PandoraSingleton<Hephaestus>.Instance.GetUserName());
            ShowJoinPopup(stringById);
            PandoraSingleton<Hephaestus>.Instance.CreateLobby(stringById, Hephaestus.LobbyPrivacy.PRIVATE, OnPlayTogetherExhibitionLobbyCreated);
            return;
        }
        unitsPosition = null;
        int skirmishRating2 = PandoraSingleton<HideoutManager>.instance.WarbandCtrlr.GetSkirmishRating(unitsPosition);
        if (PandoraSingleton<Hephaestus>.Instance.IsOnline())
        {
            if (!silent)
            {
                createPopup.Show("menu_skirmish_create_game", "lobby_choose_privacy_and_name_online", allowOffline: false, skirmishRating2, OnCreateContestPopup);
                return;
            }
            string stringById2 = PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_name_default", PandoraSingleton<Hephaestus>.Instance.GetUserName());
            ShowJoinPopup(stringById2);
            PandoraSingleton<Hephaestus>.Instance.CreateLobby(stringById2, Hephaestus.LobbyPrivacy.PRIVATE, OnPlayTogetherContestLobbyCreated);
        }
        else
        {
            messagePopup.Show("menu_skirmish_create_game", "menu_skirmish_cant_contest_offline", delegate
            {
                cancelPopupCallback();
            });
        }
    }

    private void OnPlayTogetherContestLobbyCreated(ulong lobbyid, bool success)
    {
        OnLobbyCreatedCallback(lobbyid, success, isExhibition: false, "100", "5000");
    }

    private void OnPlayTogetherExhibitionLobbyCreated(ulong lobbyid, bool success)
    {
        OnLobbyCreatedCallback(lobbyid, success, isExhibition: true, "100", "5000");
    }

    private void OnCreateExhibitionPopup(bool isConfirm)
    {
        OnCreatePopup(isConfirm, isExhibition: true, OnLobbyExhibitionCreatedCallback);
    }

    private void OnCreateContestPopup(bool isConfirm)
    {
        if (PandoraSingleton<Hephaestus>.Instance.IsOnline())
        {
            OnCreatePopup(isConfirm, isExhibition: false, OnLobbyContestCreatedCallback);
        }
        else
        {
            messagePopup.Show("menu_skirmish_create_game", "menu_skirmish_cant_contest_offline", delegate
            {
                cancelPopupCallback();
            });
        }
    }

    private void OnCreatePopup(bool isConfirm, bool isExhibition, Hephaestus.OnLobbyCreatedCallback lobbyCreateCb)
    {
        if (isConfirm)
        {
            PandoraDebug.LogInfo("OnCreatePopup - confirmed = ", "SKIRMISH");
            int curSel = createPopup.lobbyPrivacy.CurSel;
            string text = createPopup.lobbyName.get_textComponent().get_text();
            PandoraSingleton<Hephaestus>.Instance.CreateLobby(text, (Hephaestus.LobbyPrivacy)curSel, lobbyCreateCb);
            if (PandoraSingleton<Hephaestus>.Instance.IsOnline() && curSel != 3)
            {
                ShowJoinPopup(text);
            }
        }
        else if (cancelPopupCallback != null)
        {
            cancelPopupCallback();
        }
    }

    private void ShowJoinPopup(string lobbyName = null)
    {
        string text = lobbyName ?? createPopup.lobbyName.get_textComponent().get_text();
        if (string.IsNullOrEmpty(text))
        {
            text = " ";
        }
        string stringById = PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_joining_desc", text);
        string stringById2 = PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_joining_title");
        messagePopup.ShowLocalized(stringById2, stringById, null, hideButtons: true);
    }

    private void OnLobbyExhibitionCreatedCallback(ulong lobbyId, bool success)
    {
        string ratingMin = createPopup.GetRatingMin();
        string ratingMax = createPopup.GetRatingMax();
        OnLobbyCreatedCallback(lobbyId, success, isExhibition: true, ratingMin, ratingMax);
    }

    private void OnLobbyContestCreatedCallback(ulong lobbyId, bool success)
    {
        string ratingMin = createPopup.GetRatingMin();
        string ratingMax = createPopup.GetRatingMax();
        OnLobbyCreatedCallback(lobbyId, success, isExhibition: false, ratingMin, ratingMax);
    }

    private void OnLobbyCreatedCallback(ulong lobbyId, bool success, bool isExhibition, string ratingMin, string ratingMax)
    {
        if (success)
        {
            PandoraSingleton<Hermes>.Instance.StartHosting();
            if (isExhibition)
            {
                BuildUnitPosition();
            }
            else
            {
                unitsPosition = null;
            }
            PandoraSingleton<MissionStartData>.Instance.InitSkirmish(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr, unitsPosition, isExhibition);
            PandoraSingleton<Hephaestus>.Instance.SetLobbyData("warband", ((int)PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.Id).ToLowerString());
            PandoraSingleton<Hephaestus>.Instance.SetLobbyData("exhibition", isExhibition.ToString());
            PandoraSingleton<Hephaestus>.Instance.SetLobbyData("rating_min", ratingMin);
            PandoraSingleton<Hephaestus>.Instance.SetLobbyData("rating_max", ratingMax);
            HideJoinPopup();
            PandoraSingleton<HideoutManager>.instance.StateMachine.ChangeState(4);
        }
        else
        {
            HideJoinPopup();
            messagePopup.Show("join_lobby_title_failed_to_create_lobby", "join_lobby_desc_failed_to_create_lobby", null);
        }
    }

    public void OnLobbyEntered()
    {
        ulong userId = (ulong)PandoraSingleton<NoticeManager>.Instance.Parameters[0];
        ready = false;
        PandoraSingleton<Hermes>.Instance.NewConnection(userId);
        if (PandoraSingleton<MissionStartData>.Instance.FightingWarbands.Count > 1 || !PandoraSingleton<Hephaestus>.Instance.Lobby.joinable)
        {
            PandoraSingleton<MissionStartData>.Instance.SendKickPlayer(9);
            return;
        }
        PandoraDebug.LogDebug("OnLobbyEntered", "SKIRMISH");
        if (playerJoinSound != null)
        {
            PandoraSingleton<HideoutTabManager>.Instance.audioSource.PlayOneShot(playerJoinSound);
        }
        PandoraSingleton<Hephaestus>.Instance.SetLobbyJoinable(joinable: false);
    }

    public void OnLobbyLeft()
    {
        PandoraDebug.LogDebug("OnLobbyLeft", "SKIRMISH");
        if (playerLeaveSound != null)
        {
            PandoraSingleton<HideoutTabManager>.Instance.audioSource.PlayOneShot(playerLeaveSound);
        }
        ulong num = (ulong)PandoraSingleton<NoticeManager>.Instance.Parameters[0];
        ulong userId = PandoraSingleton<Hephaestus>.Instance.GetUserId();
        if (PandoraSingleton<Hermes>.Instance.IsHost() && num != userId)
        {
            PandoraSingleton<Hermes>.Instance.RemoveConnection(num);
            if (PandoraSingleton<MissionStartData>.Instance.FightingWarbands.Count > 1 && PandoraSingleton<MissionStartData>.Instance.FightingWarbands[1].PlayerTypeId == PlayerTypeId.PLAYER)
            {
                RemoveOpponent();
            }
        }
        else
        {
            LeaveLobby();
        }
    }

    public void OnKick(Hephaestus.LobbyConnexionResult result)
    {
        PandoraDebug.LogDebug("OnKick", "SKIRMISH");
        HideJoinPopup();
        ShowPopup(result);
        LeaveLobby();
    }

    public void LeaveLobby()
    {
        PandoraSingleton<Hephaestus>.Instance.LeaveLobby();
        PandoraSingleton<Hermes>.Instance.StopConnections();
        if (PandoraSingleton<HideoutManager>.Instance.StateMachine.GetActiveStateId() == 3)
        {
            HideJoinPopup();
        }
    }

    public void JoinLobby(int index)
    {
        JoinLobby(PandoraSingleton<Hephaestus>.Instance.Lobbies[index].id, PandoraSingleton<Hephaestus>.Instance.Lobbies[index].name, PandoraSingleton<Hephaestus>.Instance.Lobbies[index].isExhibition, PandoraSingleton<Hephaestus>.Instance.Lobbies[index].ratingMin, PandoraSingleton<Hephaestus>.Instance.Lobbies[index].ratingMax);
    }

    public void JoinLobby(ulong lobbyId, string lobbyName, bool isExhibition = false, int ratingMin = 0, int ratingMax = 5000)
    {
        if (!isExhibition && !IsContestAvailable(out string reason))
        {
            messagePopup.ShowLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_joining_title"), PandoraSingleton<LocalizationManager>.Instance.GetStringById(reason), null);
        }
        else if (PandoraSingleton<Hephaestus>.Instance.IsOnline())
        {
            PandoraDebug.LogDebug("Joining lobbyId:" + lobbyId + " (name:" + lobbyName + ")", "SKIRMISH");
            if (string.IsNullOrEmpty(lobbyName))
            {
                lobbyName = ".";
            }
            if (isExhibition)
            {
                BuildUnitPosition();
            }
            else
            {
                unitsPosition = null;
            }
            if (PandoraUtils.IsBetween(PandoraSingleton<HideoutManager>.instance.WarbandCtrlr.GetSkirmishRating(unitsPosition), ratingMin, ratingMax))
            {
                messagePopup.ShowLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_joining_title"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_joining_desc", lobbyName), OnJoiningLobbyPopupCancel);
                joiningLobby = true;
                lobbyJoinTimer = Time.time + 30f;
                PandoraSingleton<Hephaestus>.Instance.JoinLobby(lobbyId, OnJoinLobbyCallback, OnServerConnect);
            }
            else
            {
                messagePopup.ShowLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_joining_title"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_rating_range", ratingMin.ToConstantString(), ratingMax.ToConstantString()), null);
            }
        }
        else
        {
            messagePopup.ShowLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_joining_title"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_status_offline"), null);
        }
    }

    private void OnJoinLobbyCallback(Hephaestus.LobbyConnexionResult result)
    {
        if (result != 0)
        {
            HideJoinPopup();
            joiningLobby = false;
            ShowPopup(result);
        }
        else
        {
            ready = false;
        }
    }

    public void ShowPopup(Hephaestus.LobbyConnexionResult result)
    {
        string textId;
        string titleId;
        switch (result)
        {
            case Hephaestus.LobbyConnexionResult.DOES_NOT_EXIST:
                textId = "join_lobby_desc_no_longer_exist";
                titleId = "join_lobby_title_no_longer_exist";
                break;
            case Hephaestus.LobbyConnexionResult.FULL:
                textId = "join_lobby_desc_full";
                titleId = "join_lobby_title_full";
                break;
            case Hephaestus.LobbyConnexionResult.NOT_ALLOWED:
                textId = "join_lobby_desc_not_allowed";
                titleId = "join_lobby_title_not_allowed";
                break;
            case Hephaestus.LobbyConnexionResult.BLOCKED_A_MEMBER:
                textId = "join_lobby_desc_blocked_a_member";
                titleId = "join_lobby_title_blocked_a_member";
                break;
            case Hephaestus.LobbyConnexionResult.MEMBER_BLOCKED_YOU:
                textId = "join_lobby_desc_member_blocked_you";
                titleId = "join_lobby_title_member_blocked_you";
                break;
            case Hephaestus.LobbyConnexionResult.LIMITED_USER:
                textId = "join_lobby_desc_limited_user";
                titleId = "join_lobby_title_limited_user";
                break;
            case Hephaestus.LobbyConnexionResult.COMMUNITY_BANNED:
                textId = "join_lobby_desc_community_banned";
                titleId = "join_lobby_title_community_banned";
                break;
            case Hephaestus.LobbyConnexionResult.CLAN_DISABLED:
                textId = "join_lobby_desc_clan_disabled";
                titleId = "join_lobby_title_clan_disabled";
                break;
            case Hephaestus.LobbyConnexionResult.BANNED:
                textId = "join_lobby_desc_banned";
                titleId = "join_lobby_title_banned";
                break;
            case Hephaestus.LobbyConnexionResult.UNEXPECTED_ERROR:
                textId = "join_lobby_desc_unknown_error";
                titleId = "join_lobby_title_unknown_error";
                break;
            case Hephaestus.LobbyConnexionResult.VERSION_MISMATCH:
                textId = "join_lobby_desc_version_mismatch";
                titleId = "join_lobby_title_version_mismatch";
                break;
            case Hephaestus.LobbyConnexionResult.KICKED:
                textId = "join_lobby_desc_kicked";
                titleId = "join_lobby_title_kicked";
                break;
            default:
                textId = "join_lobby_desc_unknown_error";
                titleId = "join_lobby_title_unknown_error";
                break;
        }
        messagePopup.Show(titleId, textId, null);
    }

    private void OnJoiningLobbyPopupCancel(bool ok)
    {
        CancelJoinLobby();
    }

    private void CancelJoinLobby()
    {
        joiningLobby = false;
        HideJoinPopup();
        PandoraSingleton<Hephaestus>.Instance.CancelJoinLobby();
    }

    public void SendReady()
    {
        ready = !ready;
        PandoraSingleton<MissionStartData>.Instance.SendReady(ready);
    }

    private void OnServerConnect()
    {
        PandoraDebug.LogDebug("OnServerConnect");
        joiningLobby = false;
        HideJoinPopup();
        PandoraSingleton<MissionStartData>.Instance.OnNetworkConnected(PandoraSingleton<HideoutManager>.instance.WarbandCtrlr, unitsPosition);
        PandoraSingleton<HideoutManager>.Instance.StateMachine.ChangeState(4);
    }

    private void HideJoinPopup()
    {
        messagePopup.Hide();
    }

    public void AddAIOpponent(WarbandData wbData = null)
    {
        PandoraSingleton<Hephaestus>.Instance.SetLobbyJoinable(joinable: false);
        bool impressive = false;
        int num = 0;
        int num2 = 0;
        MissionWarbandSave missionWarbandSave = PandoraSingleton<MissionStartData>.Instance.FightingWarbands[0];
        for (int i = 0; i < missionWarbandSave.Units.Count; i++)
        {
            num2 = Mathf.Max(num2, PandoraSingleton<DataFactory>.Instance.InitData<UnitRankData>((int)missionWarbandSave.Units[i].rankId).Rank);
            int id = missionWarbandSave.Units[i].stats.id;
            UnitTypeId unitTypeId = PandoraSingleton<DataFactory>.Instance.InitData<UnitData>(id).UnitTypeId;
            switch (Unit.GetUnitTypeId(missionWarbandSave.Units[i], unitTypeId))
            {
                case UnitTypeId.IMPRESSIVE:
                    impressive = true;
                    break;
                case UnitTypeId.HERO_1:
                case UnitTypeId.HERO_2:
                case UnitTypeId.HERO_3:
                    num++;
                    break;
            }
        }
        if (getProcWarbandCoroutine != null)
        {
            StopCoroutine(getProcWarbandCoroutine);
        }
        if (wbData == null)
        {
            List<WarbandData> list = PandoraSingleton<DataFactory>.Instance.InitData<WarbandData>("basic", "1");
            int index = PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0, list.Count);
            wbData = list[index];
        }
        string stringById = PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_loading");
        string stringById2 = PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_title_ai");
        PandoraSingleton<MissionStartData>.Instance.AddFightingWarband(wbData.Id, CampaignWarbandId.NONE, stringById, stringById, stringById2, missionWarbandSave.Rank, missionWarbandSave.Rating, 1, PlayerTypeId.AI, new string[0]);
        getProcWarbandCoroutine = StartCoroutine(Mission.GetProcWarband(missionWarbandSave.Rating, missionWarbandSave.Rank, missionWarbandSave.Units.Count, impressive, wbData, num, num2, OnProcWarbandGenerated));
    }

    private void OnProcWarbandGenerated(WarbandSave warSave)
    {
        WarbandMenuController ctrlr = new WarbandMenuController(warSave);
        if (PandoraSingleton<HideoutManager>.Instance.StateMachine.GetActiveStateId() == 4 && PandoraSingleton<MissionStartData>.Instance.FightingWarbands.Count > 1 && PandoraSingleton<MissionStartData>.Instance.FightingWarbands[1].Name == PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_loading"))
        {
            PandoraSingleton<MissionStartData>.Instance.FightingWarbands.RemoveAt(1);
            PandoraSingleton<MissionStartData>.Instance.AddFightingWarband(ctrlr, PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_title_ai"), PlayerTypeId.AI);
            PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.SKIRMISH_OPPONENT_JOIN);
        }
    }

    public void RemoveOpponent()
    {
        if (!PandoraSingleton<MissionStartData>.Instance.IsLocked)
        {
            if (PandoraSingleton<MissionStartData>.Instance.FightingWarbands.Count > 1)
            {
                PandoraSingleton<MissionStartData>.Instance.FightingWarbands.RemoveAt(1);
            }
            PandoraSingleton<Hephaestus>.Instance.SetLobbyJoinable(joinable: true);
            PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.SKIRMISH_OPPONENT_LEAVE);
        }
    }

    public void LaunchMission()
    {
        PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.rating = PandoraSingleton<MissionStartData>.Instance.FightingWarbands[1].Rating;
        if (PandoraSingleton<MissionStartData>.Instance.FightingWarbands[1].PlayerTypeId == PlayerTypeId.PLAYER)
        {
            PandoraSingleton<MissionStartData>.Instance.CurrentMission.RefreshDifficulty(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetRating(), isProc: false);
        }
        PandoraSingleton<MissionStartData>.Instance.Lock();
        PandoraSingleton<MissionStartData>.Instance.SendMissionStartData();
        SceneLauncher.Instance.LaunchScene(SceneLoadingId.LAUNCH_MISSION, PandoraSingleton<MissionStartData>.Instance.FightingWarbands[1].PlayerTypeId == PlayerTypeId.PLAYER);
    }

    public bool IsContestAvailable(out string reason)
    {
        reason = string.Empty;
        if (PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave().lateShipmentCount >= Constant.GetInt(ConstantId.MAX_SHIPMENT_FAIL))
        {
            reason = "na_hideout_late_shipment_count";
            return false;
        }
        if (PandoraSingleton<HideoutManager>.Instance.IsPostMission())
        {
            reason = "na_hideout_post_mission";
            return false;
        }
        if (!PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.HasLeader(needToBeActive: true))
        {
            reason = "na_hideout_active_leader";
            return false;
        }
        if (PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.GetActiveUnitsCount() < Constant.GetInt(ConstantId.MIN_MISSION_UNITS))
        {
            reason = "na_hideout_min_active_unit";
            return false;
        }
        return true;
    }
}
