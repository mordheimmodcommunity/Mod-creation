using RAIN.Core;
using System.Collections.Generic;

public class AIGatherWeapons : AIGatherLoot
{
    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "GatherWyrdstones";
    }

    protected override int GetSearchSlot(List<Item> searchItems)
    {
        int num = -1;
        int num2 = 0;
        for (int i = 0; i < searchItems.Count; i++)
        {
            if (searchItems[i].Id != 0 && searchItems[i].TypeData.ItemCategoryId == ItemCategoryId.WEAPONS)
            {
                int rating = searchItems[i].GetRating();
                if (num == -1 || rating > num2)
                {
                    num = i;
                    num2 = rating;
                }
            }
        }
        return num;
    }
}
