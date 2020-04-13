using System.Collections.Generic;

public class StartGame : ICheapState
{
    private MissionManager missionMngr;

    public StartGame(MissionManager mission)
    {
        missionMngr = mission;
    }

    void ICheapState.Destroy()
    {
    }

    void ICheapState.Enter(int iFrom)
    {
        for (int i = 0; i < missionMngr.WarbandCtrlrs.Count; i++)
        {
            List<UnitController> unitCtrlrs = missionMngr.WarbandCtrlrs[i].unitCtrlrs;
            for (int j = 0; j < unitCtrlrs.Count; j++)
            {
                if (unitCtrlrs[j].unit.Status != UnitStateId.OUT_OF_ACTION)
                {
                    unitCtrlrs[j].StartGameInitialization();
                }
            }
        }
        missionMngr.TurnOffActionZones();
        if (!PandoraSingleton<MissionStartData>.Instance.isReload)
        {
            missionMngr.ResetLadderIdx(updateUI: false);
        }
        missionMngr.EndLoading();
    }

    void ICheapState.Exit(int iTo)
    {
    }

    void ICheapState.Update()
    {
    }

    void ICheapState.FixedUpdate()
    {
    }
}
