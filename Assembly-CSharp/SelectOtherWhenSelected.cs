using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectOtherWhenSelected : MonoBehaviour, ISelectHandler, IEventSystemHandler
{
	public GameObject target;

	public void Select(GameObject go)
	{
		EventSystem.get_current().SetSelectedGameObject(go ?? base.gameObject);
	}

	public void OnSelect(BaseEventData eventData)
	{
		StartCoroutine(Select());
	}

	public IEnumerator Select()
	{
		yield return null;
		Select(target);
	}
}
