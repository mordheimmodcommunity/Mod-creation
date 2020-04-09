using UnityEngine;

public class HangingNode : MonoBehaviour
{
	public bool isPlank = true;

	public string blockingProp;

	public Vector3 positionOffset;

	private void OnDrawGizmos()
	{
		if (isPlank)
		{
			Gizmos.color = Color.magenta;
			PandoraUtils.DrawFacingGizmoCube(base.transform, 1f, 0.5f, 1f);
		}
	}
}
