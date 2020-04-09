using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
public class Vignette : MonoBehaviour
{
	public Shader curShader;

	public float VignettePower = 5f;

	public float VignetteSpeed = 1f;

	private Material curMaterial;

	private float power;

	private Material material
	{
		get
		{
			if (curMaterial == null)
			{
				curMaterial = new Material(curShader);
				curMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
			return curMaterial;
		}
	}

	private void Start()
	{
		if (!SystemInfo.supportsImageEffects)
		{
			base.enabled = false;
		}
	}

	private void OnRenderImage(RenderTexture sourceTexture, RenderTexture destTexture)
	{
		if (curShader != null)
		{
			material.SetFloat("_VignettePower", power);
			Graphics.Blit(sourceTexture, destTexture, material);
		}
		else
		{
			Graphics.Blit(sourceTexture, destTexture);
		}
	}

	private void OnDisable()
	{
		if ((bool)curMaterial)
		{
			Object.DestroyImmediate(curMaterial);
		}
	}

	public void Activate(bool activate)
	{
		if (activate)
		{
			StartCoroutine(TurnOn());
		}
		else
		{
			StartCoroutine(TurnOff());
		}
	}

	private IEnumerator TurnOn()
	{
		power = 0f;
		base.enabled = true;
		while (power < VignettePower)
		{
			power += VignetteSpeed;
			power = ((!(power > VignettePower)) ? power : VignettePower);
			yield return 0;
		}
	}

	private IEnumerator TurnOff()
	{
		while (power > 0f)
		{
			power -= VignetteSpeed;
			power = ((!(power < 0f)) ? power : (power = 0f));
			yield return 0;
		}
		base.enabled = false;
	}
}
