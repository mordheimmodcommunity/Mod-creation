using mset;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class TransitionManager : PandoraSingleton<TransitionManager>
{
    private enum LoadingState
    {
        IDLE,
        OUT_CURRENT_SCENE,
        IN_LOADING,
        WAIT_ACTION,
        TO_NEXT_SCENE,
        IN_NEXT_SCENE
    }

    private const string LOADING_SCENE_NAME = "loading";

    [SerializeField]
    private string nextSceneName;

    [SerializeField]
    private SceneLoadingTypeId loadingType;

    private LoadingState currentState;

    public TransitionBase transition;

    public bool noTransition;

    private float transitionDuration;

    private float transitionTimer;

    private bool loadingStarted;

    private LoadingViewManager loadingObject;

    private bool waitAction;

    private bool waitForPlayers;

    private bool playersReady;

    public SceneLoadingTypeId LoadingType => loadingType;

    public bool GameLoadingDone
    {
        get;
        private set;
    }

    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        SetState(LoadingState.IDLE);
        GameLoadingDone = true;
    }

    private void LateUpdate()
    {
        if (transition != null && transitionTimer > 0f)
        {
            transitionTimer -= Time.deltaTime;
            if (transitionTimer <= 0f)
            {
                transition.EndTransition();
                switch (currentState)
                {
                    case LoadingState.OUT_CURRENT_SCENE:
                        StartCoroutine(ClearSystems());
                        break;
                    case LoadingState.TO_NEXT_SCENE:
                        if (!waitForPlayers || playersReady)
                        {
                            SetState(LoadingState.IN_NEXT_SCENE);
                        }
                        break;
                    case LoadingState.IN_NEXT_SCENE:
                        PandoraSingleton<PandoraInput>.Instance.SetActive(active: true);
                        PandoraSingleton<PandoraInput>.Instance.PopInputLayer(PandoraInput.InputLayer.TRANSITION);
                        if ((Object)(object)EventSystem.get_current() != null)
                        {
                            EventSystem.get_current().set_sendNavigationEvents(true);
                        }
                        PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.TRANSITION_DONE);
                        SetState(LoadingState.IDLE);
                        break;
                }
            }
            else
            {
                transition.ProcessTransition(1f - transitionTimer / transitionDuration);
            }
        }
        if (currentState == LoadingState.IN_LOADING)
        {
            if (SceneManager.GetActiveScene().name == "loading" && !loadingStarted && transitionTimer <= 0f)
            {
                SetTransition(show: false);
                loadingStarted = true;
                loadingObject.OnTransitionDone();
                PandoraDebug.LogInfo("Loading Level:" + nextSceneName, "LOADING", this);
                SceneManager.LoadSceneAsync(nextSceneName);
            }
            if (GameLoadingDone)
            {
                SetState((!waitAction) ? LoadingState.TO_NEXT_SCENE : LoadingState.WAIT_ACTION);
            }
        }
        else if (currentState == LoadingState.TO_NEXT_SCENE && waitForPlayers && playersReady)
        {
            SetTransition(show: true);
            waitForPlayers = false;
        }
    }

    private void OnTransitionAction()
    {
        if (currentState == LoadingState.WAIT_ACTION)
        {
            SetState(LoadingState.TO_NEXT_SCENE);
        }
    }

    private void SetState(LoadingState state)
    {
        currentState = state;
        switch (currentState)
        {
            case LoadingState.IDLE:
                break;
            case LoadingState.OUT_CURRENT_SCENE:
                SetTransition(show: true);
                break;
            case LoadingState.IN_LOADING:
                SceneManager.LoadSceneAsync("loading");
                loadingStarted = false;
                break;
            case LoadingState.WAIT_ACTION:
                PandoraSingleton<PandoraInput>.Instance.SetActive(active: true);
                PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.TRANSITION_ACTION, OnTransitionAction);
                PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.TRANSITION_WAIT_FOR_ACTION);
                break;
            case LoadingState.TO_NEXT_SCENE:
                if (waitForPlayers)
                {
                    PandoraSingleton<MissionManager>.Instance.NetworkMngr.SendReadyToStart();
                }
                else
                {
                    SetTransition(show: true);
                }
                break;
            case LoadingState.IN_NEXT_SCENE:
                if (loadingType == SceneLoadingTypeId.NONE)
                {
                    SceneManager.LoadSceneAsync(nextSceneName);
                }
                else
                {
                    Object.Destroy(loadingObject.gameObject);
                    loadingObject = null;
                }
                SetTransition(show: false);
                break;
        }
    }

    public void LoadNextScene(string nextScene, SceneLoadingTypeId loadingType, float transDuration, bool waitForAction = false, bool waitForPlayers = false, bool force = false)
    {
        if (currentState != 0)
        {
            if (!force)
            {
                PandoraDebug.LogInfo("[TransitionManager] Trying to load a scene while not in Idle State. Use the force param to load it anyways.", "LOADING");
                return;
            }
            transition.EndTransition();
            DestroyLoading(transDuration);
        }
        transitionDuration = transDuration;
        transitionTimer = transitionDuration;
        nextSceneName = nextScene;
        this.loadingType = loadingType;
        waitAction = waitForAction;
        GameLoadingDone = false;
        playersReady = false;
        loadingObject = null;
        this.waitForPlayers = waitForPlayers;
        PandoraSingleton<PandoraInput>.Instance.ClearInputLayer();
        PandoraSingleton<PandoraInput>.Instance.SetActive(active: false);
        if ((Object)(object)EventSystem.get_current() != null)
        {
            EventSystem.get_current().set_sendNavigationEvents(false);
        }
        PandoraSingleton<PandoraInput>.instance.PushInputLayer(PandoraInput.InputLayer.TRANSITION);
        PandoraSingleton<Pan>.Instance.SoundsOff();
        SetState(LoadingState.OUT_CURRENT_SCENE);
    }

    public void DestroyLoading(float transDuration = 0f)
    {
        if (loadingObject != null)
        {
            StartCoroutine(DestroyLoadingObjectDelayed(loadingObject.gameObject, transDuration));
        }
    }

    private IEnumerator DestroyLoadingObjectDelayed(GameObject loadingObject, float delay)
    {
        yield return new WaitForSeconds(delay);
        Object.Destroy(loadingObject);
    }

    public void SetGameLoadingDone(bool overrideWaitPlayer = false)
    {
        if (overrideWaitPlayer)
        {
            waitForPlayers = false;
        }
        SkyManager.Get().GlobalSky.CamExposure = PandoraSingleton<GameManager>.Instance.GetBrightnessExposureValue();
        if (currentState == LoadingState.IN_LOADING)
        {
            GameLoadingDone = true;
        }
        if (noTransition)
        {
            noTransition = false;
            PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.TRANSITION_DONE);
        }
    }

    private void SetTransition(bool show)
    {
        transitionTimer = transitionDuration;
        if (transition != null)
        {
            transition.Show(show, transitionDuration);
        }
    }

    public void RequestLoadingContent(LoadingViewManager loadingViewMan)
    {
        loadingObject = loadingViewMan;
        loadingViewMan.SetContent(loadingType, waitForPlayers);
    }

    private IEnumerator ClearSystems()
    {
        Clear();
        yield return StartCoroutine(PandoraSingleton<AssetBundleLoader>.Instance.UnloadAll());
        if (loadingType == SceneLoadingTypeId.NONE)
        {
            SetState(LoadingState.IN_NEXT_SCENE);
        }
        else
        {
            SetState(LoadingState.IN_LOADING);
        }
    }

    public void Clear(bool reset = false)
    {
        if (reset)
        {
            Init();
        }
        PandoraSingleton<NoticeManager>.Instance.RemoveListener(Notices.TRANSITION_ACTION, OnTransitionAction);
        PandoraSingleton<NoticeManager>.Instance.Clear();
        PandoraSingleton<Hermes>.Instance.ResetGUID();
        PandoraSingleton<Pan>.Instance.Clear();
    }

    public void OnPlayersReady()
    {
        playersReady = true;
    }

    public bool IsDone()
    {
        return currentState == LoadingState.IDLE;
    }

    public bool IsLoading()
    {
        return currentState == LoadingState.OUT_CURRENT_SCENE || currentState == LoadingState.IN_LOADING || currentState == LoadingState.IN_NEXT_SCENE;
    }
}
