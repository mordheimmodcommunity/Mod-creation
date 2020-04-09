using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ListGroup : MonoBehaviour
{
	public Text title;

	public RectTransform anchor;

	public RectTransform alternateAnchor;

	private GameObject item;

	public int maxCount;

	[HideInInspector]
	public List<GameObject> items = new List<GameObject>();

	private void Awake()
	{
		title = GetComponentInChildren<Text>();
	}

	public void Setup(string name, GameObject item)
	{
		string name2 = string.Empty;
		if ((Object)(object)title != null && !string.IsNullOrEmpty(name))
		{
			name2 = PandoraSingleton<LocalizationManager>.Instance.GetStringById(name);
		}
		SetupLocalized(name2, item);
	}

	public void SetupLocalized(string name, GameObject item)
	{
		if ((Object)(object)title != null && !string.IsNullOrEmpty(name))
		{
			title.set_text(name);
		}
		this.item = item;
		ClearList();
	}

	public void ClearList()
	{
		for (int i = 0; i < items.Count; i++)
		{
			items[i].transform.SetParent(null);
			Object.Destroy(items[i]);
		}
		items.Clear();
	}

	public GameObject AddToList()
	{
		GameObject gameObject = Object.Instantiate(item);
		gameObject.transform.SetParent((maxCount == 0 || !(alternateAnchor != null) || items.Count < maxCount) ? anchor : alternateAnchor, worldPositionStays: false);
		items.Add(gameObject);
		return gameObject;
	}
}
