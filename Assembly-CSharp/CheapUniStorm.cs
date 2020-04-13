using UnityEngine;

public class CheapUniStorm : MonoBehaviour
{
    public float cloudSpeed;

    public float heavyCloudSpeed;

    public GameObject lightClouds1;

    public GameObject lightClouds2;

    public GameObject lightClouds3;

    public GameObject lightClouds4;

    public GameObject lightClouds5;

    public GameObject highClouds1;

    public GameObject highClouds2;

    public GameObject mostlyCloudyClouds;

    public GameObject heavyClouds;

    public GameObject heavyCloudsLayer1;

    public GameObject heavyCloudsLayerLight;

    public GameObject starSphere;

    public Color starBrightness;

    public GameObject horizonObject;

    public GameObject rainDecal;

    private Renderer heavyCloudsRenderer;

    private Renderer heavyCloudsLayer1Renderer;

    private Renderer heavyCloudsLayerLightRenderer;

    private Renderer lightClouds1Renderer;

    private Renderer lightClouds2Renderer;

    private Renderer lightClouds3Renderer;

    private Renderer lightClouds4Renderer;

    private Renderer lightClouds5Renderer;

    private Renderer highClouds1Renderer;

    private Renderer highClouds2Renderer;

    private Renderer mostlyCloudyCloudsRenderer;

    private Renderer starSphereRenderer;

    private Renderer rainDecalRenderer;

    private void Start()
    {
        if (heavyClouds != null)
        {
            heavyCloudsRenderer = heavyClouds.GetComponent<Renderer>();
        }
        if (heavyCloudsLayer1 != null)
        {
            heavyCloudsLayer1Renderer = heavyCloudsLayer1.GetComponent<Renderer>();
        }
        if (heavyCloudsLayerLight != null)
        {
            heavyCloudsLayerLightRenderer = heavyCloudsLayerLight.GetComponent<Renderer>();
        }
        if (lightClouds1 != null)
        {
            lightClouds1Renderer = lightClouds1.GetComponent<Renderer>();
        }
        if (lightClouds2 != null)
        {
            lightClouds2Renderer = lightClouds2.GetComponent<Renderer>();
        }
        if (lightClouds3 != null)
        {
            lightClouds3Renderer = lightClouds3.GetComponent<Renderer>();
        }
        if (lightClouds4 != null)
        {
            lightClouds4Renderer = lightClouds4.GetComponent<Renderer>();
        }
        if (lightClouds5 != null)
        {
            lightClouds5Renderer = lightClouds5.GetComponent<Renderer>();
        }
        if (highClouds1 != null)
        {
            highClouds1Renderer = highClouds1.GetComponent<Renderer>();
        }
        if (highClouds2 != null)
        {
            highClouds2Renderer = highClouds2.GetComponent<Renderer>();
        }
        if (mostlyCloudyClouds != null)
        {
            mostlyCloudyCloudsRenderer = mostlyCloudyClouds.GetComponent<Renderer>();
        }
        if (starSphere != null)
        {
            starSphereRenderer = starSphere.GetComponent<Renderer>();
        }
        if (rainDecal != null)
        {
            rainDecalRenderer = rainDecal.GetComponent<Renderer>();
        }
    }

    private void Update()
    {
        float num = cloudSpeed * 0.001f;
        float num2 = heavyCloudSpeed * 0.001f;
        float num3 = 0.003f;
        float num4 = 0.004f;
        float num5 = Time.time * num;
        float num6 = Time.time * num2;
        float y = Time.time * num3;
        float y2 = Time.time * num;
        float num7 = Time.time * num4;
        if (heavyClouds != null && heavyClouds.gameObject.activeSelf)
        {
            heavyCloudsRenderer.sharedMaterial.SetVector("_Offset", new Vector4(num6, num6, 0f, 0f));
        }
        if (heavyCloudsLayer1 != null && heavyCloudsLayer1.gameObject.activeSelf)
        {
            heavyCloudsLayer1Renderer.sharedMaterial.SetVector("_Offset", new Vector4(0f, num6, 0f, 0f));
        }
        if (heavyCloudsLayerLight != null && heavyCloudsLayerLight.gameObject.activeSelf)
        {
            heavyCloudsLayerLightRenderer.sharedMaterial.SetVector("_Offset", new Vector4(0f, num6, 0f, 0f));
        }
        if (lightClouds1 != null && lightClouds1.gameObject.activeSelf)
        {
            lightClouds1Renderer.sharedMaterial.SetVector("_Offset", new Vector4(0f, num5, 0f, 0f));
        }
        if (lightClouds2 != null && lightClouds2.gameObject.activeSelf)
        {
            lightClouds2Renderer.sharedMaterial.SetVector("_Offset", new Vector4(num5, num5, 0f, 0f));
        }
        if (lightClouds3 != null && lightClouds3.gameObject.activeSelf)
        {
            lightClouds3Renderer.sharedMaterial.SetVector("_Offset", new Vector4(0f, num5, 0f, 0f));
        }
        if (lightClouds4 != null && lightClouds4.gameObject.activeSelf)
        {
            lightClouds4Renderer.sharedMaterial.SetVector("_Offset", new Vector4(num5, num5, 0f, 0f));
        }
        if (lightClouds5 != null && lightClouds5.gameObject.activeSelf)
        {
            lightClouds5Renderer.sharedMaterial.SetVector("_Offset", new Vector4(0f, num5, 0f, 0f));
        }
        if (highClouds1 != null && highClouds1.gameObject.activeSelf)
        {
            highClouds1Renderer.sharedMaterial.SetVector("_Offset", new Vector4(0f, y2, 0f, 0f));
        }
        if (highClouds2 != null && highClouds2.gameObject.activeSelf)
        {
            highClouds2Renderer.sharedMaterial.SetVector("_Offset", new Vector4(0f, y2, 0f, 0f));
        }
        if (mostlyCloudyClouds != null && mostlyCloudyClouds.gameObject.activeSelf)
        {
            mostlyCloudyCloudsRenderer.sharedMaterial.SetVector("_Offset", new Vector4(0f, y, 0f, 0f));
        }
        if (starSphere != null && starSphere.gameObject.activeSelf)
        {
            starSphereRenderer.sharedMaterial.SetVector("_Offset", new Vector4(num7 / 2f, 0.02f, 0f, 0f));
            starSphereRenderer.sharedMaterial.SetColor("_TintColor", starBrightness);
        }
        if (rainDecal != null && rainDecal.gameObject.activeSelf)
        {
            rainDecalRenderer.sharedMaterial.SetVector("_Offset", new Vector4(0f, num5 * 10f, 0f, 0f));
        }
    }
}
