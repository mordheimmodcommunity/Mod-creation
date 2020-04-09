using Pathfinding;
using Prometheus;
using System.Collections.Generic;
using UnityEngine;

public class SearchPoint : InteractivePoint
{
	public bool hideVisualOnFill;

	public bool destroyOnFull;

	public bool destroyOnEmpty;

	public bool destroyOnlyTriggers;

	public bool lockOnDrop;

	public bool isWyrdstone;

	public bool useAltTriggerOnFull;

	public bool useAltTriggerOnEmpty;

	public bool activateZoneAoeOnFull;

	public bool activateZoneAoeOnEmpty;

	public bool curseOnFull;

	public bool curseOnEmpty;

	public bool curseOnEnemyTeam;

	public bool spawnOnFull;

	public bool spawnOnEmpty;

	[HideInInspector]
	public bool hasIdol;

	[HideInInspector]
	public bool wasSearched;

	public List<SearchSlotData> slots;

	public List<GameObject> contentVisuals;

	public List<OlympusFireStarter> contentSpawnFx;

	private List<Dissolver> contentDissolvers;

	public bool playAllContentFx;

	[HideInInspector]
	public List<Item> items = new List<Item>();

	[HideInInspector]
	public UnitController unitController;

	[HideInInspector]
	public WarbandController warbandController;

	public string loc_name;

	private List<ItemId> restrictedIds;

	private Animation animComponent;

	private NavmeshCut visualNavCutter;

	private List<NavmeshCut> contentCutters = new List<NavmeshCut>();

	private bool wasEmpty;

	private bool wasFull;

	public bool IsOpened
	{
		get;
		private set;
	}

	public void Init(uint id, int capacity, bool wyrdStone = false)
	{
		slots = new List<SearchSlotData>();
		for (int i = 0; i < capacity; i++)
		{
			slots.Add(new SearchSlotData());
		}
		isWyrdstone = wyrdStone;
		Init(id);
	}

	public override void Init(uint id)
	{
		IsOpened = false;
		wasSearched = false;
		for (int i = 0; i < slots.Count; i++)
		{
			Item item = AddItem(slots[i].itemId, slots[i].itemQualityId, slots[i].runeMarkId, slots[i].runeMarkQualityId, slots[i].allegianceId);
			if (slots[i].itemId == ItemId.GOLD)
			{
				item.Save.amount = slots[i].value;
			}
		}
		contentDissolvers = new List<Dissolver>();
		for (int j = 0; j < contentVisuals.Count; j++)
		{
			if (contentVisuals[j] == null)
			{
				PandoraDebug.LogError("visual cannot be null", "SEARCH", this);
			}
			contentDissolvers.Add(contentVisuals[j].AddComponent<Dissolver>());
			contentDissolvers[j].dissolveSpeed = Mathf.Max(apparitionDelay, 0.5f);
			contentDissolvers[j].Hide(hide: true, force: true);
		}
		animComponent = GetComponent<Animation>();
		if (visual != null)
		{
			visualNavCutter = visual.GetComponentInChildren<NavmeshCut>();
			if (visualNavCutter != null)
			{
				visualNavCutter.ForceUpdate();
			}
		}
		for (int k = 0; k < contentVisuals.Count; k++)
		{
			contentCutters.Add(contentVisuals[k].GetComponent<NavmeshCut>());
			if (contentCutters[k] != null)
			{
				contentCutters[k].ForceUpdate();
			}
		}
		base.Init(id);
		if (isWyrdstone)
		{
			base.Imprint.imprintType = MapImprintType.WYRDSTONE;
		}
		Refresh();
	}

	public void InitInteraction()
	{
		wasEmpty = IsEmpty();
		wasFull = IsFull();
	}

	public void Open()
	{
		if (!IsOpened)
		{
			if (animComponent != null)
			{
				animComponent[animComponent.clip.name].speed = 1f;
				animComponent.Play(animComponent.clip.name);
			}
			SpawnFxs(activated: true);
			IsOpened = true;
		}
	}

