using System.Collections.Generic;

public class ConvertPoint : SearchPoint
{
	public ItemId convertedItemId;

	public int capacity;

	public List<Item> convertItems = new List<Item>();

	public List<Item> objectiveItems = new List<Item>();

	public override void Init(uint id)
	{
		base.Init(id);
		SetCapacity(capacity);
	}

	public void SetCapacity(int cap)
	{
		capacity = cap;
		convertItems.Clear();
		for (int i = 0; i < capacity; i++)
		{
			convertItems.Add(new Item(convertedItemId));
		}
	}

	protected override void OnItemSwitched(UnitController unitctrlr)
	{
		int num = 0;
		for (int i = 0; i < items.Count; i++)
		{
			Item item = items[i];
			if (item.Id == slots[i].restrictedItemId && capacity > 0)
			{
				capacity--;
				num++;
				items[i] = convertItems[0];
				convertItems.RemoveAt(0);
			}
		}
		if (num > 0)
		{
			unitctrlr.GetWarband().ConvertItem(convertedItemId, num);
		}
		SpawnFxs(activated: true);
	}

	public override bool IsEmpty()
	{
		return capacity == 0 && base.IsEmpty();
	}

	public override void Refresh()
	{
		base.Refresh();
		for (int i = 0; i < triggers.Count; i++)
		{
			if (triggers[i] != null)
			{
				triggers[i].SetActive(capacity > 0);
			}
		}
	}

	protected override bool CanInteract(UnitController unitCtrlr)
	{
		return (capacity > 0 || !IsEmpty()) && base.CanInteract(unitCtrlr);
	}

	public override List<Item> GetObjectiveItems()
	{
		objectiveItems.Clear();
		objectiveItems.AddRange(convertItems);
		objectiveItems.AddRange(items);
		return objectiveItems;
	}

	public override void Close(bool force = false)
	{
		base.Close();
		PandoraSingleton<MissionManager>.Instance.MissionEndData.UpdateConverters(guid, capacity);
	}
}
