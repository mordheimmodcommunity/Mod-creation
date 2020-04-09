using Flowmap;
using System;
using UnityEngine;

[ExecuteInEditMode]
public class FlowSimulationField : MonoBehaviour
{
	public static bool DrawFalloffTextures = true;

	public static bool DrawFalloffUnselected;

	public float strength = 1f;

	public Texture2D falloffTexture;

	protected Transform cachedTransform;

	protected Vector3 cachedPosition;

	protected Quaternion cachedRotation;

	protected Vector3 cachedScale;

	protected Vector2 falloffTextureDimensions;

	protected Color[] falloffTexturePixels;

	private bool initialized;

	protected bool wantsToDrawPreviewTexture;

	protected bool hasFalloffTexture;

	private Material falloffMaterial;

	[SerializeField]
	[HideInInspector]
	protected GpuRenderPlane renderPlane;

	public virtual FieldPass Pass => FieldPass.Force;

	protected virtual Shader RenderShader => null;

	public Material FalloffMaterial
	{
		get
		{
			if (!falloffMaterial)
			{
				falloffMaterial = new Material(RenderShader);
				falloffMaterial.hideFlags = HideFlags.HideAndDontSave;
				falloffMaterial.name = "FlowFieldFalloff";
			}
			if (falloffMaterial.shader != RenderShader)
			{
				falloffMaterial.shader = RenderShader;
			}
			return falloffMaterial;
		}
	}

	public GpuRenderPlane RenderPlane => renderPlane;