	public virtual void Close(bool force = false)
	{
		SpawnFxs(activated: false);
		if (animComponent != null)
		{
			int num = CountItems();
			if (IsOpened && num != 0)
			{
				animComponent[animComponent.clip.name].speed = -1f;
				animComponent.Play(animComponent.clip.name);
			}
			else if (!IsOpened && CountItems() == 0)
			{
				animComponent[animComponent.clip.name].speed = 1f;
				animComponent.Play(animComponent.clip.name);
			}
		}
		IsOpened = false;
		if ((destroyOnEmpty && IsEmpty()) || (destroyOnFull && IsFull()))
		{
			DestroyVisual(destroyOnlyTriggers, force);
		}
		else if (lockOnDrop && IsFull() && (linkedPoint == null || linkedPoint.gameObject != base.gameObject) && (links.Count == 0 || links[0].gameObject != base.gameObject))
		{
			DestroyVisual(triggersOnly: true, force);
		}
		else
		{
			SetTriggerVisual();
		}
		List<ItemSave> list = new List<ItemSave>();
		for (int i = 0; i < items.Count; i++)
		{
			list.Add(items[i].Save);
		}
		PandoraSingleton<MissionManager>.Instance.MissionEndData.UpdateSearches(guid, (unitController != null) ? unitController.uid : 0u, base.transform.position, list, wasSearched);
	}

	public override void SetTriggerVisual()
	{
		bool flag = (useAltTriggerOnEmpty && IsEmpty()) || (useAltTriggerOnFull && IsFull());
		SetTriggerVisual(!flag);
	}

	public override void ActivateZoneAoe()
	{
		if ((activateZoneAoeOnEmpty && IsEmpty()) || (activateZoneAoeOnFull && IsFull()))
		{
			base.ActivateZoneAoe();
		}
	}

	public bool ShouldTriggerCurse()
	{
		return curseId != 0 && ((curseOnEmpty && !wasEmpty && IsEmpty()) || (curseOnFull && !wasFull && IsFull())) && (!curseOnEnemyTeam || (curseOnEnemyTeam && warbandController != null && warbandController.teamIdx != PandoraSingleton<MissionManager>.Instance.GetCurrentUnit().GetWarband().teamIdx));
	}

	public override UnitController SpawnCampaignUnit()
	{
		if ((spawnOnEmpty && IsEmpty()) || (spawnOnFull && IsFull()))
		{
			return base.SpawnCampaignUnit();
		}
		return null;
	}

	protected override bool LinkValid(UnitController unitCtrlr, bool reverseCondition)
	{
		return (!reverseCondition && IsFull()) || (reverseCondition && IsEmpty());
	}

	protected override bool CanInteract(UnitController unitCtrlr)
	{
		return (!lockOnDrop || (lockOnDrop && !IsFull())) && unitCtrlr.unit.Data.UnitSizeId != UnitSizeId.LARGE && !unitCtrlr.unit.BothArmsMutated() && CompliesToSlotRestrictions(unitCtrlr) && base.CanInteract(unitCtrlr);
	}

	public bool CompliesToSlotRestrictions(UnitController unitCtrlr)
	{
		return slots.Count == 0 || (slots[0].restrictedItemId == ItemId.NONE && slots[0].restrictedItemTypeId == ItemTypeId.NONE) || (slots[0].restrictedItemId != 0 && (!unitCtrlr.unit.IsInventoryFull() || unitCtrlr.unit.HasItem(slots[0].restrictedItemId))) || (slots[0].restrictedItemTypeId != 0 && (!unitCtrlr.unit.IsInventoryFull() || unitCtrlr.unit.HasItem(slots[0].restrictedItemTypeId)));
	}

	public Item AddItem(ItemSave itemSave)
	{
		return AddItem(new Item(itemSave));
	}

