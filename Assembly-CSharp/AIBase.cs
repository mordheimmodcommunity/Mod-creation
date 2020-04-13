using RAIN.Action;
using RAIN.Core;

[RAINAction]
public class AIBase : RAINAction
{
    protected UnitController unitCtrlr;

    protected bool success;

    public AIBase()
        : this()
    {
    }

    public override void Start(AI ai)
    {
        base.actionName = "Base";
        if (unitCtrlr == null)
        {
            unitCtrlr = ai.get_Body().GetComponent<UnitController>();
        }
        success = true;
        PandoraDebug.LogInfo("START " + unitCtrlr.name + " : " + base.GetType().Name, "AI", unitCtrlr);
        ((RAINAction)this).Start(ai);
    }

    public override ActionResult Execute(AI ai)
    {
        return (ActionResult)((!success) ? 2 : 0);
    }

    public override void Stop(AI ai)
    {
        PandoraDebug.LogInfo(unitCtrlr.name + " : " + base.actionName + " " + (object)(ActionResult)((!success) ? 2 : 0), "AI", unitCtrlr);
        ((RAINAction)this).Stop(ai);
    }
}
