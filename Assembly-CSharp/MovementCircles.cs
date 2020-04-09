using System;
using UnityEngine;

public class MovementCircles : MonoBehaviour
{
	public int segments = 50;

	public float defaultHeight = 0.1f;

	private GameObject root;

	private LineRenderer line;

	private float radius;

	private void Awake()
	{
		root = new GameObject("movement_lines");
		root.transform.SetParent(null);
		PandoraSingleton<AssetBundleLoader>.Instance.LoadAssetAsync<GameObject>("Assets/prefabs/fx/", AssetBundleId.FX, "fx_move_line.prefab", delegate(UnityEngine.Object linePrefab)
		{
			line = ((GameObject)UnityEngine.Object.Instantiate(linePrefab)).GetComponent<LineRenderer>();
			line.transform.SetParent(root.transform);
			line.transform.localPosition = Vector3.zero;
			line.transform.localRotation = Quaternion.identity;
			line.SetVertexCount(segments);
			line.useWorldSpace = false;
			line.enabled = false;
		});
	}

	public void Show(Vector3 pos, float radius)
	{
		root.transform.position = pos;
		this.radius = radius;
		line.enabled = true;
		SetupCircle();
	}

	private void SetupCircle()
	{
		Vector3 position = new Vector3(0f, defaultHeight, 0f);
		float num = 360f / (float)(segments - 1);
		float num2 = 0f;
		for (int i = 0; i < segments; i++)
		{
			position.x = Mathf.Sin(MathF.PI / 180f * num2) * radius;
			position.z = Mathf.Cos(MathF.PI / 180f * num2) * radius;
			line.SetPosition(i, position);
			num2 += num;
		}
	}

	public void AdjustHeightAndRadius(float y, float radius)
	{
		Vector3 position = root.transform.position;
		position.y = y;
		root.transform.position = position;
		if (this.radius != radius)
		{
			this.radius = radius;
			SetupCircle();
		}
	}

	public void Hide()
	{
		line.enabled = false;
	}
}
