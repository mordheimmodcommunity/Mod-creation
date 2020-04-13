using System.Collections.Generic;

public class Shrine : ActivatePoint
{
    public ShrineId shrineId;

    private ShrineData shrineData;

    private List<ShrineEnchantmentData> shrineEnchantments;

    public override void Init(uint id)
    {
        base.Init(id);
        reverse = true;
        shrineData = PandoraSingleton<DataFactory>.Instance.InitData<ShrineData>((int)shrineId);
        shrineEnchantments = PandoraSingleton<DataFactory>.Instance.InitData<ShrineEnchantmentData>("fk_shrine_id", ((int)shrineData.Id).ToString());
    }

    public override void Activate(UnitController unitCtrlr, bool force = false)
    {
        if (unitCtrlr == null)
        {
            return;
        }
        activated = false;
        base.Activate(unitCtrlr);
        int rank = unitCtrlr.GetWarband().Rank;
        unitCtrlr.unit.AddToAttribute(AttributeId.TOTAL_PRAY, 1);
        for (int i = 0; i < shrineEnchantments.Count; i++)
        {
            if (shrineEnchantments[i].WarbandRankId == (WarbandRankId)rank)
            {
                unitCtrlr.unit.AddEnchantment(shrineEnchantments[i].EnchantmentId, unitCtrlr.unit, original: false);
            }
        }
    }

    protected override bool LinkValid(UnitController unitCtrlr, bool reverseCondition)
    {
        return !CanInteract(unitCtrlr);
    }
}
