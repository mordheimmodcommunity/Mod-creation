using System.Collections.Generic;
using UnityEngine;

public class AlluressFight : MonoBehaviour, ICustomMissionSetup
{
	public CampaignUnitId firstAlluress;

	public CampaignUnitId secondAlluress;

	public List<SearchPoint> searchPoints;

	public List<DecisionPoint> spawnPoints;

	void ICustomMissionSetup.Execute()
	{
		for (int i = 0; i < PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs.Count; i++)
		{
			for (int j = 0; j < PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs[i].unitCtrlrs.Count; j++)
			{
				UnitController unitController = PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs[i].unitCtrlrs[j];
				if (unitController.unit.CampaignData != null)
				{
					if (unitController.unit.CampaignData.Id == firstAlluress)
					{
						unitController.unlockSearchPointOnDeath = true;
						unitController.linkedSearchPoints = searchPoints;
					}
					if (unitController.unit.CampaignData.Id == secondAlluress)
					{
						unitController.reviveUntilSearchEmpty = true;
						unitController.linkedSearchPoints = searchPoints;
						unitController.forcedSpawnPoints = spawnPoints;
					}
				}
			}
		}
	}
}
