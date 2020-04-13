using UnityEngine;

public class HideoutSkirmish : ICheapState
{
    public enum NodeSlot
    {
        PLAYER_1,
        BANNER_1,
        PLAYER_2,
        BANNER_2
    }

    private HideoutManager controller;

    private HideoutCamAnchor camAnchor;

    private LobbyCreateModule lobbyCreateMod;

    private LobbyListModule lobbyListMod;

    private WarbandTabsModule warbandTabs;

    private MenuNode opponentBannerNode;

    private bool once = true;

    private bool isCheckingNetwork;

    private bool networkChecked;

    private string unavailbleReason;

    public HideoutSkirmish(HideoutManager ctrl, HideoutCamAnchor anchor)
    {
        controller = ctrl;
        camAnchor = anchor;
    }

    void ICheapState.Destroy()
    {
    }

    void ICheapState.Enter(int iFrom)
    {
        PandoraSingleton<HideoutManager>.Instance.CamManager.ClearLookAtFocus();
        PandoraSingleton<HideoutManager>.Instance.CamManager.CancelTransition();
        controller.CamManager.dummyCam.transform.position = camAnchor.transform.position;
        controller.CamManager.dummyCam.transform.rotation = camAnchor.transform.rotation;
        PandoraSingleton<HideoutManager>.Instance.CamManager.SetDOFTarget(camAnchor.dofTarget, 0f);
        PandoraSingleton<HideoutTabManager>.Instance.ActivateLeftTabModules(true, ModuleId.LOBBY_CREATE);
        PandoraSingleton<HideoutTabManager>.Instance.ActivateRightTabModules(true, ModuleId.LOBBY_LIST);
        PandoraSingleton<HideoutTabManager>.Instance.ActivateCenterTabModules(ModuleId.WARBAND_TABS, ModuleId.TITLE, ModuleId.NOTIFICATION);
        lobbyCreateMod = PandoraSingleton<HideoutTabManager>.Instance.GetModuleLeft<LobbyCreateModule>(ModuleId.LOBBY_CREATE);
        lobbyCreateMod.Setup();
        lobbyCreateMod.createExhibitionGame.SetDisabled();
        lobbyCreateMod.createContestGame.SetDisabled();
        lobbyListMod = PandoraSingleton<HideoutTabManager>.Instance.GetModuleRight<LobbyListModule>(ModuleId.LOBBY_LIST);
        lobbyListMod.Setup(OnServerSelect, PandoraSingleton<SkirmishManager>.Instance.JoinLobby);
        lobbyListMod.ClearServersList();
        warbandTabs = PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<WarbandTabsModule>(ModuleId.WARBAND_TABS);
        warbandTabs.Setup(PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<TitleModule>(ModuleId.TITLE));
        warbandTabs.SetCurrentTab(HideoutManager.State.SKIRMISH);
        warbandTabs.Refresh();
        Cloth cloth = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.banner.GetComponentsInChildren<Cloth>(includeInactive: true)[0];
        cloth.enabled = false;
        PandoraSingleton<HideoutManager>.Instance.skirmishNodeGroup.nodes[1].SetContent(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.banner);
        cloth.enabled = true;
        UnitMenuController leader = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.GetLeader();
        PandoraSingleton<HideoutManager>.Instance.skirmishNodeGroup.nodes[0].SetContent(leader);
        leader.SwitchWeapons(UnitSlotId.SET1_MAINHAND);
        opponentBannerNode = PandoraSingleton<HideoutManager>.Instance.skirmishNodeGroup.nodes[3];
        PandoraSingleton<HideoutManager>.Instance.CamManager.SetDOFTarget(leader.gameObject.transform, 0f);
        PandoraSingleton<HideoutManager>.Instance.skirmishNodeGroup.nodes[3].DestroyContent();
        PandoraSingleton<HideoutManager>.Instance.skirmishNodeGroup.nodes[2].DestroyContent();
        PandoraSingleton<HideoutTabManager>.Instance.button1.gameObject.SetActive(value: true);
        PandoraSingleton<HideoutTabManager>.Instance.button1.SetAction("cancel", "go_to_camp", 0, negative: false, PandoraSingleton<HideoutTabManager>.Instance.icnBack);
        PandoraSingleton<HideoutTabManager>.Instance.button1.OnAction(GoToCamp, mouseOnly: false);
        PandoraSingleton<HideoutTabManager>.Instance.button2.gameObject.SetActive(value: false);
        PandoraSingleton<HideoutTabManager>.Instance.button3.gameObject.SetActive(value: false);
        PandoraSingleton<HideoutTabManager>.Instance.button4.gameObject.SetActive(value: false);
        GetNumberOfPlayers();
        if (iFrom == 4)
        {
            PandoraSingleton<Hephaestus>.Instance.SetRichPresence(Hephaestus.RichPresenceId.HIDEOUT, active: true);
        }
        PandoraSingleton<Hermes>.Instance.StopConnections();
        PandoraSingleton<Hephaestus>.Instance.ResetNetwork();
        isCheckingNetwork = false;
        networkChecked = false;
        once = true;
        lobbyCreateMod.createExhibitionGame.effects.toggle.set_isOn(false);
        lobbyCreateMod.createContestGame.effects.toggle.set_isOn(false);
    }

    void ICheapState.Exit(int iTo)
    {
        opponentBannerNode.DestroyContent();
    }

