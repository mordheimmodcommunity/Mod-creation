using System;
using System.Collections.Generic;
using UnityEngine;

public class DynamicCircle : MonoBehaviour
{
	protected float capsuleMinHeight = 0.6f;

	protected float sphereRadius = 0.2f;

	protected float heightTreshold = 0.5f;

	protected int angleIteration = 30;

	protected float envHeight = 1.5f;

	protected float pointsTreshold = 0.45f;

	protected float collisionPointsDistMin = 0.0001f;

	private RaycastHit rayHitData;

	private Vector3 circleCenter;

	[HideInInspector]
	public List<Tuple<Vector2, Vector2>> Edges
	{
		get;
		private set;
	}

	public virtual void Init()
	{
		Edges = new List<Tuple<Vector2, Vector2>>();
	}

	private void FixedUpdate()
	{
		base.transform.rotation = Quaternion.identity;
	}

	protected void DetectCollisions(float radiusA, float radiusB, Quaternion rotation, bool flatEnv, float flatOffset, ref List<Vector3> points)
	{
		circleCenter = base.transform.position;
		points.Clear();
		Vector2 vector3 = default(Vector2);
		for (int i = 0; i < angleIteration; i++)
		{
			float num = (float)i * (360f / (float)(angleIteration + 1));
			float magnitude;
			Vector3 a;
			if (radiusA != radiusB)
			{
				float f = num * (MathF.PI / 180f) * -1f;
				float num2 = radiusA * radiusB / Mathf.Sqrt(Mathf.Pow(radiusB, 2f) + Mathf.Pow(radiusA, 2f) * Mathf.Pow(Mathf.Tan(f), 2f));
				num2 *= (((!(0f <= num) || !(num < 90f)) && (!(270f < num) || !(num <= 360f))) ? 1f : (-1f));
				float z = Mathf.Tan(f) * num2;
				a = new Vector3(num2, 0f, z);
				a = rotation * -a;
				magnitude = a.magnitude;
				a /= magnitude;
				magnitude -= sphereRadius;
			}
			else
			{
				a = Vector3.forward;
				a = Quaternion.Euler(0f, (float)i * (360f / (float)angleIteration), 0f) * a;
				a.Normalize();
				magnitude = radiusA - sphereRadius;
			}
			Vector3 point = GetPoint(circleCenter, a, magnitude);
			if (points.Count > 0)
			{
				Vector3 vector = points[points.Count - 1];
				float x = vector.x;
				Vector3 vector2 = points[points.Count - 1];
				vector3 = new Vector2(x, vector2.z);
				Vector2 vector4 = new Vector2(point.x - circleCenter.x, point.z - circleCenter.z);
				if (!IsClockwise(vector3, vector4) || Vector2.SqrMagnitude(vector4 - vector3) < 0.01f)
				{
					continue;
				}
				Vector3 vector5 = points[0];
				float x2 = vector5.x;
				Vector3 vector6 = points[0];
				if (Vector2.SqrMagnitude(vector4 - new Vector2(x2, vector6.z)) < 0.01f)
				{
					continue;
				}
			}
			if (flatEnv)
			{
				if (Physics.Raycast(point + Vector3.up * envHeight + a * flatOffset, Vector3.down, out rayHitData, 2.8f, LayerMaskManager.decisionMask))
				{
					Vector3 point2 = rayHitData.point;
					point.y = point2.y;
				}
				else
				{
					Vector3 position = base.transform.position;
					point.y = position.y;
				}
			}
			else
			{
				point.y = circleCenter.y;
			}
			point -= circleCenter;
			points.Add(point);
		}
	}

	public bool IsClockwise(Vector3 a, Vector3 b, Vector3 c)
	{
		return (b.x - a.x) * (c.z - a.z) - (c.x - a.x) * (b.z - a.z) < 0f;
	}

	public bool IsClockwise(Vector2 a, Vector2 b)
	{
		return a.x * b.y - a.y * b.x < 0f;
	}

