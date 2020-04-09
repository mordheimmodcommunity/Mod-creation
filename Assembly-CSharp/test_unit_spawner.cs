using System.Collections.Generic;
using UnityEngine;

public class test_unit_spawner : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.F12))
		{
			UnitController currentUnit = PandoraSingleton<MissionManager>.Instance.GetCurrentUnit();
			Debug.Log(currentUnit.unit.GetRating());
			StartCoroutine(PandoraSingleton<UnitFactory>.Instance.CloneUnitCtrlr(currentUnit, currentUnit.unit.Rank, currentUnit.unit.GetRating(), Vector3.zero, Quaternion.identity));
			List<UnitController> allUnitsList = PandoraSingleton<MissionManager>.Instance.allUnitsList;
			for (int i = 0; i < allUnitsList.Count; i++)
			{
				allUnitsList[i].InitTargetsData();
			}
		}
	}
}
