public class EndRound : ICheapState
{
    private MissionManager missionMngr;

    public EndRound(MissionManager mission)
    {
        missionMngr = mission;
    }

    void ICheapState.Destroy()
    {
    }

    void ICheapState.Enter(int iFrom)
    {
        PandoraDebug.LogInfo("EndTurnState = " + missionMngr.currentTurn, "FLOW");
        missionMngr.MoveCircle.Hide();
        missionMngr.ClearZoneAoes();
        missionMngr.CamManager.SwitchToCam(CameraManager.CameraType.FIXED, null, transition: false);
    }

    void ICheapState.Exit(int iTo)
    {
    }

    void ICheapState.Update()
    {
        missionMngr.currentTurn++;
        missionMngr.StateMachine.ChangeState(2);
    }

    void ICheapState.FixedUpdate()
    {
    }
}
