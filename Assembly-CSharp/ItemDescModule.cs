using System.Collections.Generic;
using UnityEngine;

public class ItemDescModule : UIModule
{
	public GameObject itemDescriptionPrefab;

	public GameObject runeDescriptionPrefab;

	public GameObject mutationDescriptionPrefab;

	private List<UIInventoryItem> uiItems;

	private List<UIInventoryItemDescription> uiItemDescs;

	private UIInventoryRune uiRune;

	private UIInventoryMutation uiMutation;

	public override void Init()
	{
		base.Init();
		uiItems = new List<UIInventoryItem>();
		uiItemDescs = new List<UIInventoryItemDescription>();
		for (int i = 0; i < 2; i++)
		{
			GameObject gameObject = Object.Instantiate(itemDescriptionPrefab);
			gameObject.transform.SetParent(base.transform, worldPositionStays: false);
			uiItems.Add(gameObject.GetComponentInChildren<UIInventoryItem>());
			uiItemDescs.Add(gameObject.GetComponentInChildren<UIInventoryItemDescription>());
		}
		GameObject gameObject2 = Object.Instantiate(runeDescriptionPrefab);
		gameObject2.transform.SetParent(base.transform, worldPositionStays: false);
		uiRune = gameObject2.GetComponentInChildren<UIInventoryRune>();
		GameObject gameObject3 = Object.Instantiate(mutationDescriptionPrefab);
		gameObject3.transform.SetParent(base.transform, worldPositionStays: false);
		uiMutation = gameObject3.GetComponentInChildren<UIInventoryMutation>();
	}

	public void HideAll()
	{
		HideDesc(0);
		for (int i = 1; i < uiItems.Count; i++)
		{
			uiItems[i].gameObject.SetActive(value: false);
		}
	}

	public void HideDesc(int idx)
	{
		uiItems[idx].gameObject.SetActive(value: false);
		uiRune.gameObject.SetActive(value: false);
		uiMutation.gameObject.SetActive(value: false);
	}

	public void SetItem(Item item, UnitSlotId slotId, int idx)
	{
		HideDesc((idx != 1) ? 1 : idx);
		uiItems[idx].gameObject.SetActive(value: true);
		uiItems[idx].Set(item);
		uiItemDescs[idx].Set(item, slotId);
	}

	public void SetRune(RuneMark runeMark)
	{
		HideDesc(1);
		uiRune.gameObject.SetActive(value: true);
		uiRune.Set(runeMark);
	}

	public void SetMutation(Mutation mutation)
	{
		HideDesc(0);
		HideDesc(1);
		uiMutation.gameObject.SetActive(value: true);
		uiMutation.Set(mutation);
	}
}
