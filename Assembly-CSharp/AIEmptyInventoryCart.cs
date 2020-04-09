using RAIN.Core;
using System.Collections.Generic;

public class AIEmptyInventoryCart : AIBase
{
	public override void Start(AI ai)
	{
		base.Start(ai);
		base.actionName = "EmptyInventoryCart";
		success = (unitCtrlr.GetWarband().wagon != null && unitCtrlr.AICtrlr.targetSearchPoint == unitCtrlr.GetWarband().wagon.chest && !unitCtrlr.AICtrlr.targetSearchPoint.IsFull() && !unitCtrlr.unit.IsInventoryEmpty());
		if (!success)
		{
			return;
		}
		int num = -1;
		int num2 = -1;
		List<int> list = new List<int>();
		for (int i = 6; i < unitCtrlr.unit.Items.Count; i++)
		{
			if (unitCtrlr.unit.Items[i].Id != 0)
			{
				list.Add(i);
			}
		}
		if (list.Count > 0)
		{
			num = list[PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0, list.Count)] - 6;
		}
		list = new List<int>();
		List<Item> items = unitCtrlr.AICtrlr.targetSearchPoint.GetItems();
		for (int j = 0; j < items.Count; j++)
		{
			if (items[j].Id == ItemId.NONE)
			{
				list.Add(j);
			}
		}
		if (list.Count > 0)
		{
			num2 = list[PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0, list.Count)];
		}
		if (num2 != -1 && num != -1)
		{
			unitCtrlr.SendInventoryChange(num2, num);
		}
		else
		{
			success = false;
		}
	}
}
