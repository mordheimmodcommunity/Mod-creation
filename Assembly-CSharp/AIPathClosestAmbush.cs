public class AIPathClosestAmbush : AIPathDecisionBase
{
	protected override DecisionPointId GetDecisionId()
	{
		return DecisionPointId.AMBUSH;
	}
}
