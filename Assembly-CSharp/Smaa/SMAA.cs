using UnityEngine;

namespace Smaa
{
	[RequireComponent(typeof(Camera))]
	[AddComponentMenu("Image Effects/Subpixel Morphological Antialiasing")]
	[ExecuteInEditMode]
	public class SMAA : MonoBehaviour
	{
		public DebugPass DebugPass;

		public QualityPreset Quality = QualityPreset.High;

		public EdgeDetectionMethod DetectionMethod = EdgeDetectionMethod.Luma;

		public bool UsePredication;

		public Preset CustomPreset;

		public PredicationPreset CustomPredicationPreset;

		public Shader Shader;

		public Texture2D AreaTex;

		public Texture2D SearchTex;

		protected Camera m_Camera;

		protected Preset m_LowPreset;

		protected Preset m_MediumPreset;

		protected Preset m_HighPreset;

		protected Preset m_UltraPreset;

		protected Material m_Material;

		public Material Material
		{
			get
			{
				if (m_Material == null)
				{
					m_Material = new Material(Shader);
					m_Material.hideFlags = HideFlags.HideAndDontSave;
				}
				return m_Material;
			}
		}

		private void OnEnable()
		{
			if (AreaTex == null)
			{
				AreaTex = Resources.Load<Texture2D>("AreaTex");
			}
			if (SearchTex == null)
			{
				SearchTex = Resources.Load<Texture2D>("SearchTex");
			}
			m_Camera = GetComponent<Camera>();
		}

		private void Start()
		{
			if (!SystemInfo.supportsImageEffects)
			{
				base.enabled = false;
				return;
			}
			if (!Shader || !Shader.isSupported)
			{
				base.enabled = false;
			}
			CreatePresets();
		}

		private void OnDisable()
		{
			if (m_Material != null)
			{
				Object.DestroyImmediate(m_Material);
			}
		}

		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			int pixelWidth = m_Camera.pixelWidth;
			int pixelHeight = m_Camera.pixelHeight;
			Preset preset = CustomPreset;
			if (Quality == QualityPreset.Low)
			{
				preset = m_LowPreset;
			}
			else if (Quality == QualityPreset.Medium)
			{
				preset = m_MediumPreset;
			}
			else if (Quality == QualityPreset.High)
			{
				preset = m_HighPreset;
			}
			else if (Quality == QualityPreset.Ultra)
			{
				preset = m_UltraPreset;
			}
			int detectionMethod = (int)DetectionMethod;
			int pass = 4;
			int pass2 = 5;
			Material.SetTexture("_AreaTex", AreaTex);
			Material.SetTexture("_SearchTex", SearchTex);
			Material.SetTexture("_SourceTex", source);
			Material.SetVector("_Metrics", new Vector4(1f / (float)pixelWidth, 1f / (float)pixelHeight, pixelWidth, pixelHeight));
			Material.SetVector("_Params1", new Vector4(preset.Threshold, preset.DepthThreshold, preset.MaxSearchSteps, preset.MaxSearchStepsDiag));
			Material.SetVector("_Params2", new Vector2(preset.CornerRounding, preset.LocalContrastAdaptationFactor));
			Shader.DisableKeyword("USE_PREDICATION");
			if (DetectionMethod == EdgeDetectionMethod.Depth)
			{
				m_Camera.depthTextureMode |= DepthTextureMode.Depth;
			}
			else if (UsePredication)
			{
				m_Camera.depthTextureMode |= DepthTextureMode.Depth;
				Shader.EnableKeyword("USE_PREDICATION");
				Material.SetVector("_Params3", new Vector3(CustomPredicationPreset.Threshold, CustomPredicationPreset.Scale, CustomPredicationPreset.Strength));
			}
			Shader.DisableKeyword("USE_DIAG_SEARCH");
			Shader.DisableKeyword("USE_CORNER_DETECTION");
			if (preset.DiagDetection)
			{
				Shader.EnableKeyword("USE_DIAG_SEARCH");
			}
			if (preset.CornerDetection)
			{
				Shader.EnableKeyword("USE_CORNER_DETECTION");
			}
			RenderTexture renderTexture = TempRT(pixelWidth, pixelHeight);
			RenderTexture renderTexture2 = TempRT(pixelWidth, pixelHeight);
			Clear(renderTexture);
			Clear(renderTexture2);
			Graphics.Blit(source, renderTexture, Material, detectionMethod);
			if (DebugPass == DebugPass.Edges)
			{
				Graphics.Blit(renderTexture, destination);
			}
			else
			{
				Graphics.Blit(renderTexture, renderTexture2, Material, pass);
				if (DebugPass == DebugPass.Weights)
				{
					Graphics.Blit(renderTexture2, destination);
				}
				else
				{
					Graphics.Blit(renderTexture2, destination, Material, pass2);
				}
			}
			RenderTexture.ReleaseTemporary(renderTexture);
			RenderTexture.ReleaseTemporary(renderTexture2);
		}

		private void Clear(RenderTexture rt)
		{
			Graphics.Blit(rt, rt, Material, 0);
		}

		private RenderTexture TempRT(int width, int height)
		{
			int depthBuffer = 0;
			return RenderTexture.GetTemporary(width, height, depthBuffer, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		}

		private void CreatePresets()
		{
			m_LowPreset = new Preset
			{
				Threshold = 0.15f,
				MaxSearchSteps = 4
			};
			m_LowPreset.DiagDetection = false;
			m_LowPreset.CornerDetection = false;
			m_MediumPreset = new Preset
			{
				Threshold = 0.1f,
				MaxSearchSteps = 8
			};
			m_MediumPreset.DiagDetection = false;
			m_MediumPreset.CornerDetection = false;
			m_HighPreset = new Preset
			{
				Threshold = 0.1f,
				MaxSearchSteps = 16,
				MaxSearchStepsDiag = 8,
				CornerRounding = 25
			};
			m_UltraPreset = new Preset
			{
				Threshold = 0.05f,
				MaxSearchSteps = 32,
				MaxSearchStepsDiag = 16,
				CornerRounding = 25
			};
		}
	}
}
