using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class KeyUpInputField : InputField
{
	[SerializeField]
	public bool clearOnDeselect = true;

	private readonly Event processingEvent = new Event();

	public UnityAction OnSelectCallBack;

	public KeyUpInputField()
		: this()
	{
	}

	public override void OnSelect(BaseEventData eventData)
	{
		((InputField)this).OnSelect(eventData);
		if (OnSelectCallBack != null)
		{
			OnSelectCallBack();
		}
	}

	public override void OnUpdateSelected(BaseEventData eventData)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Invalid comparison between Unknown and I4
		if (!((InputField)this).get_isFocused())
		{
			return;
		}
		bool flag = false;
		while (Event.PopEvent(processingEvent))
		{
			if (processingEvent.rawType == EventType.KeyDown && processingEvent.character != '\n' && processingEvent.character != '\r' && processingEvent.keyCode != KeyCode.Escape)
			{
				((InputField)this).KeyPressed(processingEvent);
				flag = true;
			}
			else if (processingEvent.rawType == EventType.KeyUp && (processingEvent.keyCode == KeyCode.KeypadEnter || processingEvent.keyCode == KeyCode.Return || processingEvent.keyCode == KeyCode.Escape) && (int)((InputField)this).KeyPressed(processingEvent) == 1)
			{
				flag = true;
				((InputField)this).DeactivateInputField();
				break;
			}
		}
		if (flag)
		{
			((InputField)this).UpdateLabel();
		}
		((AbstractEventData)eventData).Use();
	}

	public override void OnDeselect(BaseEventData eventData)
	{
		if (clearOnDeselect)
		{
			((InputField)this).set_text(string.Empty);
		}
		((InputField)this).OnDeselect(eventData);
	}

	private void Update()
	{
		if (((InputField)this).get_isFocused() && PandoraSingleton<PandoraInput>.Instance.GetKeyUp("action", 1))
		{
			((InputField)this).DeactivateInputField();
		}
	}
}