	private Vector3 GetPoint(Vector3 startPoint, Vector3 dir, float dist)
	{
		Vector3 vector;
		if (PandoraUtils.SendCapsule(startPoint, dir, capsuleMinHeight, Mathf.Max(1.5f - sphereRadius, capsuleMinHeight), dist, sphereRadius, out rayHitData))
		{
			vector = rayHitData.point;
			float num = Vector2.SqrMagnitude(new Vector2(vector.x, vector.z) - new Vector2(circleCenter.x, circleCenter.z));
			float num2 = Vector2.SqrMagnitude(new Vector2(startPoint.x, startPoint.z) - new Vector2(circleCenter.x, circleCenter.z));
			if (Mathf.Abs(num - num2) < 0.1f || num < num2)
			{
				return startPoint;
			}
			if (Mathf.Abs(vector.y - startPoint.y) <= heightTreshold)
			{
				float distance = rayHitData.distance;
				float num3 = dist - distance + ((!(distance > sphereRadius)) ? 0f : sphereRadius);
				Vector3 startPoint2 = vector - dir * sphereRadius;
				startPoint2.y = vector.y;
				if (num3 > sphereRadius)
				{
					return GetPoint(startPoint2, dir, num3);
				}
			}
		}
		else
		{
			vector = startPoint + dir * (dist + sphereRadius) + Vector3.up * (capsuleMinHeight - sphereRadius);
		}
		return vector;
	}

	protected void CreateEdges(List<Vector3> points, Vector3 circlePosition)
	{
		Edges.Clear();
		for (int i = 0; i < points.Count; i++)
		{
			int index = (i + 1 < points.Count) ? (i + 1) : 0;
			List<Tuple<Vector2, Vector2>> edges = Edges;
			Vector3 vector = points[i];
			float x = vector.x + circlePosition.x;
			Vector3 vector2 = points[i];
			Vector2 item = new Vector2(x, vector2.z + circlePosition.z);
			Vector3 vector3 = points[index];
			float x2 = vector3.x + circlePosition.x;
			Vector3 vector4 = points[index];
			edges.Add(new Tuple<Vector2, Vector2>(item, new Vector2(x2, vector4.z + circlePosition.z)));
		}
	}

	protected void CreateCylinderOutlineMesh(Mesh mesh, List<Vector3> points, float bottomOffset, float topOffset)
	{
		Vector3[] array = new Vector3[points.Count * 2];
		Vector2[] array2 = new Vector2[array.Length];
		int[] array3 = new int[points.Count * 6];
		int num = points.Count * 2;
		for (int i = 0; i < points.Count; i++)
		{
			array[i] = points[i] + Vector3.up * bottomOffset;
			array2[i] = new Vector2((i % 2 != 0) ? 1 : 0, 0f);
			array[i + points.Count] = points[i] + Vector3.up * topOffset;
			array2[i + points.Count] = new Vector2((i % 2 != 0) ? 1 : 0, 1f);
			array3[i * 3] = i;
			array3[i * 3 + 1] = (i + 1) % points.Count;
			array3[i * 3 + 2] = i + points.Count;
			if (i < points.Count - 1)
			{
				array3[(i + points.Count) * 3] = (i + points.Count + 1) % num;
				array3[(i + points.Count) * 3 + 1] = (i + points.Count) % num;
				array3[(i + points.Count) * 3 + 2] = (i + 1) % num;
			}
		}
		int num2 = points.Count - 1;
		array3[(num2 + points.Count) * 3] = (num2 + points.Count) % num;
		array3[(num2 + points.Count) * 3 + 1] = (num2 + points.Count + 1) % num;
		array3[(num2 + points.Count) * 3 + 2] = (num2 + 1) % num;
		SetMesh(mesh, array, array3);
		mesh.uv = array2;
	}

