public class Delay : ICheapState
{
    private UnitController unitCtrlr;

    public Delay(UnitController ctrlr)
    {
        unitCtrlr = ctrlr;
    }

    void ICheapState.Destroy()
    {
        unitCtrlr = null;
    }

    void ICheapState.Enter(int iFrom)
    {
        unitCtrlr.currentActionData.SetAction(unitCtrlr.CurrentAction);
        unitCtrlr.SetFixed(fix: true);
        unitCtrlr.SetGraphWalkability(walkable: false);
        PandoraSingleton<MissionManager>.Instance.PlaySequence("skill", unitCtrlr, OnSeqDone);
        unitCtrlr.lastTimer = PandoraSingleton<MissionManager>.Instance.TurnTimer.Timer;
    }

    void ICheapState.Exit(int iTo)
    {
        PandoraSingleton<MissionManager>.Instance.MissionEndData.UpdateUnit(unitCtrlr);
    }

    void ICheapState.Update()
    {
    }

    void ICheapState.FixedUpdate()
    {
    }

    private void OnSeqDone()
    {
        unitCtrlr.StateMachine.ChangeState(9);
        PandoraSingleton<MissionManager>.Instance.SendUnitBack(Constant.GetInt(ConstantId.DELAY_POSITIONS));
    }
}
