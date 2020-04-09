using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionRewardPopup : ConfirmationPopupView
{
	public Image icon;

	public ListGroup rewardItems;

	public ListGroup rewardUnits;

	public GameObject itemReward;

	public GameObject unitReward;

	public void Show(Action<bool> callback, List<WarbandSkillItemData> itemRewards, List<WarbandSkillFreeOutsiderData> freeOutsiders)
	{
		base.Show("mission_camp_reward_title", "mission_camp_reward_" + PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.Id.ToLowerString(), callback);
		Sprite sprite = Warband.GetIcon(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.Id);
		icon.set_sprite(sprite);
		rewardItems.Setup(string.Empty, itemReward);
		for (int i = 0; i < itemRewards.Count; i++)
		{
			Item item = new Item(itemRewards[i].ItemId, itemRewards[i].ItemQualityId);
			item.Save.amount = itemRewards[i].Quantity;
			GameObject gameObject = rewardItems.AddToList();
			gameObject.GetComponent<UIInventoryItem>().Set(item);
		}
		rewardUnits.Setup(string.Empty, unitReward);
		for (int j = 0; j < freeOutsiders.Count; j++)
		{
			GameObject gameObject2 = rewardUnits.AddToList();
			gameObject2.GetComponent<HireUnitDescription>().Set(freeOutsiders[j].UnitId, freeOutsiders[j].Rank);
		}
	}
}
