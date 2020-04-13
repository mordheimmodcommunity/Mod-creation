using System.Collections.Generic;

public class ProcWarbandRankJoinUnitTypeDataSorter : IComparer<ProcWarbandRankJoinUnitTypeData>
{
    int IComparer<ProcWarbandRankJoinUnitTypeData>.Compare(ProcWarbandRankJoinUnitTypeData x, ProcWarbandRankJoinUnitTypeData y)
    {
        int rating = PandoraSingleton<DataFactory>.Instance.InitData<UnitTypeData>((int)x.UnitTypeId).Rating;
        int rating2 = PandoraSingleton<DataFactory>.Instance.InitData<UnitTypeData>((int)y.UnitTypeId).Rating;
        if (rating == rating2)
        {
            return 0;
        }
        if (rating < rating2)
        {
            return 1;
        }
        if (rating > rating2)
        {
            return -1;
        }
        return 0;
    }
}
