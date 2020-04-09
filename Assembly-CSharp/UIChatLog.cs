using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIChatLog : CanvasGroupDisabler
{
	private const float CHAT_UPTIME = 5f;

	public float enlargeRatioX = 1f;

	public float enlargeRatioY = 2f;

	public ScrollGroup messages;

	private List<UIChatLogItem> messageEntries;

	public ScrollGroup combatLog;

	private List<UIChatLogItem> combatLogEntries;

	private PointerEventData scrollUpData;

	private PointerEventData scrollDownData;

	public GameObject item;

	public RectTransform maskTransform;

	public InputField text;

	public Text title;

	private bool showLog;

	private bool enlarged;

	private float lastListPos;

	private void Update()
	{
		if (PandoraSingleton<PandoraInput>.Instance.GetKey("scroll", 2))
		{
			messages.scrollRect.OnScroll(scrollUpData);
		}
		else if (PandoraSingleton<PandoraInput>.Instance.GetNegKey("scroll", 2))
		{
			messages.scrollRect.OnScroll(scrollDownData);
		}
		else if (PandoraSingleton<PandoraInput>.Instance.GetKey("scroll", 7))
		{
			combatLog.scrollRect.OnScroll(scrollUpData);
		}
		else if (PandoraSingleton<PandoraInput>.Instance.GetNegKey("scroll", 7))
		{
			combatLog.scrollRect.OnScroll(scrollDownData);
		}
	}

	public void Setup()
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected O, but got Unknown
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected O, but got Unknown
		messages.Setup(item, hideBarIfEmpty: true);
		messageEntries = new List<UIChatLogItem>();
		combatLog.Setup(item, hideBarIfEmpty: true);
		combatLogEntries = new List<UIChatLogItem>();
		((UnityEvent<string>)(object)text.get_onEndEdit()).AddListener((UnityAction<string>)SendChat);
		combatLog.HideScrollbar();
		scrollUpData = (PointerEventData)(object)new PointerEventData(EventSystem.get_current());
		scrollUpData.set_scrollDelta(new Vector2(0f, 1f));
		scrollDownData = (PointerEventData)(object)new PointerEventData(EventSystem.get_current());
		scrollDownData.set_scrollDelta(new Vector2(0f, -1f));
		PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.HERMES_CHAT, GetChat);
		PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.COMBAT_LOG, NewLog);
	}

	public void ShowChat(bool blockInput)
	{
		if (enlarged)
		{
			ShrinkLog();
		}
		base.OnEnable();
		messages.gameObject.SetActive(value: true);
		combatLog.gameObject.SetActive(value: false);
		lastListPos = 0f;
		if (blockInput)
		{
			if (PandoraSingleton<PandoraInput>.Instance.lastInputMode != PandoraInput.InputMode.JOYSTICK || !PandoraSingleton<Hephaestus>.Instance.ShowVirtualKeyboard(PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_chat"), string.Empty, 100u, multiLine: false, OnVirtualKeyboard))
			{
				((Component)(object)text).gameObject.SetActive(value: true);
				text.ActivateInputField();
				messages.ShowScrollbar(forceShow: false);
			}
			PandoraSingleton<PandoraInput>.Instance.PushInputLayer(PandoraInput.InputLayer.CHAT);
		}
		else
		{
			messages.HideScrollbar();
		}
		StopCoroutine("TurnOffChat");
		title.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_chat"));
	}

	private void OnVirtualKeyboard(bool success, string newString)
	{
		if (success)
		{
			SendChat(newString);
		}
		else
		{
			SendChat(null);
		}
	}

	public void HideLog()
	{
		if (enlarged)
		{
			ShrinkLog();
		}
		combatLog.gameObject.SetActive(value: false);
		showLog = false;
		base.OnDisable();
	}

	public void ToggleLogDisplay()
	{
		if (showLog && combatLog.gameObject.activeSelf)
		{
			if (enlarged)
			{
				HideLog();
			}
			else
			{
				EnlargeLog();
			}
		}
		else
		{
			messages.gameObject.SetActive(value: false);
			combatLog.gameObject.SetActive(value: true);
			showLog = true;
			base.OnEnable();
			title.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_log"));
		}
		lastListPos = 0f;
		StartCoroutine("ScrollToLastOnNextFrame", combatLog);
	}

	public void ShowLog()
	{
		if (showLog && combatLog.gameObject.activeSelf)
		{
			if (enlarged)
			{
				ShrinkLog();
			}
			combatLog.gameObject.SetActive(value: false);
			showLog = false;
			base.OnDisable();
		}
		else
		{
			messages.gameObject.SetActive(value: false);
			combatLog.gameObject.SetActive(value: true);
			showLog = true;
			base.OnEnable();
			lastListPos = 0f;
			title.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_log"));
		}
	}

	public void EnlargeLog()
	{
		if (!enlarged)
		{
			PandoraSingleton<PandoraInput>.Instance.PushInputLayer(PandoraInput.InputLayer.LOG);
			combatLog.ShowScrollbar();
			combatLog.OnEnable();
			Vector2 sizeDelta = ((RectTransform)base.transform).sizeDelta;
			sizeDelta.x *= enlargeRatioX;
			sizeDelta.y *= enlargeRatioY;
			((RectTransform)base.transform).sizeDelta = sizeDelta;
			enlarged = true;
		}
	}

	public void ShrinkLog()
	{
		if (enlarged)
		{
			PandoraSingleton<PandoraInput>.Instance.PopInputLayer(PandoraInput.InputLayer.LOG);
			combatLog.HideScrollbar();
			Vector2 sizeDelta = ((RectTransform)base.transform).sizeDelta;
			sizeDelta.x /= enlargeRatioX;
			sizeDelta.y /= enlargeRatioY;
			((RectTransform)base.transform).sizeDelta = sizeDelta;
			enlarged = false;
		}
	}

	private void NewLog()
	{
		string content = (string)PandoraSingleton<NoticeManager>.Instance.Parameters[0];
		GameObject gameObject = combatLog.AddToList(null, null);
		UIChatLogItem component = gameObject.GetComponent<UIChatLogItem>();
		component.Init(content);
		combatLogEntries.Add(component);
		if (combatLogEntries.Count > combatLog.items.Count)
		{
			combatLogEntries.RemoveAt(0);
		}
		if (!enlarged)
		{
			StartCoroutine("ScrollToLastOnNextFrame", combatLog);
		}
	}

	private IEnumerator ScrollToLastOnNextFrame(ScrollGroup scrollGroup)
	{
		if (scrollGroup.items.Count != 0)
		{
			yield return null;
			scrollGroup.RealignList(isOn: true, scrollGroup.items.Count - 1, force: true);
			yield return null;
		}
	}

	private void GetChat()
	{
		ulong num = (ulong)PandoraSingleton<NoticeManager>.Instance.Parameters[0];
		string content = (string)PandoraSingleton<NoticeManager>.Instance.Parameters[1];
		GameObject gameObject = messages.AddToList(null, null);
		messageEntries.Add(gameObject.GetComponent<UIChatLogItem>());
		if (messageEntries.Count > messages.items.Count)
		{
			messageEntries.RemoveAt(0);
		}
		UIChatLogItem component = gameObject.GetComponent<UIChatLogItem>();
		if (num != 0L)
		{
			((Graphic)component.GetComponent<Text>()).set_color(new Color(127f, 127f, 0f));
		}
		component.Init(content);
		StartCoroutine("ScrollToLastOnNextFrame", messages);
		ShowChat(blockInput: false);
		if (!text.get_isFocused())
		{
			StartTurnOff();
		}
	}

	private void OnEndChatEdit(string message)
	{
		if (!showLog)
		{
			SendChat(message);
		}
	}

	private void SendChat(string message)
	{
		if (!string.IsNullOrEmpty(message))
		{
			PandoraSingleton<Hermes>.Instance.SendChat(message);
			text.set_text(string.Empty);
		}
		else
		{
			StartTurnOff();
		}
		PandoraSingleton<PandoraInput>.Instance.PopInputLayer(PandoraInput.InputLayer.CHAT);
		text.DeactivateInputField();
		((Component)(object)text).gameObject.SetActive(value: false);
	}

	private void StartTurnOff()
	{
		StopCoroutine("TurnOffChat");
		StartCoroutine("TurnOffChat");
	}

	private IEnumerator TurnOffChat()
	{
		yield return new WaitForSeconds(5f);
		if (showLog)
		{
			showLog = false;
			ShowLog();
		}
		else
		{
			messages.gameObject.SetActive(value: false);
			base.OnDisable();
		}
	}
}
