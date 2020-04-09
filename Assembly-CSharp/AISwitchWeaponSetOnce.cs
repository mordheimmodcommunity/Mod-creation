using RAIN.Core;

public class AISwitchWeaponSetOnce : AISwitchWeaponSet
{
	public override void Start(AI ai)
	{
		base.Start(ai);
		base.actionName = "SwitchWeaponSetOnce";
		success &= (unitCtrlr.AICtrlr.switchCount == 0);
	}
}
