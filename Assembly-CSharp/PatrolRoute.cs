using System.Collections.Generic;
using UnityEngine;

public class PatrolRoute : MonoBehaviour
{
	public bool loop;

	public List<DecisionPoint> patrolPoints = new List<DecisionPoint>();

	private void OnDrawGizmos()
	{
		if (patrolPoints.Count >= 2)
		{
			for (int i = 0; i < patrolPoints.Count - 1; i++)
			{
				Debug.DrawLine(patrolPoints[i].transform.position + Vector3.up / 2f, patrolPoints[i + 1].transform.position + Vector3.up / 2f, Color.cyan);
			}
			if (loop)
			{
				Debug.DrawLine(patrolPoints[0].transform.position + Vector3.up / 2f, patrolPoints[patrolPoints.Count - 1].transform.position + Vector3.up / 2f, Color.cyan);
			}
		}
	}

	public void CheckValidity()
	{
		for (int num = patrolPoints.Count - 1; num >= 0; num--)
		{
			if (patrolPoints[num] == null)
			{
				patrolPoints.RemoveAt(num);
			}
		}
		if (patrolPoints.Count < 2)
		{
			Object.DestroyImmediate(base.gameObject);
		}
		else
		{
			PandoraSingleton<MissionManager>.Instance.RegisterPatrolRoute(this);
		}
	}
}
