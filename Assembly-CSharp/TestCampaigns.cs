using UnityEngine;

public class TestCampaigns : MonoBehaviour
{
	private int maxMission = 8;

	private void OnGUI()
	{
		for (int i = 1; i <= maxMission; i++)
		{
			if (GUI.Button(new Rect(Screen.width - 230, Screen.height - (maxMission + 1 - i) * 35, 220f, 22f), "Mission " + i))
			{
				LaunchMission(i);
			}
		}
	}

	private void LaunchMission(int idx)
	{
		if (!PandoraSingleton<MissionStartData>.Exists())
		{
			GameObject gameObject = new GameObject("mission_start_data");
			gameObject.AddComponent<MissionStartData>();
		}
		PandoraSingleton<MissionStartData>.Instance.ResetSeed();
		PandoraSingleton<PandoraInput>.Instance.SetActive(active: false);
		StartCoroutine(PandoraSingleton<MissionStartData>.Instance.SetMissionFull(Mission.GenerateCampaignMission(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.Id, idx), PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr, delegate
		{
			SceneLauncher.Instance.LaunchScene(SceneLoadingId.LAUNCH_MISSION_CAMPAIGN);
		}));
	}
}
