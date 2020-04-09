using UnityEngine;

public class ApolloObjectRotateStep : MonoBehaviour
{
	public bool next;

	public Vector3 rotateStep = new Vector3(0f, 180f, 0f);

	private void Update()
	{
		if (next)
		{
			next = false;
			base.transform.rotation = base.transform.rotation * Quaternion.Euler(rotateStep);
		}
	}
}
