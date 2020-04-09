using UnityEngine;

public class SearchNode : MonoBehaviour
{
	public int types;

	public bool forceInit;

	[HideInInspector]
	public bool claimed;

	public bool IsOfType(SearchNodeId id)
	{
		return (types & (1 << (int)id)) != 0;
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawIcon(base.transform.position + new Vector3(0f, 1f, 0f), "search.png", allowScaling: true);
		if (IsOfType(SearchNodeId.WYRDSTONE))
		{
			Gizmos.color = Color.green;
			PandoraUtils.DrawFacingGizmoCube(base.transform, 0.25f, 0.375f, 0.5f);
		}
		else if (IsOfType(SearchNodeId.SEARCH))
		{
			Gizmos.color = Color.yellow;
			if (IsOfType(SearchNodeId.INDOOR))
			{
				PandoraUtils.DrawFacingGizmoCube(base.transform, 0.47f, 0.242f, 0.54f);
				PandoraUtils.DrawFacingGizmoCube(base.transform, 1f, 0.5f, 0.5f, 0f, 0.783f);
			}
			else if (IsOfType(SearchNodeId.OUTDOOR))
			{
				PandoraUtils.DrawFacingGizmoCube(base.transform, 0.45f, 1.03f, 0.4f, 0.13f, -0.07f);
				PandoraUtils.DrawFacingGizmoCube(base.transform, 0.5f, 1.35f, 0.705f, 0.13f, 0f, drawTriangle: false);
			}
		}
	}
}
