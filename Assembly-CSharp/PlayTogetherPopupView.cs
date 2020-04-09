using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayTogetherPopupView : UIPopupModule
{
	public Text title;

	public Text text;

	public ButtonGroup exhibitionButton;

	public ButtonGroup contestButton;

	public ButtonGroup abandonButton;

	protected bool isShow;

	private GameObject previousSelection;

	private Action onExhibitionCallback;

	private Action onContestCallback;

	private Action onCancelCallback;

	public override void Init()
	{
		base.Init();
		if (exhibitionButton != null)
		{
			exhibitionButton.SetAction(null, "menu_skirmish_exhibition", 1);
			exhibitionButton.OnAction(OnExhibition, mouseOnly: false);
		}
		if (contestButton != null)
		{
			contestButton.SetAction(null, "menu_skirmish_contest", 1);
			contestButton.OnAction(OnContest, mouseOnly: false);
		}
		if (abandonButton != null)
		{
			abandonButton.SetAction(null, "menu_cancel", 1);
			abandonButton.OnAction(OnCancel, mouseOnly: false);
		}
	}

	protected virtual void Start()
	{
		if (!isShow)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void OnExhibition()
	{
		Hide();
		if (onExhibitionCallback != null)
		{
			onExhibitionCallback();
		}
	}

	private void OnContest()
	{
		Hide();
		if (onContestCallback != null)
		{
			onContestCallback();
		}
	}

	private void OnCancel()
	{
		Hide();
		if (onCancelCallback != null)
		{
			onCancelCallback();
		}
	}

	public void Show(string titleId, string textId, Action exhibitionCallback, Action contestCallback, Action cancelCallback)
	{
		onCancelCallback = cancelCallback;
		onContestCallback = contestCallback;
		onExhibitionCallback = exhibitionCallback;
		if (!isShow)
		{
			PandoraSingleton<PandoraInput>.Instance.PushInputLayer(PandoraInput.InputLayer.POP_UP);
		}
		isShow = true;
		if (!base.initialized)
		{
			Init();
		}
		previousSelection = ((!((UnityEngine.Object)(object)EventSystem.get_current() != null)) ? null : EventSystem.get_current().get_currentSelectedGameObject());
		if (!string.IsNullOrEmpty(titleId))
		{
			title.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById(titleId));
		}
		if (!string.IsNullOrEmpty(textId))
		{
			text.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById(textId));
		}
		contestButton.effects.toggle.set_isOn(false);
		exhibitionButton.effects.toggle.set_isOn(false);
		abandonButton.effects.toggle.set_isOn(true);
		abandonButton.SetSelected(force: true);
		base.gameObject.SetActive(value: true);
	}

	public virtual void Hide()
	{
		if (isShow)
		{
			isShow = false;
			base.gameObject.SetActive(value: false);
			PandoraSingleton<PandoraInput>.Instance.PopInputLayer(PandoraInput.InputLayer.POP_UP);
			if (previousSelection != null)
			{
				previousSelection.SetSelected();
			}
		}
	}

	private void Update()
	{
		if (isShow && !PandoraSingleton<GameManager>.Instance.Popup.isShow && (UnityEngine.Object)(object)EventSystem.get_current() != null && (EventSystem.get_current().get_currentSelectedGameObject() == null || EventSystem.get_current().get_currentSelectedGameObject().transform.root != base.transform.root))
		{
			abandonButton.SetSelected(force: true);
		}
	}
}
