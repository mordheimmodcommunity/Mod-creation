using System;

public class ObjectiveProtectIdol : Objective
{
    private Item myIdol;

    public ObjectiveProtectIdol(PrimaryObjectiveId id, WarbandController warCtrlr)
        : base(id)
    {
        myIdol = warCtrlr.ItemIdol;
        itemsToSteal.Add(myIdol);
        searchToCheck.Add(warCtrlr.wagon.idol);
        counter.y = 1f;
        desc = PandoraSingleton<LocalizationManager>.Instance.GetStringById(base.DescKey, myIdol.Name);
    }

    public ObjectiveProtectIdol(PrimaryObjectiveId id, WarbandId enemyWarbandId)
        : base(id)
    {
        WarbandData warbandData = PandoraSingleton<DataFactory>.Instance.InitData<WarbandData>((int)enemyWarbandId);
        myIdol = new Item(warbandData.ItemIdIdol);
        desc = PandoraSingleton<LocalizationManager>.Instance.GetStringById(base.DescKey, myIdol.Name);
    }

    protected override void Track(ref bool objectivesChanged)
    {
        CheckItemsToSteal(ref objectivesChanged);
    }

    public override void Reload(uint trackedUid)
    {
        throw new NotImplementedException();
    }
}