	protected void CreateCylinderOutlineFlatMesh(Mesh mesh, List<Vector3> points)
	{
		Vector3[] array = new Vector3[points.Count * 2];
		Vector2[] array2 = new Vector2[array.Length];
		int[] array3 = new int[points.Count * 6];
		int num = points.Count * 2;
		for (int i = 0; i < points.Count; i++)
		{
			array[i] = points[i];
			array2[i] = new Vector2((i % 2 != 0) ? 1 : 0, 0f);
			array[i + points.Count] = points[i] + points[i].normalized * -0.1f;
			array2[i + points.Count] = new Vector2((i % 2 != 0) ? 1 : 0, 1f);
			array3[i * 3] = i;
			array3[i * 3 + 1] = (i + 1) % points.Count;
			array3[i * 3 + 2] = i + points.Count;
			if (i < points.Count - 1)
			{
				array3[(i + points.Count) * 3] = (i + points.Count + 1) % num;
				array3[(i + points.Count) * 3 + 1] = (i + points.Count) % num;
				array3[(i + points.Count) * 3 + 2] = (i + 1) % num;
			}
		}
		int num2 = points.Count - 1;
		array3[(num2 + points.Count) * 3] = (num2 + points.Count) % num;
		array3[(num2 + points.Count) * 3 + 1] = (num2 + points.Count + 1) % num;
		array3[(num2 + points.Count) * 3 + 2] = (num2 + 1) % num;
		SetMesh(mesh, array, array3);
		mesh.uv = array2;
	}

	protected void CreateCylinderMesh(Mesh mesh, List<Vector3> points, float bottomOffset, float topOffset)
	{
		Vector3[] array = new Vector3[points.Count * 2 + 2];
		int[] array2 = new int[points.Count * 12];
		int num = points.Count * 2;
		array[array.Length - 2] = Vector3.up * bottomOffset;
		array[array.Length - 1] = Vector3.up * topOffset;
		for (int i = 0; i < points.Count; i++)
		{
			array[i] = points[i] + Vector3.up * bottomOffset;
			array[i + points.Count] = points[i] + Vector3.up * topOffset;
			array2[i * 3] = i;
			array2[i * 3 + 1] = (i + 1) % points.Count;
			array2[i * 3 + 2] = i + points.Count;
			if (i < points.Count - 1)
			{
				array2[(i + points.Count) * 3] = (i + points.Count + 1) % num;
				array2[(i + points.Count) * 3 + 1] = (i + points.Count) % num;
				array2[(i + points.Count) * 3 + 2] = (i + 1) % num;
				array2[(i + points.Count * 2) * 3] = array.Length - 2;
				array2[(i + points.Count * 2) * 3 + 1] = (i + 1) % points.Count;
				array2[(i + points.Count * 2) * 3 + 2] = i;
				array2[(i + points.Count * 3) * 3] = array.Length - 1;
				array2[(i + points.Count * 3) * 3 + 1] = i + points.Count;
				array2[(i + points.Count * 3) * 3 + 2] = i + points.Count + 1;
			}
		}
		int num2 = points.Count - 1;
		array2[(num2 + points.Count) * 3] = (num2 + points.Count) % num;
		array2[(num2 + points.Count) * 3 + 1] = (num2 + points.Count + 1) % num;
		array2[(num2 + points.Count) * 3 + 2] = (num2 + 1) % num;
		array2[(num2 + points.Count * 2) * 3] = array.Length - 2;
		array2[(num2 + points.Count * 2) * 3 + 1] = (num2 + 1) % points.Count;
		array2[(num2 + points.Count * 2) * 3 + 2] = num2;
		array2[(num2 + points.Count * 3) * 3] = array.Length - 1;
		array2[(num2 + points.Count * 3) * 3 + 1] = num2 + points.Count;
		array2[(num2 + points.Count * 3) * 3 + 2] = points.Count;
		SetMesh(mesh, array, array2);
	}

	protected void CreateFlatMesh(Mesh mesh, List<Vector3> points)
	{
		Vector3[] array = new Vector3[points.Count + 1];
		array[0] = Vector3.zero;
		int[] array2 = new int[array.Length * 3];
		for (int i = 0; i < points.Count; i++)
		{
			array[i + 1] = points[i];
			array2[i * 3] = 0;
			array2[i * 3 + 1] = i + 1;
			array2[i * 3 + 2] = (i + 2) % array.Length;
		}
		array2[array2.Length - 3] = 0;
		array2[array2.Length - 2] = array.Length - 1;
		array2[array2.Length - 1] = 1;
		SetMesh(mesh, array, array2);
	}

	private void SetMesh(Mesh mesh, Vector3[] vertices, int[] triangles)
	{
		mesh.Clear();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
	}
}
