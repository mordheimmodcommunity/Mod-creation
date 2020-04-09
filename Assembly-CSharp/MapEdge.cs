using UnityEngine;

[ExecuteInEditMode]
public class MapEdge : MonoBehaviour
{
	public int idx;

	private void OnDrawGizmos()
	{
		Gizmos.DrawIcon(base.transform.position + new Vector3(0f, 50f, 0f), "map_edge.tga", allowScaling: true);
		Gizmos.color = Color.white;
		PandoraUtils.DrawFacingGizmoCube(base.transform, 50f, 0.25f, 0.25f, 0f, 0f, drawTriangle: false);
	}
}
