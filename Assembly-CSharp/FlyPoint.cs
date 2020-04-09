public class FlyPoint : DecisionPoint
{
	public EllipsePointsChecker PointsChecker;

	private void Awake()
	{
		PointsChecker = new EllipsePointsChecker(base.transform, hasOffset: false, Constant.GetFloat(ConstantId.MELEE_RANGE_VERY_LARGE), Constant.GetFloat(ConstantId.MELEE_RANGE_VERY_VERY_LARGE));
	}
}
