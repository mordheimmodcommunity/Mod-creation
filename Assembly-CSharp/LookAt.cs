using UnityEngine;

public class LookAt : ExternalUpdator
{
	private const float LOOK_DIST = 100f;

	public bool yOnly;

	public new void Awake()
	{
		base.Awake();
	}

	public override void ExternalUpdate()
	{
		UnitController currentUnit = PandoraSingleton<MissionManager>.Instance.GetCurrentUnit();
		if (currentUnit != null && Vector3.SqrMagnitude(cachedTransform.position - currentUnit.transform.position) < 100f)
		{
			cachedTransform.LookAt(currentUnit.transform.position + Vector3.up);
		}
	}
}
