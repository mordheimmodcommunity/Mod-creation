using UnityEngine;

public class FxDisabler : ExternalUpdator
{
	public float distance = 25f;

	private ParticleSystem[] particles;

	private ParticleSystemRenderer[] particlesRenderer;

	private bool isOn;

	private bool isActiveOn;

	private float sqrDist;

	public new void Awake()
	{
		base.Awake();
		particles = GetComponentsInChildren<ParticleSystem>();
		particlesRenderer = new ParticleSystemRenderer[particles.Length];
		for (int i = 0; i < particles.Length; i++)
		{
			particlesRenderer[i] = particles[i].GetComponent<ParticleSystemRenderer>();
		}
		isOn = true;
		isActiveOn = true;
		sqrDist = distance * distance;
	}

	public override void ExternalUpdate()
	{
		if (!(Camera.main != null))
		{
			return;
		}
		float num = Vector3.SqrMagnitude(cachedTransform.position - Camera.main.transform.position);
		if (num < sqrDist)
		{
			if (isOn)
			{
				return;
			}
			isOn = true;
			base.gameObject.SetActive(value: true);
			for (int i = 0; i < particles.Length; i++)
			{
				ParticleSystem.EmissionModule emission = particles[i].emission;
				emission.enabled = true;
				particles[i].Play();
				if (particlesRenderer[i] != null)
				{
					particlesRenderer[i].enabled = true;
				}
			}
			return;
		}
		if (isOn)
		{
			isOn = false;
			for (int j = 0; j < particles.Length; j++)
			{
				ParticleSystem.EmissionModule emission2 = particles[j].emission;
				emission2.enabled = false;
				if (particlesRenderer[j] != null)
				{
					particlesRenderer[j].enabled = false;
				}
			}
		}
		for (int k = 0; k < particles.Length; k++)
		{
			if (base.gameObject.activeSelf && particles[k].particleCount == 0)
			{
				base.gameObject.SetActive(value: false);
			}
		}
	}
}
