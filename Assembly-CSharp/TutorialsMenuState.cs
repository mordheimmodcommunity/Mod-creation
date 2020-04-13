using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TutorialsMenuState : UIStateMonoBehaviour<MainMenuController>
{
    private const string TUTO_TITLE = "tutorial_0{0}_title";

    private const string TUTO_DESC = "tutorial_0{0}_menu";

    private bool isInit;

    public List<Toggle> tutorialsCompletionToggle = new List<Toggle>();

    private List<GameModeTutorialView> tutorialsView = new List<GameModeTutorialView>();

    private List<ToggleEffects> hideoutTutorials = new List<ToggleEffects>();

    public ButtonGroup actionButton;

    public ButtonGroup cancelButton;

    public Sprite icnBack;

    public GameObject camPos;

    public GameObject darkSideBar;

    public GameObject hideoutTutoPanel;

    public GameObject largeTutoImagePanel;

    private bool showingImage;

    public NextTutoImageModule imageModule;

    private int lastSelectedHideoutTuto;

    private int currentTutoIdx = -1;

    public override int StateId => 1;

    public override void StateEnter()
    {
        if (!isInit)
        {
            isInit = true;
            GetComponentsInChildren(includeInactive: true, tutorialsView);
            foreach (GameModeTutorialView item in tutorialsView)
            {
                item.Load();
                item.button.onAction.AddListener(CheckTutoStart);
            }
            imageModule.Setup();
            hideoutTutoPanel.GetComponentsInChildren(includeInactive: true, hideoutTutorials);
            for (int i = 0; i < hideoutTutorials.Count; i++)
            {
                int index = i;
                hideoutTutorials[i].onAction.AddListener(delegate
                {
                    ShowTutoImage(index);
                });
            }
            largeTutoImagePanel.gameObject.SetActive(value: false);
        }
        RefreshTutorialDoneToggle();
        darkSideBar.SetActive(value: false);
        base.StateMachine.camManager.dummyCam.transform.position = camPos.transform.position;
        base.StateMachine.camManager.dummyCam.transform.rotation = camPos.transform.rotation;
        base.StateMachine.camManager.Transition();
        Show(visible: true);
        tutorialsView[0].GetComponentsInChildren<ToggleEffects>(includeInactive: true)[0].toggle.set_isOn(true);
        cancelButton.SetAction("cancel", "menu_back", 0, negative: false, icnBack);
        cancelButton.OnAction(OnInputCancel, mouseOnly: true);
        actionButton.SetAction("action", "menu_confirm");
        actionButton.OnAction(null, mouseOnly: false);
        OnInputTypeChanged();
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.INPUT_TYPE_CHANGED, OnInputTypeChanged);
    }

    private void RefreshTutorialDoneToggle()
    {
        for (int i = 0; i < tutorialsCompletionToggle.Count; i++)
        {
            if ((Object)(object)tutorialsCompletionToggle[i] != null)
            {
                if (i < PandoraSingleton<GameManager>.Instance.Profile.TutorialCompletion.Length - 1)
                {
                    tutorialsCompletionToggle[i].set_isOn(PandoraSingleton<GameManager>.Instance.Profile.TutorialCompletion[i]);
                }
                else
                {
                    tutorialsCompletionToggle[i].set_isOn(false);
                }
            }
        }
    }

    private void ShowTutoImage(int index)
    {
        if (!showingImage)
        {
            showingImage = true;
            actionButton.gameObject.SetActive(value: false);
            lastSelectedHideoutTuto = index;
            imageModule.Set(index, OnInputCancel);
            largeTutoImagePanel.gameObject.SetActive(value: true);
        }
    }

    private void HideTutoImage()
    {
        if (showingImage)
        {
            showingImage = false;
            actionButton.gameObject.SetActive(value: true);
            RefreshTutorialDoneToggle();
            largeTutoImagePanel.gameObject.SetActive(value: false);
            hideoutTutorials[lastSelectedHideoutTuto].SetSelected(force: true);
        }
    }

    public override void OnInputCancel()
    {
        if (!showingImage)
        {
            OnQuit();
        }
        else
        {
            HideTutoImage();
        }
    }

    public void OnQuit()
    {
        base.StateMachine.ChangeState(MainMenuController.State.MAIN_MENU);
    }

    public override void StateExit()
    {
        darkSideBar.SetActive(value: true);
        cancelButton.gameObject.SetActive(value: false);
        actionButton.gameObject.SetActive(value: false);
        Show(visible: false);
        PandoraSingleton<NoticeManager>.Instance.RemoveListener(Notices.INPUT_TYPE_CHANGED, OnInputTypeChanged);
    }

    private void CheckTutoStart()
    {
        GameModeTutorialView component = EventSystem.get_current().get_currentSelectedGameObject().GetComponent<GameModeTutorialView>();
        currentTutoIdx = component.tutorialIdx + 1;
        base.StateMachine.ConfirmPopup.Show($"tutorial_0{currentTutoIdx}_title", $"tutorial_0{currentTutoIdx}_menu", StartTutorial);
    }

    private void StartTutorial(bool confirmed)
    {
        if (!confirmed)
        {
            currentTutoIdx = -1;
            return;
        }
        if (!PandoraSingleton<MissionStartData>.Exists())
        {
            GameObject gameObject = new GameObject("mission_start_data");
            gameObject.AddComponent<MissionStartData>();
        }
        PandoraSingleton<MissionStartData>.Instance.ResetSeed();
        canvasGroup.interactable = false;
        EventSystem.get_current().get_currentInputModule().DeactivateModule();
        PandoraSingleton<PandoraInput>.Instance.SetActive(active: false);
        StartCoroutine(PandoraSingleton<MissionStartData>.Instance.SetMissionFull(Mission.GenerateCampaignMission(WarbandId.NONE, currentTutoIdx), null, delegate
        {
            currentTutoIdx = -1;
            SceneLauncher.Instance.LaunchScene(SceneLoadingId.LAUNCH_TUTO);
        }));
    }

    private void OnInputTypeChanged()
    {
        actionButton.gameObject.SetActive(PandoraSingleton<PandoraInput>.Instance.lastInputMode == PandoraInput.InputMode.JOYSTICK);
    }
}
