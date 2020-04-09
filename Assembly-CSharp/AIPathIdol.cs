using RAIN.Action;
using RAIN.Core;
using System.Collections.Generic;

public class AIPathIdol : AIBase
{
	private ActionResult currentResult;

	private List<Destructible> targets = new List<Destructible>();

	public override void Start(AI ai)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		base.Start(ai);
		base.actionName = "PathIdol";
		SetTargets();
		if (targets.Count > 0)
		{
			currentResult = (ActionResult)1;
			unitCtrlr.AICtrlr.FindPath(targets, OnDestructiblesChecked);
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

	private void SetTargets()
	{
		targets.Clear();
		int teamIdx = unitCtrlr.GetWarband().teamIdx;
		for (int i = 0; i < PandoraSingleton<MissionManager>.Instance.MapImprints.Count; i++)
		{
			Destructible destructible = PandoraSingleton<MissionManager>.Instance.MapImprints[i].Destructible;
			if (destructible != null && destructible.Owner != null && destructible.Owner.GetWarband().teamIdx != teamIdx)
			{
				targets.Add(destructible);
			}
		}
	}

	private void OnDestructiblesChecked(bool foundPath)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		foundPath &= (unitCtrlr.AICtrlr.currentPath != null);
		success = foundPath;
		currentResult = (ActionResult)((!success) ? 2 : 0);
	}
}
