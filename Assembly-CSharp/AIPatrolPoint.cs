using Pathfinding;
using RAIN.Action;
using RAIN.Core;

public class AIPatrolPoint : AIBase
{
	private Path path;

	private bool searching;

	public override void Start(AI ai)
	{
		base.Start(ai);
		base.actionName = "PatrolPoint";
		searching = true;
		unitCtrlr.ReduceAlliesNavCutterSize(delegate
		{
			float num = unitCtrlr.unit.Movement * unitCtrlr.unit.CurrentStrategyPoints;
			int length = (int)PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(unitCtrlr.unit.Movement, num);
			RandomPath p = RandomPath.Construct(unitCtrlr.transform.position, length, OnPathFinish);
			PandoraSingleton<MissionManager>.Instance.PathSeeker.StartPath(p, OnPathFinish, 1);
		});
	}

	public override ActionResult Execute(AI ai)
	{
		if (!searching)
		{
			if (!success)
			{
				return (ActionResult)2;
			}
			return (ActionResult)0;
		}
		return (ActionResult)1;
	}

	public override void Stop(AI ai)
	{
		base.Stop(ai);
	}

	private void OnPathFinish(Path p)
	{
		searching = false;
		success = (p != null && p.GetTotalLength() > 0f);
		path = p;
	}
}
