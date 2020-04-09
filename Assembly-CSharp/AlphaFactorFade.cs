using System.Collections;
using UnityEngine;

public class AlphaFactorFade : MonoBehaviour
{
	public float timeToFade;

	public float fadeTime;

	public float fadeFrom;

	public float fadeTo;

	public bool duplicateMat;

	private float elapsed;

	private float fade;

	private Material mat;

	private void Awake()
	{
		elapsed = 0f;
		if (duplicateMat)
		{
			mat = GetComponent<Renderer>().material;
		}
		else
		{
			mat = GetComponent<Renderer>().sharedMaterial;
		}
		if (!mat.HasProperty("_AlphaFactor"))
		{
			Object.DestroyImmediate(this);
		}
		else
		{
			mat.SetFloat("_AlphaFactor", fadeFrom);
		}
	}

	private void Update()
	{
		elapsed += Time.deltaTime;
		if (elapsed >= timeToFade)
		{
			StartCoroutine(FadeAlphaFactor());
		}
	}

	private IEnumerator FadeAlphaFactor()
	{
		fade = mat.GetFloat("_AlphaFactor");
		float t = 0f;
		while (t < 1f)
		{
			t += Time.deltaTime;
			fade = Mathf.SmoothStep(t: t / fadeTime, from: fade, to: fadeTo);
			mat.SetFloat("_AlphaFactor", fade);
			yield return 0;
		}
	}
}
