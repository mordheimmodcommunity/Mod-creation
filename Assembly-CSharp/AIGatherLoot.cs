using RAIN.Core;
using System.Collections.Generic;

public class AIGatherLoot : AIBase
{
	public override void Start(AI ai)
	{
		base.Start(ai);
		base.actionName = "GatherLoot";
		WarbandWagon wagon = unitCtrlr.GetWarband().wagon;
		success = (!unitCtrlr.unit.IsInventoryFull() && (wagon == null || unitCtrlr.AICtrlr.targetSearchPoint != wagon.chest) && !unitCtrlr.AICtrlr.targetSearchPoint.IsEmpty());
		int num = -1;
		int num2 = -1;
		if (!success)
		{
			return;
		}
		List<int> list = new List<int>();
		for (int i = 6; i < unitCtrlr.unit.Items.Count; i++)
		{
			if (unitCtrlr.unit.Items[i].Id == ItemId.NONE)
			{
				list.Add(i);
			}
		}
		if (list.Count > 0)
		{
			num = list[PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0, list.Count)] - 6;
		}
		List<Item> items = unitCtrlr.AICtrlr.targetSearchPoint.GetItems();
		num2 = GetSearchSlot(items);
		if (num2 != -1 && num != -1)
		{
			unitCtrlr.SendInventoryChange(num2, num);
		}
		else
		{
			success = false;
		}
	}

	protected virtual int GetSearchSlot(List<Item> searchItems)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < searchItems.Count; i++)
		{
			if (searchItems[i].Id != 0)
			{
				list.Add(i);
			}
		}
		if (list.Count > 0)
		{
			return list[PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0, list.Count)];
		}
		return -1;
	}
}
