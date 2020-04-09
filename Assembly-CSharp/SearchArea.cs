using System.Collections.Generic;
using UnityEngine;

public class SearchArea : MonoBehaviour
{
	private List<Bounds> bounds;

	private void Start()
	{
		bounds = new List<Bounds>();
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Transform child = base.transform.GetChild(i);
			if (child.name.Contains("bounds"))
			{
				Bounds item = child.GetComponent<Renderer>().bounds;
				bounds.Add(item);
			}
		}
	}

	public bool Contains(Vector3 pos)
	{
		for (int i = 0; i < bounds.Count; i++)
		{
			if (bounds[i].Contains(pos))
			{
				return true;
			}
		}
		return false;
	}
}
