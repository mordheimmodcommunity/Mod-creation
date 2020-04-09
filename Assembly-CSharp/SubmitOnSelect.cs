using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SubmitOnSelect : MonoBehaviour, ISelectHandler, IEventSystemHandler
{
	private Button selectable;

	private void Awake()
	{
		selectable = GetComponent<Button>();
	}

	public void OnSelect(BaseEventData eventData)
	{
		selectable.OnSubmit(eventData);
	}
}
