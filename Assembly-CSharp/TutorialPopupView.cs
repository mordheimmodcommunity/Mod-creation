using System;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPopupView : ConfirmationPopupView
{
	public Image image;

	protected override void Awake()
	{
		base.Awake();
		base.gameObject.SetActive(value: false);
		PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.GAME_TUTO_MESSAGE, ShowMessage);
	}

	public void ShowMessage()
	{
		bool flag = (bool)PandoraSingleton<NoticeManager>.Instance.Parameters[0];
		string text = (string)PandoraSingleton<NoticeManager>.Instance.Parameters[1];
		if (flag)
		{
			base.gameObject.SetActive(value: true);
			Show(text.Replace("_console", string.Empty).Replace("message", "title"), text, OnTutorialConfirm);
			Sprite sprite = PandoraSingleton<AssetBundleLoader>.Instance.LoadAsset<Sprite>("Assets/gui/assets/tutorial/tutorial/pc/", AssetBundleId.LOADING, text.Replace("message", "image").Replace("_console", string.Empty) + ".png");
			if (sprite != null)
			{
				((Component)(object)image).gameObject.SetActive(value: true);
				image.set_overrideSprite(sprite);
			}
			else
			{
				image.set_overrideSprite((Sprite)null);
				((Component)(object)image).gameObject.SetActive(value: false);
			}
		}
		else
		{
			Hide();
		}
	}

	private void OnTutorialConfirm(bool obj)
	{
	}

	public override void Show(string titleId, string textId, Action<bool> callback, bool hideButtons = false, bool hideCancel = false)
	{
		base.Show(callback, hideButtons, hideCancel: true);
		if (!string.IsNullOrEmpty(titleId))
		{
			title.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById(titleId));
		}
		if (!string.IsNullOrEmpty(textId))
		{
			string stringById = PandoraSingleton<LocalizationManager>.Instance.GetStringById(textId);
			text.set_text(PandoraSingleton<LocalizationManager>.Instance.ReplaceAllActionsWithButtonName(stringById));
		}
		confirmButton.SetAction("action", "menu_confirm", 1);
		confirmButton.SetInteractable(inter: false);
		confirmButton.OnAction(null, mouseOnly: false);
	}
}
