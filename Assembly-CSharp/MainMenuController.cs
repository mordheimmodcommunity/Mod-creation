using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : UIStateMachineMonoBehaviour
{
    [Serializable]
    public enum State
    {
        MAIN_MENU,
        TUTORIALS,
        STORE,
        OPTIONS,
        CREDITS,
        NEW_CAMPAIGN,
        LOAD_CAMPAIGN,
        NB_STATE
    }

    public enum InputLayer
    {
        BASE,
        POP_UP
    }

    public const string VERSION = "1.4.4.4";

    public ConfirmationPopupView ConfirmPopup;

    public ContinuePopupView ContinuePopup;

    public List<ButtonMapView> buttons = new List<ButtonMapView>();

    public CanvasGroupDisabler uiContainer;

    public CameraManager camManager;

    public Transform dofTarget;

    public GameObject environment;

    private void Awake()
    {
        PandoraSingleton<UIStateMachineMonoBehaviour>.instance = GetComponent<MainMenuController>();
        PandoraDebug.LogInfo("Welcome to Mordheim V 1.4.4.4", "INIT", this);
        DOTween.Init((bool?)false, (bool?)true, (LogBehaviour?)null);
        PandoraSingleton<GameManager>.Instance.campaign = -1;
        PandoraSingleton<GameManager>.Instance.currentSave = null;
        camManager = Camera.main.GetComponent<CameraManager>();
        CameraSetter component = GetComponent<CameraSetter>();
        component.SetCameraInfo(camManager.GetComponent<Camera>());
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.GAME_LOADED, OnCampaignLoad);
        PandoraSingleton<AssetBundleLoader>.Instance.LoadAsync("loading");
    }

    private void OnDestroy()
    {
    }

    private void Start()
    {
        StartCoroutine(WaitForHephaestus());
    }

    private IEnumerator WaitForHephaestus()
    {
        while (!PandoraSingleton<Hephaestus>.Instance.IsInitialized())
        {
            yield return null;
        }
        PandoraSingleton<Hephaestus>.Instance.RefreshSaveData(WaitForCopyright);
    }

    private IEnumerator WaitForCopyrightAsync()
    {
        PandoraSingleton<TransitionManager>.Instance.SetGameLoadingDone();
        while (PandoraSingleton<GameManager>.Instance.inCopyright || PandoraSingleton<TransitionManager>.Instance.IsLoading())
        {
            yield return null;
        }
        PandoraSingleton<Hermes>.Instance.StopConnections();
        PandoraSingleton<Pan>.Instance.PlayMusic();
        PandoraSingleton<PandoraInput>.Instance.SetCurrentState(PandoraInput.States.MENU, showMouse: true);
        base.ChangeState(0);
        camManager.SetDOFTarget(dofTarget, 0f);
        PandoraSingleton<Hephaestus>.Instance.SetRichPresence(Hephaestus.RichPresenceId.MAIN_MENU, active: true);
    }

    private void WaitForCopyright()
    {
        StartCoroutine(WaitForCopyrightAsync());
    }

    protected override void Update()
    {
        bool inCopyright = PandoraSingleton<GameManager>.Instance.inCopyright;
        bool flag = PandoraSingleton<Hephaestus>.Instance.IsJoiningInvite();
        bool flag2 = PandoraSingleton<Hephaestus>.Instance.IsPlayTogether();
        if (!inCopyright && base.CurrentState != null && !PandoraSingleton<TransitionManager>.Instance.IsLoading() && base.CurrentState.StateId != 6 && (base.NextState == null || base.NextState.StateId != 6) && (flag || flag2))
        {
            if (PandoraSingleton<GameManager>.Instance.Save.HasCampaigns())
            {
                if (ContinuePopup != null && ContinuePopup.IsVisible)
                {
                    ContinuePopup.Hide();
                }
                ChangeState(State.LOAD_CAMPAIGN);
            }
            else if (flag)
            {
                PandoraSingleton<Hephaestus>.Instance.ResetInvite();
                PandoraSingleton<GameManager>.Instance.ShowSystemPopup(GameManager.SystemPopupId.INVITE, "invite_no_warband_title", "invite_no_warband_desc", null);
            }
            else if (flag2)
            {
                PandoraSingleton<Hephaestus>.Instance.ResetPlayTogether(setPassive: true);
                PandoraSingleton<GameManager>.Instance.ShowSystemPopup(GameManager.SystemPopupId.INVITE, "play_together_no_warband_title", "play_together_no_warband_desc", null);
            }
        }
        base.Update();
        if (PandoraSingleton<PandoraInput>.Instance.GetKeyDown("hide_gui", -1))
        {
            uiContainer.enabled = !uiContainer.enabled;
        }
    }

    private void OnCampaignLoad()
    {
        if (PandoraSingleton<Hephaestus>.Instance.IsJoiningInvite())
        {
            Warband warband = new Warband(PandoraSingleton<GameManager>.Instance.currentSave);
            if (warband.ValidateWarbandForInvite(inMission: false))
            {
                SceneLauncher.Instance.LaunchScene(SceneLoadingId.LAUNCH_HIDEOUT, waitForPlayers: false, force: true);
            }
        }
        else if (PandoraSingleton<GameManager>.Instance.currentSave.inMission)
        {
            MissionEndDataSave endMission = PandoraSingleton<GameManager>.Instance.currentSave.endMission;
            if (endMission.lastVersion < 11)
            {
                PandoraSingleton<GameManager>.Instance.currentSave.inMission = false;
                endMission = new MissionEndDataSave();
                SceneLauncher.Instance.LaunchScene(SceneLoadingId.LAUNCH_HIDEOUT, waitForPlayers: false, force: true);
            }
            else if (PandoraSingleton<GameManager>.Instance.currentSave.endMission.myrtilusLadder.Count == 0)
            {
                PandoraSingleton<GameManager>.Instance.currentSave.inMission = false;
                PandoraSingleton<GameManager>.Instance.currentSave.endMission = null;
                PandoraSingleton<GameManager>.Instance.Save.SaveCampaign(PandoraSingleton<GameManager>.Instance.currentSave, PandoraSingleton<GameManager>.Instance.campaign);
                ConfirmPopup.Show("mission_progress_reset_title", "mission_progress_reset_desc", delegate
                {
                    SceneLauncher.Instance.LaunchScene(SceneLoadingId.LAUNCH_HIDEOUT, waitForPlayers: false, force: true);
                }, hideButtons: false, hideCancel: true);
            }
            else if (endMission.isVsAI && !endMission.isSkirmish && !endMission.missionFinished)
            {
                ContinuePopup.Show("popup_load_fallback_title", "popup_load_fallback_desc", ResumeOrAbandonMission);
                ContinuePopup.abandonButton.SetAction(string.Empty, (!endMission.routable) ? "menu_quit_mission_title" : "menu_quit_mission_voluntary_rout_title");
                ContinuePopup.cancelButton.SetSelected();
            }
            else
            {
                AbandonMission();
            }
        }
        else
        {
            SceneLauncher.Instance.LaunchScene(SceneLoadingId.LAUNCH_HIDEOUT, waitForPlayers: false, force: true);
        }
    }

    private void ResumeOrAbandonMission(bool resume)
    {
        PandoraSingleton<NoticeManager>.Instance.RemoveListener(Notices.GAME_LOADED, OnCampaignLoad);
        if (resume)
        {
            ResumeMission();
        }
        else
        {
            AbandonMission();
        }
    }

    private void ResumeMission()
    {
        MissionEndDataSave endMission = PandoraSingleton<GameManager>.Instance.currentSave.endMission;
        if (!PandoraSingleton<MissionStartData>.Exists())
        {
            GameObject gameObject = new GameObject("mission_start_data");
            gameObject.AddComponent<MissionStartData>();
        }
        else
        {
            PandoraSingleton<MissionStartData>.Instance.Clear();
        }
        PandoraSingleton<MissionStartData>.Instance.ReloadMission(endMission, PandoraSingleton<GameManager>.Instance.currentSave);
        if (PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isCampaign)
        {
            SceneLauncher.Instance.LaunchScene(SceneLoadingId.LAUNCH_MISSION_CAMPAIGN, waitForPlayers: false, force: true);
        }
        else
        {
            SceneLauncher.Instance.LaunchScene(SceneLoadingId.LAUNCH_MISSION, waitForPlayers: false, force: true);
        }
    }

    private void AbandonMission()
    {
        MissionEndDataSave endMission = PandoraSingleton<GameManager>.Instance.currentSave.endMission;
        if (endMission.VictoryType == VictoryTypeId.LOSS && !endMission.routable)
        {
            endMission.crushed = true;
            for (int i = 0; i < endMission.units.Count; i++)
            {
                endMission.units[i].status = UnitStateId.OUT_OF_ACTION;
            }
        }
        if (!endMission.isVsAI)
        {
            PandoraSingleton<Hephaestus>.Instance.UnlockAchievement(Hephaestus.TrophyId.ALTF4);
        }
        SceneLauncher.Instance.LaunchScene(SceneLoadingId.LAUNCH_END_GAME, waitForPlayers: false, force: true);
    }

    public void ChangeState(State state)
    {
        ChangeState((int)state);
    }
}
