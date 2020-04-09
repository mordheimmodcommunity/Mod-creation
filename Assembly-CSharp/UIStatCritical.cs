public class UIStatCritical : UIStat
{
	public override void RefreshAttribute(Unit unit)
	{
		statId = ((!unit.HasRange()) ? AttributeId.CRITICAL_MELEE_ATTEMPT_ROLL : AttributeId.CRITICAL_RANGE_ATTEMPT_ROLL);
		base.RefreshAttribute(unit);
	}
}
