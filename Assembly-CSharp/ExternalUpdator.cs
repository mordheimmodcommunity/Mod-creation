using UnityEngine;

public abstract class ExternalUpdator : MonoBehaviour
{
	protected Transform cachedTransform;

	public void Awake()
	{
		if (PandoraSingleton<MissionManager>.Exists())
		{
			PandoraSingleton<MissionManager>.Instance.RegisterExternalUpdator(this);
		}
		base.enabled = false;
		cachedTransform = base.transform;
	}

	private void OnDestroy()
	{
		if (PandoraSingleton<MissionManager>.Exists())
		{
			PandoraSingleton<MissionManager>.Instance.ReleaseExternalUpdator(this);
		}
	}

	public abstract void ExternalUpdate();
}
