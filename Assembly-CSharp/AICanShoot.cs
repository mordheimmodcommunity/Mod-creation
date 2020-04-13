using RAIN.Core;

public class AICanShoot : AIBase
{
    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "CanShoot";
        success = false;
        int num = 0;
        while (true)
        {
            if (num < unitCtrlr.actionStatus.Count)
            {
                if (unitCtrlr.actionStatus[num].IsShootAction() && unitCtrlr.actionStatus[num].Available)
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
