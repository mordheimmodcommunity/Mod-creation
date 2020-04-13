using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class BloodSplatter : MonoBehaviour
{
    public enum OverlayBlendMode
    {
        Additive,
        ScreenBlend,
        Multiply,
        Overlay,
        AlphaBlend
    }

    public float intensity = 1f;

    public OverlayBlendMode blendMode = OverlayBlendMode.Overlay;

    public Shader overlayShader;

    public int frameByImage;

    public int frameBySplatter;

    public List<Texture2D> splatters;

    private Material mat;

    private int currentCount;

    private int maxCount;

    private int offset;

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
        if (!SystemInfo.supportsImageEffects)
        {
            Object.DestroyImmediate(this);
            return;
        }
        maxCount = frameBySplatter * frameByImage;
        Vector4 vector = new Vector4(1f, 0f, 0f, 1f);
        overlayMaterial.SetVector("_UV_Transform", vector);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (currentCount < maxCount)
        {
            overlayMaterial.SetFloat("_Intensity", intensity);
            int index = (int)((float)currentCount / (float)frameByImage) + offset * frameBySplatter;
            overlayMaterial.SetTexture("_Overlay", splatters[index]);
            Graphics.Blit(source, destination, overlayMaterial, (int)blendMode);
            currentCount++;
            currentCount = ((currentCount >= maxCount) ? (maxCount - 1) : currentCount);
        }
    }

    public void Activate()
    {
        offset = 0;
        currentCount = 0;
        base.enabled = true;
    }

    public void Deactivate()
    {
        base.enabled = false;
    }
}
