public class UnitMovement : ICheapState
{
    private MissionManager missionMngr;

    public UnitMovement(MissionManager mission)
    {
        missionMngr = mission;
    }

    void ICheapState.Destroy()
    {
        missionMngr = null;
    }

    void ICheapState.Enter(int iFrom)
    {
        PandoraDebug.LogInfo("MissionManager UnitMovement", "SUBFLOW");
    }

    void ICheapState.Exit(int iTo)
    {
    }

    void ICheapState.Update()
    {
        missionMngr.CheckUnitTurnFinished();
    }

    void ICheapState.FixedUpdate()
    {
    }
}
