using mset;
using Rewired;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameManager : PandoraSingleton<GameManager>
{
    private enum graphicsQualitySettings
    {
        NO_SHADOWS,
        LOW_SHADOWS,
        MEDIUM_SHADOWS,
        HIGH_SHADOWS,
        VERY_HIGH_SHADOWS
    }

    public enum SystemPopupId
    {
        USER_DISCONNECTED,
        ONLINE_STATUS,
        LOST_CONNECTION,
        CONTROLLER_DISCONNECTED,
        DLC,
        INVITE,
        CONNECTION_VALIDATION
    }

    private class SystemPopupData
    {
        public SystemPopupId popupId;

        public string title;

        public string desc;

        public string descParam;

        public Action<bool> cb;

        public bool showCancel;
    }

    public const float FF_SPEED_AI = 1.5f;

    public const float FF_SPEED_GENERAL = 1.15f;

    private const string PROFILE_FILE = "profile.sg";

    private const string OPTIONS_NAME = "options.sg";

    private const int DELETE_OPTIONS_UNDER = 50;

    public static readonly string SAVE_FOLDER = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/my games/Mordheim/";

    private static readonly string OPTIONS_FILE = SAVE_FOLDER + "options.sg";

    private bool firstOptionsSetting = true;

    [HideInInspector]
    public int campaign = -1;

    public WarbandSave currentSave;

    [HideInInspector]
    public bool profileInitialized;

    public bool graphicOptionsSet;

    public GameObject popupPrefab;

    private ConfirmationPopupView popup;

    [NonSerialized]
    [HideInInspector]
    public bool skipLogos;

    [NonSerialized]
    [HideInInspector]
    public bool inCopyright;

    [NonSerialized]
    [HideInInspector]
    public bool inVideo;

    private List<SystemPopupData> systemPopups = new List<SystemPopupData>();

    public SaveManager Save
    {
        get;
        private set;
    }

    public OptionSave Options
    {
        get;
        private set;
    }

    public Profile Profile
    {
        get;
        private set;
    }

    public Tyche LocalTyche
    {
        get;
        private set;
    }

    public bool TacticalViewHelpersEnabled => Options.tacticalViewHelpersEnabled;

    public bool WagonBeaconsEnabled => Options.wagonBeaconsEnabled;

    public bool AutoExitTacticalEnabled => Options.autoExitTacticalEnabled;

    public ConfirmationPopupView Popup
    {
        get
        {
            if (popup == null)
            {
                GameObject gameObject = UnityEngine.Object.Instantiate(popupPrefab);
                Canvas component = gameObject.GetComponent<Canvas>();
                component.sortingOrder = 999998;
                popup = gameObject.GetComponent<ConfirmationPopupView>();
                UnityEngine.Object.DontDestroyOnLoad(gameObject);
            }
            return popup;
        }
    }

    public bool IsFastForwarded => Options.fastForwarded && !PandoraSingleton<Hermes>.Instance.IsConnected();

    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        profileInitialized = false;
        graphicOptionsSet = false;
        Save = new SaveManager();
        LocalTyche = new Tyche((int)(UnityEngine.Random.value * 2.14748365E+09f), log: false);
        campaign = -1;
        PandoraDebug.LogDebug("INIT", "GAME MANAGER", this);
        using (Process process = Process.GetCurrentProcess())
        {
            process.PriorityClass = ProcessPriorityClass.High;
        }
        string[] commandLineArgs = Environment.GetCommandLineArgs();
        Save.EraseOldSaveGame();
        EraseOldOptions(50);
        if (PandoraSingleton<Hephaestus>.Instance.FileExists("options.sg"))
        {
            PandoraSingleton<Hephaestus>.Instance.FileDelete("options.sg", OnOptionsDelete);
        }
        Options = new OptionSave();
        Shader.WarmupAllShaders();
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.CONTROLLER_CONNECTED, OnControllerConnected);
        InitClient();
    }

    public void InitClient(UnityAction profileLoadedCb = null)
    {
        profileInitialized = false;
        graphicOptionsSet = false;
        StartCoroutine(InitializeHephaestusClient(profileLoadedCb));
    }

    private IEnumerator InitializeHephaestusClient(UnityAction profileLoadedCb)
    {
        yield return StartCoroutine(PandoraSingleton<Hephaestus>.Instance.InitializeClient());
        while (!PandoraSingleton<Hephaestus>.Instance.IsInitialized())
        {
            yield return null;
        }
        ReadOptions();
        while (!graphicOptionsSet)
        {
            yield return null;
        }
        if (profileLoadedCb != null)
        {
            while (!profileInitialized)
            {
                yield return null;
            }
            profileLoadedCb();
        }
    }

    public void ReadOptions()
    {
        if (File.Exists(OPTIONS_FILE))
        {
            Thoth.ReadFromFile(OPTIONS_FILE, Options);
            ProcessOptions();
        }
        else
        {
            Options = new OptionSave();
            WriteOptions();
        }
    }

    public void WriteOptions()
    {
        Thoth.WriteToFile(OPTIONS_FILE, Options);
        ProcessOptions();
    }

    private void ProcessOptions()
    {
        SetGraphicOptions();
        SetVolumeOptions();
        SetControlsOptions();
        SetMappingOptions();
        SetGameplayOptions();
        ReadProfile();
    }

    public void EraseOldOptions(int minVersion)
    {
        if (File.Exists(OPTIONS_FILE))
        {
            bool flag = false;
            byte[] array = File.ReadAllBytes(OPTIONS_FILE);
            if (array.Length < 4)
            {
                flag = true;
            }
            else
            {
                using (MemoryStream memoryStream = new MemoryStream(array))
                {
                    using (BinaryReader binaryReader = new BinaryReader(memoryStream))
                    {
                        int num = binaryReader.ReadInt32();
                        if (num < minVersion)
                        {
                            flag = true;
                        }
                    }
                    memoryStream.Close();
                }
            }
            if (flag)
            {
                File.Delete(OPTIONS_FILE);
            }
        }
    }

    private void OnOptionsDelete(bool success)
    {
    }

    public void SetGraphicOptions()
    {
        StartCoroutine(SetGraphicOptionsCoroutine());
    }

    private IEnumerator SetGraphicOptionsCoroutine()
    {
        if (firstOptionsSetting)
        {
            switch (Options.shadowsQuality)
            {
                case 0:
                    QualitySettings.SetQualityLevel(0);
                    break;
                case 1:
                    QualitySettings.SetQualityLevel(1);
                    break;
                case 2:
                    QualitySettings.SetQualityLevel(2);
                    break;
                case 3:
                    QualitySettings.SetQualityLevel(3);
                    break;
                case 4:
                    QualitySettings.SetQualityLevel(4);
                    break;
                default:
                    QualitySettings.SetQualityLevel(4);
                    break;
            }
            switch (Options.textureQuality)
            {
                case 0:
                    QualitySettings.masterTextureLimit = 3;
                    break;
                case 1:
                    QualitySettings.masterTextureLimit = 2;
                    break;
                case 2:
                    QualitySettings.masterTextureLimit = 1;
                    break;
                case 3:
                    QualitySettings.masterTextureLimit = 0;
                    break;
                default:
                    QualitySettings.masterTextureLimit = 2;
                    break;
            }
            switch (Options.shadowCascades)
            {
                case 0:
                    QualitySettings.shadowCascades = 0;
                    break;
                case 1:
                    QualitySettings.shadowCascades = 2;
                    break;
                case 2:
                    QualitySettings.shadowCascades = 4;
                    break;
                default:
                    QualitySettings.shadowCascades = 0;
                    break;
            }
            QualitySettings.vSyncCount = (Options.vsync ? 1 : 0);
        }
        graphicOptionsSet = true;
        while (Camera.main == null)
        {
            yield return null;
        }
        CameraManager mngr = Camera.main.GetComponent<CameraManager>();
        if ((bool)mngr)
        {
            SkyManager.Get().GlobalSky.CamExposure = GetBrightnessExposureValue();
            mngr.SetDOFActive(Options.graphicsDof);
            mngr.SetSSAOActive(Options.graphicsSsao);
            mngr.SetSMAALevel(Options.graphicsSmaa);
        }
        if (!firstOptionsSetting)
        {
            bool resValid = false;
            for (int i = 0; i < Screen.resolutions.Length; i++)
            {
                if (Screen.resolutions[i].width == Options.resolution.width && Screen.resolutions[i].height == Options.resolution.height)
                {
                    resValid = true;
                    break;
                }
            }
            if (!resValid)
            {
                Options.resolution = Screen.resolutions[Screen.resolutions.Length - 1];
            }
            Screen.SetResolution(Options.resolution.width, Options.resolution.height, Options.fullScreen, 0);
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
        firstOptionsSetting = false;
    }

    public void SetVolumeOptions()
    {
        PandoraSingleton<Pan>.Instance.SetVolume(Pan.Type.FX, Options.fxVolume);
        PandoraSingleton<Pan>.Instance.SetVolume(Pan.Type.MASTER, Options.masterVolume);
        PandoraSingleton<Pan>.Instance.SetVolume(Pan.Type.MUSIC, Options.musicVolume);
        PandoraSingleton<Pan>.Instance.SetVolume(Pan.Type.VOICE, Options.voiceVolume);
        PandoraSingleton<Pan>.Instance.SetVolume(Pan.Type.AMBIENT, Options.ambientVolume);
    }

    public void SetControlsOptions()
    {
        if (Options.gamepadEnabled)
        {
            PandoraSingleton<PandoraInput>.Instance.ActivateController((ControllerType)2);
        }
        else
        {
            PandoraSingleton<PandoraInput>.Instance.DeactivateController((ControllerType)2);
        }
        PandoraSingleton<PandoraInput>.Instance.SetActionInverted("mouse_x", Options.cameraXInverted);
        PandoraSingleton<PandoraInput>.Instance.SetActionInverted("cam_x", Options.cameraXInverted);
        PandoraSingleton<PandoraInput>.Instance.SetActionInverted("mouse_y", Options.cameraYInverted);
        PandoraSingleton<PandoraInput>.Instance.SetActionInverted("cam_y", Options.cameraYInverted);
        PandoraSingleton<PandoraInput>.Instance.SetLeftHandedMouse(Options.leftHandedMouse, includeUserAssignables: true);
        PandoraSingleton<PandoraInput>.Instance.SetLeftHandedController(Options.leftHandedController, includeUserAssignables: true);
        PandoraSingleton<PandoraInput>.Instance.SetMouseSensitivity(Options.mouseSensitivity);
        PandoraSingleton<PandoraInput>.Instance.SetJoystickSensitivity(Options.joystickSensitivity);
    }

    public void SetGameplayOptions()
    {
        PandoraSingleton<LocalizationManager>.Instance.SetLanguage((SupportedLanguage)Options.language);
        if (!PandoraSingleton<MissionManager>.Exists())
        {
            return;
        }
        for (int i = 0; i < PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs.Count; i++)
        {
            if (PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs[i].Beacon != null)
            {
                PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs[i].Beacon.SetActive(Options.wagonBeaconsEnabled);
            }
        }
    }

    public void SetMappingOptions()
    {
        PandoraSingleton<PandoraInput>.Instance.LoadMappingFromXml((ControllerType)0, Options.keyboardMappingData);
        PandoraSingleton<PandoraInput>.Instance.LoadMappingFromXml((ControllerType)2, Options.joystickMappingData);
        PandoraSingleton<PandoraInput>.Instance.LoadMappingFromXml((ControllerType)1, Options.mouseMappingData);
    }

    public float GetBrightnessExposureValue()
    {
        if (Options.graphicsBrightness <= 0.5f)
        {
            return 0.1f + Options.graphicsBrightness / 0.5f * 0.9f;
        }
        return 1f + (Options.graphicsBrightness - 0.5f) / 0.5f * 9f;
    }

    private void OnControllerConnected()
    {
        PandoraSingleton<PandoraInput>.Instance.LoadMappingFromXml((ControllerType)2, Options.joystickMappingData);
    }

    public void SaveProfile()
    {
        byte[] data = Thoth.WriteToArray(Profile.ProfileSave);
        PandoraSingleton<Hephaestus>.Instance.FileWrite("profile.sg", data, OnSaveProfile);
    }

    private void OnSaveProfile(bool success)
    {
        if (success)
        {
        }
    }

    public void ReadProfile()
    {
        if (PandoraSingleton<Hephaestus>.Instance.FileExists("profile.sg"))
        {
            PandoraDebug.LogInfo("Profile Read START");
            PandoraSingleton<Hephaestus>.Instance.FileRead("profile.sg", OnProfileRead);
        }
        else
        {
            GenerateProfile();
            profileInitialized = true;
        }
    }

    private void OnProfileRead(byte[] data, bool success)
    {
        if (success)
        {
            ProfileSave profileSave = new ProfileSave();
            Thoth.ReadFromArray(data, profileSave);
            Profile = new Profile(profileSave);
        }
        else
        {
            GenerateProfile();
        }
        profileInitialized = true;
    }

    private void GenerateProfile()
    {
        Profile = new Profile(new ProfileSave());
        SaveProfile();
    }

    private void OnDeleteProfile(bool success)
    {
    }

    public void EnableInput()
    {
        PandoraSingleton<PandoraInput>.Instance.SetActive(active: true);
        if ((UnityEngine.Object)(object)EventSystem.get_current() != null)
        {
            EventSystem.get_current().set_sendNavigationEvents(true);
        }
        PandoraSingleton<PandoraInput>.Instance.PopInputLayer(PandoraInput.InputLayer.TRANSITION);
        PandoraSingleton<Pan>.Instance.Narrate("main_menu" + PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(1, 6));
        PandoraSingleton<PandoraInput>.Instance.SetCurrentState(PandoraInput.States.MENU, showMouse: true);
    }

    public void NoControllerConnected()
    {
        ShowSystemPopup(SystemPopupId.CONTROLLER_DISCONNECTED, "no_controller_connected_title", "no_controller_connected_desc", delegate
        {
        });
    }

    public void UserDisconnected(string playerName)
    {
        if (!string.IsNullOrEmpty(playerName))
        {
            ShowSystemPopup(SystemPopupId.USER_DISCONNECTED, "user_disconnected_desc", "user_disconnected_title", playerName, delegate
            {
                PandoraSingleton<Hephaestus>.Instance.Reset();
                Save.Reset();
                PandoraSingleton<TransitionManager>.Instance.Clear(reset: true);
                PandoraSingleton<TransitionManager>.Instance.DestroyLoading();
                PandoraSingleton<PandoraInput>.Instance.ClearInputLayer();
                StartCoroutine(PandoraSingleton<AssetBundleLoader>.Instance.UnloadAll());
                SceneManager.LoadScene("copyright");
            });
        }
    }

    public void OnlineStatusChanged(string title, string desc)
    {
        ShowSystemPopup(SystemPopupId.ONLINE_STATUS, title, desc, null);
    }

    public void ShowSystemPopup(SystemPopupId popupId, string title, string desc, Action<bool> cb, bool showCancel = false)
    {
        ShowSystemPopup(popupId, title, desc, string.Empty, cb, showCancel);
    }

    public void ShowSystemPopup(SystemPopupId popupId, string title, string desc, string param, Action<bool> cb, bool showCancel = false)
    {
        SystemPopupData systemPopupData = null;
        if (popupId != SystemPopupId.DLC)
        {
            for (int i = 0; i < systemPopups.Count; i++)
            {
                if (systemPopups[i].popupId == popupId)
                {
                    systemPopupData = systemPopups[i];
                    systemPopups.RemoveAt(i);
                }
            }
        }
        if (systemPopupData == null)
        {
            systemPopupData = new SystemPopupData();
        }
        systemPopupData.popupId = popupId;
        systemPopupData.title = title;
        systemPopupData.desc = desc;
        systemPopupData.descParam = param;
        systemPopupData.cb = cb;
        systemPopupData.showCancel = showCancel;
        systemPopups.Add(systemPopupData);
        DisplaySystemPopup();
    }

    private void DisplaySystemPopup()
    {
        if (systemPopups.Count != 0)
        {
            SystemPopupData data = systemPopups[systemPopups.Count - 1];
            Popup.ShowLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById(data.title), (!string.IsNullOrEmpty(data.descParam)) ? PandoraSingleton<LocalizationManager>.Instance.GetStringById(data.desc, data.descParam) : PandoraSingleton<LocalizationManager>.Instance.GetStringById(data.desc), delegate (bool confirm)
            {
                if (data.cb != null)
                {
                    data.cb(confirm);
                }
                systemPopups.Remove(data);
                DisplaySystemPopup();
            });
            if (!data.showCancel)
            {
                Popup.HideCancelButton();
            }
        }
    }

    public bool IsPopupExist(SystemPopupId popupId)
    {
        for (int i = 0; i < systemPopups.Count; i++)
        {
            if (systemPopups[i].popupId == popupId)
            {
                return true;
            }
        }
        return false;
    }

    public void ResetTimeScale()
    {
        Time.timeScale = ((!IsFastForwarded || !PandoraSingleton<MissionManager>.Exists()) ? 1f : 1.15f);
    }
}
