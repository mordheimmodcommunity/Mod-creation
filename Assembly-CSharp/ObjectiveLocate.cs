using System.Collections.Generic;
using UnityEngine;

public class ObjectiveLocate : Objective
{
    private List<Item> locatedItems;

    private List<LocateZone> locateZones;

    private PrimaryObjectiveLocateData locateData;

    public ObjectiveLocate(PrimaryObjectiveId id)
        : base(id)
    {
        DataFactory instance = PandoraSingleton<DataFactory>.Instance;
        int num = (int)id;
        locateData = instance.InitData<PrimaryObjectiveLocateData>("fk_primary_objective_id", num.ToString())[0];
        locatedItems = new List<Item>();
        if (locateData.ItemId != 0)
        {
            counter = new Vector2(0f, locateData.ItemCount);
            desc = PandoraSingleton<LocalizationManager>.Instance.GetStringById(base.DescKey, PandoraSingleton<LocalizationManager>.Instance.GetStringById(locateData.ItemId.ToLowerString()));
        }
        else
        {
            locateZones = PandoraSingleton<MissionManager>.Instance.GetLocateZones(locateData.Zone);
            counter = new Vector2(0f, locateData.ZoneCount);
            desc = PandoraSingleton<LocalizationManager>.Instance.GetStringById(base.DescKey, PandoraSingleton<LocalizationManager>.Instance.GetStringById(locateData.Zone));
        }
    }

    public override void SetLocked(bool loc)
    {
        bool locked = base.Locked;
        base.SetLocked(loc);
        if (locked == loc || loc || locateZones == null)
        {
            return;
        }
        List<UnitController> myAliveUnits = PandoraSingleton<MissionManager>.Instance.GetMyAliveUnits();
        for (int i = 0; i < locateZones.Count; i++)
        {
            for (int j = 0; j < myAliveUnits.Count; j++)
            {
                if (locateZones[i].ColliderBounds.Contains(myAliveUnits[j].transform.position))
                {
                    PandoraSingleton<MissionManager>.Instance.GetMyWarbandCtrlr().LocateZone(locateZones[i]);
                    break;
                }
            }
        }
    }

    protected override void Track(ref bool objectivesChanged)
    {
        if (locateData.ItemId != 0)
        {
            counter.x = locatedItems.Count;
        }
    }

    public void UpdateLocatedItems(List<Item> items)
    {
        if (counter.x == counter.y)
        {
            return;
        }
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].Id == locateData.ItemId && locatedItems.IndexOf(items[i]) == -1)
            {
                locatedItems.Add(items[i]);
                PandoraSingleton<MissionManager>.Instance.MissionEndData.UpdateObjective(guid, 0u);
            }
        }
    }

    public void UpdateLocatedZone(LocateZone zone, bool checkEndGame = true)
    {
        if (!(zone.name == locateData.Zone))
        {
            return;
        }
        int num = locateZones.IndexOf(zone);
        if (num != -1)
        {
            counter.x += 1f;
            locateZones.RemoveAt(num);
            PandoraSingleton<MissionManager>.Instance.MissionEndData.UpdateObjective(guid, zone.guid);
            if (checkEndGame)
            {
                PandoraSingleton<MissionManager>.Instance.CheckEndGame();
            }
        }
    }

    public override void Reload(uint trackedUid)
    {
        if (locateData.ItemId != 0)
        {
            locatedItems.Add(new Item(ItemId.NONE));
            return;
        }
        List<LocateZone> list = PandoraSingleton<MissionManager>.Instance.GetLocateZones();
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].guid == trackedUid)
            {
                UpdateLocatedZone(list[i], checkEndGame: false);
            }
        }
    }
}
