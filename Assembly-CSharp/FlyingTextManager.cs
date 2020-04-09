using System;
using System.Collections.Generic;
using UnityEngine;

public class FlyingTextManager : PandoraSingleton<FlyingTextManager>
{
	public Canvas canvas;

	private Dictionary<FlyingTextId, List<FlyingText>> flyingTexts;

	public Vector3[] canvasCorners = new Vector3[4];

	public Transform beaconContainer;

	public Transform unitContainer;

	public Transform deploymentContainter;

	public Transform miscContainer;

	public void Init()
	{
		canvas.worldCamera = Camera.main;
		flyingTexts = new Dictionary<FlyingTextId, List<FlyingText>>();
		AddContainer("misc", ref miscContainer);
		AddContainer("deploy", ref deploymentContainter);
		AddContainer("unit", ref unitContainer);
		AddContainer("beacon", ref beaconContainer);
		ResetWorldCorners();
	}

	private void AddContainer(string name, ref Transform trans)
	{
		GameObject gameObject = new GameObject();
		gameObject.name = name;
		gameObject.transform.SetParent(base.transform);
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		trans = gameObject.transform;
	}

	public void ResetWorldCorners()
	{
		(base.transform.parent as RectTransform).GetWorldCorners(canvasCorners);
	}

	public void GetFlyingText(FlyingTextId id, Action<FlyingText> cb)
	{
		FlyingText flyingTxt = null;
		if (!flyingTexts.ContainsKey(id))
		{
			flyingTexts[id] = new List<FlyingText>();
		}
		for (int i = 0; i < flyingTexts[id].Count; i++)
		{
			if (flyingTexts[id][i].Done)
			{
				flyingTxt = flyingTexts[id][i];
				flyingTxt.gameObject.SetActive(value: true);
				cb(flyingTxt);
				return;
			}
		}
		PandoraSingleton<AssetBundleLoader>.Instance.LoadResourceAsync<GameObject>("prefabs/flying_text/" + id.ToLowerString(), delegate(UnityEngine.Object flyPrefab)
		{
			flyingTxt = UnityEngine.Object.Instantiate((GameObject)flyPrefab).GetComponent<FlyingText>();
			flyingTxt.transform.SetParent(base.transform, worldPositionStays: false);
			flyingTexts[id].Add(flyingTxt);
			flyingTxt.gameObject.SetActive(value: true);
			cb(flyingTxt);
		}, cached: true);
	}
}