	protected void CreateMesh()
	{
		if ((bool)renderPlane && (bool)renderPlane.gameObject)
		{
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(renderPlane.gameObject);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(renderPlane.gameObject);
			}
		}
		if (!(this == null))
		{
			if (renderPlane == null)
			{
				GameObject gameObject = new GameObject(base.name + " render plane");
				gameObject.hideFlags = HideFlags.HideInHierarchy;
				gameObject.layer = FlowmapGenerator.GpuRenderLayer;
				renderPlane = gameObject.AddComponent<GpuRenderPlane>();
				renderPlane.field = this;
			}
			MeshFilter meshFilter = renderPlane.GetComponent<MeshFilter>();
			if (!meshFilter)
			{
				meshFilter = renderPlane.gameObject.AddComponent<MeshFilter>();
			}
			meshFilter.sharedMesh = Primitives.PlaneMesh;
			MeshRenderer meshRenderer = renderPlane.GetComponent<MeshRenderer>();
			if (!meshRenderer)
			{
				meshRenderer = renderPlane.gameObject.AddComponent<MeshRenderer>();
			}
			meshRenderer.material = FalloffMaterial;
			meshRenderer.enabled = false;
		}
	}

	private void Awake()
	{
		Init();
	}

	protected virtual void Update()
	{
		if (!initialized)
		{
			Init();
		}
		if (Application.isPlaying)
		{
			UpdateRenderPlane();
		}
	}

	public void DisableRenderPlane()
	{
		if ((bool)renderPlane)
		{
			renderPlane.GetComponent<Renderer>().enabled = false;
		}
	}

	public void DrawFalloffTextureEnabled(bool state)
	{
		wantsToDrawPreviewTexture = state;
	}

	public virtual void UpdateRenderPlane()
	{
		if (renderPlane == null || renderPlane.field != this)
		{
			CreateMesh();
		}
		renderPlane.transform.position = base.transform.position;
		renderPlane.transform.localScale = base.transform.lossyScale;
		renderPlane.transform.rotation = base.transform.rotation;
		FalloffMaterial.SetTexture("_MainTex", falloffTexture);
		FalloffMaterial.SetFloat("_Strength", strength);
		renderPlane.GetComponent<Renderer>().enabled = (DrawFalloffTextures && (wantsToDrawPreviewTexture || DrawFalloffUnselected) && base.enabled);
	}

	public virtual void Init()
	{
		if (!initialized)
		{
			cachedTransform = base.transform;
			CreateMesh();
			renderPlane.GetComponent<Renderer>().enabled = wantsToDrawPreviewTexture;
			cachedTransform = base.transform;
			cachedPosition = cachedTransform.position;
			cachedRotation = cachedTransform.rotation;
			cachedScale = cachedTransform.lossyScale;
			hasFalloffTexture = (falloffTexture != null);
			if ((bool)falloffTexture)
			{
				falloffTextureDimensions = new Vector2(falloffTexture.width, falloffTexture.height);
				falloffTexturePixels = falloffTexture.GetPixels();
			}
			else
			{
				falloffTextureDimensions = Vector2.zero;
			}
			initialized = true;
		}
	}

	public virtual void TickStart()
	{
		if (!base.enabled)
		{
			return;
		}
		switch (FlowmapGenerator.SimulationPath)
		{
		case SimulationPath.GPU:
			UpdateRenderPlane();
			FalloffMaterial.SetFloat("_Renderable", 1f);
			renderPlane.GetComponent<Renderer>().enabled = true;
			break;
		case SimulationPath.CPU:
			cachedTransform = base.transform;
			cachedPosition = cachedTransform.position;
			cachedRotation = cachedTransform.rotation;
			cachedScale = cachedTransform.lossyScale;
			hasFalloffTexture = (falloffTexture != null);
			if ((bool)falloffTexture)
			{
				falloffTextureDimensions = new Vector2(falloffTexture.width, falloffTexture.height);
				falloffTexturePixels = falloffTexture.GetPixels();
			}
			else
			{
				falloffTextureDimensions = Vector2.zero;
			}
			break;
		}
	}

	public virtual void TickEnd()
	{
		if (FlowmapGenerator.SimulationPath == SimulationPath.GPU)
		{
			UpdateRenderPlane();
			FalloffMaterial.SetFloat("_Renderable", 0f);
		}
	}

	public Vector2 GetUvScale(FlowmapGenerator generator)
	{
		float x = cachedScale.x;
		Vector2 dimensions = generator.Dimensions;
		float x2 = x / dimensions.x;
		float z = cachedScale.z;
		Vector2 dimensions2 = generator.Dimensions;
		return new Vector2(x2, z / dimensions2.y);
	}

	public Vector2 GetUvTransform(FlowmapGenerator generator)
	{
		Vector3 position = generator.Position;
		float num = position.x - cachedPosition.x;
		Vector2 dimensions = generator.Dimensions;
		float x = num / dimensions.x;
		Vector3 position2 = generator.Position;
		float num2 = position2.z - cachedPosition.z;
		Vector2 dimensions2 = generator.Dimensions;
		return new Vector2(x, num2 / dimensions2.y);
	}

	public float GetUvRotation(FlowmapGenerator generator)
	{
		Vector3 eulerAngles = cachedRotation.eulerAngles;
		return eulerAngles.y * (MathF.PI / 180f);
	}

	public float GetStrengthCpu(FlowmapGenerator generator, Vector2 uv)
	{
		Vector2 vector = TransformSampleUv(generator, uv, invertY: false);
		float num = strength;
		if (vector.x < 0f || vector.x > 1f || vector.y < 0f || vector.y > 1f)
		{
			num = 0f;
		}
		if (FlowmapGenerator.ThreadCount > 1)
		{
			float num2 = num;
			float num3;
			if (hasFalloffTexture)
			{
				Color color = TextureUtilities.SampleColorBilinear(falloffTexturePixels, (int)falloffTextureDimensions.x, (int)falloffTextureDimensions.y, vector.x, vector.y);
				num3 = color.r;
			}
			else
			{
				num3 = 1f;
			}
			return num2 * num3;
		}
		float num4 = num;
		float num5;
		if (hasFalloffTexture)
		{
			Color pixelBilinear = falloffTexture.GetPixelBilinear(vector.x, vector.y);
			num5 = pixelBilinear.r;
		}
		else
		{
			num5 = 1f;
		}
		return num4 * num5;
	}

	protected Vector2 TransformSampleUv(FlowmapGenerator generator, Vector2 uv, bool invertY)
	{
		Vector2 a = uv;
		float x = a.x;
		Vector2 uvTransform = GetUvTransform(generator);
		float x2 = x + uvTransform.x;
		float y = a.y;
		Vector2 uvTransform2 = GetUvTransform(generator);
		a = new Vector2(x2, y + uvTransform2.y);
		a -= Vector2.one * 0.5f;
		a = new Vector2(a.x * Mathf.Cos(GetUvRotation(generator)) - a.y * Mathf.Sin(GetUvRotation(generator)), a.x * Mathf.Sin(GetUvRotation(generator)) + a.y * Mathf.Cos(GetUvRotation(generator)));
		float x3 = a.x;
		Vector2 uvScale = GetUvScale(generator);
		float x4 = x3 / uvScale.x * (float)((!invertY) ? 1 : (-1));
		float y2 = a.y;
		Vector2 uvScale2 = GetUvScale(generator);
		a = new Vector2(x4, y2 / uvScale2.y * (float)((!invertY) ? 1 : (-1)));
		return a + Vector2.one * 0.5f;
	}

	protected virtual void OnDrawGizmosSelected()
	{
		Vector3 position = cachedTransform.position;
		Vector3 right = cachedTransform.right;
		Vector3 lossyScale = cachedTransform.lossyScale;
		Vector3 a = position + right * ((0f - lossyScale.x) / 2f);
		Vector3 forward = cachedTransform.forward;
		Vector3 lossyScale2 = cachedTransform.lossyScale;
		Vector3 from = a + forward * ((0f - lossyScale2.z) / 2f);
		Vector3 position2 = cachedTransform.position;
		Vector3 right2 = cachedTransform.right;
		Vector3 lossyScale3 = cachedTransform.lossyScale;
		Vector3 a2 = position2 + right2 * (lossyScale3.x / 2f);
		Vector3 forward2 = cachedTransform.forward;
		Vector3 lossyScale4 = cachedTransform.lossyScale;
		Vector3 vector = a2 + forward2 * ((0f - lossyScale4.z) / 2f);
		Vector3 position3 = cachedTransform.position;
		Vector3 right3 = cachedTransform.right;
		Vector3 lossyScale5 = cachedTransform.lossyScale;
		Vector3 a3 = position3 + right3 * ((0f - lossyScale5.x) / 2f);
		Vector3 forward3 = cachedTransform.forward;
		Vector3 lossyScale6 = cachedTransform.lossyScale;
		Vector3 vector2 = a3 + forward3 * (lossyScale6.z / 2f);
		Vector3 position4 = cachedTransform.position;
		Vector3 right4 = cachedTransform.right;
		Vector3 lossyScale7 = cachedTransform.lossyScale;
		Vector3 a4 = position4 + right4 * (lossyScale7.x / 2f);
		Vector3 forward4 = cachedTransform.forward;
		Vector3 lossyScale8 = cachedTransform.lossyScale;
		Vector3 to = a4 + forward4 * (lossyScale8.z / 2f);
		Gizmos.DrawLine(from, vector);
		Gizmos.DrawLine(vector2, to);
		Gizmos.DrawLine(from, vector2);
		Gizmos.DrawLine(vector, to);
		wantsToDrawPreviewTexture = true;
		UpdateRenderPlane();
	}

	protected virtual void OnDrawGizmos()
	{
		wantsToDrawPreviewTexture = false;
		UpdateRenderPlane();
	}

	private void OnDisable()
	{
		wantsToDrawPreviewTexture = false;
		if ((bool)renderPlane)
		{
			renderPlane.GetComponent<Renderer>().enabled = (DrawFalloffTextures && wantsToDrawPreviewTexture);
		}
	}

	private void OnDestroy()
	{
		Cleaup();
	}

	protected virtual void Cleaup()
	{
		if ((bool)renderPlane && (bool)renderPlane.gameObject)
		{
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(renderPlane.gameObject);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(renderPlane.gameObject);
			}
		}
		if ((bool)falloffMaterial)
		{
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(falloffMaterial);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(falloffMaterial);
			}
		}
	}
}