	public Item AddItem(ItemId itemId, ItemQualityId itemQualityId)
	{
		return AddItem(new Item(itemId, itemQualityId));
	}

	public Item AddItem(ItemId itemId, ItemQualityId itemQualityId, RuneMarkId runeMarkId, RuneMarkQualityId runeMarkQualityId, AllegianceId allegianceId, bool first = false)
	{
		Item item = new Item(itemId, itemQualityId);
		if (runeMarkId != 0)
		{
			item.AddRuneMark(runeMarkId, runeMarkQualityId, allegianceId);
		}
		return AddItem(item, first);
	}

	public Item AddItem(Item item, bool first = false)
	{
		if (item.IsStackable)
		{
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i].IsSame(item))
				{
					items[i].Save.amount += item.Save.amount;
					return items[i];
				}
			}
		}
		if (first)
		{
			items.Insert(0, item);
		}
		else
		{
			items.Add(item);
		}
		int num = items.Count - slots.Count;
		for (int j = 0; j < num; j++)
		{
			if (first)
			{
				slots.Insert(0, new SearchSlotData());
			}
			else
			{
				slots.Add(new SearchSlotData());
			}
		}
		return item;
	}

	public Item SetItem(ItemSave itemSave, int index)
	{
		return SetItem(new Item(itemSave), index);
	}

	public Item SetItem(int index, ItemId itemId, ItemQualityId itemQualityId = ItemQualityId.NORMAL, RuneMarkId runeMarkId = RuneMarkId.NONE, RuneMarkQualityId runeMarkQualityId = RuneMarkQualityId.NONE, AllegianceId allegianceId = AllegianceId.NONE)
	{
		Item item = new Item(itemId, itemQualityId);
		if (runeMarkId != 0)
		{
			item.AddRuneMark(runeMarkId, runeMarkQualityId, allegianceId);
		}
		items[index] = item;
		return item;
	}

	private Item SetItem(Item item, int index)
	{
		items[index] = item;
		return item;
	}

	public bool Contains(Item item)
	{
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i] == item)
			{
				return true;
			}
		}
		return false;
	}

	public List<Item> GetItems()
	{
		return items;
	}

	public virtual List<Item> GetObjectiveItems()
	{
		return items;
	}

	public virtual Item SwitchItem(UnitController unitCtrlr, int index, Item switchItem = null)
	{
		if (switchItem == null || (switchItem.IsStackable && switchItem.Id == items[index].Id))
		{
			switchItem = new Item(ItemId.NONE);
		}
		Item result;
		if (CanSwitchItem(index, switchItem))
		{
			result = items[index];
			items[index] = switchItem;
			OnItemSwitched(unitCtrlr);
			Refresh();
		}
		else
		{
			result = switchItem;
		}
		SortItems();
		if (hasIdol)
		{
			hasIdol = false;
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i].IsIdol)
				{
					AddIdolImprint(items[i]);
					break;
				}
			}
			if (!hasIdol)
			{
				RemoveIdolImprint();
			}
		}
		return result;
	}

	public void SortItems()
	{
		Item.SortEmptyItems(items, 0);
	}

	public void AddIdolImprint(Item idol)
	{
		hasIdol = true;
		if (base.Imprint == null && warbandController != null)
		{
			warbandController.wagon.mapImprint.idolTexture = idol.GetIcon();
		}
		else if (base.Imprint != null)
		{
			base.Imprint.idolTexture = idol.GetIcon();
		}
	}

	public void RemoveIdolImprint()
	{
		hasIdol = true;
		if (base.Imprint == null && warbandController != null)
		{
			warbandController.wagon.mapImprint.idolTexture = null;
		}
		else if (base.Imprint != null)
		{
			base.Imprint.idolTexture = null;
		}
	}

	public bool CanSwitchItem(int index, Item switchItem)
	{
		return !switchItem.IsUndroppable && (!lockOnDrop || (lockOnDrop && items[index].Id == ItemId.NONE)) && (switchItem.Id == ItemId.NONE || ((slots[index].restrictedItemId == ItemId.NONE || switchItem.Id == slots[index].restrictedItemId) && (slots[index].restrictedItemTypeId == ItemTypeId.NONE || switchItem.TypeData.Id == slots[index].restrictedItemTypeId)));
	}

	protected virtual void OnItemSwitched(UnitController unitCtrlr)
	{
	}

	public virtual void Refresh()
	{
		int num = CountItems();
		for (int i = 0; i < contentVisuals.Count; i++)
		{
			if (i < contentSpawnFx.Count && !contentDissolvers[i].Dissolving && !contentDissolvers[i].Ressolved && (playAllContentFx || i < num))
			{
				PandoraSingleton<Prometheus.Prometheus>.Instance.SpawnFx(contentSpawnFx[i], null);
			}
			contentVisuals[i].SetActive(value: true);
			contentDissolvers[i].Hide(i + 1 > num, force: false, OnContentDissolved);
		}
		if (visualDissolver != null && visualDissolver.gameObject.activeInHierarchy)
		{
			visualDissolver.Hide(hideVisualOnFill && num != 0, force: false, OnVisualDissolved);
		}
	}

	private void OnContentDissolved()
	{
		for (int i = 0; i < contentDissolvers.Count; i++)
		{
			if (!contentDissolvers[i].Dissolving)
			{
				if (contentCutters[i] != null)
				{
					contentCutters[i].enabled = !contentDissolvers[i].Dissolved;
				}
				contentVisuals[i].SetActive(!contentDissolvers[i].Dissolved);
			}
		}
	}

	private void OnVisualDissolved()
	{
		if (visualNavCutter != null)
		{
			visualNavCutter.enabled = !visualDissolver.Dissolved;
		}
		visual.SetActive(!visualDissolver.Dissolved);
	}

	public virtual bool IsEmpty()
	{
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].Id != 0)
			{
				return false;
			}
		}
		return true;
	}

	public bool IsFull()
	{
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].Id == ItemId.NONE)
			{
				return false;
			}
		}
		return true;
	}

	public int CountItems()
	{
		int num = 0;
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].Id != 0)
			{
				num++;
			}
		}
		return num;
	}

	public List<Item> GetItemsAndClear()
	{
		List<Item> list = new List<Item>();
		for (int num = items.Count - 1; num >= 0; num--)
		{
			if (items[num].Id != 0)
			{
				list.Add(items[num]);
				items[num] = new Item(ItemId.NONE);
			}
		}
		return list;
	}

	public int GetEmptySlot()
	{
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].Id == ItemId.NONE)
			{
				return i;
			}
		}
		return -1;
	}

	public override string GetLocAction()
	{
		UnitController currentUnit = PandoraSingleton<MissionManager>.Instance.GetCurrentUnit();
		if (!string.IsNullOrEmpty(loc_action_enemy) && currentUnit != null && ((unitController != null && unitController.GetWarband().teamIdx != currentUnit.GetWarband().teamIdx) || (warbandController != null && warbandController.teamIdx != currentUnit.GetWarband().teamIdx)))
		{
			return loc_action_enemy;
		}
		string locAction = base.GetLocAction();
		return (!string.IsNullOrEmpty(locAction)) ? locAction : "action_name_search";
	}

	public bool HasRequiredItem()
	{
		for (int i = 0; i < slots.Count; i++)
		{
			if (slots[i].restrictedItemId != 0)
			{
				return true;
			}
		}
		return false;
	}

	public List<ItemId> GetRestrictedItemIds()
	{
		if (restrictedIds == null)
		{
			restrictedIds = new List<ItemId>();
			for (int i = 0; i < slots.Count; i++)
			{
				restrictedIds.Add(slots[i].restrictedItemId);
			}
		}
		return restrictedIds;
	}
}
