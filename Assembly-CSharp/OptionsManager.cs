using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Rewired;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsManager : MonoBehaviour
{
    private const float RESTORE_DELAY = 15f;

    private CameraManager camMan;

    [Header("UI items")]
    public ConfirmationPopupView confirmPopup;

    public ButtonGroup butExit;

    public ButtonGroup butBack;

    public ButtonGroup butRestore;

    public ButtonGroup butApply;

    public Action onSaveQuitGame;

    public Action onCloseOptionsMenu;

    public Action<bool> onQuitGame;

    public Sprite icnBack;

    public CanvasGroup overDescription;

    private Text overDescriptionText;

    private string backButtonLoc = "menu_back_main_menu";

    private string quitButtonLoc = "menu_quit_mission";

    private string quitAltButtonLoc = "menu_voluntary_rout";

    private string quitButtonOverDescLoc = "menu_abandon_preview_desc";

    private string quitAltButtonOverDescLoc = "menu_voluntary_rout_preview_desc";

    private string quitButtonOverDisabledDescLoc = "menu_no_abandon_desc";

    private string quitAltButtonOverDisabledDescLoc = "menu_no_voluntary_rout_desc";

    private string saveAndQuitDisabledDescLoc = "menu_no_save_and_quit_desc";

    private bool panelOpen;

    [Header("Panel toggles")]
    public ToggleEffects audioOptions;

    public ToggleEffects controlsOptions;

    public ToggleEffects gameplayOptions;

    public ToggleEffects graphics;

    public ToggleEffects mappings;

    public ToggleEffects butSaveAndQuit;

    public ToggleEffects butQuit;

    public ToggleEffects butQuitAlt;

    public ToggleEffects butHelp;

    public ToggleEffects butOpponentProfile;

    [Header("Audio")]
    public ToggleGroup audioPanel;

    public SliderGroup masterVolume;

    public SliderGroup effectsVolume;

    public SliderGroup musicVolume;

    public SliderGroup voiceVolume;

    public SliderGroup ambientVolume;

    private AudioSource audioSrc;

    private AudioClip ambientVolumeSample;

    [Header("Graphics")]
    public ToggleGroup graphicsPanel;

    public Toggle fullscreen;

    public SelectorGroup resolutions;

    public Toggle vsync;

    public SelectorGroup textureQuality;

    public SelectorGroup shadowsQuality;

    public SelectorGroup shadowCascades;

    public Toggle dof;

    public Toggle bloom;

    public Toggle ssao;

    public SelectorGroup smaa;

    public SliderGroup brightness;

    public SliderGroup guiScale;

    [Header("Controls")]
    public ToggleGroup controlsPanel;

    public Toggle gamepadEnabled;

    public Toggle invertCameraHorizontalEnabled;

    public Toggle invertCameraVerticalEnabled;

    public Toggle leftHandedMouseEnabled;

    public SliderGroup mouseSensitivitySlider;

    public Toggle leftHandedControllerEnabled;

    public SliderGroup joystickSensitivitySlider;

    [Header("Gameplay")]
    public ToggleGroup gameplayPanel;

    public SelectorGroup languageSelect;

    public Toggle tacticalViewHelpersEnabled;

    public Toggle wagonBeaconsEnabled;

    public Toggle autoExitTacticalEnabled;

    public Toggle displayFullUI;

    public Toggle fastForward;

    public Toggle skipTuto;

    [Header("Mapping")]
    public ToggleGroup mappingPanel;

    public GameObject controlEntry;

    public ScrollGroup controlsList;

    public RemapButtonPopupView controlsRemapPopup;

    private bool mappingsUnselected;

    private bool mappingChanged;

    private bool needRevertOptionsConfirm;

    private bool countdownToRestore;

    private float restoreTime;

    private int lastTimeDisplayed;

    private UIControlMappingItem remappedEntry;

    private int remappedButtonIndex;

    private ActionElementMap remappedAction;

    private GameObject lastSelection;

    private ActionElementMap conflictingAction;

    private Pole newMapKeyPole;

    private ControllerPollingInfo newMapInput;

    private List<SupportedLanguage> availableLangs = new List<SupportedLanguage>();

    private bool initialized;

    private void Awake()
    {
        controlsRemapPopup = UnityEngine.Object.Instantiate(controlsRemapPopup);
        SceneManager.MoveGameObjectToScene(controlsRemapPopup.gameObject, base.gameObject.scene);
        confirmPopup = UnityEngine.Object.Instantiate(confirmPopup);
        SceneManager.MoveGameObjectToScene(confirmPopup.gameObject, base.gameObject.scene);
        audioSrc = GetComponent<AudioSource>();
        overDescriptionText = overDescription.GetComponentsInChildren<Text>(includeInactive: true)[0];
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.CONTROLLER_CONNECTED, FillControlsPanel);
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.CONTROLLER_DISCONNECTED, FillControlsPanel);
        availableLangs = PandoraSingleton<Hephaestus>.Instance.GetAvailableLanguages();
        PandoraSingleton<Pan>.Instance.GetSound("snd_interface_general_click", cache: true, delegate (AudioClip clip)
        {
            ambientVolumeSample = clip;
        });
    }

    private void Init()
    {
        if (!initialized)
        {
            initialized = true;
            InitButtons();
            InitAudio();
            InitControls();
            InitGameplay();
            InitGraphics();
            InitMappings();
            butSaveAndQuit.onAction.AddListener(delegate
            {
                onSaveQuitGame();
            });
            butSaveAndQuit.onSelect.AddListener(ShowSaveAndQuitDesc);
            butSaveAndQuit.onUnselect.AddListener(HideOverDesc);
            butSaveAndQuit.onPointerEnter.AddListener(ShowSaveAndQuitDesc);
            butSaveAndQuit.onPointerExit.AddListener(HideOverDesc);
            ((UnityEvent<bool>)(object)butSaveAndQuit.toggle.onValueChanged).AddListener((UnityAction<bool>)delegate (bool isOn)
            {
                SetMappingsPanelButtonsVisible(!isOn);
            });
            butQuit.GetComponentsInChildren<Text>(includeInactive: true)[0].set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById(quitButtonLoc));
            butQuit.onAction.AddListener(delegate
            {
                onQuitGame(obj: true);
            });
            butQuit.onSelect.AddListener(ShowQuitDesc);
            butQuit.onUnselect.AddListener(HideOverDesc);
            butQuit.onPointerEnter.AddListener(ShowQuitDesc);
            butQuit.onPointerExit.AddListener(HideOverDesc);
            ((UnityEvent<bool>)(object)butQuit.toggle.onValueChanged).AddListener((UnityAction<bool>)delegate (bool isOn)
            {
                SetMappingsPanelButtonsVisible(!isOn);
            });
            butQuitAlt.GetComponentsInChildren<Text>(includeInactive: true)[0].set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById(quitAltButtonLoc));
            butQuitAlt.onAction.AddListener(delegate
            {
                onQuitGame(obj: false);
            });
            butQuitAlt.onSelect.AddListener(ShowAltQuitDesc);
            butQuitAlt.onUnselect.AddListener(HideOverDesc);
            butQuitAlt.onPointerEnter.AddListener(ShowAltQuitDesc);
            butQuitAlt.onPointerExit.AddListener(HideOverDesc);
            ((UnityEvent<bool>)(object)butQuitAlt.toggle.onValueChanged).AddListener((UnityAction<bool>)delegate (bool isOn)
            {
                SetMappingsPanelButtonsVisible(!isOn);
            });
            butHelp.gameObject.SetActive(value: false);
            butOpponentProfile.gameObject.SetActive(value: false);
            SetMappingsPanelButtonsVisible(visible: false);
        }
    }

    private void ShowQuitDesc()
    {
        if (!string.IsNullOrEmpty(quitButtonOverDescLoc))
        {
            if (butQuit.actionDisabled)
            {
                overDescriptionText.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById(quitButtonOverDisabledDescLoc));
            }
            else
            {
                overDescriptionText.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById(quitButtonOverDescLoc));
            }
            TweenOverDesc(1f);
        }
    }

    private void ShowAltQuitDesc()
    {
        if (!string.IsNullOrEmpty(quitAltButtonOverDescLoc))
        {
            if (butQuitAlt.actionDisabled)
            {
                overDescriptionText.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById(quitAltButtonOverDisabledDescLoc));
            }
            else
            {
                overDescriptionText.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById(quitAltButtonOverDescLoc));
            }
            TweenOverDesc(1f);
        }
    }

    private void ShowSaveAndQuitDesc()
    {
        if (butSaveAndQuit.actionDisabled)
        {
            overDescriptionText.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById(saveAndQuitDisabledDescLoc));
            TweenOverDesc(1f);
        }
    }

    private void HideOverDesc()
    {
        TweenOverDesc(0f);
    }

    private void TweenOverDesc(float start)
    {
        TweenSettingsExtensions.SetTarget<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => (overDescription != null) ? overDescription.alpha : 0f), (DOSetter<float>)delegate (float alpha)
        {
            if (overDescription != null)
            {
                overDescription.alpha = alpha;
            }
        }, start, 0.3f), (object)this);
    }

    private void OnDestroy()
    {
        if (PandoraSingleton<NoticeManager>.Instance != null)
        {
            PandoraSingleton<NoticeManager>.Instance.RemoveListener(Notices.CONTROLLER_CONNECTED, FillControlsPanel);
            PandoraSingleton<NoticeManager>.Instance.RemoveListener(Notices.CONTROLLER_DISCONNECTED, FillControlsPanel);
        }
    }

    private void OnEnable()
    {
        panelOpen = false;
    }

    private void OnDisable()
    {
        audioOptions.toggle.set_isOn(false);
        graphics.toggle.set_isOn(false);
        controlsOptions.toggle.set_isOn(false);
        gameplayOptions.toggle.set_isOn(false);
        mappings.toggle.set_isOn(false);
        butQuit.toggle.set_isOn(false);
        butQuitAlt.toggle.set_isOn(false);
        butQuit.enabled = true;
        butQuitAlt.enabled = true;
    }

    private void Update()
    {
        if (countdownToRestore)
        {
            int num = Mathf.CeilToInt(restoreTime - Time.time);
            if (num <= 0)
            {
                countdownToRestore = false;
                confirmPopup.Hide();
                RevertChanges();
            }
            else if (lastTimeDisplayed != num)
            {
                lastTimeDisplayed = num;
                confirmPopup.text.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("popup_apply_changes_revert", num.ToString()));
            }
        }
        if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("cancel", 6))
        {
            if (panelOpen)
            {
                OnBackToOptions();
            }
            else
            {
                OnBack();
            }
        }
    }

    public void OnShow()
    {
        Init();
        ((MonoBehaviour)(object)audioOptions.toggle).SetSelected(force: true);
        audioOptions.toggle.set_isOn(true);
        InitButtons();
        mappingChanged = false;
        FillControlsPanel();
        InitAudioData();
        InitControlsData();
        InitGameplayData();
        InitGraphicsData();
        InitMappingsData();
        overDescription.alpha = 0f;
        if (camMan == null && Camera.main != null)
        {
            camMan = Camera.main.GetComponent<CameraManager>();
        }
        if (camMan != null)
        {
            camMan.ActivateOverlay(active: true, 0f);
        }
        mappingsUnselected = true;
    }

    public void OnHide()
    {
        if (camMan == null && Camera.main != null)
        {
            camMan = Camera.main.GetComponent<CameraManager>();
        }
        if (camMan != null)
        {
            camMan.ActivateOverlay(active: false, 0f);
        }
        if (confirmPopup.IsVisible)
        {
            confirmPopup.Hide();
        }
        controlsRemapPopup.Hide();
        CancelMappingChanges();
        controlsList.ClearList();
        controlsList.DestroyItems();
        PandoraSingleton<GameManager>.Instance.SetVolumeOptions();
        PandoraSingleton<GameManager>.Instance.SetMappingOptions();
    }

    private void SetMappingsPanelButtonsVisible(bool visible)
    {
        if (butRestore != null)
        {
            butRestore.gameObject.SetActive(visible);
        }
    }

    private void InitButtons()
    {
        butExit.SetAction("cancel", "menu_back", 6, negative: false, icnBack);
        butExit.OnAction(OnBack, mouseOnly: true);
        butExit.gameObject.SetActive(value: true);
        butBack.gameObject.SetActive(value: false);
        butApply.SetAction("apply_changes", "menu_apply_changes", 6);
        butApply.OnAction(OnApplyChanges, mouseOnly: false);
        butApply.gameObject.SetActive(value: true);
        butRestore.SetAction("restore_default_mapping", "menu_restore_default_mappings", 6);
        butRestore.OnAction(ResetMappings, mouseOnly: false);
        butRestore.gameObject.SetActive(value: false);
    }

    public void HideQuitOption()
    {
        //IL_000b: Unknown result type (might be due to invalid IL or missing references)
        //IL_0010: Unknown result type (might be due to invalid IL or missing references)
        //IL_001d: Unknown result type (might be due to invalid IL or missing references)
        //IL_0022: Unknown result type (might be due to invalid IL or missing references)
        //IL_002f: Unknown result type (might be due to invalid IL or missing references)
        //IL_0034: Unknown result type (might be due to invalid IL or missing references)
        //IL_005d: Unknown result type (might be due to invalid IL or missing references)
        //IL_006f: Unknown result type (might be due to invalid IL or missing references)
        Navigation navigation = ((Selectable)butQuit.toggle).get_navigation();
        Navigation navigation2 = ((Selectable)((Component)(object)((Navigation)(ref navigation)).get_selectOnUp()).GetComponent<Toggle>()).get_navigation();
        Navigation navigation3 = ((Selectable)((Component)(object)((Navigation)(ref navigation)).get_selectOnDown()).GetComponent<Toggle>()).get_navigation();
        ((Navigation)(ref navigation2)).set_selectOnDown(((Navigation)(ref navigation)).get_selectOnDown());
        ((Navigation)(ref navigation3)).set_selectOnUp(((Navigation)(ref navigation)).get_selectOnUp());
        ((Selectable)((Component)(object)((Navigation)(ref navigation)).get_selectOnUp()).GetComponent<Toggle>()).set_navigation(navigation2);
        ((Selectable)((Component)(object)((Navigation)(ref navigation)).get_selectOnDown()).GetComponent<Toggle>()).set_navigation(navigation3);
        butQuit.gameObject.SetActive(value: false);
        HideSaveAndQuitOption();
        HideAltQuitOption();
    }

    public void HideAltQuitOption()
    {
        //IL_000b: Unknown result type (might be due to invalid IL or missing references)
        //IL_0010: Unknown result type (might be due to invalid IL or missing references)
        //IL_001d: Unknown result type (might be due to invalid IL or missing references)
        //IL_0022: Unknown result type (might be due to invalid IL or missing references)
        //IL_003d: Unknown result type (might be due to invalid IL or missing references)
        Navigation navigation = ((Selectable)butQuitAlt.toggle).get_navigation();
        Navigation navigation2 = ((Selectable)((Component)(object)((Navigation)(ref navigation)).get_selectOnUp()).GetComponent<Toggle>()).get_navigation();
        ((Navigation)(ref navigation2)).set_selectOnDown(((Navigation)(ref navigation)).get_selectOnDown());
        ((Selectable)((Component)(object)((Navigation)(ref navigation)).get_selectOnUp()).GetComponent<Toggle>()).set_navigation(navigation2);
        butQuitAlt.gameObject.SetActive(value: false);
    }

    public void HideSaveAndQuitOption()
    {
        //IL_000b: Unknown result type (might be due to invalid IL or missing references)
        //IL_0010: Unknown result type (might be due to invalid IL or missing references)
        //IL_001d: Unknown result type (might be due to invalid IL or missing references)
        //IL_0022: Unknown result type (might be due to invalid IL or missing references)
        //IL_003d: Unknown result type (might be due to invalid IL or missing references)
        //IL_004f: Unknown result type (might be due to invalid IL or missing references)
        //IL_0054: Unknown result type (might be due to invalid IL or missing references)
        //IL_006f: Unknown result type (might be due to invalid IL or missing references)
        Navigation navigation = ((Selectable)butSaveAndQuit.toggle).get_navigation();
        Navigation navigation2 = ((Selectable)((Component)(object)((Navigation)(ref navigation)).get_selectOnUp()).GetComponent<Toggle>()).get_navigation();
        ((Navigation)(ref navigation2)).set_selectOnDown(((Navigation)(ref navigation)).get_selectOnDown());
        ((Selectable)((Component)(object)((Navigation)(ref navigation)).get_selectOnUp()).GetComponent<Toggle>()).set_navigation(navigation2);
        Navigation navigation3 = ((Selectable)((Component)(object)((Navigation)(ref navigation)).get_selectOnDown()).GetComponent<Toggle>()).get_navigation();
        ((Navigation)(ref navigation3)).set_selectOnUp(((Navigation)(ref navigation)).get_selectOnUp());
        ((Selectable)((Component)(object)((Navigation)(ref navigation)).get_selectOnDown()).GetComponent<Toggle>()).set_navigation(navigation3);
        butSaveAndQuit.gameObject.SetActive(value: false);
    }

    public void SetBackButtonLoc(string loc)
    {
        backButtonLoc = loc;
    }

    public void SetQuitButtonLoc(string loc, string desc = "")
    {
        quitButtonLoc = loc;
        quitButtonOverDescLoc = desc;
    }

    public void SetQuitAltButtonLoc(string loc, string desc = "")
    {
        quitAltButtonLoc = loc;
        quitAltButtonOverDescLoc = desc;
    }

    public void DisableQuitOption()
    {
        butQuit.DisableAction();
        ((Selectable)butQuit.toggle).set_interactable(false);
        butQuitAlt.EnableAction();
        ((Selectable)butQuitAlt.toggle).set_interactable(true);
    }

    public void DisableAltQuitOption()
    {
        butQuit.EnableAction();
        ((Selectable)butQuit.toggle).set_interactable(true);
        butQuitAlt.DisableAction();
        ((Selectable)butQuitAlt.toggle).set_interactable(false);
    }

    public void DisableSaveAndQuitOption()
    {
        butSaveAndQuit.DisableAction();
        ((Selectable)butSaveAndQuit.toggle).set_interactable(false);
    }

    public void RemoveFromNav(ToggleEffects itm)
    {
        //IL_0006: Unknown result type (might be due to invalid IL or missing references)
        //IL_000b: Unknown result type (might be due to invalid IL or missing references)
        //IL_002c: Unknown result type (might be due to invalid IL or missing references)
        //IL_0031: Unknown result type (might be due to invalid IL or missing references)
        //IL_0041: Unknown result type (might be due to invalid IL or missing references)
        //IL_0067: Unknown result type (might be due to invalid IL or missing references)
        //IL_006c: Unknown result type (might be due to invalid IL or missing references)
        //IL_007d: Unknown result type (might be due to invalid IL or missing references)
        Navigation navigation = ((Selectable)itm.toggle).get_navigation();
        if ((UnityEngine.Object)(object)((Navigation)(ref navigation)).get_selectOnUp() != null)
        {
            Toggle component = ((Component)(object)((Navigation)(ref navigation)).get_selectOnUp()).GetComponent<Toggle>();
            Navigation navigation2 = ((Selectable)component).get_navigation();
            ((Navigation)(ref navigation2)).set_selectOnDown(((Navigation)(ref navigation)).get_selectOnDown());
            ((Selectable)component).set_navigation(navigation2);
        }
        if ((UnityEngine.Object)(object)((Navigation)(ref navigation)).get_selectOnDown() != null)
        {
            Toggle component2 = ((Component)(object)((Navigation)(ref navigation)).get_selectOnDown()).GetComponent<Toggle>();
            Navigation navigation3 = ((Selectable)component2).get_navigation();
            ((Navigation)(ref navigation3)).set_selectOnUp(((Navigation)(ref navigation)).get_selectOnUp());
            ((Selectable)component2).set_navigation(navigation3);
        }
    }

    public void AddBackToNav(ToggleEffects itm)
    {
        //IL_0006: Unknown result type (might be due to invalid IL or missing references)
        //IL_000b: Unknown result type (might be due to invalid IL or missing references)
        //IL_002c: Unknown result type (might be due to invalid IL or missing references)
        //IL_0031: Unknown result type (might be due to invalid IL or missing references)
        //IL_0040: Unknown result type (might be due to invalid IL or missing references)
        //IL_0066: Unknown result type (might be due to invalid IL or missing references)
        //IL_006b: Unknown result type (might be due to invalid IL or missing references)
        //IL_007b: Unknown result type (might be due to invalid IL or missing references)
        Navigation navigation = ((Selectable)itm.toggle).get_navigation();
        if ((UnityEngine.Object)(object)((Navigation)(ref navigation)).get_selectOnUp() != null)
        {
            Toggle component = ((Component)(object)((Navigation)(ref navigation)).get_selectOnUp()).GetComponent<Toggle>();
            Navigation navigation2 = ((Selectable)component).get_navigation();
            ((Navigation)(ref navigation2)).set_selectOnDown((Selectable)(object)itm.toggle);
            ((Selectable)component).set_navigation(navigation2);
        }
        if ((UnityEngine.Object)(object)((Navigation)(ref navigation)).get_selectOnDown() != null)
        {
            Toggle component2 = ((Component)(object)((Navigation)(ref navigation)).get_selectOnDown()).GetComponent<Toggle>();
            Navigation navigation3 = ((Selectable)component2).get_navigation();
            ((Navigation)(ref navigation3)).set_selectOnUp((Selectable)(object)itm.toggle);
            ((Selectable)component2).set_navigation(navigation3);
        }
    }

    private void ToggleOffLastSelection()
    {
        bool activeSelf = ((Component)(object)audioPanel).gameObject.activeSelf;
        ((Component)(object)audioPanel).gameObject.SetActive(value: true);
        audioPanel.SetAllTogglesOff();
        ((Component)(object)audioPanel).gameObject.SetActive(activeSelf);
        activeSelf = ((Component)(object)graphicsPanel).gameObject.activeSelf;
        ((Component)(object)graphicsPanel).gameObject.SetActive(value: true);
        graphicsPanel.SetAllTogglesOff();
        ((Component)(object)graphicsPanel).gameObject.SetActive(activeSelf);
        activeSelf = ((Component)(object)controlsPanel).gameObject.activeSelf;
        ((Component)(object)controlsPanel).gameObject.SetActive(value: true);
        controlsPanel.SetAllTogglesOff();
        ((Component)(object)controlsPanel).gameObject.SetActive(activeSelf);
        activeSelf = ((Component)(object)gameplayPanel).gameObject.activeSelf;
        ((Component)(object)gameplayPanel).gameObject.SetActive(value: true);
        gameplayPanel.SetAllTogglesOff();
        ((Component)(object)gameplayPanel).gameObject.SetActive(activeSelf);
        activeSelf = ((Component)(object)mappingPanel).gameObject.activeSelf;
        ((Component)(object)mappingPanel).gameObject.SetActive(value: true);
        mappingPanel.SetAllTogglesOff();
        ((Component)(object)mappingPanel).gameObject.SetActive(activeSelf);
    }

    private void OnPanelOpen()
    {
        ToggleOffLastSelection();
        panelOpen = true;
    }

    public void OnBack()
    {
        if (CheckNeedApply())
        {
            confirmPopup.Show("popup_apply_exit_title", "popup_apply_exit_desc", delegate (bool confirm)
            {
                if (confirm)
                {
                    DoApplyChanges(onCloseOptionsMenu);
                }
                else
                {
                    CancelMappingChanges();
                    onCloseOptionsMenu();
                }
            });
        }
        else
        {
            onCloseOptionsMenu();
        }
    }

    public void OnBackToOptions()
    {
        panelOpen = false;
        GameObject currentSelectedGameObject = EventSystem.get_current().get_currentSelectedGameObject();
        if (audioOptions.toggle.get_isOn())
        {
            audioPanel.SetAllTogglesOff();
            ((Selectable)audioOptions.toggle).Select();
        }
        else if (graphics.toggle.get_isOn())
        {
            graphicsPanel.SetAllTogglesOff();
            ((Selectable)graphics.toggle).Select();
        }
        else if (controlsOptions.toggle.get_isOn())
        {
            controlsPanel.SetAllTogglesOff();
            ((Selectable)controlsOptions.toggle).Select();
        }
        else if (gameplayOptions.toggle.get_isOn())
        {
            gameplayPanel.SetAllTogglesOff();
            ((Selectable)gameplayOptions.toggle).Select();
        }
        else if (mappings.toggle.get_isOn())
        {
            mappingPanel.SetAllTogglesOff();
            ((Selectable)mappings.toggle).Select();
        }
    }

    private bool CheckNeedApply()
    {
        return CheckAudioNeedApply() || CheckControlsNeedApply() || CheckGameplayNeedApply() || CheckMappingNeedApply() || CheckGraphicsNeedApply();
    }

    private void OnApplyChanges()
    {
        if (CheckNeedApply())
        {
            DoApplyChanges();
        }
    }

    private void DoApplyChanges(Action onDone = null)
    {
        bool noRestartNeeded = true;
        noRestartNeeded &= ApplyAudioChanges();
        noRestartNeeded &= ApplyGraphicsChanges();
        noRestartNeeded &= ApplyMappingChanges();
        noRestartNeeded &= ApplyControlsChanges();
        noRestartNeeded &= ApplyGameplayChanges();
        if (needRevertOptionsConfirm)
        {
            countdownToRestore = true;
            restoreTime = Time.time + 15f;
            lastTimeDisplayed = Mathf.CeilToInt(15f);
            confirmPopup.ShowLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("popup_apply_changes_title"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("popup_apply_changes_revert", 15f.ToString()), delegate (bool confirm)
            {
                countdownToRestore = false;
                if (confirm)
                {
                    SaveChanges(noRestartNeeded, onDone);
                }
                else
                {
                    RevertChanges();
                }
            });
        }
        else
        {
            SaveChanges(noRestartNeeded, onDone);
        }
    }

    private void SaveChanges(bool noRestartNeeded = false, Action onDone = null)
    {
        PandoraSingleton<GameManager>.Instance.WriteOptions();
        if (!noRestartNeeded)
        {
            confirmPopup.Show("menu_title_changes_after_resart", "menu_desc_changes_after_resart", delegate
            {
                FillControlsPanel();
                if (onDone != null)
                {
                    onDone();
                }
            });
            confirmPopup.HideCancelButton();
            return;
        }
        FillControlsPanel();
        if (onDone != null)
        {
            onDone();
        }
    }

    private void RevertChanges()
    {
        PandoraSingleton<GameManager>.Instance.ReadOptions();
    }

    private void InitAudio()
    {
        audioOptions.onAction.AddListener(OnAudioPanelOpen);
        audioOptions.onSelect.AddListener(OnAudioPanelDisplayed);
        InitAudioData();
    }

    private void InitAudioData()
    {
        musicVolume.SetValue(PandoraSingleton<GameManager>.Instance.Options.musicVolume * 100f + 0.5f);
        musicVolume.id = 2;
        musicVolume.onValueChanged = OnVolumeValueChanged;
        effectsVolume.SetValue(PandoraSingleton<GameManager>.Instance.Options.fxVolume * 100f + 0.5f);
        effectsVolume.id = 1;
        effectsVolume.onValueChanged = OnVolumeValueChanged;
        masterVolume.SetValue(PandoraSingleton<GameManager>.Instance.Options.masterVolume * 100f + 0.5f);
        masterVolume.id = 0;
        masterVolume.onValueChanged = OnVolumeValueChanged;
        voiceVolume.SetValue(PandoraSingleton<GameManager>.Instance.Options.voiceVolume * 100f + 0.5f);
        voiceVolume.id = 4;
        voiceVolume.onValueChanged = OnVolumeValueChanged;
        ambientVolume.SetValue(PandoraSingleton<GameManager>.Instance.Options.ambientVolume * 100f + 0.5f);
        ambientVolume.id = 3;
        ambientVolume.onValueChanged = OnVolumeValueChanged;
    }

    private void OnAudioPanelDisplayed()
    {
        ToggleOffLastSelection();
        SetMappingsPanelButtonsVisible(visible: false);
    }

    private void OnAudioPanelOpen()
    {
        OnPanelOpen();
        ((MonoBehaviour)(object)masterVolume.GetComponentsInChildren<Slider>(includeInactive: true)[0]).SetSelected(force: true);
        OnAudioPanelDisplayed();
    }

    private bool CheckAudioNeedApply()
    {
        if (!Mathf.Approximately(PandoraSingleton<GameManager>.Instance.Options.masterVolume, (float)masterVolume.GetValue() / 100f))
        {
            PandoraDebug.LogDebug("Options require Apply action, masterVolume changed from " + PandoraSingleton<GameManager>.Instance.Options.masterVolume + " to " + (float)masterVolume.GetValue() / 100f, "OPTIONS", this);
            return true;
        }
        if (!Mathf.Approximately(PandoraSingleton<GameManager>.Instance.Options.fxVolume, (float)effectsVolume.GetValue() / 100f))
        {
            PandoraDebug.LogDebug("Options require Apply action, fxVolume changed from " + PandoraSingleton<GameManager>.Instance.Options.fxVolume + " to " + (float)effectsVolume.GetValue() / 100f, "OPTIONS", this);
            return true;
        }
        if (!Mathf.Approximately(PandoraSingleton<GameManager>.Instance.Options.musicVolume, (float)musicVolume.GetValue() / 100f))
        {
            PandoraDebug.LogDebug("Options require Apply action, fxVolume changed from " + PandoraSingleton<GameManager>.Instance.Options.musicVolume + " to " + (float)musicVolume.GetValue() / 100f, "OPTIONS", this);
            return true;
        }
        if (!Mathf.Approximately(PandoraSingleton<GameManager>.Instance.Options.voiceVolume, (float)voiceVolume.GetValue() / 100f))
        {
            PandoraDebug.LogDebug("Options require Apply action, fxVolume changed from " + PandoraSingleton<GameManager>.Instance.Options.voiceVolume + " to " + (float)voiceVolume.GetValue() / 100f, "OPTIONS", this);
            return true;
        }
        if (!Mathf.Approximately(PandoraSingleton<GameManager>.Instance.Options.ambientVolume, (float)ambientVolume.GetValue() / 100f))
        {
            PandoraDebug.LogDebug("Options require Apply action, ambient Volume changed from " + PandoraSingleton<GameManager>.Instance.Options.ambientVolume + " to " + (float)ambientVolume.GetValue() / 100f, "OPTIONS", this);
            return true;
        }
        return false;
    }

    private bool ApplyAudioChanges()
    {
        bool flag = false;
        if (!Mathf.Approximately(PandoraSingleton<GameManager>.Instance.Options.masterVolume, (float)masterVolume.GetValue() / 100f))
        {
            flag = true;
            PandoraSingleton<GameManager>.Instance.Options.masterVolume = (float)masterVolume.GetValue() / 100f;
        }
        if (!Mathf.Approximately(PandoraSingleton<GameManager>.Instance.Options.fxVolume, (float)effectsVolume.GetValue() / 100f))
        {
            flag = true;
            PandoraSingleton<GameManager>.Instance.Options.fxVolume = (float)effectsVolume.GetValue() / 100f;
        }
        if (!Mathf.Approximately(PandoraSingleton<GameManager>.Instance.Options.musicVolume, (float)musicVolume.GetValue() / 100f))
        {
            flag = true;
            PandoraSingleton<GameManager>.Instance.Options.musicVolume = (float)musicVolume.GetValue() / 100f;
        }
        if (!Mathf.Approximately(PandoraSingleton<GameManager>.Instance.Options.voiceVolume, (float)voiceVolume.GetValue() / 100f))
        {
            flag = true;
            PandoraSingleton<GameManager>.Instance.Options.voiceVolume = (float)voiceVolume.GetValue() / 100f;
        }
        if (!Mathf.Approximately(PandoraSingleton<GameManager>.Instance.Options.ambientVolume, (float)ambientVolume.GetValue() / 100f))
        {
            flag = true;
            PandoraSingleton<GameManager>.Instance.Options.ambientVolume = (float)ambientVolume.GetValue() / 100f;
        }
        if (flag)
        {
            PandoraSingleton<GameManager>.Instance.SetVolumeOptions();
        }
        return true;
    }

    private void OnVolumeValueChanged(int id, float value)
    {
        value /= 100f;
        PandoraSingleton<Pan>.Instance.SetVolume((Pan.Type)id, value);
        switch (id)
        {
            case 0:
            case 1:
            case 4:
                audioSrc.PlayOneShot(ambientVolumeSample, value);
                break;
            case 3:
                if (ambientVolumeSample != null)
                {
                    audioSrc.PlayOneShot(ambientVolumeSample, value);
                }
                break;
        }
    }

    private void InitGraphics()
    {
        graphics.onAction.AddListener(OnGraphicsPanelOpen);
        graphics.onSelect.AddListener(OnGraphicsPanelDisplayed);
        InitGraphicsData();
    }

    private void InitGraphicsData()
    {
        fullscreen.set_isOn(PandoraSingleton<GameManager>.Instance.Options.fullScreen);
        resolutions.selections.Clear();
        int currentSel = Screen.resolutions.Length - 1;
        for (int i = 0; i < Screen.resolutions.Length; i++)
        {
            if (Screen.resolutions[i].width == PandoraSingleton<GameManager>.Instance.Options.resolution.width && Screen.resolutions[i].height == PandoraSingleton<GameManager>.Instance.Options.resolution.height)
            {
                currentSel = i;
            }
            resolutions.selections.Add($"{Screen.resolutions[i].width}x{Screen.resolutions[i].height}");
        }
        resolutions.SetCurrentSel(currentSel);
        vsync.set_isOn(PandoraSingleton<GameManager>.Instance.Options.vsync);
        shadowsQuality.selections.Clear();
        shadowsQuality.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_off"));
        shadowsQuality.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById("opt_low"));
        shadowsQuality.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById("opt_medium"));
        shadowsQuality.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById("opt_high"));
        shadowsQuality.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById("opt_very_high"));
        shadowsQuality.SetCurrentSel(PandoraSingleton<GameManager>.Instance.Options.shadowsQuality);
        shadowCascades.selections.Clear();
        shadowCascades.selections.Add("0");
        shadowCascades.selections.Add("2");
        shadowCascades.selections.Add("4");
        shadowCascades.SetCurrentSel(PandoraSingleton<GameManager>.Instance.Options.shadowCascades);
        textureQuality.selections.Clear();
        textureQuality.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById("opt_low"));
        textureQuality.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById("opt_medium"));
        textureQuality.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById("opt_high"));
        textureQuality.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById("opt_very_high"));
        textureQuality.SetCurrentSel(PandoraSingleton<GameManager>.Instance.Options.textureQuality);
        dof.set_isOn(PandoraSingleton<GameManager>.Instance.Options.graphicsDof);
        ssao.set_isOn(PandoraSingleton<GameManager>.Instance.Options.graphicsSsao);
        smaa.selections.Clear();
        smaa.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_off"));
        smaa.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById("opt_low"));
        smaa.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById("opt_medium"));
        smaa.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById("opt_high"));
        smaa.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById("opt_very_high"));
        smaa.SetCurrentSel(PandoraSingleton<GameManager>.Instance.Options.graphicsSmaa);
        brightness.SetValue(PandoraSingleton<GameManager>.Instance.Options.graphicsBrightness * 100f);
        guiScale.SetValue(PandoraSingleton<GameManager>.Instance.Options.graphicsGuiScale * 100f);
    }

    private void OnGraphicsPanelDisplayed()
    {
        SetMappingsPanelButtonsVisible(visible: false);
    }

    private void OnGraphicsPanelOpen()
    {
        OnPanelOpen();
        ((MonoBehaviour)(object)fullscreen).SetSelected(force: true);
        OnGraphicsPanelDisplayed();
    }

    private bool CheckGraphicsNeedApply()
    {
        if (PandoraSingleton<GameManager>.Instance.Options.fullScreen != fullscreen.get_isOn())
        {
            PandoraDebug.LogDebug("Options require Apply action, fullScreen changed from " + PandoraSingleton<GameManager>.Instance.Options.fullScreen + " to " + fullscreen.get_isOn(), "OPTIONS", this);
            return true;
        }
        if (PandoraSingleton<GameManager>.Instance.Options.resolution.width != Screen.resolutions[resolutions.CurSel].width || PandoraSingleton<GameManager>.Instance.Options.resolution.height != Screen.resolutions[resolutions.CurSel].height)
        {
            PandoraDebug.LogDebug("Options require Apply action, resolution changed from " + PandoraSingleton<GameManager>.Instance.Options.resolution + " to " + Screen.resolutions[resolutions.CurSel], "OPTIONS", this);
            return true;
        }
        if (PandoraSingleton<GameManager>.Instance.Options.vsync != vsync.get_isOn())
        {
            PandoraDebug.LogDebug("Options require Apply action, vsync changed from " + PandoraSingleton<GameManager>.Instance.Options.vsync + " to " + vsync.get_isOn(), "OPTIONS", this);
            return true;
        }
        if (PandoraSingleton<GameManager>.Instance.Options.textureQuality != textureQuality.CurSel)
        {
            PandoraDebug.LogDebug("Options require Apply action, textureQuality changed from " + PandoraSingleton<GameManager>.Instance.Options.textureQuality + " to " + textureQuality.CurSel, "OPTIONS", this);
            return true;
        }
        if (PandoraSingleton<GameManager>.Instance.Options.shadowsQuality != shadowsQuality.CurSel)
        {
            PandoraDebug.LogDebug("Options require Apply action, shadow quality changed from " + PandoraSingleton<GameManager>.Instance.Options.shadowsQuality + " to " + shadowsQuality.CurSel, "OPTIONS", this);
            return true;
        }
        if (PandoraSingleton<GameManager>.Instance.Options.shadowCascades != shadowCascades.CurSel)
        {
            PandoraDebug.LogDebug("Options require Apply action, shadow cascades changed from " + PandoraSingleton<GameManager>.Instance.Options.shadowCascades + " to " + shadowCascades.CurSel, "OPTIONS", this);
            return true;
        }
        if (PandoraSingleton<GameManager>.Instance.Options.graphicsDof != dof.get_isOn())
        {
            PandoraDebug.LogDebug("Options require Apply action, DoF changed from " + PandoraSingleton<GameManager>.Instance.Options.graphicsDof + " to " + dof.get_isOn(), "OPTIONS", this);
            return true;
        }
        if (PandoraSingleton<GameManager>.Instance.Options.graphicsSsao != ssao.get_isOn())
        {
            PandoraDebug.LogDebug("Options require Apply action, SSAO changed from " + PandoraSingleton<GameManager>.Instance.Options.graphicsSsao + " to " + ssao.get_isOn(), "OPTIONS", this);
            return true;
        }
        if (PandoraSingleton<GameManager>.Instance.Options.graphicsSmaa != smaa.CurSel)
        {
            PandoraDebug.LogDebug("Options require Apply action, SMAA changed from " + PandoraSingleton<GameManager>.Instance.Options.graphicsSmaa + " to " + smaa.CurSel, "OPTIONS", this);
            return true;
        }
        if (!Mathf.Approximately(PandoraSingleton<GameManager>.Instance.Options.graphicsBrightness, (float)brightness.GetValue() / 100f))
        {
            PandoraDebug.LogDebug("Options require Apply action, Brightness changed from " + PandoraSingleton<GameManager>.Instance.Options.graphicsBrightness + " to " + (float)brightness.GetValue() / 100f, "OPTIONS", this);
            return true;
        }
        if (!Mathf.Approximately(PandoraSingleton<GameManager>.Instance.Options.graphicsGuiScale, (float)guiScale.GetValue() / 100f))
        {
            PandoraDebug.LogDebug("Options require Apply action, Gui Scale changed from " + PandoraSingleton<GameManager>.Instance.Options.graphicsGuiScale + " to " + (float)guiScale.GetValue() / 100f, "OPTIONS", this);
            return true;
        }
        return false;
    }

    private bool ApplyGraphicsChanges()
    {
        bool flag = false;
        bool result = true;
        needRevertOptionsConfirm = false;
        if (PandoraSingleton<GameManager>.Instance.Options.fullScreen != fullscreen.get_isOn())
        {
            flag = true;
            PandoraSingleton<GameManager>.Instance.Options.fullScreen = fullscreen.get_isOn();
        }
        if (PandoraSingleton<GameManager>.Instance.Options.resolution.width != Screen.resolutions[resolutions.CurSel].width || PandoraSingleton<GameManager>.Instance.Options.resolution.height != Screen.resolutions[resolutions.CurSel].height)
        {
            flag = true;
            PandoraSingleton<GameManager>.Instance.Options.resolution = Screen.resolutions[resolutions.CurSel];
        }
        if (PandoraSingleton<GameManager>.Instance.Options.vsync != vsync.get_isOn())
        {
            flag = true;
            PandoraSingleton<GameManager>.Instance.Options.vsync = vsync.get_isOn();
        }
        if (PandoraSingleton<GameManager>.Instance.Options.textureQuality != textureQuality.CurSel)
        {
            flag = true;
            result = false;
            PandoraSingleton<GameManager>.Instance.Options.textureQuality = textureQuality.CurSel;
        }
        if (PandoraSingleton<GameManager>.Instance.Options.shadowsQuality != shadowsQuality.CurSel)
        {
            flag = true;
            result = false;
            PandoraSingleton<GameManager>.Instance.Options.shadowsQuality = shadowsQuality.CurSel;
        }
        if (PandoraSingleton<GameManager>.Instance.Options.shadowCascades != shadowCascades.CurSel)
        {
            flag = true;
            PandoraSingleton<GameManager>.Instance.Options.shadowCascades = shadowCascades.CurSel;
        }
        if (PandoraSingleton<GameManager>.Instance.Options.graphicsDof != dof.get_isOn())
        {
            flag = true;
            PandoraSingleton<GameManager>.Instance.Options.graphicsDof = dof.get_isOn();
        }
        if (PandoraSingleton<GameManager>.Instance.Options.graphicsSsao != ssao.get_isOn())
        {
            flag = true;
            PandoraSingleton<GameManager>.Instance.Options.graphicsSsao = ssao.get_isOn();
        }
        if (PandoraSingleton<GameManager>.Instance.Options.graphicsSmaa != smaa.CurSel)
        {
            flag = true;
            PandoraSingleton<GameManager>.Instance.Options.graphicsSmaa = smaa.CurSel;
        }
        if (!Mathf.Approximately(PandoraSingleton<GameManager>.Instance.Options.graphicsBrightness, (float)brightness.GetValue() / 100f))
        {
            flag = true;
            PandoraSingleton<GameManager>.Instance.Options.graphicsBrightness = (float)brightness.GetValue() / 100f;
        }
        if (!Mathf.Approximately(PandoraSingleton<GameManager>.Instance.Options.graphicsGuiScale, (float)guiScale.GetValue() / 100f))
        {
            flag = true;
            PandoraSingleton<GameManager>.Instance.Options.graphicsGuiScale = (float)guiScale.GetValue() / 100f;
        }
        if (flag)
        {
            PandoraSingleton<GameManager>.Instance.SetGraphicOptions();
            needRevertOptionsConfirm = true;
        }
        return result;
    }

    private void InitMappings()
    {
        mappings.onAction.AddListener(OnMappingPanelOpen);
        mappings.onSelect.AddListener(OnMappingPanelDisplayed);
        ((UnityEvent<bool>)(object)mappings.toggle.onValueChanged).AddListener((UnityAction<bool>)ClearMappings);
        InitMappingsData();
    }

    private void InitMappingsData()
    {
        FillControlsPanel();
        mappingChanged = false;
    }

    private void OnMappingPanelDisplayed()
    {
        Debug.Log("OnMappingPanelDisplayed " + mappingsUnselected);
        if (mappingsUnselected)
        {
            SetMappingsPanelButtonsVisible(visible: true);
            FillControlsPanel();
            StartCoroutine(RealignOnNextFrame());
        }
        else
        {
            controlsList.ResetSelection();
        }
    }

    private IEnumerator RealignOnNextFrame()
    {
        yield return null;
        controlsList.RealignList(isOn: true, 0, force: true);
    }

    private void OnMappingPanelOpen()
    {
        Debug.Log("OnMappingPanelOpen " + mappingsUnselected);
        if (mappingsUnselected)
        {
            SetMappingsPanelButtonsVisible(visible: true);
            FillControlsPanel();
        }
        OnPanelOpen();
        controlsList.GetComponentsInChildren<ToggleEffects>(includeInactive: true)[0].SetOn();
    }

    private void ClearMappings(bool value)
    {
        Debug.Log("ClearMappings " + mappingsUnselected);
        if (!value)
        {
            controlsList.ClearList();
            controlsList.DestroyItems();
            mappingsUnselected = true;
        }
    }

    public void FillControlsPanel()
    {
        //IL_0078: Unknown result type (might be due to invalid IL or missing references)
        //IL_007e: Invalid comparison between Unknown and I4
        if (base.gameObject.activeInHierarchy && mappings.toggle.get_isOn())
        {
            mappingsUnselected = false;
            Debug.Log("Fill list");
            controlsList.ClearList();
            controlsList.Setup(controlEntry, hideBarIfEmpty: false);
            IEnumerable<InputAction> enumerable = ReInput.get_mapping().UserAssignableActionsInCategory("game_input");
            foreach (InputAction item in enumerable)
            {
                if ((int)item.get_type() == 1)
                {
                    if (item.get_name() == "more_info_unit")
                    {
                        string stringById = PandoraSingleton<LocalizationManager>.Instance.GetStringById(item.get_descriptiveName());
                        CreateControlMappingEntry(stringById, item.get_id(), isPositiveInput: true, "game_input", controlsList);
                        int num = 0;
                        foreach (InputAction item2 in enumerable)
                        {
                            if (item2.get_name() == "unit_info_buffs" || item2.get_name() == "unit_info_resists" || item2.get_name() == "unit_info_stats")
                            {
                                stringById = PandoraSingleton<LocalizationManager>.Instance.GetStringById(item2.get_descriptiveName());
                                CreateControlMappingEntry(stringById, item2.get_id(), isPositiveInput: true, "game_input", controlsList, allowGamepadMapping: false);
                                if (++num >= 3)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    else if (item.get_name() != "unit_info_buffs" && item.get_name() != "unit_info_resists" && item.get_name() != "unit_info_stats")
                    {
                        string stringById2 = PandoraSingleton<LocalizationManager>.Instance.GetStringById(item.get_descriptiveName());
                        CreateControlMappingEntry(stringById2, item.get_id(), isPositiveInput: true, "game_input", controlsList);
                    }
                }
                else if (item.get_name() == "v" || item.get_name() == "cam_y")
                {
                    string stringById3 = PandoraSingleton<LocalizationManager>.Instance.GetStringById(item.get_positiveDescriptiveName());
                    CreateControlMappingEntry(stringById3, item.get_id(), isPositiveInput: true, "game_input", controlsList);
                    stringById3 = PandoraSingleton<LocalizationManager>.Instance.GetStringById(item.get_negativeDescriptiveName());
                    CreateControlMappingEntry(stringById3, item.get_id(), isPositiveInput: false, "game_input", controlsList);
                }
                else
                {
                    string stringById4 = PandoraSingleton<LocalizationManager>.Instance.GetStringById(item.get_negativeDescriptiveName());
                    CreateControlMappingEntry(stringById4, item.get_id(), isPositiveInput: false, "game_input", controlsList);
                    stringById4 = PandoraSingleton<LocalizationManager>.Instance.GetStringById(item.get_positiveDescriptiveName());
                    CreateControlMappingEntry(stringById4, item.get_id(), isPositiveInput: true, "game_input", controlsList);
                }
            }
            SetupControlsListNavig(controlsList);
            if (base.gameObject.activeInHierarchy)
            {
                StartCoroutine(OnListFilled());
            }
        }
    }

    private void SetupControlsListNavig(ScrollGroup controlsList)
    {
        UIControlMappingItem[] componentsInChildren = controlsList.GetComponentsInChildren<UIControlMappingItem>(includeInactive: true);
        for (int i = 0; i < componentsInChildren.Length; i++)
        {
            UIControlMappingItem up = (i != 0) ? componentsInChildren[i - 1] : componentsInChildren[componentsInChildren.Length - 1];
            UIControlMappingItem down = (i != componentsInChildren.Length - 1) ? componentsInChildren[i + 1] : componentsInChildren[0];
            componentsInChildren[i].SetNav(up, down);
        }
    }

    private IEnumerator OnListFilled()
    {
        yield return null;
        if (base.gameObject.activeInHierarchy)
        {
            int timer = Mathf.CeilToInt(restoreTime - Time.time);
            if (confirmPopup.IsVisible && timer <= 0)
            {
                confirmPopup.Hide();
            }
            if (EventSystem.get_current().get_currentSelectedGameObject() == null)
            {
                controlsList.GetComponentsInChildren<ToggleEffects>(includeInactive: true)[0].SetOn();
            }
        }
    }

    private void CreateControlMappingEntry(string actionName, int actionId, bool isPositiveInput, string inputCategory, ScrollGroup controlsList, bool allowGamepadMapping = true)
    {
        //IL_00f9: Unknown result type (might be due to invalid IL or missing references)
        //IL_00ff: Invalid comparison between Unknown and I4
        //IL_017b: Unknown result type (might be due to invalid IL or missing references)
        //IL_018a: Unknown result type (might be due to invalid IL or missing references)
        //IL_0190: Invalid comparison between Unknown and I4
        //IL_019d: Unknown result type (might be due to invalid IL or missing references)
        //IL_0248: Unknown result type (might be due to invalid IL or missing references)
        //IL_0257: Unknown result type (might be due to invalid IL or missing references)
        //IL_0266: Unknown result type (might be due to invalid IL or missing references)
        //IL_026c: Invalid comparison between Unknown and I4
        UIControlMappingItem uIControlMappingItem = controlsList.AddToList(null, null).GetComponentsInChildren<UIControlMappingItem>(includeInactive: true)[0];
        Toggle[] componentsInChildren = uIControlMappingItem.GetComponentsInChildren<Toggle>(includeInactive: true);
        for (int i = 0; i < componentsInChildren.Length; i++)
        {
            componentsInChildren[i].set_group(mappingPanel);
        }
        UIGroupEffects[] componentsInChildren2 = uIControlMappingItem.GetComponentsInChildren<UIGroupEffects>(includeInactive: true);
        for (int j = 0; j < componentsInChildren2.Length; j++)
        {
            componentsInChildren2[j].highlight = controlsList.GetComponentsInChildren<HightlightAnimate>(includeInactive: true)[0];
        }
        uIControlMappingItem.actionLabel.set_text(actionName);
        uIControlMappingItem.actionId = actionId;
        uIControlMappingItem.isPositiveInput = isPositiveInput;
        uIControlMappingItem.inputCategory = inputCategory;
        uIControlMappingItem.OnMappingButton = OnButtonRemap;
        if (PandoraSingleton<PandoraInput>.Instance.player.controllers.get_Keyboard() != null)
        {
            ActionElementMap[] buttonMapsWithAction = PandoraSingleton<PandoraInput>.Instance.player.controllers.maps.GetFirstMapInCategory((ControllerType)0, 0, uIControlMappingItem.inputCategory).GetButtonMapsWithAction(actionId);
            if (buttonMapsWithAction != null)
            {
                for (int k = 0; k < buttonMapsWithAction.Length; k++)
                {
                    if ((int)buttonMapsWithAction[k].get_axisContribution() == 0 == isPositiveInput)
                    {
                        uIControlMappingItem.SetMapping(buttonMapsWithAction[k], 0);
                    }
                }
            }
        }
        if (PandoraSingleton<PandoraInput>.Instance.player.controllers.get_Mouse() != null)
        {
            ActionElementMap[] elementMapsWithAction = PandoraSingleton<PandoraInput>.Instance.player.controllers.maps.GetFirstMapInCategory((ControllerType)1, 0, uIControlMappingItem.inputCategory).GetElementMapsWithAction(actionId);
            if (elementMapsWithAction != null)
            {
                for (int l = 0; l < elementMapsWithAction.Length; l++)
                {
                    if ((int)elementMapsWithAction[l].get_axisType() != 0 || (int)elementMapsWithAction[l].get_axisContribution() == 0 == isPositiveInput)
                    {
                        if ((int)elementMapsWithAction[l].get_axisType() != 0)
                        {
                            uIControlMappingItem.SetMapping(elementMapsWithAction[l], 0, remappable: false);
                        }
                        else
                        {
                            uIControlMappingItem.SetMapping(elementMapsWithAction[l], 0);
                        }
                    }
                }
            }
        }
        if (PandoraSingleton<PandoraInput>.Instance.player.controllers.get_Joysticks().Count <= 0)
        {
            return;
        }
        if (!allowGamepadMapping)
        {
            uIControlMappingItem.SetMapping(null, 1, remappable: false);
            return;
        }
        ActionElementMap[] elementMapsWithAction2 = PandoraSingleton<PandoraInput>.Instance.player.controllers.maps.GetFirstMapInCategory((ControllerType)2, 0, uIControlMappingItem.inputCategory).GetElementMapsWithAction(actionId);
        if (elementMapsWithAction2 == null)
        {
            return;
        }
        int num = 0;
        while (true)
        {
            if (num < elementMapsWithAction2.Length)
            {
                if (((int)elementMapsWithAction2[num].get_axisRange() == 0 && (int)elementMapsWithAction2[num].get_axisType() != 0) || (int)elementMapsWithAction2[num].get_axisContribution() == 0 == isPositiveInput)
                {
                    break;
                }
                num++;
                continue;
            }
            return;
        }
        string text = elementMapsWithAction2[num].get_elementIdentifierName().ToLowerInvariant().Replace(" ", "_");
        if (text.Equals("left_stick_x") || text.Equals("left_stick_y") || text.Equals("right_stick_x") || text.Equals("right_stick_y"))
        {
            uIControlMappingItem.SetMapping(elementMapsWithAction2[num], 1, remappable: false);
        }
        else
        {
            uIControlMappingItem.SetMapping(elementMapsWithAction2[num], 1);
        }
    }

    public void OnButtonRemap(UIControlMappingItem mappingEntry, int buttonIndex, ActionElementMap actionMap)
    {
        remappedEntry = mappingEntry;
        remappedButtonIndex = buttonIndex;
        remappedAction = actionMap;
        if (buttonIndex == 0 || PandoraSingleton<PandoraInput>.Instance.player.controllers.get_Joysticks().Count > 0)
        {
            controlsRemapPopup.Show(OnMapKeyPressed, (ControllerType)((buttonIndex != 0) ? 2 : 0), mappingEntry.actionLabel.get_text());
            return;
        }
        string stringById = PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_no_controller_desc");
        confirmPopup.ShowLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_no_controller_title"), stringById, null);
        confirmPopup.HideCancelButton();
    }

    public void OnMapKeyPressed(Pole pole, ControllerPollingInfo pollInfo)
    {
        //IL_0004: Unknown result type (might be due to invalid IL or missing references)
        //IL_0009: Unknown result type (might be due to invalid IL or missing references)
        //IL_000b: Unknown result type (might be due to invalid IL or missing references)
        //IL_001e: Expected I4, but got Unknown
        //IL_001e: Unknown result type (might be due to invalid IL or missing references)
        //IL_0022: Invalid comparison between Unknown and I4
        //IL_005c: Unknown result type (might be due to invalid IL or missing references)
        //IL_005d: Unknown result type (might be due to invalid IL or missing references)
        //IL_009c: Unknown result type (might be due to invalid IL or missing references)
        //IL_00d8: Unknown result type (might be due to invalid IL or missing references)
        //IL_00de: Invalid comparison between Unknown and I4
        //IL_0107: Unknown result type (might be due to invalid IL or missing references)
        //IL_010d: Invalid comparison between Unknown and I4
        //IL_0124: Unknown result type (might be due to invalid IL or missing references)
        //IL_0142: Unknown result type (might be due to invalid IL or missing references)
        //IL_0143: Unknown result type (might be due to invalid IL or missing references)
        //IL_01a0: Unknown result type (might be due to invalid IL or missing references)
        //IL_01a5: Unknown result type (might be due to invalid IL or missing references)
        //IL_01a7: Unknown result type (might be due to invalid IL or missing references)
        //IL_01ba: Expected I4, but got Unknown
        //IL_01c1: Unknown result type (might be due to invalid IL or missing references)
        int keyIdentifier = -1;
        ControllerType controllerType = ((ControllerPollingInfo)(ref pollInfo)).get_controllerType();
        switch ((int)controllerType)
        {
            default:
                if ((int)controllerType != 20)
                {
                    PandoraDebug.LogWarning("Unknown controller type", "INPUT_MAPPING", this);
                    break;
                }
                goto case 1;
            case 1:
            case 2:
                keyIdentifier = ((ControllerPollingInfo)(ref pollInfo)).get_elementIdentifierId();
                break;
            case 0:
                keyIdentifier = (int)((ControllerPollingInfo)(ref pollInfo)).get_keyboardKey();
                break;
        }
        newMapInput = pollInfo;
        remappedEntry.mappingButtons[remappedButtonIndex].SetSelected(force: true);
        conflictingAction = PandoraSingleton<PandoraInput>.Instance.GetFirstConflictingActionMap(remappedEntry.actionId, remappedEntry.inputCategory, ((ControllerPollingInfo)(ref newMapInput)).get_controllerType(), keyIdentifier);
        if (conflictingAction != null && (conflictingAction.get_actionId() != remappedEntry.actionId || (int)conflictingAction.get_axisContribution() == 0 != remappedEntry.isPositiveInput))
        {
            InputAction action = ReInput.get_mapping().GetAction(conflictingAction.get_actionId());
            string key = ((int)action.get_type() == 1) ? action.get_descriptiveName() : (((int)conflictingAction.get_axisContribution() != 0) ? action.get_negativeDescriptiveName() : action.get_positiveDescriptiveName());
            newMapKeyPole = pole;
            string stringById = PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_already_mapped", PandoraSingleton<LocalizationManager>.Instance.GetStringById(key));
            confirmPopup.ShowLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_already_mapped_title"), stringById, OnSwapMappingPopupResult);
            return;
        }
        controllerType = ((ControllerPollingInfo)(ref newMapInput)).get_controllerType();
        switch ((int)controllerType)
        {
            case 2:
                MapNewJoystickKey(newMapInput);
                break;
            case 0:
                MapNewKeyboardKey(((ControllerPollingInfo)(ref newMapInput)).get_keyboardKey());
                break;
            case 1:
                MapNewMouseButton(((ControllerPollingInfo)(ref newMapInput)).get_elementIdentifierId());
                break;
        }
    }

    public void OnSwapMappingPopupResult(bool confirm)
    {
        //IL_0038: Unknown result type (might be due to invalid IL or missing references)
        //IL_0077: Unknown result type (might be due to invalid IL or missing references)
        //IL_007e: Unknown result type (might be due to invalid IL or missing references)
        //IL_007f: Unknown result type (might be due to invalid IL or missing references)
        //IL_008b: Unknown result type (might be due to invalid IL or missing references)
        //IL_00b4: Unknown result type (might be due to invalid IL or missing references)
        //IL_00e3: Unknown result type (might be due to invalid IL or missing references)
        //IL_00f9: Unknown result type (might be due to invalid IL or missing references)
        //IL_011f: Unknown result type (might be due to invalid IL or missing references)
        //IL_0165: Unknown result type (might be due to invalid IL or missing references)
        //IL_0183: Unknown result type (might be due to invalid IL or missing references)
        //IL_0195: Unknown result type (might be due to invalid IL or missing references)
        //IL_01a7: Unknown result type (might be due to invalid IL or missing references)
        //IL_01ad: Invalid comparison between Unknown and I4
        //IL_01d0: Unknown result type (might be due to invalid IL or missing references)
        //IL_01ff: Unknown result type (might be due to invalid IL or missing references)
        //IL_0215: Unknown result type (might be due to invalid IL or missing references)
        //IL_023b: Unknown result type (might be due to invalid IL or missing references)
        //IL_0280: Unknown result type (might be due to invalid IL or missing references)
        //IL_029e: Unknown result type (might be due to invalid IL or missing references)
        //IL_02b0: Unknown result type (might be due to invalid IL or missing references)
        //IL_02cb: Unknown result type (might be due to invalid IL or missing references)
        //IL_0370: Unknown result type (might be due to invalid IL or missing references)
        //IL_0376: Invalid comparison between Unknown and I4
        //IL_03b4: Unknown result type (might be due to invalid IL or missing references)
        //IL_0440: Unknown result type (might be due to invalid IL or missing references)
        //IL_0456: Unknown result type (might be due to invalid IL or missing references)
        //IL_046c: Unknown result type (might be due to invalid IL or missing references)
        //IL_0482: Unknown result type (might be due to invalid IL or missing references)
        //IL_04cc: Unknown result type (might be due to invalid IL or missing references)
        //IL_056e: Unknown result type (might be due to invalid IL or missing references)
        //IL_0574: Invalid comparison between Unknown and I4
        //IL_05af: Unknown result type (might be due to invalid IL or missing references)
        if (confirm)
        {
            if (remappedButtonIndex == 0)
            {
                ActionElementMap mapping = null;
                if (remappedAction == null)
                {
                    PandoraSingleton<PandoraInput>.Instance.player.controllers.maps.GetFirstMapInCategory(((ControllerPollingInfo)(ref newMapInput)).get_controllerType(), 0, remappedEntry.inputCategory).DeleteElementMap(conflictingAction.get_id());
                    mapping = null;
                }
                else
                {
                    ControllerType val = (remappedAction.get_keyCode() != 0) ? ((ControllerType)0) : ((ControllerType)1);
                    ElementAssignment val2 = default(ElementAssignment);
                    if ((int)val == 0)
                    {
                        if ((int)((ControllerPollingInfo)(ref newMapInput)).get_controllerType() == 0)
                        {
                            ((ElementAssignment)(ref val2))._002Ector(remappedAction.get_keyCode(), (ModifierKeyFlags)0, conflictingAction.get_actionId(), conflictingAction.get_axisContribution(), conflictingAction.get_id());
                            PandoraSingleton<PandoraInput>.Instance.player.controllers.maps.GetFirstMapInCategory(((ControllerPollingInfo)(ref newMapInput)).get_controllerType(), 0, remappedEntry.inputCategory).ReplaceElementMap(val2);
                        }
                        else
                        {
                            PandoraSingleton<PandoraInput>.Instance.player.controllers.maps.GetFirstMapInCategory(((ControllerPollingInfo)(ref newMapInput)).get_controllerType(), 0, remappedEntry.inputCategory).DeleteElementMap(conflictingAction.get_id());
                            ((ElementAssignment)(ref val2))._002Ector(remappedAction.get_keyCode(), (ModifierKeyFlags)0, conflictingAction.get_actionId(), conflictingAction.get_axisContribution());
                            PandoraSingleton<PandoraInput>.Instance.player.controllers.maps.GetFirstMapInCategory(val, 0, remappedEntry.inputCategory).CreateElementMap(val2);
                        }
                    }
                    else if ((int)((ControllerPollingInfo)(ref newMapInput)).get_controllerType() == 1)
                    {
                        ((ElementAssignment)(ref val2))._002Ector(remappedAction.get_elementIdentifierId(), conflictingAction.get_actionId(), conflictingAction.get_axisContribution(), conflictingAction.get_id());
                        PandoraSingleton<PandoraInput>.Instance.player.controllers.maps.GetFirstMapInCategory(((ControllerPollingInfo)(ref newMapInput)).get_controllerType(), 0, remappedEntry.inputCategory).ReplaceElementMap(val2);
                    }
                    else
                    {
                        PandoraSingleton<PandoraInput>.Instance.player.controllers.maps.GetFirstMapInCategory(((ControllerPollingInfo)(ref newMapInput)).get_controllerType(), 0, remappedEntry.inputCategory).DeleteElementMap(conflictingAction.get_id());
                        ((ElementAssignment)(ref val2))._002Ector(remappedAction.get_elementIdentifierId(), conflictingAction.get_actionId(), conflictingAction.get_axisContribution());
                        PandoraSingleton<PandoraInput>.Instance.player.controllers.maps.GetFirstMapInCategory(val, 0, remappedEntry.inputCategory).CreateElementMap(val2);
                    }
                    ActionElementMap[] elementMapsWithAction = PandoraSingleton<PandoraInput>.Instance.player.controllers.maps.GetFirstMapInCategory(val, 0, remappedEntry.inputCategory).GetElementMapsWithAction(conflictingAction.get_actionId());
                    foreach (ActionElementMap val3 in elementMapsWithAction)
                    {
                        if (val3.get_keyCode() == remappedAction.get_keyCode())
                        {
                            mapping = val3;
                            break;
                        }
                    }
                }
                for (int j = 0; j < controlsList.items.Count; j++)
                {
                    UIControlMappingItem component = controlsList.items[j].GetComponent<UIControlMappingItem>();
                    if (component.actionId == conflictingAction.get_actionId() && component.isPositiveInput == ((int)conflictingAction.get_axisContribution() == 0))
                    {
                        component.SetMapping(mapping, remappedButtonIndex);
                        break;
                    }
                }
                if ((int)((ControllerPollingInfo)(ref newMapInput)).get_controllerType() == 0)
                {
                    MapNewKeyboardKey(((ControllerPollingInfo)(ref newMapInput)).get_keyboardKey());
                }
                else
                {
                    MapNewMouseButton(((ControllerPollingInfo)(ref newMapInput)).get_elementIdentifierId());
                }
            }
            else
            {
                ActionElementMap mapping2 = null;
                if (remappedAction == null)
                {
                    PandoraSingleton<PandoraInput>.Instance.player.controllers.maps.GetFirstMapInCategory((ControllerType)2, 0, remappedEntry.inputCategory).DeleteElementMap(conflictingAction.get_id());
                    mapping2 = null;
                }
                else
                {
                    ElementAssignment val4 = default(ElementAssignment);
                    ((ElementAssignment)(ref val4))._002Ector((ControllerType)2, remappedAction.get_elementType(), remappedAction.get_elementIdentifierId(), remappedAction.get_axisRange(), remappedAction.get_keyCode(), remappedAction.get_modifierKeyFlags(), conflictingAction.get_actionId(), conflictingAction.get_axisContribution(), conflictingAction.get_invert(), conflictingAction.get_id());
                    ControllerMap firstMapInCategory = PandoraSingleton<PandoraInput>.Instance.player.controllers.maps.GetFirstMapInCategory((ControllerType)2, 0, remappedEntry.inputCategory);
                    bool flag = firstMapInCategory.ReplaceElementMap(val4);
                    ActionElementMap[] elementMapsWithAction2 = firstMapInCategory.GetElementMapsWithAction(conflictingAction.get_actionId());
                    foreach (ActionElementMap val5 in elementMapsWithAction2)
                    {
                        if (val5.get_elementIdentifierId() == remappedAction.get_elementIdentifierId())
                        {
                            mapping2 = val5;
                            break;
                        }
                    }
                }
                for (int l = 0; l < controlsList.items.Count; l++)
                {
                    UIControlMappingItem component2 = controlsList.items[l].GetComponent<UIControlMappingItem>();
                    if (component2.actionId == conflictingAction.get_actionId() && component2.isPositiveInput == ((int)conflictingAction.get_axisContribution() == 0))
                    {
                        component2.SetMapping(mapping2, remappedButtonIndex);
                        break;
                    }
                }
                MapNewJoystickKey(newMapInput);
            }
        }
        remappedEntry.mappingButtons[remappedButtonIndex].SetSelected(force: true);
    }

    private void MapNewJoystickKey(ControllerPollingInfo pollInfo)
    {
        //IL_000f: Unknown result type (might be due to invalid IL or missing references)
        //IL_0016: Unknown result type (might be due to invalid IL or missing references)
        //IL_0024: Unknown result type (might be due to invalid IL or missing references)
        //IL_002a: Invalid comparison between Unknown and I4
        //IL_0037: Unknown result type (might be due to invalid IL or missing references)
        //IL_003d: Invalid comparison between Unknown and I4
        //IL_00b4: Unknown result type (might be due to invalid IL or missing references)
        //IL_00c4: Unknown result type (might be due to invalid IL or missing references)
        //IL_00cb: Unknown result type (might be due to invalid IL or missing references)
        //IL_00d9: Unknown result type (might be due to invalid IL or missing references)
        //IL_00df: Invalid comparison between Unknown and I4
        //IL_00ec: Unknown result type (might be due to invalid IL or missing references)
        //IL_00f2: Invalid comparison between Unknown and I4
        //IL_0154: Unknown result type (might be due to invalid IL or missing references)
        if (remappedAction != null)
        {
            ElementAssignment val = default(ElementAssignment);
            ((ElementAssignment)(ref val))._002Ector(((ControllerPollingInfo)(ref pollInfo)).get_controllerType(), ((ControllerPollingInfo)(ref pollInfo)).get_elementType(), ((ControllerPollingInfo)(ref pollInfo)).get_elementIdentifierId(), (AxisRange)(((int)((ControllerPollingInfo)(ref pollInfo)).get_elementType() != 1) ? (((int)((ControllerPollingInfo)(ref pollInfo)).get_axisPole() != 1) ? 1 : 2) : 0), ((ControllerPollingInfo)(ref pollInfo)).get_keyboardKey(), (ModifierKeyFlags)0, remappedEntry.actionId, (Pole)((!remappedEntry.isPositiveInput) ? 1 : 0), remappedAction.get_invert(), remappedAction.get_id());
            PandoraSingleton<PandoraInput>.Instance.player.controllers.maps.GetFirstMapInCategory((ControllerType)2, 0, remappedEntry.inputCategory).ReplaceElementMap(val);
        }
        else
        {
            ElementAssignment val2 = default(ElementAssignment);
            ((ElementAssignment)(ref val2))._002Ector(((ControllerPollingInfo)(ref pollInfo)).get_controllerType(), ((ControllerPollingInfo)(ref pollInfo)).get_elementType(), ((ControllerPollingInfo)(ref pollInfo)).get_elementIdentifierId(), (AxisRange)(((int)((ControllerPollingInfo)(ref pollInfo)).get_elementType() != 1) ? (((int)((ControllerPollingInfo)(ref pollInfo)).get_axisPole() != 1) ? 1 : 2) : 0), ((ControllerPollingInfo)(ref pollInfo)).get_keyboardKey(), (ModifierKeyFlags)0, remappedEntry.actionId, (Pole)((!remappedEntry.isPositiveInput) ? 1 : 0), false);
            PandoraSingleton<PandoraInput>.Instance.player.controllers.maps.GetFirstMapInCategory((ControllerType)2, 0, remappedEntry.inputCategory).CreateElementMap(val2);
        }
        ActionElementMap mapping = null;
        ActionElementMap[] elementMapsWithAction = PandoraSingleton<PandoraInput>.Instance.player.controllers.maps.GetFirstMapInCategory((ControllerType)2, 0, remappedEntry.inputCategory).GetElementMapsWithAction(remappedEntry.actionId);
        foreach (ActionElementMap val3 in elementMapsWithAction)
        {
            if (val3.get_elementIdentifierId() == ((ControllerPollingInfo)(ref pollInfo)).get_elementIdentifierId())
            {
                mapping = val3;
                break;
            }
        }
        remappedEntry.SetMapping(mapping, remappedButtonIndex);
        mappingChanged = true;
    }

    private void MapNewMouseButton(int buttonId)
    {
        //IL_0076: Unknown result type (might be due to invalid IL or missing references)
        //IL_0109: Unknown result type (might be due to invalid IL or missing references)
        //IL_0165: Unknown result type (might be due to invalid IL or missing references)
        if (remappedAction != null)
        {
            if (remappedAction.get_keyCode() == KeyCode.None)
            {
                ElementAssignment val = default(ElementAssignment);
                ((ElementAssignment)(ref val))._002Ector(buttonId, remappedEntry.actionId, (Pole)((!remappedEntry.isPositiveInput) ? 1 : 0), remappedAction.get_id());
                PandoraSingleton<PandoraInput>.Instance.player.controllers.maps.GetFirstMapInCategory((ControllerType)1, 0, remappedEntry.inputCategory).ReplaceOrCreateElementMap(val);
            }
            else
            {
                PandoraSingleton<PandoraInput>.Instance.player.controllers.maps.GetFirstMapInCategory((ControllerType)0, 0, remappedEntry.inputCategory).DeleteElementMap(remappedAction.get_id());
                ElementAssignment val2 = default(ElementAssignment);
                ((ElementAssignment)(ref val2))._002Ector(buttonId, remappedEntry.actionId, (Pole)((!remappedEntry.isPositiveInput) ? 1 : 0));
                PandoraSingleton<PandoraInput>.Instance.player.controllers.maps.GetFirstMapInCategory((ControllerType)1, 0, remappedEntry.inputCategory).CreateElementMap(val2);
            }
        }
        else
        {
            ElementAssignment val3 = default(ElementAssignment);
            ((ElementAssignment)(ref val3))._002Ector(buttonId, remappedEntry.actionId, (Pole)((!remappedEntry.isPositiveInput) ? 1 : 0));
            PandoraSingleton<PandoraInput>.Instance.player.controllers.maps.GetFirstMapInCategory((ControllerType)1, 0, remappedEntry.inputCategory).CreateElementMap(val3);
        }
        ActionElementMap mapping = null;
        ActionElementMap[] elementMapsWithAction = PandoraSingleton<PandoraInput>.Instance.player.controllers.maps.GetFirstMapInCategory((ControllerType)1, 0, remappedEntry.inputCategory).GetElementMapsWithAction(remappedEntry.actionId);
        foreach (ActionElementMap val4 in elementMapsWithAction)
        {
            if (val4.get_elementIdentifierId() == buttonId)
            {
                mapping = val4;
                break;
            }
        }
        remappedEntry.SetMapping(mapping, remappedButtonIndex);
        mappingChanged = true;
    }

    private void MapNewKeyboardKey(KeyCode key)
    {
        //IL_0077: Unknown result type (might be due to invalid IL or missing references)
        if (remappedAction != null)
        {
            if (remappedAction.get_keyCode() != 0)
            {
                ElementAssignment val = default(ElementAssignment);
                ((ElementAssignment)(ref val))._002Ector(key, (ModifierKeyFlags)0, remappedEntry.actionId, (Pole)((!remappedEntry.isPositiveInput) ? 1 : 0), remappedAction.get_id());
                PandoraSingleton<PandoraInput>.Instance.player.controllers.maps.GetFirstMapInCategory((ControllerType)0, 0, remappedEntry.inputCategory).ReplaceOrCreateElementMap(val);
            }
            else
            {
                PandoraSingleton<PandoraInput>.Instance.player.controllers.maps.GetFirstMapInCategory((ControllerType)1, 0, remappedEntry.inputCategory).DeleteElementMap(remappedAction.get_id());
                PandoraSingleton<PandoraInput>.Instance.MapKeyboardKey(remappedEntry.inputCategory, key, remappedEntry.actionId, remappedEntry.isPositiveInput);
            }
        }
        else
        {
            PandoraSingleton<PandoraInput>.Instance.MapKeyboardKey(remappedEntry.inputCategory, key, remappedEntry.actionId, remappedEntry.isPositiveInput);
        }
        ActionElementMap mapping = null;
        ActionElementMap[] elementMapsWithAction = PandoraSingleton<PandoraInput>.Instance.player.controllers.maps.GetFirstMapInCategory((ControllerType)0, 0, remappedEntry.inputCategory).GetElementMapsWithAction(remappedEntry.actionId);
        foreach (ActionElementMap val2 in elementMapsWithAction)
        {
            if (val2.get_keyCode() == key)
            {
                mapping = val2;
                break;
            }
        }
        remappedEntry.SetMapping(mapping, remappedButtonIndex);
        mappingChanged = true;
    }

    public void ResetMappings()
    {
        lastSelection = EventSystem.get_current().get_currentSelectedGameObject();
        confirmPopup.Show("menu_restore_default_mappings", "menu_confirm_restore_mappings", OnConfirmResetMappings);
    }

    public void OnConfirmResetMappings(bool confirmed)
    {
        if (confirmed)
        {
            PandoraSingleton<PandoraInput>.Instance.RestoreDefaultMappings();
            FillControlsPanel();
            mappings.SetSelected(force: true);
            panelOpen = false;
            mappingChanged = true;
        }
        else if (lastSelection != null)
        {
            lastSelection.SetSelected(force: true);
        }
    }

    private void UntoggleSelected()
    {
        if (EventSystem.get_current().get_currentSelectedGameObject() != null)
        {
            Toggle component = EventSystem.get_current().get_currentSelectedGameObject().GetComponent<Toggle>();
            if ((UnityEngine.Object)(object)component != null)
            {
                component.set_isOn(false);
            }
        }
    }

    private void SaveJoystickMappingData()
    {
        JoystickMapSaveData[] mapSaveData = PandoraSingleton<PandoraInput>.Instance.player.controllers.maps.GetMapSaveData<JoystickMapSaveData>(0, true);
        PandoraSingleton<GameManager>.Instance.Options.joystickMappingData = new List<string>();
        if (mapSaveData != null)
        {
            for (int i = 0; i < mapSaveData.Length; i++)
            {
                PandoraSingleton<GameManager>.Instance.Options.joystickMappingData.Add(((ControllerMapSaveData)mapSaveData[i]).get_map().ToXmlString());
            }
        }
    }

    private void SaveKeyboardMappingData()
    {
        KeyboardMapSaveData[] mapSaveData = PandoraSingleton<PandoraInput>.Instance.player.controllers.maps.GetMapSaveData<KeyboardMapSaveData>(0, true);
        PandoraSingleton<GameManager>.Instance.Options.keyboardMappingData = new List<string>();
        if (mapSaveData != null)
        {
            for (int i = 0; i < mapSaveData.Length; i++)
            {
                PandoraSingleton<GameManager>.Instance.Options.keyboardMappingData.Add(((ControllerMapSaveData)mapSaveData[i]).get_map().ToXmlString());
            }
        }
    }

    private void SaveMouseMappingData()
    {
        MouseMapSaveData[] mapSaveData = PandoraSingleton<PandoraInput>.Instance.player.controllers.maps.GetMapSaveData<MouseMapSaveData>(0, true);
        PandoraSingleton<GameManager>.Instance.Options.mouseMappingData = new List<string>();
        if (mapSaveData != null)
        {
            for (int i = 0; i < mapSaveData.Length; i++)
            {
                PandoraSingleton<GameManager>.Instance.Options.mouseMappingData.Add(((ControllerMapSaveData)mapSaveData[i]).get_map().ToXmlString());
            }
        }
    }

    private bool ApplyMappingChanges()
    {
        SaveJoystickMappingData();
        SaveKeyboardMappingData();
        SaveMouseMappingData();
        mappingChanged = false;
        return true;
    }

    private void CancelMappingChanges()
    {
        if (mappingChanged)
        {
            PandoraSingleton<GameManager>.Instance.SetMappingOptions();
            mappingChanged = false;
        }
    }

    private bool CheckMappingNeedApply()
    {
        if (mappingChanged)
        {
            PandoraDebug.LogDebug("Options require Apply action, mappings changed ", "OPTIONS", this);
        }
        return mappingChanged;
    }

    private void InitControls()
    {
        controlsOptions.onAction.AddListener(OnControlsPanelOpen);
        controlsOptions.onSelect.AddListener(OnControlsPanelDisplayed);
        InitControlsData();
    }

    private void InitControlsData()
    {
        invertCameraHorizontalEnabled.set_isOn(PandoraSingleton<GameManager>.Instance.Options.cameraXInverted);
        invertCameraVerticalEnabled.set_isOn(PandoraSingleton<GameManager>.Instance.Options.cameraYInverted);
        joystickSensitivitySlider.SetValue(PandoraSingleton<GameManager>.Instance.Options.joystickSensitivity * 100f);
        leftHandedControllerEnabled.set_isOn(PandoraSingleton<GameManager>.Instance.Options.leftHandedController);
        leftHandedMouseEnabled.set_isOn(PandoraSingleton<GameManager>.Instance.Options.leftHandedMouse);
        mouseSensitivitySlider.SetValue(PandoraSingleton<GameManager>.Instance.Options.mouseSensitivity * 100f);
        gamepadEnabled.set_isOn(PandoraSingleton<GameManager>.Instance.Options.gamepadEnabled);
    }

    private void OnControlsPanelDisplayed()
    {
        SetMappingsPanelButtonsVisible(visible: false);
    }

    private void OnControlsPanelOpen()
    {
        OnPanelOpen();
        ((MonoBehaviour)(object)gamepadEnabled).SetSelected(force: true);
        OnControlsPanelDisplayed();
    }

    private bool CheckControlsNeedApply()
    {
        if (PandoraSingleton<GameManager>.Instance.Options.gamepadEnabled != gamepadEnabled.get_isOn())
        {
            PandoraDebug.LogDebug("Options require Apply action, gamepadEnabled changed from " + PandoraSingleton<GameManager>.Instance.Options.gamepadEnabled + " to " + gamepadEnabled.get_isOn(), "OPTIONS", this);
            return true;
        }
        if (PandoraSingleton<GameManager>.Instance.Options.cameraXInverted != invertCameraHorizontalEnabled.get_isOn())
        {
            PandoraDebug.LogDebug("Options require Apply action, cameraXInvert changed from " + PandoraSingleton<GameManager>.Instance.Options.cameraXInverted + " to " + invertCameraHorizontalEnabled.get_isOn(), "OPTIONS", this);
            return true;
        }
        if (PandoraSingleton<GameManager>.Instance.Options.cameraYInverted != invertCameraVerticalEnabled.get_isOn())
        {
            PandoraDebug.LogDebug("Options require Apply action, cameraYInvert changed from " + PandoraSingleton<GameManager>.Instance.Options.cameraYInverted + " to " + invertCameraVerticalEnabled.get_isOn(), "OPTIONS", this);
            return true;
        }
        if (PandoraSingleton<GameManager>.Instance.Options.leftHandedMouse != leftHandedMouseEnabled.get_isOn())
        {
            PandoraDebug.LogDebug("Options require Apply action, left handed mouse changed from " + PandoraSingleton<GameManager>.Instance.Options.leftHandedMouse + " to " + leftHandedMouseEnabled.get_isOn(), "OPTIONS", this);
            return true;
        }
        if (!Mathf.Approximately(PandoraSingleton<GameManager>.Instance.Options.mouseSensitivity, (float)mouseSensitivitySlider.GetValue() / 100f))
        {
            PandoraDebug.LogDebug("Options require Apply action, mouseSensitivity changed from " + PandoraSingleton<GameManager>.Instance.Options.mouseSensitivity + " to " + (float)mouseSensitivitySlider.GetValue() / 100f, "OPTIONS", this);
            return true;
        }
        if (PandoraSingleton<GameManager>.Instance.Options.leftHandedController != leftHandedControllerEnabled.get_isOn())
        {
            PandoraDebug.LogDebug("Options require Apply action, left handed controller changed from " + PandoraSingleton<GameManager>.Instance.Options.leftHandedController + " to " + leftHandedControllerEnabled.get_isOn(), "OPTIONS", this);
            return true;
        }
        if (!Mathf.Approximately(PandoraSingleton<GameManager>.Instance.Options.joystickSensitivity, (float)joystickSensitivitySlider.GetValue() / 100f))
        {
            PandoraDebug.LogDebug("Options require Apply action, joystickSensitibity changed from " + PandoraSingleton<GameManager>.Instance.Options.joystickSensitivity + " to " + (float)joystickSensitivitySlider.GetValue() / 100f, "OPTIONS", this);
            return true;
        }
        return false;
    }

    private bool ApplyControlsChanges()
    {
        bool flag = false;
        bool flag2 = false;
        bool flag3 = false;
        if (PandoraSingleton<GameManager>.Instance.Options.gamepadEnabled != gamepadEnabled.get_isOn())
        {
            flag = true;
            PandoraSingleton<GameManager>.Instance.Options.gamepadEnabled = gamepadEnabled.get_isOn();
        }
        if (PandoraSingleton<GameManager>.Instance.Options.cameraXInverted != invertCameraHorizontalEnabled.get_isOn())
        {
            flag = true;
            PandoraSingleton<GameManager>.Instance.Options.cameraXInverted = invertCameraHorizontalEnabled.get_isOn();
            flag2 = true;
            flag3 = true;
        }
        if (PandoraSingleton<GameManager>.Instance.Options.cameraYInverted != invertCameraVerticalEnabled.get_isOn())
        {
            flag = true;
            PandoraSingleton<GameManager>.Instance.Options.cameraYInverted = invertCameraVerticalEnabled.get_isOn();
            flag2 = true;
            flag3 = true;
        }
        if (PandoraSingleton<GameManager>.Instance.Options.leftHandedMouse != leftHandedMouseEnabled.get_isOn())
        {
            flag = true;
            PandoraSingleton<GameManager>.Instance.Options.leftHandedMouse = leftHandedMouseEnabled.get_isOn();
            flag2 = true;
        }
        if (!Mathf.Approximately(PandoraSingleton<GameManager>.Instance.Options.mouseSensitivity, (float)mouseSensitivitySlider.GetValue() / 100f))
        {
            flag = true;
            PandoraSingleton<GameManager>.Instance.Options.mouseSensitivity = (float)mouseSensitivitySlider.GetValue() / 100f;
        }
        if (PandoraSingleton<GameManager>.Instance.Options.leftHandedController != leftHandedControllerEnabled.get_isOn())
        {
            flag = true;
            PandoraSingleton<GameManager>.Instance.Options.leftHandedController = leftHandedControllerEnabled.get_isOn();
            flag3 = true;
        }
        if (!Mathf.Approximately(PandoraSingleton<GameManager>.Instance.Options.joystickSensitivity, (float)joystickSensitivitySlider.GetValue() / 100f))
        {
            flag = true;
            PandoraSingleton<GameManager>.Instance.Options.joystickSensitivity = (float)joystickSensitivitySlider.GetValue() / 100f;
        }
        if (flag)
        {
            PandoraSingleton<GameManager>.Instance.SetControlsOptions();
            if (flag2)
            {
                SaveMouseMappingData();
            }
            if (flag3)
            {
                SaveJoystickMappingData();
            }
        }
        return true;
    }

    private void InitGameplay()
    {
        gameplayOptions.onAction.AddListener(OnGameplayPanelOpen);
        gameplayOptions.onSelect.AddListener(OnGameplayPanelDisplayed);
        InitGameplayData();
    }

    private void InitGameplayData()
    {
        languageSelect.selections.Clear();
        int currentSel = 0;
        for (int i = 0; i < availableLangs.Count; i++)
        {
            languageSelect.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById("lang_" + availableLangs[i].ToString()));
            if (availableLangs[i] == (SupportedLanguage)PandoraSingleton<GameManager>.Instance.Options.language)
            {
                currentSel = i;
            }
        }
        languageSelect.SetCurrentSel(currentSel);
        tacticalViewHelpersEnabled.set_isOn(PandoraSingleton<GameManager>.Instance.Options.tacticalViewHelpersEnabled);
        wagonBeaconsEnabled.set_isOn(PandoraSingleton<GameManager>.Instance.Options.wagonBeaconsEnabled);
        autoExitTacticalEnabled.set_isOn(PandoraSingleton<GameManager>.Instance.Options.autoExitTacticalEnabled);
        displayFullUI.set_isOn(PandoraSingleton<GameManager>.Instance.Options.displayFullUI);
        fastForward.set_isOn(PandoraSingleton<GameManager>.Instance.Options.fastForwarded);
        skipTuto.set_isOn(PandoraSingleton<GameManager>.Instance.Options.skipTuto);
    }

    private void OnGameplayPanelDisplayed()
    {
        SetMappingsPanelButtonsVisible(visible: false);
    }

    private void OnGameplayPanelOpen()
    {
        OnPanelOpen();
        ((MonoBehaviour)(object)languageSelect.selection).SetSelected(force: true);
        OnGameplayPanelDisplayed();
    }

    private bool CheckGameplayNeedApply()
    {
        if (PandoraSingleton<GameManager>.Instance.Options.language != languageSelect.CurSel)
        {
            PandoraDebug.LogDebug("Options require Apply action, language changed from " + PandoraSingleton<GameManager>.Instance.Options.language + " to " + gamepadEnabled.get_isOn(), "OPTIONS", this);
            return true;
        }
        if (PandoraSingleton<GameManager>.Instance.Options.tacticalViewHelpersEnabled != tacticalViewHelpersEnabled.get_isOn())
        {
            PandoraDebug.LogDebug("Options require Apply action, tacticalviewHelpers changed from " + PandoraSingleton<GameManager>.Instance.Options.tacticalViewHelpersEnabled + " to " + tacticalViewHelpersEnabled.get_isOn(), "OPTIONS", this);
            return true;
        }
        if (PandoraSingleton<GameManager>.Instance.Options.wagonBeaconsEnabled != wagonBeaconsEnabled.get_isOn())
        {
            PandoraDebug.LogDebug("Options require Apply action, wagonBeacons changed from " + PandoraSingleton<GameManager>.Instance.Options.wagonBeaconsEnabled + " to " + wagonBeaconsEnabled.get_isOn(), "OPTIONS", this);
            return true;
        }
        if (PandoraSingleton<GameManager>.Instance.Options.autoExitTacticalEnabled != autoExitTacticalEnabled.get_isOn())
        {
            PandoraDebug.LogDebug("Options require Apply action, autoExitTactical changed from " + PandoraSingleton<GameManager>.Instance.Options.autoExitTacticalEnabled + " to " + autoExitTacticalEnabled.get_isOn(), "OPTIONS", this);
            return true;
        }
        if (PandoraSingleton<GameManager>.Instance.Options.fastForwarded != fastForward.get_isOn())
        {
            PandoraDebug.LogDebug("Options require Apply action, fastForwarded changed from " + PandoraSingleton<GameManager>.Instance.Options.fastForwarded + " to " + fastForward.get_isOn(), "OPTIONS", this);
            return true;
        }
        if (PandoraSingleton<GameManager>.Instance.Options.skipTuto != skipTuto.get_isOn())
        {
            PandoraDebug.LogDebug("Options require Apply action, skipTuto changed from " + PandoraSingleton<GameManager>.Instance.Options.skipTuto + " to " + skipTuto.get_isOn(), "OPTIONS", this);
            return true;
        }
        if (PandoraSingleton<GameManager>.Instance.Options.displayFullUI != displayFullUI.get_isOn())
        {
            PandoraDebug.LogDebug("Options require Apply action, displayFullUI changed from " + PandoraSingleton<GameManager>.Instance.Options.displayFullUI + " to " + displayFullUI.get_isOn(), "OPTIONS", this);
            return true;
        }
        return false;
    }

    private bool ApplyGameplayChanges()
    {
        bool flag = false;
        bool result = true;
        if (PandoraSingleton<GameManager>.Instance.Options.language != languageSelect.CurSel)
        {
            flag = true;
            result = false;
            PandoraSingleton<GameManager>.Instance.Options.language = (int)availableLangs[languageSelect.CurSel];
        }
        if (PandoraSingleton<GameManager>.Instance.Options.tacticalViewHelpersEnabled != tacticalViewHelpersEnabled.get_isOn())
        {
            flag = true;
            PandoraSingleton<GameManager>.Instance.Options.tacticalViewHelpersEnabled = tacticalViewHelpersEnabled.get_isOn();
        }
        if (PandoraSingleton<GameManager>.Instance.Options.wagonBeaconsEnabled != wagonBeaconsEnabled.get_isOn())
        {
            flag = true;
            PandoraSingleton<GameManager>.Instance.Options.wagonBeaconsEnabled = wagonBeaconsEnabled.get_isOn();
        }
        if (PandoraSingleton<GameManager>.Instance.Options.autoExitTacticalEnabled != autoExitTacticalEnabled.get_isOn())
        {
            flag = true;
            PandoraSingleton<GameManager>.Instance.Options.autoExitTacticalEnabled = autoExitTacticalEnabled.get_isOn();
        }
        if (PandoraSingleton<GameManager>.Instance.Options.displayFullUI != displayFullUI.get_isOn())
        {
            flag = true;
            PandoraSingleton<GameManager>.Instance.Options.displayFullUI = displayFullUI.get_isOn();
            if (PandoraSingleton<UIMissionManager>.Exists())
            {
                PandoraSingleton<UIMissionManager>.Instance.OnSetOptionFullUI(resetUI: true);
            }
        }
        if (PandoraSingleton<GameManager>.Instance.Options.fastForwarded != fastForward.get_isOn())
        {
            flag = true;
            PandoraSingleton<GameManager>.Instance.Options.fastForwarded = fastForward.get_isOn();
        }
        if (PandoraSingleton<GameManager>.Instance.Options.skipTuto != skipTuto.get_isOn())
        {
            flag = true;
            PandoraSingleton<GameManager>.Instance.Options.skipTuto = skipTuto.get_isOn();
        }
        if (flag)
        {
            PandoraSingleton<GameManager>.Instance.SetGameplayOptions();
        }
        return result;
    }
}
