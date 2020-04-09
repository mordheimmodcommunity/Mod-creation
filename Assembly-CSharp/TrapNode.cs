using UnityEngine;

public class TrapNode : MonoBehaviour
{
	public TrapTypeId typeId;

	public bool forceInactive;

	private void Awake()
	{
		base.gameObject.AddComponent<MeshBatcherBlocker>();
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawIcon(base.transform.position + new Vector3(0f, 0.5f, 0f), "traps.tga", allowScaling: true);
	}
}
