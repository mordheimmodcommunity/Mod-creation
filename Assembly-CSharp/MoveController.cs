using UnityEngine;

public class MoveController : MonoBehaviour
{
	protected float currentSpeed;

	private Vector3 direction;

	private void Awake()
	{
		base.enabled = false;
	}

	private void FixedUpdate()
	{
		Vector3 position = base.transform.position + direction * currentSpeed * Time.fixedDeltaTime;
		base.transform.position = position;
	}

	public void StartMoving(Vector3 dir, float speed)
	{
		base.enabled = true;
		direction = dir;
		currentSpeed = speed;
	}

	public virtual void StopMoving()
	{
		base.enabled = false;
		currentSpeed = 0f;
	}
}
