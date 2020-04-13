using RAIN.Core;

public class AIHasSpells : AIBase
{
    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "HasSpells";
        success = false;
        int num = 0;
        while (true)
        {
            if (num < unitCtrlr.actionStatus.Count)
            {
                if (unitCtrlr.actionStatus[num].skillData.SkillTypeId == SkillTypeId.SPELL_ACTION)
                {
                    break;
                }
                num++;
                continue;
            }
            return;
        }
        success = true;
    }
}
