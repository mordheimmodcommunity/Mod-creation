using System.Collections.Generic;

public class TargetData
{
    public UnitController unitCtrlr;

    public List<BoneTargetRange> boneTargetRange;

    public List<BoneTargetRange> boneTargetRangeBlockingUnit;

    public TargetData(UnitController ctrlr)
    {
        unitCtrlr = ctrlr;
        boneTargetRange = new List<BoneTargetRange>();
        boneTargetRangeBlockingUnit = new List<BoneTargetRange>();
        for (int i = 0; i < ctrlr.boneTargets.Count; i++)
        {
            boneTargetRange.Add(new BoneTargetRange());
            boneTargetRangeBlockingUnit.Add(new BoneTargetRange());
        }
    }

    public float GetCover(bool blockingUnit)
    {
        int num = 0;
        List<BoneTargetRange> list = (!blockingUnit) ? boneTargetRange : boneTargetRangeBlockingUnit;
        for (int i = 0; i < list.Count; i++)
        {
            if (!list[i].hitBone)
            {
                num++;
            }
        }
        return (float)num / (float)list.Count;
    }
}
