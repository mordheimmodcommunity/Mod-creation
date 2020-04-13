using RAIN.Core;
using System.Collections.Generic;
using UnityEngine;

public class AIUseBuff : AIBase
{
    private List<ActionStatus> buffs = new List<ActionStatus>();

    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "AIUseBuff";
        unitCtrlr.UpdateActionStatus(notice: false);
        buffs.Clear();
        for (int i = 0; i < unitCtrlr.actionStatus.Count; i++)
        {
            if (unitCtrlr.actionStatus[i].Available && unitCtrlr.actionStatus[i].skillData.EffectTypeId == EffectTypeId.BUFF && (unitCtrlr.actionStatus[i].ActionId == UnitActionId.SKILL || unitCtrlr.actionStatus[i].ActionId == UnitActionId.SPELL))
            {
                buffs.Add(unitCtrlr.actionStatus[i]);
            }
        }
        List<UnitController> aliveAllies = PandoraSingleton<MissionManager>.Instance.GetAliveAllies(unitCtrlr.GetWarband().idx);
        for (int num = buffs.Count - 1; num >= 0; num--)
        {
            if (buffs[num].TargetingId != TargetingId.SINGLE_TARGET)
            {
                bool flag = false;
                int num2 = 0;
                while (!flag && num2 < aliveAllies.Count)
                {
                    float num3 = buffs[num].RangeMax + buffs[num].Radius;
                    num3 *= num3;
                    if (Vector3.SqrMagnitude(aliveAllies[num2].transform.position - unitCtrlr.transform.position) < num3)
                    {
                        flag = true;
                    }
                    num2++;
                }
                if (!flag)
                {
                    buffs.RemoveAt(num);
                }
            }
        }
    }
}
