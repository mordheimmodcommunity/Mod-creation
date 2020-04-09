using UnityEngine;
using UnityEngine.Events;

public class BezierMoveController : MoveController
{
	private Vector3 start = Vector3.zero;

	private Vector3 end = Vector3.zero;

	private Vector3 summit = Vector3.zero;

	private float timer;

	private float currentTime;

	private UnityAction onDestination;

	private void FixedUpdate()
	{
		Move();
	}

	private void Move()
	{
		if (timer > 0f)
		{
			if (currentTime > timer)
			{
				currentTime = timer;
			}
			float num = currentTime / timer;
			Vector3 vector = (1f - num) * ((1f - num) * start + num * summit) + num * ((1f - num) * summit + num * end);
			Vector3 position = base.transform.position;
			base.transform.position = vector;
			Vector3 vector2 = position - vector;
			vector2 = new Vector3(0f - vector2.x, vector2.y, 0f - vector2.z);
			base.transform.rotation = Quaternion.LookRotation(vector2);
			if (Mathf.Approximately(currentTime, timer))
			{
				StopMoving();
			}
			else
			{
				currentTime += Time.fixedDeltaTime;
			}
		}
	}

	public void StartMoving(Vector3 startPosition, Vector3 endPosition, float height, float speed, UnityAction onDestination)
	{
		base.enabled = true;
		currentSpeed = speed * 10f;
		base.transform.position = startPosition;
		if (Vector3.SqrMagnitude(startPosition - endPosition) < 4f)
		{
			height = 0f;
		}
		start = startPosition;
		end = endPosition;
		summit = startPosition + (endPosition - startPosition) / 2f + Vector3.up * height;
		timer = Vector3.Distance(startPosition, endPosition) / currentSpeed;
		currentTime = 0f;
		this.onDestination = onDestination;
		Move();
	}

	public override void StopMoving()
	{
		base.StopMoving();
		timer = 0f;
		if (onDestination != null)
		{
			onDestination();
		}
	}
}
