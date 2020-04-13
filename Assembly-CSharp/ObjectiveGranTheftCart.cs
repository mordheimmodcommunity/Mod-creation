using System;

public class ObjectiveGranTheftCart : Objective
{
    private Item enemyIdol;

    public ObjectiveGranTheftCart(PrimaryObjectiveId id, WarbandController warCtrlr, WarbandId enemyWarbandId, WarbandController enemyWarCtrlr)
        : base(id)
    {
        enemyIdol = enemyWarCtrlr.ItemIdol;
        itemsToSteal.Add(enemyIdol);
        searchToCheck.Add(warCtrlr.wagon.chest);
        counter.y = 1f;
    }

    public ObjectiveGranTheftCart(PrimaryObjectiveId id, WarbandId enemyWarbandId)
        : base(id)
    {
        WarbandData warbandData = PandoraSingleton<DataFactory>.Instance.InitData<WarbandData>((int)enemyWarbandId);
        enemyIdol = new Item(warbandData.ItemIdIdol);
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
