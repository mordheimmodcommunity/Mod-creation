using UnityEngine;

public abstract class CameraBase : MonoBehaviour
{
	protected Transform target;

	public virtual void GetNextPositionAngle(ref Vector3 position, ref Quaternion angle)
	{
		position = base.transform.position;
		angle = base.transform.rotation;
	}

	public virtual void SetTarget(Transform target)
	{
		this.target = target;
	}

	public virtual Transform GetTarget()
	{
		return target;
	}

	public Vector3 OffsetPosition(Transform trans, Vector3 offset)
	{
		Vector3 position = trans.position;
		position += trans.forward * offset.z;
		position += trans.up * offset.y;
		return position + trans.right * offset.x;
	}

	public Vector3 OrientOffset(Transform trans, Vector3 offset)
	{
		Vector3 zero = Vector3.zero;
		zero += trans.forward * offset.z;
		zero += trans.up * offset.y;
		return zero + trans.right * offset.x;
	}
}
