using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CopyrightManager : MonoBehaviour
{
    private const float SHOW_TIME = 5f;

    private const string MAIN_MENU_SCENE = "main_menu";

    public GameObject vidContainer;

    public VideoPlayer vidPlayer;

    public ImageGroup skipButton;

    public GameObject copyrightPanel;

    public GameObject copyrightInfo;

    public GameObject autoSaveInfo;

    public ImageGroup saveButton;

    public Text subtitleText;

    public GameObject cometLogo;

    public string[] movieQueue;

    private float time = -1f;

    private bool playingMovie;

    private bool transitionToMenu;

    private bool transitionning;

    private int movieQueueIdx;

    public FadeAction fade;

    private MainMenuController mainMenuController;

    private Camera mainMenuCamera;

    private bool videoSkipped;

    private void Awake()
    {
        PandoraSingleton<PandoraInput>.Instance.SetCurrentState(PandoraInput.States.MENU, showMouse: false);
        PandoraSingleton<PandoraInput>.Instance.PushInputLayer(PandoraInput.InputLayer.TRANSITION);
        Object.Destroy(cometLogo);
        PandoraSingleton<GameManager>.Instance.inCopyright = true;
        PandoraSingleton<GameManager>.Instance.inVideo = true;
    }

    private void Start()
    {
        skipButton.SetAction("action", "controls_action_skip_intro", -1);
        skipButton.gameObject.SetActive(value: false);
        autoSaveInfo.SetActive(value: false);
        playingMovie = false;
        transitionToMenu = false;
        transitionning = false;
        time = -1f;
        if (movieQueue.Length <= 0)
        {
            transitionToMenu = true;
        }
        if (PandoraSingleton<GameManager>.Instance.skipLogos)
        {
            movieQueueIdx = movieQueue.Length;
            copyrightInfo.SetActive(value: false);
        }
        StartCoroutine(LoadMainMenu());
    }

    private void Update()
    {
        if (time >= 0f)
        {
            time += Time.deltaTime;
        }
        if (time >= 5f)
        {
            if (transitionToMenu)
            {
                transitionning = true;
                transitionToMenu = false;
                FadeToMainMenu();
            }
            else if (!transitionning && !playingMovie)
            {
                PlayMovie();
            }
        }
        if (playingMovie && skipButton.gameObject.activeSelf && !videoSkipped && PandoraSingleton<PandoraInput>.Instance.GetKeyUp("action", -1))
        {
            videoSkipped = true;
            vidPlayer.Stop();
            OnMovieDone();
        }
    }

    private void FadeToMainMenu()
    {
        PandoraDebug.LogInfo("main_menu Start Fade", "FLOW", this);
        fade.destroy = true;
        fade.Fade(DestroyCopyright, PandoraSingleton<GameManager>.Instance.EnableInput);
        PandoraSingleton<Pan>.Instance.UnPauseMusic();
        PandoraSingleton<PandoraInput>.Instance.SetActive(active: false);
    }

    private void StartGame()
    {
        StartCoroutine(LoadMainMenu());
    }

    private IEnumerator LoadMainMenu()
    {
        while (!PandoraSingleton<GameManager>.Instance.profileInitialized)
        {
            yield return null;
        }
        Application.backgroundLoadingPriority = ThreadPriority.Low;
        yield return SceneManager.LoadSceneAsync("main_menu", LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("main_menu"));
        yield return null;
        if (!PandoraSingleton<GameManager>.Instance.skipLogos)
        {
            time = 0f;
        }
        mainMenuCamera = GameObject.Find("game_camera").GetComponent<Camera>();
        mainMenuCamera.GetComponent<Camera>().enabled = false;
        mainMenuCamera.GetComponent<AudioListener>().enabled = false;
        mainMenuController = GameObject.Find("gui/main_menu").GetComponent<MainMenuController>();
        mainMenuController.uiContainer.enabled = false;
        mainMenuController.environment.SetActive(value: false);
        PandoraSingleton<Pan>.Instance.PauseMusic();
        if (PandoraSingleton<GameManager>.Instance.skipLogos)
        {
            OnMovieDone();
        }
        else
        {
            PandoraSingleton<GameManager>.Instance.skipLogos = true;
        }
    }

    private void PlayMovie()
    {
        QualitySettings.vSyncCount = 0;
        vidContainer.SetActive(value: true);
        StartCoroutine(vidPlayer.Play(movieQueue[movieQueueIdx], OnMovieDone));
        copyrightPanel.SetActive(value: false);
        playingMovie = true;
        movieQueueIdx++;
        if (movieQueueIdx == movieQueue.Length)
        {
            skipButton.gameObject.SetActive(value: true);
            StopCoroutine("SubtitlePlayer");
            ((Component)(object)subtitleText).gameObject.SetActive(value: false);
            StartCoroutine("SubtitlePlayer");
        }
    }

    private void OnMovieDone()
    {
        playingMovie = false;
        if (movieQueueIdx >= movieQueue.Length)
        {
            transitionToMenu = true;
        }
    }

    private void DestroyCopyright()
    {
        mainMenuCamera.GetComponent<Camera>().enabled = true;
        mainMenuCamera.GetComponent<AudioListener>().enabled = true;
        mainMenuController.uiContainer.enabled = true;
        mainMenuController.environment.SetActive(value: true);
        QualitySettings.vSyncCount = (PandoraSingleton<GameManager>.Instance.Options.vsync ? 1 : 0);
        Object.Destroy(base.gameObject);
        PandoraSingleton<GameManager>.Instance.inCopyright = false;
        Application.backgroundLoadingPriority = ThreadPriority.Normal;
    }

    private IEnumerator SubtitlePlayer()
    {
        int MAX_SUB = 36;
        int curSubIdx = 0;
        float[] subTimes = new float[36]
        {
            13f,
            0.7f,
            0f,
            0.2f,
            0.2f,
            0.2f,
            4.5f,
            0.1f,
            0.5f,
            2.8f,
            0.1f,
            0f,
            0.1f,
            0.1f,
            0.5f,
            0.1f,
            0.1f,
            5.5f,
            0.1f,
            0.3f,
            0.1f,
            0.1f,
            0.1f,
            0.5f,
            0.1f,
            0.1f,
            0.1f,
            0.1f,
            0.1f,
            0.3f,
            0.1f,
            0.1f,
            0f,
            0.1f,
            0.1f,
            0f
        };
        float[] hideTimes = new float[36]
        {
            5.2f,
            3.8f,
            3.1f,
            6.5f,
            3.8f,
            3.5f,
            3.2f,
            5f,
            6.4f,
            4.9f,
            5.6f,
            4.9f,
            5.3f,
            5.5f,
            3f,
            3.2f,
            3.7f,
            3.2f,
            3.2f,
            3.2f,
            6.5f,
            5.2f,
            3.7f,
            3.5f,
            5f,
            2.9f,
            4.2f,
            4.3f,
            7.2f,
            4.4f,
            2.1f,
            4f,
            4f,
            4.6f,
            3.2f,
            5.2f
        };
        for (; curSubIdx < MAX_SUB; curSubIdx++)
        {
            yield return new WaitForSeconds(subTimes[curSubIdx]);
            ((Component)(object)subtitleText).gameObject.SetActive(value: true);
            subtitleText.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("cut_scene_sub_" + (curSubIdx + 1)));
            yield return new WaitForSeconds(hideTimes[curSubIdx]);
            ((Component)(object)subtitleText).gameObject.SetActive(value: false);
        }
        yield return null;
    }
}
