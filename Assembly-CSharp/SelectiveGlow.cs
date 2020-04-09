using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Heathen/Image Effects/Selective Glow")]
[RequireComponent(typeof(Camera))]
public class SelectiveGlow : MonoBehaviour
{
	public bool UseFastBlur;

	[Range(1f, 10f)]
	public int SampleDivision = 4;

	[Range(1f, 20f)]
	public int iterations = 5;

	[Range(0f, 1f)]
	public float blurSpread = 0.6f;

	[Range(0f, 25f)]
	public float Intensity = 4f;

	public Shader compositeShader;

	public Shader renderGlowShader;

	public Shader blurShader;

	public Shader fastBlurShader;

	private Material m_Material;

	private Material m_CompositeMaterial;

	private RenderTexture renderTexture;

	private GameObject shaderCamera;

	private Material GetMaterial()
	{
		if (m_Material == null || (Application.isEditor && !Application.isPlaying))
		{
			if (UseFastBlur)
			{
				m_Material = new Material(fastBlurShader);
			}
			else
			{
				m_Material = new Material(blurShader);
			}
			m_Material.hideFlags = HideFlags.HideAndDontSave;
		}
		return m_Material;
	}

	private Material GetCompositeMaterial()
	{
		if (m_CompositeMaterial == null)
		{
			m_CompositeMaterial = new Material(compositeShader);
			m_CompositeMaterial.hideFlags = HideFlags.HideAndDontSave;
		}
		return m_CompositeMaterial;
	}

	private void OnDisable()
	{
		if ((bool)m_Material)
		{
			Object.DestroyImmediate(m_Material);
		}
		Object.DestroyImmediate(m_CompositeMaterial);
		Object.DestroyImmediate(shaderCamera);
		if (renderTexture != null)
		{
			RenderTexture.ReleaseTemporary(renderTexture);
			renderTexture = null;
		}
	}

	private void Start()
	{
		if (compositeShader == null)
		{
			base.enabled = false;
			Debug.LogWarning("Composite Shader is not assigned");
		}
		else if (renderGlowShader == null)
		{
			base.enabled = false;
			Debug.LogWarning("Render Glow Shader is not assigned");
		}
		else if (blurShader == null)
		{
			base.enabled = false;
			Debug.LogWarning("Blur Shader is not assigned");
		}
		else if (!SystemInfo.supportsImageEffects)
		{
			base.enabled = false;
			Debug.LogWarning("Image Effects are not supported");
		}
		else if (!GetMaterial().shader.isSupported)
		{
			base.enabled = false;
			Debug.LogWarning("Blur shader can't run on the users graphics card");
		}
	}

	private void OnPreRender()
	{
		if (base.enabled && base.gameObject.activeSelf)
		{
			if (renderTexture != null)
			{
				RenderTexture.ReleaseTemporary(renderTexture);
				renderTexture = null;
			}
			Debug.Log("Camera rect = " + GetComponent<Camera>().pixelWidth.ToString() + "," + GetComponent<Camera>().pixelHeight + " screen = " + Screen.width.ToString() + "," + Screen.height.ToString());
			renderTexture = RenderTexture.GetTemporary((int)((float)Screen.width * (1f / GetComponent<Camera>().rect.height)), (int)((float)Screen.height * (1f / GetComponent<Camera>().rect.width)), 16);
			if (!shaderCamera)
			{
				shaderCamera = new GameObject("ShaderCamera", typeof(Camera));
				shaderCamera.GetComponent<Camera>().enabled = false;
				shaderCamera.hideFlags = HideFlags.HideAndDontSave;
				shaderCamera.GetComponent<Camera>().rect = GetComponent<Camera>().rect;
			}
			Camera component = shaderCamera.GetComponent<Camera>();
			component.CopyFrom(GetComponent<Camera>());
			component.rect = new Rect(0f, 0f, 1f, 1f);
			component.backgroundColor = new Color(0f, 0f, 0f, 0f);
			component.clearFlags = CameraClearFlags.Color;
			component.targetTexture = renderTexture;
			component.RenderWithShader(renderGlowShader, "RenderType");
		}
	}

	private void FourTapCone(RenderTexture source, RenderTexture dest, int iteration)
	{
		RenderTexture.active = dest;
		source.SetGlobalShaderProperty("__RenderTex");
		Material material = GetMaterial();
		if (UseFastBlur)
		{
			float num = 1f / (1f * (float)(1 << SampleDivision));
			float num2 = (float)iteration * 1f;
			material.SetVector("_Parameter", new Vector4(blurSpread * num + num2, (0f - blurSpread) * num - num2, 0f, 0f));
			RenderTexture temporary = RenderTexture.GetTemporary(dest.width, dest.height, 0, dest.format);
			temporary.filterMode = FilterMode.Bilinear;
			dest.filterMode = FilterMode.Bilinear;
			Graphics.Blit(source, temporary, material, 1);
			Graphics.Blit(temporary, dest, material, 2);
			RenderTexture.ReleaseTemporary(temporary);
		}
		else
		{
			float num3 = 0.5f + (float)iteration * blurSpread;
			Graphics.BlitMultiTap(source, dest, material, new Vector2(num3, num3), new Vector2(0f - num3, num3), new Vector2(num3, 0f - num3), new Vector2(0f - num3, 0f - num3));
		}
	}

	private void DownSample4x(RenderTexture source, RenderTexture dest)
	{
		RenderTexture.active = dest;
		source.SetGlobalShaderProperty("__RenderTex");
		Material material = GetMaterial();
		float num = 1f;
		if (UseFastBlur)
		{
			float num2 = 1f / (1f * (float)(1 << SampleDivision));
			material.SetVector("_Parameter", new Vector4(blurSpread * num2, (0f - blurSpread) * num2, 0f, 0f));
			Graphics.Blit(source, dest, material, 0);
		}
		else
		{
			Graphics.BlitMultiTap(source, dest, material, new Vector2(num, num), new Vector2(0f - num, num), new Vector2(num, 0f - num), new Vector2(0f - num, 0f - num));
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (base.enabled && base.gameObject.activeSelf)
		{
			int width = source.width / SampleDivision;
			int height = source.height / SampleDivision;
			RenderTexture renderTexture = RenderTexture.GetTemporary(width, height, 0);
			DownSample4x(this.renderTexture, renderTexture);
			for (int i = 0; i < iterations; i++)
			{
				RenderTexture temporary = RenderTexture.GetTemporary(width, height, 0);
				FourTapCone(renderTexture, temporary, i);
				RenderTexture.ReleaseTemporary(renderTexture);
				renderTexture = temporary;
			}
			Material compositeMaterial = GetCompositeMaterial();
			compositeMaterial.SetTexture("_BlurTex", renderTexture);
			compositeMaterial.SetTexture("_BlurRamp", this.renderTexture);
			compositeMaterial.SetFloat("_Outter", Intensity);
			Graphics.Blit(source, destination, compositeMaterial);
			RenderTexture.ReleaseTemporary(renderTexture);
			if (this.renderTexture != null)
			{
				RenderTexture.ReleaseTemporary(this.renderTexture);
				this.renderTexture = null;
			}
		}
	}
}
