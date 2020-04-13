using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewCampaignView : UIStateMonoBehaviour<MainMenuController>
{
    public WarbandRankPopupView warbandRankPopup;

    public MenuNodeGroup characterNodes;

    public MenuNodeGroup flagNodes;

    public Camera mainCamera;

    public Text raceTitle;

    public Text raceDescription;

    public ButtonGroup butConfirm;

    public ButtonGroup butExit;

    public GameObject darkSideBar;

    public Sprite backIcon;

    public GameObject camPos;

    private int unitMenuNodeIndex;

    private bool needActivateNode;

    private List<MainMenuUnit> leaderUnits;

    private bool initialized;

    public override int StateId => 5;

    protected override void Start()
    {
        if (!initialized)
        {
            initialized = true;
            base.Start();
            leaderUnits = new List<MainMenuUnit>();
            for (int i = 0; i < characterNodes.nodes.Count; i++)
            {
                leaderUnits.Add(characterNodes.nodes[i].content.GetComponent<MainMenuUnit>());
                leaderUnits[i].Hide(visible: true);
            }
            for (int j = 0; j < flagNodes.nodes.Count; j++)
            {
                flagNodes.nodes[j].content.GetComponent<Dissolver>().Hide(hide: true, force: true);
            }
            characterNodes.Deactivate();
            flagNodes.Deactivate();
            unitMenuNodeIndex = -1;
        }
    }

    public override void StateEnter()
    {
        if (!initialized)
        {
            Start();
        }
        base.StateMachine.camManager.dummyCam.transform.position = camPos.transform.position;
        base.StateMachine.camManager.dummyCam.transform.rotation = camPos.transform.rotation;
        base.StateMachine.camManager.Transition();
        darkSideBar.SetActive(value: false);
        needActivateNode = true;
        for (int i = 0; i < leaderUnits.Count; i++)
        {
            leaderUnits[i].Hide(visible: false);
            if ((bool)flagNodes.nodes[i].GetContent())
            {
                flagNodes.nodes[i].GetContent().GetComponent<Dissolver>().Hide(hide: true, force: true);
            }
        }
        Show(visible: true);
        butExit.SetAction("cancel", "menu_back", 0, negative: false, backIcon);
        butExit.OnAction(OnInputCancel, mouseOnly: false);
        butConfirm.SetAction("action", "menu_confirm");
        OnInputTypeChanged();
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.INPUT_TYPE_CHANGED, OnInputTypeChanged);
    }

    public override void StateUpdate()
    {
        base.StateUpdate();
        if (needActivateNode)
        {
            characterNodes.Activate(NodeSelected, NodeUnSeleteced, NodeConfirmed, PandoraInput.InputLayer.NORMAL, unselectOverOut: false);
            characterNodes.SelectNode(characterNodes.nodes[0]);
            needActivateNode = false;
        }
    }

    public override void StateExit()
    {
        darkSideBar.SetActive(value: true);
        for (int i = 0; i < leaderUnits.Count; i++)
        {
            leaderUnits[i].Hide(visible: true);
            if (flagNodes.nodes[i].GetContent() != null)
            {
                flagNodes.nodes[i].GetContent().GetComponent<Dissolver>().Hide(hide: true);
            }
        }
        characterNodes.Deactivate();
        flagNodes.Deactivate();
        Show(visible: false);
        PandoraSingleton<NoticeManager>.Instance.RemoveListener(Notices.INPUT_TYPE_CHANGED, OnInputTypeChanged);
    }

    public void SetDescription(WarbandId warbandId)
    {
        raceTitle.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("com_wb_type_" + warbandId.ToString().ToLowerInvariant()));
        raceDescription.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("com_wb_desc_" + warbandId.ToString().ToLowerInvariant()));
    }

    private void NodeSelected(MenuNode node, int idx)
    {
        unitMenuNodeIndex = idx;
        if (node.GetContent() != null)
        {
            MainMenuUnit mainMenuUnit = leaderUnits[idx];
            SetDescription(mainMenuUnit.warbandId);
            mainMenuUnit.LaunchAction(UnitActionId.MISC, success: true, UnitStateId.NONE, 1);
        }
        for (int i = 0; i < flagNodes.nodes.Count; i++)
        {
            if ((bool)flagNodes.nodes[i].GetContent())
            {
                flagNodes.nodes[i].GetContent().GetComponent<Dissolver>().Hide(i != idx);
            }
        }
    }

    private void NodeUnSeleteced(MenuNode node, int idx)
    {
    }

    private void NodeConfirmed(MenuNode node, int idx)
    {
        MainMenuUnit mainMenuUnit = leaderUnits[unitMenuNodeIndex];
        WarbandId warbandId = mainMenuUnit.warbandId;
        if ((warbandId == WarbandId.WITCH_HUNTERS && !PandoraSingleton<Hephaestus>.Instance.OwnsDLC(Hephaestus.DlcId.WITCH_HUNTERS)) || (warbandId == WarbandId.UNDEAD && !PandoraSingleton<Hephaestus>.Instance.OwnsDLC(Hephaestus.DlcId.UNDEAD)))
        {
            ShowDlcPopup(warbandId);
        }
        else
        {
            CheckWarbandLevel(warbandId);
        }
    }

    private void OnCheckNetwork(bool result, string reason, WarbandId wId)
    {
        if (result)
        {
            OpenStore(wId);
        }
        else
        {
            PandoraSingleton<GameManager>.Instance.ShowSystemPopup(GameManager.SystemPopupId.CONNECTION_VALIDATION, "console_offline_error_title", PandoraSingleton<Hephaestus>.Instance.GetOfflineReason(), null, null);
        }
    }

    private void ShowDlcPopup(WarbandId wId)
    {
        base.StateMachine.ConfirmPopup.ShowLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("com_wb_title_dlc"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("com_wb_desc_dlc", PandoraSingleton<LocalizationManager>.Instance.GetStringById("com_wb_type_" + wId.ToString().ToLowerInvariant())), delegate (bool confirm)
        {
            OnDLCBuyPopup(confirm, wId);
        });
    }

    public void OnDLCBuyPopup(bool confirm, WarbandId wId)
    {
        if (confirm)
        {
            OpenStore(wId);
        }
    }

    private static void OpenStore(WarbandId wId)
    {
        switch (wId)
        {
            case WarbandId.WITCH_HUNTERS:
                PandoraSingleton<Hephaestus>.Instance.OpenStore(Hephaestus.DlcId.WITCH_HUNTERS);
                break;
            case WarbandId.UNDEAD:
                PandoraSingleton<Hephaestus>.Instance.OpenStore(Hephaestus.DlcId.UNDEAD);
                break;
        }
    }

    public override void OnInputCancel()
    {
        base.StateMachine.GoToPrev();
    }

    private void CheckWarbandLevel(WarbandId id)
    {
        if (PandoraSingleton<GameManager>.Instance.Profile.Rank < 5)
        {
            CreateNewCampaign(id);
        }
        else
        {
            warbandRankPopup.Show(delegate (bool confirm, int rank)
            {
                if (confirm)
                {
                    CreateNewCampaign(id, rank);
                }
            });
        }
    }

    private void CreateNewCampaign(WarbandId id, int rank = 0)
    {
        PandoraSingleton<GameManager>.Instance.campaign = PandoraSingleton<GameManager>.Instance.Save.GetEmptyCampaignSlot();
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.GAME_SAVED, OnCampaignSaved);
        PandoraSingleton<GameManager>.Instance.Save.NewCampaign(PandoraSingleton<GameManager>.Instance.campaign, id, rank);
    }

    private void OnCampaignSaved()
    {
        PandoraSingleton<NoticeManager>.Instance.RemoveListener(Notices.GAME_SAVED, OnCampaignSaved);
        SceneLauncher.Instance.LaunchScene(SceneLoadingId.NEW_CAMPAIGN);
    }

    private void OnInputTypeChanged()
    {
        butConfirm.gameObject.SetActive(PandoraSingleton<PandoraInput>.Instance.lastInputMode == PandoraInput.InputMode.JOYSTICK);
    }
}
