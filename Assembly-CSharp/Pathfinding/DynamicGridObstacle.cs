using System;
using UnityEngine;

namespace Pathfinding
{
	[RequireComponent(typeof(Collider))]
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_dynamic_grid_obstacle.php")]
	public class DynamicGridObstacle : MonoBehaviour
	{
		private Collider col;

		private Transform tr;

		public float updateError = 1f;

		public float checkTime = 0.2f;

		private Bounds prevBounds;

		private Quaternion prevRotation;

		private bool prevEnabled;

		private float lastCheckTime = -9999f;

		private void Start()
		{
			col = GetComponent<Collider>();
			tr = base.transform;
			if (col == null)
			{
				throw new Exception("A collider must be attached to the GameObject for the DynamicGridObstacle to work");
			}
			prevBounds = col.bounds;
			prevEnabled = col.enabled;
			prevRotation = tr.rotation;
		}

		private void Update()
		{
			if (!col)
			{
				Debug.LogError("Removed collider from DynamicGridObstacle", this);
				base.enabled = false;
				return;
			}
			while (AstarPath.active == null || AstarPath.active.isScanning)
			{
				lastCheckTime = Time.realtimeSinceStartup;
			}
			if (Time.realtimeSinceStartup - lastCheckTime < checkTime)
			{
				return;
			}
			if (col.enabled)
			{
				Bounds bounds = col.bounds;
				Quaternion rotation = tr.rotation;
				Vector3 vector = prevBounds.min - bounds.min;
				Vector3 vector2 = prevBounds.max - bounds.max;
				float magnitude = bounds.extents.magnitude;
				float num = magnitude * Quaternion.Angle(prevRotation, rotation) * (MathF.PI / 180f);
				if (vector.sqrMagnitude > updateError * updateError || vector2.sqrMagnitude > updateError * updateError || num > updateError || !prevEnabled)
				{
					DoUpdateGraphs();
				}
			}
			else if (prevEnabled)
			{
				DoUpdateGraphs();
			}
		}

		private void OnDestroy()
		{
			if (AstarPath.active != null)
			{
				GraphUpdateObject ob = new GraphUpdateObject(prevBounds);
				AstarPath.active.UpdateGraphs(ob);
			}
		}

		public void DoUpdateGraphs()
		{
			if (col == null)
			{
				return;
			}
			if (!col.enabled)
			{
				AstarPath.active.UpdateGraphs(prevBounds);
			}
			else
			{
				Bounds bounds = col.bounds;
				Bounds bounds2 = bounds;
				bounds2.Encapsulate(prevBounds);
				if (BoundsVolume(bounds2) < BoundsVolume(bounds) + BoundsVolume(prevBounds))
				{
					AstarPath.active.UpdateGraphs(bounds2);
				}
				else
				{
					AstarPath.active.UpdateGraphs(prevBounds);
					AstarPath.active.UpdateGraphs(bounds);
				}
				prevBounds = bounds;
			}
			prevEnabled = col.enabled;
			prevRotation = tr.rotation;
			lastCheckTime = Time.realtimeSinceStartup;
		}

		private static float BoundsVolume(Bounds b)
		{
			Vector3 size = b.size;
			float x = size.x;
			Vector3 size2 = b.size;
			float num = x * size2.y;
			Vector3 size3 = b.size;
			return Math.Abs(num * size3.z);
		}
	}
}
