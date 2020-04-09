using UnityEngine;
using UnityEngine.EventSystems;

public class SelectOnOver : MonoBehaviour, IEventSystemHandler, IPointerEnterHandler
{
	public void OnPointerEnter(PointerEventData eventData)
	{
		EventSystem.get_current().SetSelectedGameObject(base.gameObject);
	}
}
