using UnityEngine;

public class ManticoreFight : MonoBehaviour, ICustomMissionSetup
{
    public FlyPoint spawnPoint;

    void ICustomMissionSetup.Execute()
    {
        for (int i = 0; i < PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs.Count; i++)
        {
            for (int j = 0; j < PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs[i].unitCtrlrs.Count; j++)
            {
                UnitController unitController = PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs[i].unitCtrlrs[j];
                if (unitController.unit.Id == UnitId.MANTICORE && unitController.AICtrlr != null)
                {
                    unitController.AICtrlr.targetDecisionPoint = spawnPoint;
                    return;
                }
            }
        }
    }
}
