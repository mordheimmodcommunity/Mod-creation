using UnityEngine;

public class CandleFX : ExternalUpdator
{
	private const float SHADOW_DIST = 625f;

	private const float LIGHT_DIST = 2500f;

	public float intensityOccurence = 3f;

	[Range(0f, 1f)]
	public float intensityChange = 0.2f;

	public float rangeOccurence = 3f;

	[Range(0f, 1f)]
	public float rangeChange = 0.2f;

	private Light lightSource;

	private float intensity;

	private LightShadows defaultShadows;

	public new void Awake()
	{
		base.Awake();
		lightSource = GetComponent<Light>();
		intensity = lightSource.intensity;
		defaultShadows = lightSource.shadows;
	}

	public override void ExternalUpdate()
	{
		if (!PandoraSingleton<TransitionManager>.Instance.GameLoadingDone || Camera.main == null)
		{
			return;
		}
		float num = Vector3.SqrMagnitude(cachedTransform.position - Camera.main.transform.position);
		if (num < 2500f)
		{
			lightSource.enabled = true;
			if (num < 625f)
			{
				lightSource.shadows = defaultShadows;
			}
			else
			{
				lightSource.shadows = LightShadows.None;
			}
			if (lightSource.intensity > intensity)
			{
				lightSource.intensity -= Time.deltaTime;
			}
			else
			{
				lightSource.intensity += Time.deltaTime;
			}
			if (Random.value < intensityOccurence / 60f)
			{
				if (lightSource.intensity > intensity)
				{
					lightSource.intensity = intensity - intensity * intensityChange * Random.value;
				}
				else
				{
					lightSource.intensity = intensity + intensity * intensityChange * Random.value;
				}
			}
		}
		else
		{
			lightSource.enabled = false;
		}
	}
}
