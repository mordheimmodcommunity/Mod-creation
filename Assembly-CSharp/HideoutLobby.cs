using System.Collections;
using UnityEngine;

public class HideoutLobby : ICheapState
{
    private static int currentLoadedCharacterIdx;

    private HideoutCamAnchor camAnchor;

    private LobbyDetailModule lobbyDetail;

    private LobbyPlayersModule playersMod;

    private DescriptionModule descModule;

    private LobbyChatModule chatModule;

    private WarbandSwapModule swapModule;

    private bool canLaunchMission;

    private bool warbandChanged;

    private TitleModule titleModule;

    private bool lastIsExhibition;

    public HideoutLobby(HideoutManager mng, HideoutCamAnchor anchor)
    {
        camAnchor = anchor;
    }

    void ICheapState.Destroy()
    {
    }

    void ICheapState.Enter(int iFrom)
    {
        warbandChanged = true;
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.SKIRMISH_OPPONENT_JOIN, SetOpponent);
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.SKIRMISH_OPPONENT_LEAVE, OpponentLeft);
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.SKIRMISH_OPPONENT_READY, OpponentReady);
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.SKIRMISH_LOBBY_UPDATED, RefreshLobbyData);
        PandoraSingleton<HideoutManager>.Instance.CamManager.dummyCam.transform.position = camAnchor.transform.position;
        PandoraSingleton<HideoutManager>.Instance.CamManager.dummyCam.transform.rotation = camAnchor.transform.rotation;
        PandoraSingleton<HideoutManager>.Instance.CamManager.SetDOFTarget(camAnchor.dofTarget, 0f);
        PandoraSingleton<HideoutTabManager>.Instance.ActivateLeftTabModules(true, ModuleId.LOBBY_DETAIL);
        chatModule = null;
        if (PandoraSingleton<Hephaestus>.Instance.IsOnline())
        {
            PandoraSingleton<HideoutTabManager>.Instance.ActivateRightTabModules(true, ModuleId.LOBBY_PLAYERS, ModuleId.CHAT);
            chatModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleRight<LobbyChatModule>(ModuleId.CHAT);
            chatModule.Setup();
        }
        else
        {
            PandoraSingleton<HideoutTabManager>.Instance.ActivateRightTabModules(true, ModuleId.LOBBY_PLAYERS);
        }
        PandoraSingleton<HideoutTabManager>.Instance.ActivateCenterTabModules(ModuleId.TITLE, ModuleId.DESC);
        titleModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<TitleModule>(ModuleId.TITLE);
        descModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<DescriptionModule>(ModuleId.DESC);
        descModule.Show(visible: false);
        lobbyDetail = PandoraSingleton<HideoutTabManager>.Instance.GetModuleLeft<LobbyDetailModule>(ModuleId.LOBBY_DETAIL);
        if (PandoraSingleton<Hermes>.Instance.IsHost())
        {
            lobbyDetail.SetLobbyData(PandoraSingleton<Hephaestus>.Instance.Lobby, PandoraSingleton<Hermes>.Instance.IsHost());
        }
        lobbyDetail.LinkDescriptions(descModule.desc.Set, descModule.desc.SetLocalized);
        playersMod = PandoraSingleton<HideoutTabManager>.Instance.GetModuleRight<LobbyPlayersModule>(ModuleId.LOBBY_PLAYERS);
        playersMod.RefreshPlayers();
        swapModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<WarbandSwapModule>(ModuleId.SWAP);
        if (!swapModule.initialized)
        {
            swapModule.Init();
        }
        swapModule.Set(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband, PandoraSingleton<SkirmishManager>.Instance.unitsPosition, PandoraSingleton<Hephaestus>.Instance.Lobby.ratingMin, PandoraSingleton<Hephaestus>.Instance.Lobby.ratingMax);
        PandoraSingleton<HideoutManager>.Instance.skirmishNodeGroup.nodes[3].DestroyContent();
        PandoraSingleton<HideoutManager>.Instance.skirmishNodeGroup.nodes[2].DestroyContent();
        lastIsExhibition = PandoraSingleton<Hephaestus>.Instance.Lobby.isExhibition;
        SetTitle();
        PandoraSingleton<Hephaestus>.Instance.SetRichPresence((!PandoraSingleton<Hephaestus>.Instance.Lobby.isExhibition) ? Hephaestus.RichPresenceId.LOBBY_CONTEST : Hephaestus.RichPresenceId.LOBBY_EXHIBITION, active: true);
        if (!PandoraSingleton<HideoutManager>.Instance.skirmishNodeGroup.nodes[1].IsOccupied())
        {
            Cloth cloth = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.banner.GetComponentsInChildren<Cloth>(includeInactive: true)[0];
            cloth.enabled = false;
            PandoraSingleton<HideoutManager>.Instance.skirmishNodeGroup.nodes[1].SetContent(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.banner);
            cloth.enabled = true;
            UnitMenuController leader = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.GetLeader();
            PandoraSingleton<HideoutManager>.Instance.skirmishNodeGroup.nodes[0].SetContent(leader.gameObject);
            leader.SwitchWeapons(UnitSlotId.SET1_MAINHAND);
        }
        SetOpponent();
        PandoraSingleton<Hermes>.Instance.DoNotDisconnectMode = false;
        SetButtons();
    }

    void ICheapState.Exit(int iTo)
    {
        PandoraSingleton<NoticeManager>.Instance.RemoveListener(Notices.SKIRMISH_OPPONENT_JOIN, SetOpponent);
        PandoraSingleton<NoticeManager>.Instance.RemoveListener(Notices.SKIRMISH_OPPONENT_LEAVE, OpponentLeft);
        PandoraSingleton<NoticeManager>.Instance.RemoveListener(Notices.SKIRMISH_OPPONENT_READY, OpponentReady);
        PandoraSingleton<NoticeManager>.Instance.RemoveListener(Notices.SKIRMISH_LOBBY_UPDATED, RefreshLobbyData);
        PandoraSingleton<HideoutManager>.Instance.skirmishNodeGroup.nodes[3].DestroyContent();
        PandoraSingleton<HideoutManager>.Instance.skirmishNodeGroup.nodes[2].DestroyContent();
        if (PandoraSingleton<LobbyPlayerPopup>.Exists())
        {
            PandoraSingleton<LobbyPlayerPopup>.Instance.gameObject.SetActive(value: false);
        }
    }

    void ICheapState.Update()
    {
        if (PandoraSingleton<Hephaestus>.Instance.Lobby == null)
        {
            PandoraSingleton<HideoutManager>.Instance.StateMachine.ChangeState(3);
            return;
        }
        if (!PandoraSingleton<Hermes>.Instance.IsHost() && !PandoraSingleton<Hermes>.Instance.IsConnected())
        {
            PandoraDebug.LogDebug("Client leaving lobby because there is no connection");
            PandoraSingleton<Hephaestus>.Instance.LeaveLobby();
            return;
        }
        if (lastIsExhibition != PandoraSingleton<Hephaestus>.Instance.Lobby.isExhibition)
        {
            lastIsExhibition = PandoraSingleton<Hephaestus>.Instance.Lobby.isExhibition;
            titleModule.Set((!PandoraSingleton<Hephaestus>.Instance.Lobby.isExhibition) ? "menu_skirmish_contest" : "menu_skirmish_exhibition");
            PandoraSingleton<Hephaestus>.Instance.SetRichPresence((!PandoraSingleton<Hephaestus>.Instance.Lobby.isExhibition) ? Hephaestus.RichPresenceId.LOBBY_CONTEST : Hephaestus.RichPresenceId.LOBBY_EXHIBITION, active: true);
        }
        string empty = string.Empty;
        if (warbandChanged)
        {
            canLaunchMission = CanLaunchMission();
        }
        if (PandoraSingleton<Hermes>.Instance.IsHost())
        {
            bool flag = PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.deployCount > 1 && PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.deployCount == PandoraSingleton<MissionStartData>.Instance.FightingWarbands.Count;
            bool visible = canLaunchMission && flag && PandoraSingleton<MissionStartData>.Instance.FightingWarbands[1].IsReady;
            lobbyDetail.SetLaunchButtonVisible(flag, disabled: false);
            if (flag)
            {
                lobbyDetail.SetLaunchButtonVisible(visible, disabled: true);
            }
        }
        else
        {
            lobbyDetail.SetLaunchButtonVisible(canLaunchMission, disabled: false);
        }
        if (chatModule != null)
        {
            float axis = PandoraSingleton<PandoraInput>.Instance.GetAxis("cam_y");
            if (axis != 0f)
            {
                chatModule.messages.ForceScroll(axis < 0f, setSelected: false);
            }
        }
        playersMod.SetErrorMessage(empty);
    }

    void ICheapState.FixedUpdate()
    {
    }

    private IEnumerator SetButtonsAsync()
    {
        PandoraSingleton<HideoutTabManager>.Instance.DeactivateAllButtons();
        yield return new WaitForSeconds(1f);
        SetButtons();
    }

    private void SetTitle()
    {
        if (PandoraSingleton<Hephaestus>.Instance.Lobby.privacy == Hephaestus.LobbyPrivacy.OFFLINE)
        {
            titleModule.Set("menu_skirmish_exhibition");
            PandoraSingleton<SkirmishManager>.Instance.AddAIOpponent();
        }
        else
        {
            titleModule.Set((!PandoraSingleton<Hephaestus>.Instance.Lobby.isExhibition) ? "menu_skirmish_contest" : "menu_skirmish_exhibition");
        }
    }

    private void SetButtons()
    {
        PandoraSingleton<HideoutTabManager>.Instance.button1.gameObject.SetActive(value: true);
        PandoraSingleton<HideoutTabManager>.Instance.button1.SetAction("cancel", "menu_leave", 0, negative: false, PandoraSingleton<HideoutTabManager>.Instance.icnBack);
        PandoraSingleton<HideoutTabManager>.Instance.button1.OnAction(PandoraSingleton<SkirmishManager>.Instance.LeaveLobby, mouseOnly: false);
        SetProfileButton();
        lobbyDetail.swapButton.SetAction(string.Empty, "hideout_swap_unit", 0, negative: false, lobbyDetail.swapIcon);
        lobbyDetail.swapButton.OnAction(ShowMissionPrep, mouseOnly: false);
        if (PandoraSingleton<Hermes>.Instance.IsHost())
        {
            lobbyDetail.launchButton.SetAction(string.Empty, "menu_launch_mission", 0, negative: false, lobbyDetail.launchIcon);
            lobbyDetail.launchButton.OnAction(PandoraSingleton<SkirmishManager>.Instance.LaunchMission, mouseOnly: false);
            lobbyDetail.SetLaunchButtonVisible(visible: false, disabled: true);
        }
        else
        {
            UpdateReadyButton(PandoraSingleton<SkirmishManager>.Instance.ready);
            lobbyDetail.launchButton.OnAction(delegate
            {
                PandoraSingleton<SkirmishManager>.Instance.SendReady();
                SetButtons();
            }, mouseOnly: false);
        }
        if (chatModule != null)
        {
            PandoraSingleton<HideoutTabManager>.Instance.button2.SetAction("show_chat", "menu_chat");
            PandoraSingleton<HideoutTabManager>.Instance.button2.OnAction(ShowChat, mouseOnly: false);
            if (PandoraSingleton<Hermes>.Instance.IsHost() && PandoraSingleton<Hermes>.Instance.IsConnected() && PandoraSingleton<MissionStartData>.Instance.FightingWarbands.Count > 1)
            {
                PandoraSingleton<HideoutTabManager>.Instance.button3.SetAction("rename_warband", "menu_skirmish_kick_player");
                PandoraSingleton<HideoutTabManager>.Instance.button3.OnAction(KickPlayer, mouseOnly: false);
            }
            else
            {
                PandoraSingleton<HideoutTabManager>.Instance.button3.gameObject.SetActive(value: false);
            }
            PandoraSingleton<HideoutTabManager>.Instance.button4.gameObject.SetActive(value: false);
            PandoraSingleton<HideoutTabManager>.Instance.button5.gameObject.SetActive(value: false);
        }
        else
        {
            PandoraSingleton<HideoutTabManager>.Instance.button2.gameObject.SetActive(value: false);
            PandoraSingleton<HideoutTabManager>.Instance.button3.gameObject.SetActive(value: false);
            PandoraSingleton<HideoutTabManager>.Instance.button4.gameObject.SetActive(value: false);
            PandoraSingleton<HideoutTabManager>.Instance.button5.gameObject.SetActive(value: false);
        }
    }

    private void SetProfileButton()
    {
        lobbyDetail.displayProfile.gameObject.SetActive(value: false);
    }

    private void ShowChat()
    {
        if (chatModule != null)
        {
            chatModule.Select();
        }
    }

    private void OnChatTextEntered(bool success, string text)
    {
        if (success)
        {
            chatModule.SendChat(text);
        }
    }

    private void UpdateReadyButton(bool ready)
    {
        if (!ready)
        {
            lobbyDetail.launchButton.SetAction(string.Empty, "menu_set_ready", 0, negative: false, lobbyDetail.launchIcon);
        }
        else
        {
            lobbyDetail.launchButton.SetAction(string.Empty, "menu_not_ready", 0, negative: false, lobbyDetail.launchIcon);
        }
    }

    public bool CanLaunchMission()
    {
        string reason = string.Empty;
        return swapModule != null && swapModule.CanLaunchMission(out reason);
    }

    private void ShowMissionPrep()
    {
        if (PandoraSingleton<LobbyPlayerPopup>.Exists() && PandoraSingleton<LobbyPlayerPopup>.Instance.isActiveAndEnabled)
        {
            PandoraSingleton<LobbyPlayerPopup>.Instance.GetComponent<CanvasGroup>().alpha = 0f;
        }
        PandoraSingleton<HideoutTabManager>.Instance.ActivateCenterTabModules(ModuleId.SWAP, ModuleId.TITLE);
        PandoraSingleton<HideoutTabManager>.Instance.ActivateLeftTabModules(false);
        PandoraSingleton<HideoutTabManager>.Instance.ActivateRightTabModules(false);
        swapModule.Set(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband, OnPrepConfirm, isMission: false, isCampaign: false, !PandoraSingleton<Hephaestus>.Instance.Lobby.isExhibition, PandoraSingleton<SkirmishManager>.Instance.unitsPosition, pushLayer: false, PandoraSingleton<Hephaestus>.Instance.Lobby.ratingMin, PandoraSingleton<Hephaestus>.Instance.Lobby.ratingMax);
    }

    private void OnPrepConfirm(bool confirm)
    {
        if (swapModule.HasChanged)
        {
            PandoraSingleton<MissionStartData>.Instance.RefreshMyWarband(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr, PandoraSingleton<SkirmishManager>.Instance.unitsPosition);
            if (PandoraSingleton<MissionStartData>.Instance.FightingWarbands.Count > 1 && PandoraSingleton<MissionStartData>.Instance.FightingWarbands[1].PlayerTypeId == PlayerTypeId.AI)
            {
                PandoraSingleton<SkirmishManager>.Instance.RemoveOpponent();
                PandoraSingleton<SkirmishManager>.Instance.AddAIOpponent();
            }
            warbandChanged = true;
        }
        PandoraSingleton<HideoutTabManager>.Instance.ActivateLeftTabModules(true, ModuleId.LOBBY_DETAIL);
        chatModule = null;
        if (PandoraSingleton<Hephaestus>.Instance.IsOnline() && PandoraSingleton<Hephaestus>.Instance.Lobby.privacy != Hephaestus.LobbyPrivacy.OFFLINE)
        {
            PandoraSingleton<HideoutTabManager>.Instance.ActivateRightTabModules(true, ModuleId.LOBBY_PLAYERS, ModuleId.CHAT);
            chatModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleRight<LobbyChatModule>(ModuleId.CHAT);
        }
        else
        {
            PandoraSingleton<HideoutTabManager>.Instance.ActivateRightTabModules(true, ModuleId.LOBBY_PLAYERS);
        }
        PandoraSingleton<HideoutTabManager>.Instance.ActivateCenterTabModules(ModuleId.DESC, ModuleId.TITLE);
        TitleModule moduleCenter = PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<TitleModule>(ModuleId.TITLE);
        moduleCenter.Set((!PandoraSingleton<Hephaestus>.Instance.Lobby.isExhibition) ? "menu_skirmish_contest" : "menu_skirmish_exhibition");
        if (PandoraSingleton<LobbyPlayerPopup>.Exists() && PandoraSingleton<LobbyPlayerPopup>.Instance.isActiveAndEnabled)
        {
            PandoraSingleton<LobbyPlayerPopup>.Instance.GetComponent<CanvasGroup>().alpha = 1f;
        }
        if (!PandoraSingleton<Hermes>.Instance.IsHost())
        {
            RefreshLobbyData();
        }
        playersMod.RefreshPlayers();
        descModule.Show(visible: false);
        SetButtons();
        lobbyDetail.LinkDescriptions(descModule.desc.Set, descModule.desc.SetLocalized);
        lobbyDetail.ForceMapReselect();
        lobbyDetail.StartCoroutine(SelectOnNextFrame());
    }

    private IEnumerator SelectOnNextFrame()
    {
        yield return null;
        lobbyDetail.swapButton.SetSelected(force: true);
    }

    private void RefreshLobbyData()
    {
        if (lobbyDetail.isActiveAndEnabled)
        {
            lobbyDetail.SetLobbyData(PandoraSingleton<Hephaestus>.Instance.Lobby, isHost: false);
            lobbyDetail.LinkDescriptions(descModule.desc.Set, descModule.desc.SetLocalized);
        }
        SetTitle();
    }

    private void SetOpponent()
    {
        lobbyDetail.RefreshPlayerNames();
        playersMod.RefreshPlayers();
        if (PandoraSingleton<MissionStartData>.Instance.FightingWarbands.Count > 1 || !PandoraSingleton<Hephaestus>.Instance.Lobby.joinable)
        {
            if (PandoraSingleton<Hermes>.Instance.IsHost())
            {
                if (PandoraSingleton<Hermes>.Instance.IsConnected())
                {
                    MissionWarbandSave missionWarbandSave = PandoraSingleton<MissionStartData>.Instance.FightingWarbands[1];
                    PandoraSingleton<LobbyPlayerPopup>.Instance.Show(missionWarbandSave.PlayerName, "menu_not_ready");
                }
            }
            else
            {
                PandoraSingleton<LobbyPlayerPopup>.Instance.Show(null, "menu_not_ready");
            }
            WarbandMenuController warbandMenuController = new WarbandMenuController(PandoraSingleton<MissionStartData>.Instance.FightingWarbands[PandoraSingleton<Hermes>.Instance.IsHost() ? 1 : 0].ToWarbandSave());
            MenuNode opponentNode = PandoraSingleton<HideoutManager>.Instance.skirmishNodeGroup.nodes[2];
            opponentNode.DestroyContent();
            opponentNode.gameObject.SetActive(value: false);
            currentLoadedCharacterIdx++;
            int idx = currentLoadedCharacterIdx;
            Unit leaderUnit = warbandMenuController.GetLeaderUnit();
            if (leaderUnit != null)
            {
                PandoraSingleton<HideoutManager>.Instance.StartCoroutine(UnitMenuController.LoadUnitPrefabAsync(leaderUnit, delegate (GameObject obj)
                {
                    if (idx == currentLoadedCharacterIdx)
                    {
                        opponentNode.gameObject.SetActive(value: true);
                        opponentNode.SetContent(obj);
                    }
                    else
                    {
                        Object.Destroy(obj);
                    }
                }, null));
            }
            MenuNode bannerNode = PandoraSingleton<HideoutManager>.Instance.skirmishNodeGroup.nodes[3];
            bannerNode.DestroyContent();
            WarbandMenuController.GenerateBanner(Warband.GetBannerName(warbandMenuController.Warband.WarbandData.Id), delegate (GameObject banner)
            {
                Cloth componentInChildren = banner.GetComponentInChildren<Cloth>(includeInactive: true);
                componentInChildren.enabled = false;
                bannerNode.SetContent(banner);
                componentInChildren.enabled = true;
            });
            if (PandoraSingleton<Hermes>.Instance.IsHost())
            {
                lobbyDetail.SetWarbandType(PandoraSingleton<MissionStartData>.Instance.FightingWarbands[1].WarbandId);
                if (PandoraSingleton<Hermes>.Instance.IsConnected())
                {
                    lobbyDetail.LockAI(lockButtons: true);
                    lobbyDetail.SetInviteButtonVisible(visible: false);
                    SetButtons();
                }
                else
                {
                    lobbyDetail.LockAI(lockButtons: false);
                    lobbyDetail.SetInviteButtonVisible(PandoraSingleton<MissionStartData>.Instance.FightingWarbands.Count < 2);
                }
            }
        }
        SetButtons();
    }

    private void OpponentLeft()
    {
        lobbyDetail.LockAI(lockButtons: false);
        lobbyDetail.SetInviteButtonVisible(visible: true);
        playersMod.RefreshPlayers();
        lobbyDetail.RefreshPlayerNames();
        lobbyDetail.SetLaunchButtonVisible(visible: false, disabled: false);
        PandoraSingleton<HideoutManager>.Instance.skirmishNodeGroup.nodes[3].DestroyContent();
        PandoraSingleton<HideoutManager>.Instance.skirmishNodeGroup.nodes[2].DestroyContent();
        PandoraSingleton<LobbyPlayerPopup>.Instance.gameObject.SetActive(value: false);
        PandoraSingleton<HideoutTabManager>.Instance.button2.gameObject.SetActive(value: false);
        SetButtons();
    }

    private void OpponentReady()
    {
        if (playersMod == null || PandoraSingleton<MissionStartData>.Instance.FightingWarbands.Count < 2)
        {
            return;
        }
        playersMod.RefreshPlayers();
        if (PandoraSingleton<Hermes>.Instance.IsHost())
        {
            if (PandoraSingleton<Hermes>.Instance.IsConnected())
            {
                MissionWarbandSave missionWarbandSave = PandoraSingleton<MissionStartData>.Instance.FightingWarbands[1];
                PandoraSingleton<LobbyPlayerPopup>.Instance.Show(missionWarbandSave.PlayerName, (!missionWarbandSave.IsReady) ? "menu_not_ready" : "menu_set_ready");
            }
        }
        else
        {
            MissionWarbandSave missionWarbandSave2 = PandoraSingleton<MissionStartData>.Instance.FightingWarbands[1];
            PandoraSingleton<LobbyPlayerPopup>.Instance.Show(null, (!missionWarbandSave2.IsReady) ? "menu_not_ready" : "menu_set_ready");
            UpdateReadyButton(missionWarbandSave2.IsReady);
        }
    }

    private void KickPlayer()
    {
        PandoraSingleton<HideoutManager>.Instance.messagePopup.ShowLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_skirmish_kick_player"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_skirmish_kick_player_desc", PandoraSingleton<MissionStartData>.Instance.FightingWarbands[1].PlayerName), OnKickPlayer);
    }

    private void OnKickPlayer(bool confirm)
    {
        if (confirm)
        {
            PandoraSingleton<MissionStartData>.Instance.KickPlayerFromLobby(12);
        }
    }
}
