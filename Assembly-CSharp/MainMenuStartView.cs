using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuStartView : UIStateMonoBehaviour<MainMenuController>
{
    private const string welcomeStringId = "menu_welcome_desc";

    public ToggleEffects btnContinue;

    public ToggleEffects btnLoadGame;

    public ToggleEffects btnNewGame;

    public ToggleEffects btnTutorials;

    public ToggleEffects btnOptions;

    public ToggleEffects btnCredits;

    public ToggleEffects btnExit;

    public Text welcomeDesc;

    public Text versionLabel;

    public GameObject grpCommunity;

    public ButtonGroup butCommunity;

    public ButtonGroup butConfirm;

    public ButtonGroup butExit;

    public GameObject camPos;

    public GameObject playerInfo;

    public Image playerPic;

    public Text playerName;

    public ButtonGroup butSwitchAccount;

    private bool once = true;

    public override int StateId => 0;

    public override void Awake()
    {
        base.Awake();
        btnContinue.onAction.AddListener(OnContinueCampaign);
        btnLoadGame.onAction.AddListener(delegate
        {
            base.StateMachine.ChangeState(MainMenuController.State.LOAD_CAMPAIGN);
        });
        btnNewGame.onAction.AddListener(OnCreateCampaign);
        btnTutorials.onAction.AddListener(delegate
        {
            base.StateMachine.ChangeState(MainMenuController.State.TUTORIALS);
        });
        btnOptions.onAction.AddListener(delegate
        {
            base.StateMachine.ChangeState(MainMenuController.State.OPTIONS);
        });
        btnCredits.onAction.AddListener(delegate
        {
            base.StateMachine.ChangeState(MainMenuController.State.CREDITS);
        });
        btnExit.onAction.AddListener(OnInputCancel);
        versionLabel.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById(versionLabel.get_text(), "1.4.4.4 Paranoia Mod 1.13 + Frankenstein AI 0.2"));
        welcomeDesc.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_welcome_desc"));
    }

    public override void StateEnter()
    {
        Show(visible: true);
        base.StateMachine.camManager.dummyCam.transform.position = camPos.transform.position;
        base.StateMachine.camManager.dummyCam.transform.rotation = camPos.transform.rotation;
        base.StateMachine.camManager.Transition();
        once = true;
        btnContinue.gameObject.SetActive(value: false);
        btnLoadGame.gameObject.SetActive(value: false);
        btnNewGame.SetSelected();
        StartCoroutine(CheckContinue());
        playerInfo.SetActive(value: false);
    }

    private IEnumerator CheckContinue()
    {
        while (PandoraSingleton<GameManager>.Instance.Profile == null || !PandoraSingleton<GameManager>.Instance.profileInitialized)
        {
            yield return null;
        }
        bool showContinue = PandoraSingleton<GameManager>.Instance.Profile.LastPlayedCampaign != -1 && PandoraSingleton<GameManager>.Instance.Save.CampaignExist(PandoraSingleton<GameManager>.Instance.Profile.LastPlayedCampaign);
        bool showLoad = PandoraSingleton<GameManager>.Instance.Save.HasCampaigns();
        btnContinue.gameObject.SetActive(showContinue);
        btnLoadGame.gameObject.SetActive(showLoad);
        while (PandoraSingleton<GameManager>.Instance.Popup.isShow)
        {
            yield return null;
        }
        if (showContinue)
        {
            btnContinue.SetSelected(force: true);
            btnNewGame.toggle.set_isOn(false);
        }
        else if (showLoad)
        {
            btnLoadGame.SetSelected(force: true);
            btnNewGame.toggle.set_isOn(false);
        }
    }

    private void OnPlayerPictureLoaded(Sprite sprite)
    {
        playerPic.set_sprite(sprite);
    }

    private void Setup()
    {
        butCommunity.SetAction("hub", "info_news");
        butCommunity.OnAction(OpenCommunity, mouseOnly: false);
        butConfirm.gameObject.SetActive(value: false);
        butExit.gameObject.SetActive(value: false);
    }

    private void OpenCommunity()
    {
        PandoraSingleton<Hephaestus>.Instance.OpenCommunity();
    }

    public override void StateUpdate()
    {
        base.StateUpdate();
        if (once)
        {
            Setup();
            once = false;
        }
    }

    public override void OnInputCancel()
    {
        base.StateMachine.ConfirmPopup.Show("menu_warning", "menu_exit_game_confirm", OnPopup);
    }

    private void OnPopup(bool confirm)
    {
        if (confirm)
        {
            Application.Quit();
        }
    }

    private void OnCreateCampaign()
    {
        if (!PandoraSingleton<GameManager>.Instance.Save.HasCampaigns() && !PandoraSingleton<GameManager>.Instance.Profile.HasCompletedTutorials())
        {
            base.StateMachine.ConfirmPopup.Show("menu_new_campaign_tuto_title", "menu_new_campaign_tuto_desc", OnCreateNoTuto);
            return;
        }
        if (PandoraSingleton<GameManager>.Instance.Save.EmptyCampaignSaveExists())
        {
            base.StateMachine.ChangeState(MainMenuController.State.NEW_CAMPAIGN);
            return;
        }
        base.StateMachine.ConfirmPopup.Show("menu_warning", "menu_out_of_campaign_slots", null);
        base.StateMachine.ConfirmPopup.HideCancelButton();
    }

    private void OnCreateNoTuto(bool confirm)
    {
        if (confirm)
        {
            base.StateMachine.ChangeState(MainMenuController.State.NEW_CAMPAIGN);
        }
    }

    private void OnContinueCampaign()
    {
        if (PandoraSingleton<TransitionManager>.Instance.IsDone())
        {
            int lastPlayedCampaign = PandoraSingleton<GameManager>.Instance.Profile.LastPlayedCampaign;
            if (lastPlayedCampaign != -1 && PandoraSingleton<GameManager>.Instance.Save.CampaignExist(lastPlayedCampaign))
            {
                PandoraSingleton<GameManager>.Instance.campaign = lastPlayedCampaign;
                PandoraSingleton<GameManager>.Instance.Save.LoadCampaign(lastPlayedCampaign);
            }
            else
            {
                base.StateMachine.ChangeState(MainMenuController.State.NEW_CAMPAIGN);
            }
        }
    }

    public override void StateExit()
    {
        butConfirm.gameObject.SetActive(value: false);
        Show(visible: false);
        StopCoroutine(CheckContinue());
    }
}
