using Pathfinding;
using System.Collections.Generic;
using UnityEngine;

public class DynamicCombatCircle : DynamicCircle
{
	public Material friendly;

	public Material friendlyEngaged;

	public Material enemy;

	public Material enemyEngaged;

	private List<Vector3> displayPoints = new List<Vector3>();

	private List<Vector3> collisionPoints = new List<Vector3>();

	private List<Vector3> navCutPoints = new List<Vector3>();

	private NavmeshCut navCutter;

	private MeshRenderer meshRenderer;

	private Mesh displayMesh;

	private Mesh navCutMesh;

	private Mesh collisionMesh;

	private Color currentColor;

	public MeshCollider Collider
	{
		get;
		private set;
	}

	public bool NavCutterEnabled => navCutter.enabled;

	public override void Init()
	{
		base.Init();
		capsuleMinHeight = 0.6f;
		sphereRadius = 0.2f;
		heightTreshold = 0.5f;
		angleIteration = 30;
		envHeight = 0.05f;
		pointsTreshold = 0.45f;
		collisionPointsDistMin = 0.0001f;
		displayMesh = new Mesh();
		displayMesh.MarkDynamic();
		GetComponent<MeshFilter>().mesh = displayMesh;
		meshRenderer = GetComponent<MeshRenderer>();
		navCutter = GetComponent<NavmeshCut>();
		navCutMesh = new Mesh();
		navCutMesh.MarkDynamic();
		navCutter.mesh = navCutMesh;
		collisionMesh = new Mesh();
		collisionMesh.MarkDynamic();
		Collider = GetComponentInChildren<MeshCollider>();
		Collider.sharedMesh = collisionMesh;
	}

	public void Show(bool visible)
	{
		meshRenderer.enabled = visible;
	}

	public void SetNavCutterEnabled(bool enabled)
	{
		navCutter.enabled = enabled;
		if (enabled)
		{
			navCutter.ForceContourRefresh();
			navCutter.ForceUpdate();
		}
	}

	public void OverrideNavCutterRadius(float radius)
	{
		DetectCollisions(radius, radius, base.transform.parent.rotation, flatEnv: true, 0f, ref navCutPoints);
		CreateFlatMesh(navCutMesh, navCutPoints);
	}

	public void SetNavCutter()
	{
		CreateFlatMesh(navCutMesh, displayPoints);
	}

	public void Set(bool isEnemy, bool isEngaged, bool currentUnitIsPlayed, float sizeA, float sizeB, float currentUnitRadius, Quaternion rotation)
	{
		Show(currentUnitIsPlayed);
		SetMaterial(isEnemy, isEngaged);
		sizeA += 0.02f;
		sizeB += 0.02f;
		DetectCollisions(sizeA, sizeB, rotation, flatEnv: true, -0.1f, ref displayPoints);
		CreateEdges(displayPoints, base.transform.position);
		CreateCylinderOutlineMesh(displayMesh, displayPoints, 0f, 0.2f);
		for (int i = 0; i < displayPoints.Count; i++)
		{
			Vector3 value = displayPoints[i];
			value.y = 0f;
			displayPoints[i] = value;
		}
		SetNavCutter();
		sizeA -= currentUnitRadius;
		sizeB -= currentUnitRadius;
		DetectCollisions(sizeA, sizeB, rotation, flatEnv: false, 0f, ref collisionPoints);
		CreateCylinderMesh(collisionMesh, collisionPoints, -0.1f, 1.5f);
		Collider.enabled = false;
		Collider.enabled = true;
		SetNavCutterEnabled(enabled: true);
		base.transform.rotation = Quaternion.identity;
	}

	public void SetMaterial(bool isEnemy, bool isEngaged)
	{
		Material sharedMaterial = (!isEnemy) ? ((!isEngaged) ? friendly : friendlyEngaged) : ((!isEngaged) ? enemy : enemyEngaged);
		meshRenderer.sharedMaterial = sharedMaterial;
		currentColor = meshRenderer.sharedMaterial.GetColor("_Color");
	}

	public void SetAlpha(float a)
	{
		currentColor.a = a;
		meshRenderer.material.SetColor("_Color", currentColor);
	}
}
