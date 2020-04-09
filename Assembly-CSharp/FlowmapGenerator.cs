using Flowmap;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Flowmaps/Generator")]
public class FlowmapGenerator : MonoBehaviour
{
	public static SimulationPath SimulationPath;

	private static int _threadCount = 1;

	[SerializeField]
	private List<FlowSimulationField> fields = new List<FlowSimulationField>();

	public bool gpuAcceleration;

	public bool autoAddChildFields = true;

	public int maxThreadCount = 1;

	[SerializeField]
	private Vector2 dimensions = Vector2.one;

	private Vector3 cachedPosition;

	public int outputFileFormat;

	private FlowSimulator flowSimulator;

	private FlowHeightmap heightmap;

	public static LayerMask GpuRenderLayer => LayerMask.NameToLayer("Default");

	public static bool SupportsGPUPath => (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf) && SystemInfo.supportsRenderTextures) ? true : false;

	public static int ThreadCount
	{
		get
		{
			return _threadCount;
		}
		set
		{
			_threadCount = value;
		}
	}

	public static RenderTextureFormat GetSingleChannelRTFormat => (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RFloat)) ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.RFloat;

	public static RenderTextureFormat GetTwoChannelRTFormat => (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGFloat)) ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.RGFloat;

	public static RenderTextureFormat GetFourChannelRTFormat => (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBFloat)) ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGBFloat;

	public FlowSimulationField[] Fields
	{
		get
		{
			CleanNullFields();
			return fields.ToArray();
		}
	}

	public Vector2 Dimensions
	{
		get
		{
			return dimensions;
		}
		set
		{
			dimensions = value;
		}
	}

	public Vector3 Position => cachedPosition;

	public FlowSimulator FlowSimulator
	{
		get
		{
			if (!flowSimulator)
			{
				flowSimulator = GetComponent<FlowSimulator>();
			}
			return flowSimulator;
		}
	}

	public FlowHeightmap Heightmap
	{
		get
		{
			if (!heightmap)
			{
				heightmap = GetComponent<FlowHeightmap>();
			}
			return heightmap;
		}
	}

	public SimulationPath GetSimulationPath()
	{
		return (!gpuAcceleration || !SupportsGPUPath) ? SimulationPath.CPU : SimulationPath.GPU;
	}

	private void Awake()
	{
		base.transform.rotation = Quaternion.identity;
		cachedPosition = base.transform.position;
		UpdateThreadCount();
	}

	private void Start()
	{
		UpdateSimulationPath();
		if ((bool)FlowSimulator)
		{
			FlowSimulator.Init();
			if (FlowSimulator.simulateOnPlay && Application.isPlaying)
			{
				FlowSimulator.StartSimulating();
			}
		}
	}

	public void UpdateSimulationPath()
	{
		SimulationPath = GetSimulationPath();
	}

	public void UpdateThreadCount()
	{
		_threadCount = maxThreadCount;
	}

	private void Update()
	{
		base.transform.rotation = Quaternion.identity;
		cachedPosition = base.transform.position;
		if (autoAddChildFields)
		{
			FlowSimulationField[] componentsInChildren = GetComponentsInChildren<FlowSimulationField>();
			foreach (FlowSimulationField field in componentsInChildren)
			{
				AddSimulationField(field);
			}
		}
	}

	public void CleanNullFields()
	{
		fields.RemoveAll((FlowSimulationField i) => i == null);
	}

	public void AddSimulationField(FlowSimulationField field)
	{
		if (!fields.Contains(field))
		{
			fields.Add(field);
		}
	}

	public void ClearAllFields()
	{
		fields.Clear();
	}

	private void OnDrawGizmos()
	{
		Vector3 position = base.transform.position;
		Vector2 vector = Dimensions;
		float x = vector.x;
		Vector2 vector2 = Dimensions;
		Gizmos.DrawWireCube(position, new Vector3(x, 0f, vector2.y));
	}
}
