using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public static class UGUIExtensions
{
	private static bool waitingToSelect;

	public static void SetSelected(this EventSystem eventSystem, MonoBehaviour selected, bool force = false)
	{
		eventSystem.SetSelected(selected.gameObject, force);
	}

	public static void SetSelected(this EventSystem eventSystem, GameObject selected, bool force = false)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		if (!((Object)(object)eventSystem == null))
		{
			BaseEventData val = (BaseEventData)(object)new BaseEventData(eventSystem);
			if (!eventSystem.get_alreadySelecting())
			{
				eventSystem.SetSelectedGameObject(selected, val);
			}
			else if (force && !waitingToSelect)
			{
				waitingToSelect = true;
				((MonoBehaviour)(object)eventSystem).StartCoroutine(SelectOnNextTrame(eventSystem, selected, val));
			}
		}
	}

	private static IEnumerator SelectOnNextTrame(EventSystem eventSystem, GameObject selected, BaseEventData eventData)
	{
		yield return null;
		eventSystem.SetSelectedGameObject(selected, eventData);
		waitingToSelect = false;
	}

	public static void SetSelected(this MonoBehaviour selected, bool force = false)
	{
		EventSystem.get_current().SetSelected(selected, force);
	}

	public static void SetSelected(this GameObject selected, bool force = false)
	{
		EventSystem.get_current().SetSelected(selected, force);
	}

	public static void AddUnityEvent(this EventTrigger trigger, EventTriggerType triggerType, UnityAction<BaseEventData> action)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		Entry val = (Entry)(object)new Entry();
		val.eventID = triggerType;
		((UnityEvent<BaseEventData>)(object)val.callback).AddListener(action);
		if (trigger.get_triggers() == null)
		{
			trigger.set_triggers(new List<Entry>());
		}
		trigger.get_triggers().Add(val);
	}

	public static void AddUnityEvent(this MonoBehaviour mono, EventTriggerType triggerType, UnityAction<BaseEventData> action)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		mono.gameObject.AddUnityEvent(triggerType, action);
	}

	public static void AddUnityEvent(this GameObject go, EventTriggerType triggerType, UnityAction<BaseEventData> action)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		EventTrigger val = go.GetComponent<EventTrigger>();
		if ((Object)(object)val == null)
		{
			val = go.AddComponent<EventTrigger>();
		}
		val.AddUnityEvent(triggerType, action);
	}

	public static void ResetUnityEvent(this GameObject go)
	{
		EventTrigger component = go.GetComponent<EventTrigger>();
		if ((Object)(object)component != null)
		{
			component.get_triggers().Clear();
		}
	}

	public static void ResetUnityEvent(this MonoBehaviour go)
	{
		go.gameObject.ResetUnityEvent();
	}
}
