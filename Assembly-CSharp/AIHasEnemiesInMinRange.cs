using RAIN.Core;
using System.Collections.Generic;
using UnityEngine;

public class AIHasEnemiesInMinRange : AIBase
{
    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "AIHasEnemiesInMinRange";
        int num = 0;
        if (unitCtrlr.unit.Items[(int)unitCtrlr.unit.ActiveWeaponSlot].RangeMin != 0)
        {
            num = unitCtrlr.unit.Items[(int)unitCtrlr.unit.ActiveWeaponSlot].RangeMin;
        }
        else if (unitCtrlr.unit.Items[(int)unitCtrlr.unit.InactiveWeaponSlot].RangeMin != 0)
        {
            num = unitCtrlr.unit.Items[(int)unitCtrlr.unit.InactiveWeaponSlot].RangeMin;
        }
        success = false;
        if (num == 0)
        {
            return;
        }
        Vector3 position = unitCtrlr.transform.position;
        List<UnitController> spottedEnemies = unitCtrlr.GetWarband().SquadManager.GetSpottedEnemies();
        int num2 = 0;
        while (true)
        {
            if (num2 < spottedEnemies.Count)
            {
                if (Vector3.SqrMagnitude(spottedEnemies[num2].transform.position - position) <= (float)(num * num))
                {
                    break;
                }
                num2++;
                continue;
            }
            return;
        }
        success = true;
    }
}
