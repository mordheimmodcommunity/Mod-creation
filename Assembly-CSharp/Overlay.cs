using UnityEngine;

[AddComponentMenu("Image Effects/Other/Overlay")]
[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class Overlay : MonoBehaviour
{
    public enum OverlayBlendMode
    {
        Additive,
        ScreenBlend,
        Multiply,
        Overlay,
        AlphaBlend
    }

    public OverlayBlendMode blendMode = OverlayBlendMode.Overlay;

    public float intensity = 1f;

    public float intensitySpeed = 1f;

    public Texture2D texture;

    public Shader overlayShader;

    private float currentIntensity;

    private Material mat;

    private Material overlayMaterial
    {
        get
        {
            if (mat == null)
            {
                mat = new Material(overlayShader);
                mat.hideFlags = HideFlags.HideAndDontSave;
            }
            return mat;
        }
    }

    private void Start()
    {
        currentIntensity = 0f;
        if (!SystemInfo.supportsImageEffects)
        {
            Object.DestroyImmediate(this);
            return;
        }
        Vector4 vector = new Vector4(1f, 0f, 0f, 1f);
        overlayMaterial.SetVector("_UV_Transform", vector);
        overlayMaterial.SetTexture("_Overlay", texture);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        overlayMaterial.SetFloat("_Intensity", currentIntensity);
        Graphics.Blit(source, destination, overlayMaterial, (int)blendMode);
    }
}
