using RAIN.Action;
using RAIN.Core;
using RAIN.Representation;
using UnityEngine;

public class AILogAction : AIBase
{
	public Expression message = (Expression)(object)new Expression();

	public override void Start(AI ai)
	{
		base.Start(ai);
		base.actionName = "LogAction";
	}

	public override ActionResult Execute(AI ai)
	{
		Debug.Log("[BT] Debug action : " + message);
		return (ActionResult)0;
	}
}
