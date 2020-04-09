using System.Collections.Generic;
using UnityEngine;

public class OccupationChecker : MonoBehaviour
{
	private List<Collider> occupiers = new List<Collider>();

	public int Occupation => occupiers.Count;

	private void Awake()
	{
		base.gameObject.SetLayerRecursively(LayerMask.NameToLayer("Ignore Raycast"));
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.name != "climb_collider" && other.name != "action_zone" && other.name != "action_collision" && other.name != "large_collision" && !other.name.Contains("collision_walk") && occupiers.IndexOf(other) == -1)
		{
			occupiers.Add(other);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.name != "climb_collider" && other.name != "action_zone" && other.name != "action_collision" && other.name != "large_collision" && !other.name.Contains("collision_walk"))
		{
			int num = occupiers.IndexOf(other);
			if (num != -1)
			{
				occupiers.RemoveAt(num);
			}
		}
	}
}
