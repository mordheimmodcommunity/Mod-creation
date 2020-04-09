using Flowmap;
using UnityEngine;

[RequireComponent(typeof(FlowmapGenerator))]
[AddComponentMenu("Flowmaps/Heightmap/Render From Scene")]
[ExecuteInEditMode]
public class FlowRenderHeightmap : FlowHeightmap
{
	public int resolutionX = 256;

	public int resolutionY = 256;

	public FluidDepth fluidDepth;

	public float heightMax = 1f;

	public float heightMin = 1f;

	public LayerMask cullingMask = 1;

	public bool dynamicUpdating;

	private Camera renderingCamera;

	private RenderTexture heightmap;

	private Material compareMaterial;

	private Material resizeMaterial;

	public static bool Supported => SystemInfo.supportsRenderTextures;

	public static string UnsupportedReason
	{
		get
		{
			string result = string.Empty;
			if (!SystemInfo.supportsRenderTextures)
			{
				result = "System doesn't support RenderTextures.";
			}
			return result;
		}
	}

	public override Texture HeightmapTexture
	{
		get
		{
			return heightmap;
		}
		set
		{
			Debug.LogWarning("Can't set HeightmapTexture.");
		}
	}

	public override Texture PreviewHeightmapTexture => HeightmapTexture;

	private Shader ClippedHeightShader => Shader.Find("Hidden/DepthToHeightClipped");

	private Shader HeightShader => Shader.Find("Hidden/DepthToHeight");

