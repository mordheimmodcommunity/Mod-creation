using UnityEngine;

public class SpawnZone : MonoBehaviour
{
	public SpawnZoneId type;

	public bool indoor;

	public float range;

	[HideInInspector]
	public Bounds bounds;

	private int claimMask;

	private void Start()
	{
		bounds = default(Bounds);
		foreach (Transform item in base.transform)
		{
			if (item.name == "bounds")
			{
				bounds = item.GetComponent<Renderer>().bounds;
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (type == SpawnZoneId.SCATTER)
		{
			Gizmos.color = Color.white;
			Gizmos.DrawWireSphere(base.transform.position, range);
		}
	}

	public void Claim(DeploymentId deployId)
	{
		claimMask |= 1 << (int)deployId;
	}

	public bool IsClaimed(DeploymentId deployId)
	{
		SpawnZoneId spawnZoneId = type;
		if (spawnZoneId == SpawnZoneId.AMBUSH)
		{
			return (claimMask & (1 << (int)deployId)) != 0;
		}
		return claimMask != 0;
	}
}
