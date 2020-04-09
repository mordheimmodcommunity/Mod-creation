public class AIPathClosestOverwatch : AIPathDecisionBase
{
	protected override DecisionPointId GetDecisionId()
	{
		return DecisionPointId.OVERWATCH;
	}
}
