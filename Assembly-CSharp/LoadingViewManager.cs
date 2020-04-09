using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LoadingViewManager : MonoBehaviour
{
	public GameObject loadingText;

	public ButtonGroup continueButton;

	public Text textWaitingForPlayer;

	private Dictionary<SceneLoadingTypeId, LoadingView> views = new Dictionary<SceneLoadingTypeId, LoadingView>(5);

	private Canvas canvas;

	private AudioSource audioSrc;

	private bool showWaitingMessage;

	public Text loadingProgress;

	private SceneLoadingTypeId currentLoadType;

	protected virtual void Awake()
	{
		canvas = base.gameObject.GetComponentsInChildren<Canvas>(includeInactive: true)[0];
		audioSrc = GetComponent<AudioSource>();
		continueButton.SetAction("load_confirm", "menu_continue", 1000);
		continueButton.OnAction(OnActionDone, mouseOnly: false);
		continueButton.gameObject.SetActive(value: false);
		((Behaviour)(object)textWaitingForPlayer).enabled = false;
		PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.TRANSITION_WAIT_FOR_ACTION, OnLoadingDone);
		LoadingView[] componentsInChildren = GetComponentsInChildren<LoadingView>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			views.Add(componentsInChildren[i].id, componentsInChildren[i]);
		}
		Object.DontDestroyOnLoad(base.gameObject);
		PandoraSingleton<TransitionManager>.Instance.RequestLoadingContent(this);
	}

	public void SetContent(SceneLoadingTypeId loadType, bool waitForPlayers)
	{
		currentLoadType = loadType;
		views[loadType].Show();
		showWaitingMessage = waitForPlayers;
	}

	private void OnActionDone()
	{
		PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.TRANSITION_ACTION);
		if (showWaitingMessage)
		{
			((Behaviour)(object)textWaitingForPlayer).enabled = true;
			continueButton.gameObject.SetActive(value: false);
		}
		PandoraSingleton<Pan>.Instance.SoundsOn();
	}

	private void OnLoadingDone()
	{
		canvas.sortingOrder = canvas.sortingOrder;
		if (continueButton != null)
		{
			continueButton.RefreshImage();
			continueButton.gameObject.SetActive(value: true);
			if (PandoraSingleton<PandoraInput>.Instance.CurrentInputLayer != 1)
			{
				continueButton.SetSelected(force: true);
			}
		}
		if (loadingText != null)
		{
			Graphic[] componentsInChildren = loadingText.GetComponentsInChildren<Graphic>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].CrossFadeAlpha(0f, 1f, true);
			}
		}
	}

	private void Update()
	{
		if (continueButton != null && continueButton.gameObject.activeInHierarchy)
		{
			Color color = ((Graphic)continueButton.label).get_color();
			color.a = 0.2f + Mathf.Abs(Mathf.Sin(Time.time));
			((Graphic)continueButton.label).set_color(color);
			if ((Object)(object)EventSystem.get_current() != null && continueButton != null && EventSystem.get_current().get_currentSelectedGameObject() != continueButton && PandoraSingleton<PandoraInput>.Instance.CurrentInputLayer != 1)
			{
				continueButton.SetSelected(force: true);
			}
		}
		else if ((Object)(object)textWaitingForPlayer != null && ((Behaviour)(object)textWaitingForPlayer).enabled)
		{
			Color color2 = ((Graphic)textWaitingForPlayer).get_color();
			color2.a = 0.2f + Mathf.Abs(Mathf.Sin(Time.time));
			((Graphic)textWaitingForPlayer).set_color(color2);
		}
		if (PandoraSingleton<MissionLoader>.Exists())
		{
			if (!((Behaviour)(object)loadingProgress).enabled)
			{
				((Behaviour)(object)loadingProgress).enabled = true;
			}
			loadingProgress.set_text(Constant.ToString(PandoraSingleton<MissionLoader>.Instance.percent) + "%");
		}
		else if (((Behaviour)(object)loadingProgress).enabled)
		{
			((Behaviour)(object)loadingProgress).enabled = false;
		}
	}

	public void OnTransitionDone()
	{
		views[currentLoadType].PlayDialog();
	}

	private void OnDestroy()
	{
		PandoraSingleton<NoticeManager>.Instance.RemoveListener(Notices.TRANSITION_WAIT_FOR_ACTION, OnLoadingDone);
	}
}
