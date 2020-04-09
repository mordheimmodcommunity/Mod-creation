using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectorGroup : MonoBehaviour
{
	public delegate void OnValueChanged(int id, int index);

	private Text text;

	private Button left;

	private Button right;

	[HideInInspector]
	public Button selection;

	public int id;

	public bool loopAround = true;

	public bool repeat;

	public List<string> selections = new List<string>();

	public OnValueChanged onValueChanged;

	private bool init;

	private bool isActive = true;

	private int mod;

	private float time;

	private bool isCycling;

	private float currentInputWait = 0.2f;

	private Vector2 clampInputWait = new Vector2(0.05f, 0.2f);

	public int CurSel
	{
		get;
		private set;
	}

	private void Awake()
	{
		if (!init)
		{
			Init();
		}
	}

	private void Init()
	{
		init = true;
		isActive = true;
		Text[] componentsInChildren = GetComponentsInChildren<Text>(includeInactive: true);
		Text[] array = componentsInChildren;
		foreach (Text val in array)
		{
			if (((Component)(object)val).gameObject.name.Equals("value", StringComparison.OrdinalIgnoreCase))
			{
				text = val;
				break;
			}
		}
		if ((UnityEngine.Object)(object)text == null)
		{
			text = componentsInChildren[0];
		}
		Button[] componentsInChildren2 = GetComponentsInChildren<Button>(includeInactive: true);
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			if (((UnityEngine.Object)(object)componentsInChildren2[j]).name.Contains("left"))
			{
				left = componentsInChildren2[j];
			}
			else if (((UnityEngine.Object)(object)componentsInChildren2[j]).name.Contains("right"))
			{
				right = componentsInChildren2[j];
			}
		}
		selection = ((Component)(object)text).GetComponent<Button>();
		EventTrigger trigger = ((Component)(object)left).gameObject.AddComponent<EventTrigger>();
		EventTrigger trigger2 = ((Component)(object)right).gameObject.AddComponent<EventTrigger>();
		trigger.AddUnityEvent((EventTriggerType)9, OnLeft);
		trigger.AddUnityEvent((EventTriggerType)4, OnLeft);
		if (repeat)
		{
			trigger.AddUnityEvent((EventTriggerType)2, StartLeft);
			trigger.AddUnityEvent((EventTriggerType)1, EndLeft);
			trigger.AddUnityEvent((EventTriggerType)3, EndLeft);
		}
		trigger2.AddUnityEvent((EventTriggerType)9, OnRight);
		trigger2.AddUnityEvent((EventTriggerType)4, OnRight);
		if (repeat)
		{
			trigger2.AddUnityEvent((EventTriggerType)2, StartRight);
			trigger2.AddUnityEvent((EventTriggerType)1, EndRight);
			trigger2.AddUnityEvent((EventTriggerType)3, EndRight);
		}
		CurSel = 0;
	}

	private void StartLeft(BaseEventData eventData)
	{
		isCycling = true;
		time = 0f;
		currentInputWait = clampInputWait.y;
		mod = -1;
	}

	private void EndLeft(BaseEventData eventData)
	{
		isCycling = false;
		((MonoBehaviour)(object)selection).SetSelected(force: true);
	}

	private void OnLeft(BaseEventData eventData)
	{
		if (isActive)
		{
			CurSel = ((CurSel - 1 >= 0) ? (CurSel - 1) : (loopAround ? (selections.Count - 1) : 0));
			UpdateSelection();
		}
		((MonoBehaviour)(object)selection).SetSelected(force: true);
	}

	private void StartRight(BaseEventData eventData)
	{
		isCycling = true;
		time = 0f;
		currentInputWait = clampInputWait.y;
		mod = 1;
	}

	private void EndRight(BaseEventData eventData)
	{
		isCycling = false;
		((MonoBehaviour)(object)selection).SetSelected(force: true);
	}

	private void OnRight(BaseEventData eventData)
	{
		if (isActive)
		{
			CurSel = ((CurSel + 1 < selections.Count) ? (CurSel + 1) : ((!loopAround) ? (selections.Count - 1) : 0));
			UpdateSelection();
		}
		((MonoBehaviour)(object)selection).SetSelected(force: true);
	}

	private void UpdateSelection()
	{
		text.set_text(selections[CurSel]);
		if (onValueChanged != null)
		{
			onValueChanged(id, CurSel);
		}
	}

	public void SetButtonsVisible(bool show)
	{
		if (!init)
		{
			Init();
		}
		isActive = show;
		((Behaviour)(object)((Selectable)left).get_image()).enabled = show;
		((Behaviour)(object)((Selectable)right).get_image()).enabled = show;
	}

	public void SetCurrentSel(int sel)
	{
		CurSel = sel;
		text.set_text(selections[CurSel]);
	}

	public void Update()
	{
		if (!isActive || !isCycling)
		{
			return;
		}
		time += Time.deltaTime;
		if (time > currentInputWait)
		{
			currentInputWait = Mathf.Clamp(currentInputWait - 0.01f, clampInputWait.x, clampInputWait.y);
			time = 0f;
			int curSel = CurSel;
			if (mod > 0)
			{
				CurSel = ((CurSel + 1 < selections.Count) ? (CurSel + 1) : ((!loopAround) ? (selections.Count - 1) : 0));
			}
			else
			{
				CurSel = ((CurSel - 1 >= 0) ? (CurSel - 1) : (loopAround ? (selections.Count - 1) : 0));
			}
			if (CurSel != curSel)
			{
				UpdateSelection();
			}
		}
	}
}
