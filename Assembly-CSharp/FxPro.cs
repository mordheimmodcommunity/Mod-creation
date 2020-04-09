using FxProNS;
using UnityEngine;

[AddComponentMenu("Image Effects/FxPro™")]
[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class FxPro : MonoBehaviour
{
	private const bool VisualizeLensCurvature = false;

	public EffectsQuality Quality = EffectsQuality.Normal;

	private static Material _mat;

	private static Material _tapMat;

	public bool BloomEnabled = true;

	public BloomHelperParams BloomParams = new BloomHelperParams();

	public bool VisualizeBloom;

	public Texture2D LensDirtTexture;

	[Range(0f, 2f)]
	public float LensDirtIntensity = 1f;

	public bool ChromaticAberration = true;

	public bool ChromaticAberrationPrecise;

	[Range(1f, 2.5f)]
	public float ChromaticAberrationOffset = 1f;

	[Range(0f, 1f)]
	public float SCurveIntensity = 0.5f;

	public bool LensCurvatureEnabled = true;

	[Range(1f, 2f)]
	public float LensCurvaturePower = 1.1f;

	public bool LensCurvaturePrecise;

	[Range(0f, 1f)]
	public float FilmGrainIntensity = 0.5f;

	[Range(1f, 10f)]
	public float FilmGrainTiling = 4f;

	[Range(0f, 1f)]
	public float VignettingIntensity = 0.5f;

	public bool DOFEnabled = true;

	public bool BlurCOCTexture = true;

	public DOFHelperParams DOFParams = new DOFHelperParams();

	public bool VisualizeCOC;

	private Texture2D _gridTexture;

	public bool ColorEffectsEnabled = true;

	public Color CloseTint = new Color(1f, 0.5f, 0f, 1f);

	public Color FarTint = new Color(0f, 0f, 1f, 1f);

	[Range(0f, 1f)]
	public float CloseTintStrength = 0.5f;

	[Range(0f, 1f)]
	public float FarTintStrength = 0.5f;

	[Range(0f, 2f)]
	public float DesaturateDarksStrength = 0.5f;

	[Range(0f, 1f)]
	public float DesaturateFarObjsStrength = 0.5f;

	public Color FogTint = Color.white;

	[Range(0f, 1f)]
	public float FogStrength = 0.5f;

	public static Material Mat
	{
		get
		{
			if (null == _mat)
			{
				Material material = new Material(Shader.Find("Hidden/FxPro"));
				material.hideFlags = HideFlags.HideAndDontSave;
				_mat = material;
			}
			return _mat;
		}
	}

	private static Material TapMat
	{
		get
		{
			if (null == _tapMat)
			{
				Material material = new Material(Shader.Find("Hidden/FxProTap"));
				material.hideFlags = HideFlags.HideAndDontSave;
				_tapMat = material;
			}
			return _tapMat;
		}
	}

	public void Start()
	{
		if (!SystemInfo.supportsImageEffects || !SystemInfo.supportsRenderTextures)
		{
			Debug.LogError("Image effects are not supported on this platform.");
			base.enabled = false;
		}
	}

	public void Init(bool searchForNonDepthmapAlphaObjects)
	{
		Mat.SetFloat("_DirtIntensity", Mathf.Exp(LensDirtIntensity) - 1f);
		if (null == LensDirtTexture || LensDirtIntensity <= 0f)
		{
			Mat.DisableKeyword("LENS_DIRT_ON");
			Mat.EnableKeyword("LENS_DIRT_OFF");
		}
		else
		{
			Mat.SetTexture("_LensDirtTex", LensDirtTexture);
			Mat.EnableKeyword("LENS_DIRT_ON");
			Mat.DisableKeyword("LENS_DIRT_OFF");
		}
		if (ChromaticAberration)
		{
			Mat.EnableKeyword("CHROMATIC_ABERRATION_ON");
			Mat.DisableKeyword("CHROMATIC_ABERRATION_OFF");
		}
		else
		{
			Mat.EnableKeyword("CHROMATIC_ABERRATION_OFF");
			Mat.DisableKeyword("CHROMATIC_ABERRATION_ON");
		}
		if (GetComponent<Camera>().hdr)
		{
			Shader.EnableKeyword("FXPRO_HDR_ON");
			Shader.DisableKeyword("FXPRO_HDR_OFF");
		}
		else
		{
			Shader.EnableKeyword("FXPRO_HDR_OFF");
			Shader.DisableKeyword("FXPRO_HDR_ON");
		}
		Mat.SetFloat("_SCurveIntensity", SCurveIntensity);
		if (DOFEnabled)
		{
			if (null == DOFParams.EffectCamera)
			{
				DOFParams.EffectCamera = GetComponent<Camera>();
			}
			DOFParams.DepthCompression = Mathf.Clamp(DOFParams.DepthCompression, 2f, 8f);
			Singleton<DOFHelper>.Instance.SetParams(DOFParams);
			Singleton<DOFHelper>.Instance.Init(searchForNonDepthmapAlphaObjects);
			Mat.DisableKeyword("DOF_DISABLED");
			Mat.EnableKeyword("DOF_ENABLED");
			if (!DOFParams.DoubleIntensityBlur)
			{
				Singleton<DOFHelper>.Instance.SetBlurRadius((Quality != EffectsQuality.Fastest && Quality != EffectsQuality.Fast) ? 5 : 3);
			}
			else
			{
				Singleton<DOFHelper>.Instance.SetBlurRadius((Quality != EffectsQuality.Fastest && Quality != EffectsQuality.Fast) ? 10 : 5);
			}
		}
		else
		{
			Mat.EnableKeyword("DOF_DISABLED");
			Mat.DisableKeyword("DOF_ENABLED");
		}
		if (BloomEnabled)
		{
			BloomParams.Quality = Quality;
			Singleton<BloomHelper>.Instance.SetParams(BloomParams);
			Singleton<BloomHelper>.Instance.Init();
			Mat.DisableKeyword("BLOOM_DISABLED");
			Mat.EnableKeyword("BLOOM_ENABLED");
		}
		else
		{
			Mat.EnableKeyword("BLOOM_DISABLED");
			Mat.DisableKeyword("BLOOM_ENABLED");
		}
		if (LensCurvatureEnabled)
		{
			UpdateLensCurvatureZoom();
			Mat.SetFloat("_LensCurvatureBarrelPower", LensCurvaturePower);
		}
		if (FilmGrainIntensity >= 0.001f)
		{
			Mat.SetFloat("_FilmGrainIntensity", FilmGrainIntensity);
			Mat.SetFloat("_FilmGrainTiling", FilmGrainTiling);
			Mat.EnableKeyword("FILM_GRAIN_ON");
			Mat.DisableKeyword("FILM_GRAIN_OFF");
		}
		else
		{
			Mat.EnableKeyword("FILM_GRAIN_OFF");
			Mat.DisableKeyword("FILM_GRAIN_ON");
		}
		if (VignettingIntensity <= 1f)
		{
			Mat.SetFloat("_VignettingIntensity", VignettingIntensity);
			Mat.EnableKeyword("VIGNETTING_ON");
			Mat.DisableKeyword("VIGNETTING_OFF");
		}
		else
		{
			Mat.EnableKeyword("VIGNETTING_OFF");
			Mat.DisableKeyword("VIGNETTING_ON");
		}
		Mat.SetFloat("_ChromaticAberrationOffset", ChromaticAberrationOffset);
		if (ColorEffectsEnabled)
		{
			Mat.EnableKeyword("COLOR_FX_ON");
			Mat.DisableKeyword("COLOR_FX_OFF");
			Mat.SetColor("_CloseTint", CloseTint);
			Mat.SetColor("_FarTint", FarTint);
			Mat.SetFloat("_CloseTintStrength", CloseTintStrength);
			Mat.SetFloat("_FarTintStrength", FarTintStrength);
			Mat.SetFloat("_DesaturateDarksStrength", DesaturateDarksStrength);
			Mat.SetFloat("_DesaturateFarObjsStrength", DesaturateFarObjsStrength);
			Mat.SetColor("_FogTint", FogTint);
			Mat.SetFloat("_FogStrength", FogStrength);
		}
		else
		{
			Mat.EnableKeyword("COLOR_FX_OFF");
			Mat.DisableKeyword("COLOR_FX_ON");
		}
	}

	public void OnEnable()
	{
		Init(searchForNonDepthmapAlphaObjects: true);
	}

	public void OnDisable()
	{
		if (null != Mat)
		{
			Object.DestroyImmediate(Mat);
		}
		RenderTextureManager.Instance.Dispose();
		Singleton<DOFHelper>.Instance.Dispose();
		Singleton<BloomHelper>.Instance.Dispose();
	}

	public void OnValidate()
	{
		Init(searchForNonDepthmapAlphaObjects: false);
	}

	public static RenderTexture DownsampleTex(RenderTexture input, float downsampleBy)
	{
		RenderTexture renderTexture = RenderTextureManager.Instance.RequestRenderTexture(Mathf.RoundToInt((float)input.width / downsampleBy), Mathf.RoundToInt((float)input.height / downsampleBy), input.depth, input.format);
		renderTexture.filterMode = FilterMode.Bilinear;
		Graphics.BlitMultiTap(input, renderTexture, TapMat, new Vector2(-1f, -1f), new Vector2(-1f, 1f), new Vector2(1f, 1f), new Vector2(1f, -1f));
		return renderTexture;
	}

	private RenderTexture ApplyColorEffects(RenderTexture input)
	{
		if (!ColorEffectsEnabled)
		{
			return input;
		}
		RenderTexture renderTexture = RenderTextureManager.Instance.RequestRenderTexture(input.width, input.height, input.depth, input.format);
		Graphics.Blit(input, renderTexture, Mat, 5);
		return renderTexture;
	}

	private RenderTexture ApplyLensCurvature(RenderTexture input)
	{
		if (!LensCurvatureEnabled)
		{
			return input;
		}
		RenderTexture renderTexture = RenderTextureManager.Instance.RequestRenderTexture(input.width, input.height, input.depth, input.format);
		Graphics.Blit(input, renderTexture, Mat, (!LensCurvaturePrecise) ? 4 : 3);
		return renderTexture;
	}

	private RenderTexture ApplyChromaticAberration(RenderTexture input)
	{
		if (!ChromaticAberration)
		{
			return null;
		}
		RenderTexture renderTexture = RenderTextureManager.Instance.RequestRenderTexture(input.width, input.height, input.depth, input.format);
		renderTexture.filterMode = FilterMode.Bilinear;
		Graphics.Blit(input, renderTexture, Mat, 2);
		Mat.SetTexture("_ChromAberrTex", renderTexture);
		return renderTexture;
	}

	private Vector2 ApplyLensCurvature(Vector2 uv, float barrelPower, bool precise)
	{
		uv = uv * 2f - Vector2.one;
		uv.x *= GetComponent<Camera>().aspect * 2f;
		float f = Mathf.Atan2(uv.y, uv.x);
		float magnitude = uv.magnitude;
		magnitude = ((!precise) ? Mathf.Lerp(magnitude, magnitude * magnitude, Mathf.Clamp01(barrelPower - 1f)) : Mathf.Pow(magnitude, barrelPower));
		uv.x = magnitude * Mathf.Cos(f);
		uv.y = magnitude * Mathf.Sin(f);
		uv.x /= GetComponent<Camera>().aspect * 2f;
		return 0.5f * (uv + Vector2.one);
	}

	private void UpdateLensCurvatureZoom()
	{
		Vector2 vector = ApplyLensCurvature(new Vector2(1f, 1f), LensCurvaturePower, LensCurvaturePrecise);
		float value = 1f / vector.x;
		Mat.SetFloat("_LensCurvatureZoom", value);
	}

	private void RenderEffects(RenderTexture source, RenderTexture destination)
	{
		source.filterMode = FilterMode.Bilinear;
		RenderTexture tex = source;
		RenderTexture a = source;
		RenderTexture a2 = ApplyColorEffects(source);
		RenderTextureManager.Instance.SafeAssign(ref a2, ApplyLensCurvature(a2));
		if (ChromaticAberrationPrecise)
		{
			tex = ApplyChromaticAberration(a2);
		}
		RenderTextureManager.Instance.SafeAssign(ref a, DownsampleTex(a2, 2f));
		if (Quality == EffectsQuality.Fastest)
		{
			RenderTextureManager.Instance.SafeAssign(ref a, DownsampleTex(a, 2f));
		}
		RenderTexture renderTexture = null;
		RenderTexture renderTexture2 = null;
		if (DOFEnabled)
		{
			if (null == DOFParams.EffectCamera)
			{
				Debug.LogError("null == DOFParams.camera");
				return;
			}
			renderTexture = RenderTextureManager.Instance.RequestRenderTexture(a.width, a.height, a.depth, a.format);
			Singleton<DOFHelper>.Instance.RenderCOCTexture(a, renderTexture, (!BlurCOCTexture) ? 0f : 1.5f);
			if (VisualizeCOC)
			{
				Graphics.Blit(renderTexture, destination, DOFHelper.Mat, 3);
				RenderTextureManager.Instance.ReleaseRenderTexture(renderTexture);
				RenderTextureManager.Instance.ReleaseRenderTexture(a);
				return;
			}
			renderTexture2 = RenderTextureManager.Instance.RequestRenderTexture(a.width, a.height, a.depth, a.format);
			Singleton<DOFHelper>.Instance.RenderDOFBlur(a, renderTexture2, renderTexture);
			Mat.SetTexture("_DOFTex", renderTexture2);
			Mat.SetTexture("_COCTex", renderTexture);
			Graphics.Blit(renderTexture2, destination);
		}
		if (!ChromaticAberrationPrecise)
		{
			tex = ApplyChromaticAberration(a);
		}
		if (BloomEnabled)
		{
			RenderTexture renderTexture3 = RenderTextureManager.Instance.RequestRenderTexture(a.width, a.height, a.depth, a.format);
			Singleton<BloomHelper>.Instance.RenderBloomTexture(a, renderTexture3);
			Mat.SetTexture("_BloomTex", renderTexture3);
			if (VisualizeBloom)
			{
				Graphics.Blit(renderTexture3, destination);
				return;
			}
		}
		Graphics.Blit(a2, destination, Mat, 0);
		RenderTextureManager.Instance.ReleaseRenderTexture(renderTexture);
		RenderTextureManager.Instance.ReleaseRenderTexture(renderTexture2);
		RenderTextureManager.Instance.ReleaseRenderTexture(a);
		RenderTextureManager.Instance.ReleaseRenderTexture(tex);
	}

	[ImageEffectTransformsToLDR]
	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		RenderEffects(source, destination);
		RenderTextureManager.Instance.ReleaseAllRenderTextures();
	}
}
