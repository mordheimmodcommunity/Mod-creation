using System.Collections.Generic;

public class CompareItem : IComparer<Item>
{
    int IComparer<Item>.Compare(Item x, Item y)
    {
        if (x.QualityData.Id > y.QualityData.Id)
        {
            return -1;
        }
        if (x.QualityData.Id < y.QualityData.Id)
        {
            return 1;
        }
        if (x.Save.runeMarkQualityId > y.Save.runeMarkQualityId)
        {
            return -1;
        }
        if (x.Save.runeMarkQualityId < y.Save.runeMarkQualityId)
        {
            return 1;
        }
        if (x.PriceBuy > y.PriceBuy)
        {
            return -1;
        }
        if (x.PriceBuy < y.PriceBuy)
        {
            return 1;
        }
        return string.Compare(x.Name, y.Name);
    }
}