    void ICheapState.Update()
    {
        if (!networkChecked || !PandoraSingleton<Hephaestus>.Instance.IsOnline() || PandoraSingleton<PandoraInput>.Instance.CurrentInputLayer == 1000 || PandoraSingleton<PandoraInput>.Instance.CurrentInputLayer == 1 || PandoraSingleton<HideoutManager>.Instance.showingTuto || PandoraSingleton<HideoutManager>.Instance.IsCheckingInvite())
        {
            return;
        }
        if (PandoraSingleton<Hephaestus>.Instance.IsJoiningInvite())
        {
            if (PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.IsSkirmishAvailable(out unavailbleReason))
            {
                PandoraSingleton<Hephaestus>.Instance.JoinInvite();
            }
            else
            {
                PandoraSingleton<HideoutManager>.Instance.messagePopup.Show("lobby_joining_title", unavailbleReason, GoToCamp, hideButtons: false, hideCancel: true);
            }
        }
        else if (PandoraSingleton<Hephaestus>.Instance.IsPlayTogether() || PandoraSingleton<Hephaestus>.Instance.IsPlayTogetherPassive())
        {
            if (PandoraSingleton<SkirmishManager>.Instance.IsContestAvailable(out string _))
            {
                PandoraSingleton<HideoutManager>.Instance.playTogetherPopup.Show("popup_play_together_title", "popup_play_together_desc", OnPlayTogetherExhibition, OnPlayTogetherContest, OnPlayTogetherCancel);
            }
            else
            {
                OnPlayTogetherExhibition();
            }
        }
    }

    void ICheapState.FixedUpdate()
    {
        if (once && !PandoraSingleton<Hephaestus>.Instance.IsJoiningInvite() && !PandoraSingleton<Hephaestus>.Instance.IsPlayTogether() && !PandoraSingleton<Hephaestus>.Instance.IsPlayTogetherPassive() && PandoraSingleton<PandoraInput>.Instance.CurrentInputLayer == 0)
        {
            once = false;
            PandoraSingleton<HideoutManager>.Instance.ShowHideoutTuto(HideoutManager.HideoutTutoType.SKIRMISH);
        }
        else if (!isCheckingNetwork && !networkChecked && !PandoraSingleton<HideoutManager>.Instance.showingTuto && PandoraSingleton<PandoraInput>.Instance.CurrentInputLayer == 0)
        {
            isCheckingNetwork = true;
            PandoraSingleton<Hephaestus>.Instance.CheckNetworkServicesAvailability(OnNetworkCheck);
        }
    }

    private void GoToCamp(bool confirm)
    {
        GoToCamp();
    }

    private void GoToCamp()
    {
        PandoraSingleton<HideoutManager>.Instance.StateMachine.ChangeState(0);
    }

    private void OnPlayTogetherExhibition()
    {
        lobbyCreateMod.CreateExhibitionGamePopup(silent: true);
    }

    private void OnPlayTogetherContest()
    {
        lobbyCreateMod.CreateContestGamePopup(silent: true);
    }

    private void OnPlayTogetherCancel()
    {
        PandoraSingleton<Hephaestus>.Instance.ResetPlayTogether(setPassive: false);
        lobbyListMod.btnRefresh.SetSelected(force: true);
    }

    private void OnNetworkCheck(bool result, string reason)
    {
        isCheckingNetwork = false;
        networkChecked = true;
        if (PandoraSingleton<HideoutManager>.Instance.StateMachine.GetActiveStateId() != 3)
        {
            return;
        }
        lobbyCreateMod.createExhibitionGame.SetDisabled(disabled: false);
        lobbyCreateMod.createExhibitionGame.SetSelected(force: true);
        if (result)
        {
            if (!PandoraSingleton<Hephaestus>.Instance.IsJoiningInvite())
            {
                if (PandoraSingleton<SkirmishManager>.Instance.IsContestAvailable(out string reason2))
                {
                    lobbyCreateMod.createContestGame.SetDisabled(disabled: false);
                }
                else
                {
                    lobbyCreateMod.LockContest(reason2);
                }
                lobbyListMod.LookForGames();
            }
        }
        else
        {
            PandoraSingleton<Hephaestus>.Instance.ResetInvite();
            PandoraSingleton<Hephaestus>.Instance.ResetPlayTogether(setPassive: false);
            if (!string.IsNullOrEmpty(reason))
            {
                lobbyCreateMod.LockContest(reason);
                PandoraSingleton<GameManager>.Instance.ShowSystemPopup(GameManager.SystemPopupId.CONNECTION_VALIDATION, "console_offline_error_title", reason, null, null);
            }
        }
    }

    private void GetNumberOfPlayers()
    {
        lobbyListMod.availableGames.set_text(string.Empty);
        PandoraSingleton<Hephaestus>.Instance.RequestNumberOfCurrentPlayers(OnNumberOfPlayers);
    }

    private void OnNumberOfPlayers(int number)
    {
        if (number > 0)
        {
            lobbyListMod.availableGames.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_skirmish_players_online", number.ToString()));
        }
    }

    private void OnServerSelect(bool isOn, int index)
    {
        if (isOn)
        {
            opponentBannerNode.DestroyContent();
            WarbandMenuController.GenerateBanner(Warband.GetBannerName((WarbandId)PandoraSingleton<Hephaestus>.Instance.Lobbies[index].warbandId), delegate (GameObject banner)
            {
                Cloth componentInChildren = banner.GetComponentInChildren<Cloth>(includeInactive: true);
                componentInChildren.enabled = false;
                opponentBannerNode.SetContent(banner);
                componentInChildren.enabled = true;
            });
        }
    }
}