	private Material CompareMaterial
	{
		get
		{
			if (!compareMaterial)
			{
				compareMaterial = new Material(Shader.Find("Hidden/DepthCompare"));
				compareMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
			return compareMaterial;
		}
	}

	private Material ResizeMaterial
	{
		get
		{
			if (!resizeMaterial)
			{
				resizeMaterial = new Material(Shader.Find("Hidden/RenderHeightmapResize"));
				resizeMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
			return resizeMaterial;
		}
	}

	private void Awake()
	{
		UpdateHeightmap();
	}

	public void UpdateHeightmap()
	{
		if (heightmap == null || heightmap.width != resolutionX || heightmap.height != resolutionY)
		{
			heightmap = new RenderTexture(resolutionX, resolutionY, 24, FlowmapGenerator.GetSingleChannelRTFormat, RenderTextureReadWrite.Linear);
			heightmap.hideFlags = HideFlags.HideAndDontSave;
		}
		if (renderingCamera == null)
		{
			renderingCamera = new GameObject("Render Heightmap", typeof(Camera)).GetComponent<Camera>();
			renderingCamera.gameObject.hideFlags = HideFlags.HideAndDontSave;
			renderingCamera.enabled = false;
			renderingCamera.renderingPath = RenderingPath.Forward;
			renderingCamera.clearFlags = CameraClearFlags.Color;
			renderingCamera.backgroundColor = Color.black;
			renderingCamera.orthographic = true;
		}
		renderingCamera.cullingMask = cullingMask;
		renderingCamera.transform.rotation = Quaternion.identity;
		Camera camera = renderingCamera;
		Vector2 dimensions = base.Generator.Dimensions;
		float x = dimensions.x;
		Vector2 dimensions2 = base.Generator.Dimensions;
		camera.orthographicSize = Mathf.Max(x, dimensions2.y) * 0.5f;
		renderingCamera.transform.position = base.Generator.transform.position + Vector3.up * heightMax;
		renderingCamera.transform.rotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
		switch (fluidDepth)
		{
		case FluidDepth.DeepWater:
		{
			RenderTexture temporary = RenderTexture.GetTemporary(resolutionX, resolutionY, 24, FlowmapGenerator.GetSingleChannelRTFormat, RenderTextureReadWrite.sRGB);
			RenderTexture temporary2 = RenderTexture.GetTemporary(resolutionX, resolutionY, 24, FlowmapGenerator.GetSingleChannelRTFormat, RenderTextureReadWrite.sRGB);
			RenderTexture temporary3 = RenderTexture.GetTemporary(resolutionX, resolutionY, 24, FlowmapGenerator.GetSingleChannelRTFormat, RenderTextureReadWrite.sRGB);
			Vector3 position3 = base.Generator.transform.position;
			Shader.SetGlobalFloat("_HeightmapRenderDepthMin", position3.y);
			Vector3 position4 = base.Generator.transform.position;
			Shader.SetGlobalFloat("_HeightmapRenderDepthMax", position4.y - heightMin);
			renderingCamera.targetTexture = temporary;
			renderingCamera.nearClipPlane = 0.01f;
			renderingCamera.farClipPlane = 100f;
			renderingCamera.RenderWithShader(ClippedHeightShader, "RenderType");
			Vector3 position5 = base.Generator.transform.position;
			Shader.SetGlobalFloat("_HeightmapRenderDepthMin", position5.y);
			Vector3 position6 = base.Generator.transform.position;
			Shader.SetGlobalFloat("_HeightmapRenderDepthMax", position6.y - heightMin);
			renderingCamera.nearClipPlane = heightMax;
			renderingCamera.farClipPlane = heightMin + heightMax;
			renderingCamera.targetTexture = temporary2;
			renderingCamera.RenderWithShader(HeightShader, "RenderType");
			Vector3 position7 = base.Generator.transform.position;
			Shader.SetGlobalFloat("_HeightmapRenderDepthMin", position7.y);
			Vector3 position8 = base.Generator.transform.position;
			Shader.SetGlobalFloat("_HeightmapRenderDepthMax", position8.y - heightMin);
			renderingCamera.nearClipPlane = 0.01f;
			renderingCamera.farClipPlane = heightMin + heightMax;
			renderingCamera.targetTexture = temporary3;
			renderingCamera.RenderWithShader(HeightShader, "RenderType");
			CompareMaterial.SetTexture("_OverhangMaskTex", temporary);
			CompareMaterial.SetTexture("_HeightBelowSurfaceTex", temporary2);
			CompareMaterial.SetTexture("_HeightIntersectingTex", temporary3);
			Graphics.Blit(null, heightmap, CompareMaterial);
			RenderTexture.ReleaseTemporary(temporary);
			RenderTexture.ReleaseTemporary(temporary2);
			RenderTexture.ReleaseTemporary(temporary3);
			break;
		}
		case FluidDepth.Surface:
		{
			Vector3 position = base.Generator.transform.position;
			Shader.SetGlobalFloat("_HeightmapRenderDepthMax", position.y - heightMin);
			Vector3 position2 = base.Generator.transform.position;
			Shader.SetGlobalFloat("_HeightmapRenderDepthMin", position2.y + heightMax);
			renderingCamera.nearClipPlane = 0.001f;
			renderingCamera.farClipPlane = heightMin + heightMax;
			renderingCamera.targetTexture = heightmap;
			renderingCamera.RenderWithShader(HeightShader, "RenderType");
			break;
		}
		}
		Vector2 dimensions3 = base.Generator.Dimensions;
		float x2 = dimensions3.x;
		Vector2 dimensions4 = base.Generator.Dimensions;
		if (x2 != dimensions4.y)
		{
			RenderTexture temporary4 = RenderTexture.GetTemporary(resolutionX, resolutionY, 24, FlowmapGenerator.GetSingleChannelRTFormat, RenderTextureReadWrite.Linear);
			ResizeMaterial.SetTexture("_Heightmap", heightmap);
			Vector2 dimensions5 = base.Generator.Dimensions;
			float y = dimensions5.y;
			Vector2 dimensions6 = base.Generator.Dimensions;
			if (y > dimensions6.x)
			{
				Material material = ResizeMaterial;
				Vector2 dimensions7 = base.Generator.Dimensions;
				float x3 = dimensions7.x;
				Vector2 dimensions8 = base.Generator.Dimensions;
				material.SetVector("_AspectRatio", new Vector4(x3 / dimensions8.y, 1f, 0f, 0f));
			}
			else
			{
				Material material2 = ResizeMaterial;
				Vector2 dimensions9 = base.Generator.Dimensions;
				float x4 = dimensions9.x;
				Vector2 dimensions10 = base.Generator.Dimensions;
				material2.SetVector("_AspectRatio", new Vector4(1f, 1f / (x4 / dimensions10.y), 0f, 0f));
			}
			Graphics.Blit(null, temporary4, ResizeMaterial, 0);
			Graphics.Blit(temporary4, heightmap);
			RenderTexture.ReleaseTemporary(temporary4);
		}
	}

	protected override void OnDrawGizmosSelected()
	{
		base.OnDrawGizmosSelected();
		Vector3 center = base.transform.position + Vector3.up * (heightMax - heightMin) / 2f;
		Vector2 dimensions = base.Generator.Dimensions;
		float x = dimensions.x;
		float y = heightMax + heightMin;
		Vector2 dimensions2 = base.Generator.Dimensions;
		Gizmos.DrawWireCube(center, new Vector3(x, y, dimensions2.y));
	}
}
