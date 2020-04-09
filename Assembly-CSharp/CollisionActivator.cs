using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionActivator : MonoBehaviour
{
	public GameObject[] gos;

	private List<Material> mats = new List<Material>();

	private ParticleSystem[] parts;

	private float a;

	private void Awake()
	{
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			mats.Add(componentsInChildren[i].material);
		}
		parts = GetComponentsInChildren<ParticleSystem>();
		for (int j = 0; j < mats.Count; j++)
		{
			Color color = (!mats[j].HasProperty("_TintColor")) ? mats[j].color : mats[j].GetColor("_TintColor");
			color.a = a;
			mats[j].color = color;
		}
		for (int k = 0; k < parts.Length; k++)
		{
			Color startColor = parts[k].startColor;
			startColor.a = a;
			parts[k].startColor = startColor;
		}
		for (int l = 0; l < gos.Length; l++)
		{
			gos[l].SetActive(value: false);
		}
	}

	private void OnTriggerEnter()
	{
		StopCoroutine("FadeOutFx");
		StartCoroutine("FadeInFx");
	}

	private void OnTriggerExit()
	{
		StopCoroutine("FadeInFx");
		StartCoroutine("FadeOutFx");
	}

	private IEnumerator FadeInFx()
	{
		for (int k = 0; k < gos.Length; k++)
		{
			gos[k].SetActive(value: true);
		}
		while (a < 1.1f)
		{
			for (int j = 0; j < mats.Count; j++)
			{
				Color c2 = (!mats[j].HasProperty("_TintColor")) ? mats[j].color : mats[j].GetColor("_TintColor");
				c2.a = a;
				mats[j].color = c2;
			}
			for (int i = 0; i < parts.Length; i++)
			{
				Color c = parts[i].startColor;
				c.a = a;
				parts[i].startColor = c;
			}
			a += 0.01f;
			yield return new WaitForFixedUpdate();
		}
	}

	private IEnumerator FadeOutFx()
	{
		while (a > -0.01f)
		{
			if (a < 0f)
			{
				a = 0f;
			}
			for (int j = 0; j < mats.Count; j++)
			{
				Color c2 = (!mats[j].HasProperty("_TintColor")) ? mats[j].color : mats[j].GetColor("_TintColor");
				c2.a = a;
				mats[j].color = c2;
			}
			for (int i = 0; i < parts.Length; i++)
			{
				Color c = parts[i].startColor;
				c.a = a;
				parts[i].startColor = c;
			}
			a -= 0.01f;
			yield return new WaitForFixedUpdate();
		}
		yield return new WaitForSeconds(2f);
		for (int k = 0; k < gos.Length; k++)
		{
			gos[k].SetActive(value: false);
		}
	}
}
