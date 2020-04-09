using UnityEngine;

public class BoundingBox : MonoBehaviour
{
	public Vector3 size;

	public Vector3 center;

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(center, size);
	}
}
