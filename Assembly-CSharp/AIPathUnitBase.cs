using RAIN.Action;
using RAIN.Core;
using RAIN.Representation;
using System.Collections.Generic;

public abstract class AIPathUnitBase : AIBase
{
    private ActionResult currentResult;

    public Expression useOnlySpottedEnemies = (Expression)(object)new Expression();

    public Expression getOnlyReachables = (Expression)(object)new Expression();

    private bool useSpotted;

    private bool keepOnlyReachable;

    protected List<UnitController> targets = new List<UnitController>();

    public override void Start(AI ai)
    {
        //IL_0030: Unknown result type (might be due to invalid IL or missing references)
        //IL_00d6: Unknown result type (might be due to invalid IL or missing references)
        //IL_0111: Unknown result type (might be due to invalid IL or missing references)
        base.Start(ai);
        base.actionName = "PathUnit";
        if (unitCtrlr.unit.CurrentStrategyPoints == 0)
        {
            success = false;
            currentResult = (ActionResult)2;
            return;
        }
        useSpotted = useOnlySpottedEnemies.get_IsValid();
        keepOnlyReachable = getOnlyReachables.get_IsValid();
        List<UnitController> list = (!useSpotted) ? GetUnitsPool() : unitCtrlr.GetWarband().SquadManager.GetSpottedEnemies();
        if (!keepOnlyReachable)
        {
            targets.Clear();
            SetTargets(list);
        }
        else
        {
            targets = list;
        }
        unitCtrlr.AICtrlr.RemoveInactive(targets);
        if (targets.Count > 0)
        {
            currentResult = (ActionResult)1;
            unitCtrlr.AICtrlr.FindPath(targets, OnUnitsChecked, keepOnlyReachable);
        }
        else
        {
            success = false;
            currentResult = (ActionResult)2;
        }
    }

    public override ActionResult Execute(AI ai)
    {
        //IL_0001: Unknown result type (might be due to invalid IL or missing references)
        return currentResult;
    }

    public override void Stop(AI ai)
    {
        base.Stop(ai);
    }

    protected abstract bool CheckAllies();

    private List<UnitController> GetUnitsPool()
    {
        if (CheckAllies())
        {
            return PandoraSingleton<MissionManager>.Instance.GetAliveAllies(unitCtrlr.GetWarband().idx);
        }
        return PandoraSingleton<MissionManager>.Instance.GetAliveEnemies(unitCtrlr.GetWarband().idx);
    }

    protected abstract void SetTargets(List<UnitController> unitsToCheck);

    private void OnUnitsChecked(bool pathSuccess)
    {
        //IL_016b: Unknown result type (might be due to invalid IL or missing references)
        pathSuccess &= (unitCtrlr.AICtrlr.currentPath != null);
        success = pathSuccess;
        if (success)
        {
            if (keepOnlyReachable)
            {
                unitCtrlr.AICtrlr.targetEnemy = null;
                if (unitCtrlr.AICtrlr.reachableUnits.Count > 0)
                {
                    unitCtrlr.AICtrlr.targetEnemy = null;
                    targets.Clear();
                    SetTargets(unitCtrlr.AICtrlr.reachableUnits);
                    if (targets.Count > 0)
                    {
                        unitCtrlr.AICtrlr.targetEnemy = targets[PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0, targets.Count)];
                        unitCtrlr.AICtrlr.currentPath = unitCtrlr.AICtrlr.reachableUnitsPaths[unitCtrlr.AICtrlr.targetEnemy];
                    }
                }
                success = (unitCtrlr.AICtrlr.targetEnemy != null);
            }
            unitCtrlr.AICtrlr.Squad.SetTarget(unitCtrlr.AICtrlr.targetEnemy, useSpotted);
        }
        currentResult = (ActionResult)((!success) ? 2 : 0);
    }
}
