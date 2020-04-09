using System.Collections.Generic;
using UnityEngine;
using WellFired;

public class SequenceHelper : PandoraSingleton<SequenceHelper>
{
	public UnitController currentUnit;

	public UnitController defender;

	public bool refresh = true;

	public List<ActionZoneSeqHelper> zoneHelpers;

	private List<UnitController> unitCtrlrs;

	public ActionZoneSeqHelper defaultZoneHelper;

	public bool gui;

	private List<USSequencer> sequences;

	private Vector2 scroller = default(Vector2);

	private void Start()
	{
		GameObject gameObject = GameObject.Find("units");
		unitCtrlrs = new List<UnitController>();
		unitCtrlrs.AddRange(gameObject.GetComponentsInChildren<UnitController>());
		PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.BASE_NOTICE, BaseNotice);
		Refresh();
		Object.Destroy(PandoraSingleton<MissionManager>.Instance.CamManager.GetComponent<Animator>());
	}

	private void Update()
	{
		if (PandoraSingleton<MissionManager>.Instance.CamManager.GetComponent<Camera>().enabled)
		{
			if (Input.GetKeyDown(KeyCode.Backspace))
			{
				PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.SEQUENCE_ENDED);
			}
			if (refresh)
			{
				Refresh();
			}
		}
	}

	public ActionZoneSeqHelper GetActionZoneHelper(UnitActionId id)
	{
		foreach (ActionZoneSeqHelper zoneHelper in zoneHelpers)
		{
			if (zoneHelper.actionId == id)
			{
				return zoneHelper;
			}
		}
		return defaultZoneHelper;
	}

	private void BaseNotice()
	{
		string str = (string)PandoraSingleton<NoticeManager>.Instance.Parameters[0];
		bool flag = (bool)PandoraSingleton<NoticeManager>.Instance.Parameters[1];
		Debug.Log("Notice Received : " + str + ((!flag) ? " allied " : " enemy "));
	}

	private void InitWarbands()
	{
		if (PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs.Count == 0)
		{
			MissionWarbandSave warband = new MissionWarbandSave(WarbandId.HUMAN_MERCENARIES, CampaignWarbandId.NONE, "war1", string.Empty, "cpu", 0, PandoraSingleton<Hermes>.Instance.PlayerIndex, 0, PlayerTypeId.PLAYER, null);
			WarbandController warbandController = new WarbandController(warband, DeploymentId.WAGON, 0, 0, PrimaryObjectiveTypeId.NONE, 0, 0);
			warbandController.playerIdx = PandoraSingleton<Hermes>.Instance.PlayerIndex;
			PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs.Add(warbandController);
			warbandController = new WarbandController(warband, DeploymentId.WAGON, 1, 1, PrimaryObjectiveTypeId.NONE, 0, 0);
			warbandController.playerIdx++;
			PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs.Add(warbandController);
		}
		PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs[0].unitCtrlrs.Clear();
		PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs[1].unitCtrlrs.Clear();
	}

	private void Refresh()
	{
		refresh = false;
		InitWarbands();
		foreach (UnitController unitCtrlr in unitCtrlrs)
		{
			if (unitCtrlr == currentUnit)
			{
				unitCtrlr.gameObject.SetActive(value: true);
				unitCtrlr.transform.position = Vector3.zero;
				unitCtrlr.transform.rotation = Quaternion.identity;
				PandoraSingleton<MissionManager>.Instance.InitiativeLadder.Clear();
				PandoraSingleton<MissionManager>.Instance.InitiativeLadder.Add(unitCtrlr);
				PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs[0].unitCtrlrs.Add(unitCtrlr);
				PandoraSingleton<MissionManager>.Instance.ForceFocusedUnit(unitCtrlr);
			}
		}
		foreach (UnitController unitCtrlr2 in unitCtrlrs)
		{
			if (unitCtrlr2 == defender)
			{
				unitCtrlr2.gameObject.SetActive(value: true);
				unitCtrlr2.transform.position = Vector3.forward * 5f;
				unitCtrlr2.transform.rotation = Quaternion.Euler(Vector3.up * -180f);
				PandoraSingleton<MissionManager>.Instance.GetCurrentUnit().defenderCtrlr = unitCtrlr2;
				unitCtrlr2.attackerCtrlr = PandoraSingleton<MissionManager>.Instance.GetCurrentUnit();
				PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs[1].unitCtrlrs.Add(unitCtrlr2);
			}
		}
		foreach (UnitController unitCtrlr3 in unitCtrlrs)
		{
			if (unitCtrlr3 != currentUnit && unitCtrlr3 != defender)
			{
				unitCtrlr3.gameObject.SetActive(value: false);
			}
		}
	}
}
