using mset;
using System.Collections.Generic;
using UnityEngine;

public class Apollo : MonoBehaviour
{
    public bool initialize;

    public bool normal;

    public bool spec;

    public bool final;

    public bool lightOn = true;

    public bool matColorsOn = true;

    public bool specColorsOn = true;

    public bool specDataOn = true;

    private int selectedSky;

    public Sky lightGreySky;

    public Sky darkGreySky;

    private Renderer[] meshes;

    private Color[] colors;

    private Color[] colors2;

    private Color[] colors3;

    private Color[] specColors;

    private Color[] specColors2;

    private Color[] specColors3;

    private float[] intensity;

    private float[] intensity2;

    private float[] intensity3;

    private float[] sharpness;

    private float[] sharpness2;

    private float[] sharpness3;

    private float[] fresnel;

    private float[] fresnel2;

    private float[] fresnel3;

    private Texture[] diffTextures;

    private Texture[] diffTextures2;

    private Texture[] diffTextures3;

    private Texture[] specTextures;

    private Texture[] specTextures2;

    private Texture[] specTextures3;

    private Texture[] glowTextures;

    private Texture[] normalTextures;

    private Texture[] normalTextures2;

    private Texture[] normalTextures3;

    private Texture2D greyTex;

    private Texture2D blackSpecTex;

    private Texture2D blackTex;

    private Light[] lights;

    private List<Sky> skies;

    private bool once = true;

    private ApolloCameraPlacer camPlacer;

    public GameObject[] environments;

    private int curEnv;

    public GameObject rotatingLight;

    private void Start()
    {
        RenderSettings.ambientLight = Color.black;
        greyTex = new Texture2D(16, 16);
        Color color = new Color(0.73f, 0.73f, 0.73f, 1f);
        Color[] pixels = greyTex.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }
        greyTex.SetPixels(pixels);
        greyTex.Apply(updateMipmaps: true);
        blackSpecTex = new Texture2D(16, 16);
        Color color2 = new Color(0f, 0f, 0f, 0f);
        pixels = blackSpecTex.GetPixels();
        for (int j = 0; j < pixels.Length; j++)
        {
            pixels[j] = color2;
        }
        blackSpecTex.SetPixels(pixels);
        blackSpecTex.Apply(updateMipmaps: true);
        blackTex = new Texture2D(16, 16);
        color2 = new Color(0f, 0f, 0f, 1f);
        pixels = blackTex.GetPixels();
        for (int k = 0; k < pixels.Length; k++)
        {
            pixels[k] = color2;
        }
        blackTex.SetPixels(pixels);
        blackTex.Apply(updateMipmaps: true);
        initialize = false;
        normal = false;
        spec = false;
        final = false;
        lightOn = true;
        GameObject gameObject = GameObject.Find("clouds");
        skies = new List<Sky>();
        if (gameObject != null)
        {
            skies.AddRange(gameObject.GetComponentsInChildren<Sky>(includeInactive: true));
            skies.Add(lightGreySky);
            skies.Add(darkGreySky);
        }
        else
        {
            skies.AddRange(Object.FindObjectsOfType<Sky>());
        }
        camPlacer = Object.FindObjectOfType<ApolloCameraPlacer>();
    }

    private void Update()
    {
        if (once || initialize)
        {
            once = false;
            initialize = false;
            PandoraSingleton<PandoraInput>.Instance.SetCurrentState(PandoraInput.States.MENU, showMouse: true);
            meshes = (Object.FindObjectsOfType(typeof(Renderer)) as Renderer[]);
            if (meshes != null)
            {
                colors = new Color[meshes.Length];
                colors2 = new Color[meshes.Length];
                colors3 = new Color[meshes.Length];
                specColors = new Color[meshes.Length];
                specColors2 = new Color[meshes.Length];
                specColors3 = new Color[meshes.Length];
                intensity = new float[meshes.Length];
                intensity2 = new float[meshes.Length];
                intensity3 = new float[meshes.Length];
                sharpness = new float[meshes.Length];
                sharpness2 = new float[meshes.Length];
                sharpness3 = new float[meshes.Length];
                fresnel = new float[meshes.Length];
                fresnel2 = new float[meshes.Length];
                fresnel3 = new float[meshes.Length];
                diffTextures = new Texture[meshes.Length];
                diffTextures2 = new Texture[meshes.Length];
                diffTextures3 = new Texture[meshes.Length];
                glowTextures = new Texture[meshes.Length];
                normalTextures = new Texture[meshes.Length];
                normalTextures2 = new Texture[meshes.Length];
                normalTextures3 = new Texture[meshes.Length];
                specTextures = new Texture[meshes.Length];
                specTextures2 = new Texture[meshes.Length];
                specTextures3 = new Texture[meshes.Length];
                for (int i = 0; i < meshes.Length; i++)
                {
                    if (meshes[i].material.HasProperty("_Color"))
                    {
                        colors[i] = meshes[i].material.GetColor("_Color");
                    }
                    if (meshes[i].material.HasProperty("_Color2"))
                    {
                        colors2[i] = meshes[i].material.GetColor("_Color2");
                    }
                    if (meshes[i].material.HasProperty("_Color3"))
                    {
                        colors3[i] = meshes[i].material.GetColor("_Color3");
                    }
                    if (meshes[i].material.HasProperty("_SpecColor"))
                    {
                        specColors[i] = meshes[i].material.GetColor("_SpecColor");
                    }
                    if (meshes[i].material.HasProperty("_SpecColor2"))
                    {
                        specColors2[i] = meshes[i].material.GetColor("_SpecColor2");
                    }
                    if (meshes[i].material.HasProperty("_SpecColor3"))
                    {
                        specColors3[i] = meshes[i].material.GetColor("_SpecColor3");
                    }
                    if (meshes[i].material.HasProperty("_SpecInt"))
                    {
                        intensity[i] = meshes[i].material.GetFloat("_SpecInt");
                    }
                    if (meshes[i].material.HasProperty("_SpecInt2"))
                    {
                        intensity2[i] = meshes[i].material.GetFloat("_SpecInt2");
                    }
                    if (meshes[i].material.HasProperty("_SpecInt3"))
                    {
                        intensity3[i] = meshes[i].material.GetFloat("_SpecInt3");
                    }
                    if (meshes[i].material.HasProperty("_Shininess"))
                    {
                        sharpness[i] = meshes[i].material.GetFloat("_Shininess");
                    }
                    if (meshes[i].material.HasProperty("_Shininess2"))
                    {
                        sharpness2[i] = meshes[i].material.GetFloat("_Shininess2");
                    }
                    if (meshes[i].material.HasProperty("_Shininess3"))
                    {
                        sharpness3[i] = meshes[i].material.GetFloat("_Shininess3");
                    }
                    if (meshes[i].material.HasProperty("_Fresnel"))
                    {
                        fresnel[i] = meshes[i].material.GetFloat("_Fresnel");
                    }
                    if (meshes[i].material.HasProperty("_Fresnel2"))
                    {
                        fresnel2[i] = meshes[i].material.GetFloat("_Fresnel2");
                    }
                    if (meshes[i].material.HasProperty("_Fresnel3"))
                    {
                        fresnel3[i] = meshes[i].material.GetFloat("_Fresnel3");
                    }
                    if (meshes[i].material.HasProperty("_MainTex"))
                    {
                        diffTextures[i] = meshes[i].material.GetTexture("_MainTex");
                    }
                    if (meshes[i].material.HasProperty("_MainTex2"))
                    {
                        diffTextures2[i] = meshes[i].material.GetTexture("_MainTex2");
                    }
                    if (meshes[i].material.HasProperty("_MainTex3"))
                    {
                        diffTextures3[i] = meshes[i].material.GetTexture("_MainTex3");
                    }
                    if (meshes[i].material.HasProperty("_BumpMap"))
                    {
                        normalTextures[i] = meshes[i].material.GetTexture("_BumpMap");
                    }
                    if (meshes[i].material.HasProperty("_BumpMap2"))
                    {
                        normalTextures2[i] = meshes[i].material.GetTexture("_BumpMap2");
                    }
                    if (meshes[i].material.HasProperty("_BumpMap3"))
                    {
                        normalTextures3[i] = meshes[i].material.GetTexture("_BumpMap3");
                    }
                    if (meshes[i].material.HasProperty("_SpecTex"))
                    {
                        specTextures[i] = meshes[i].material.GetTexture("_SpecTex");
                    }
                    if (meshes[i].material.HasProperty("_SpecTex2"))
                    {
                        specTextures2[i] = meshes[i].material.GetTexture("_SpecTex2");
                    }
                    if (meshes[i].material.HasProperty("_SpecTex3"))
                    {
                        specTextures3[i] = meshes[i].material.GetTexture("_SpecTex3");
                    }
                    if (meshes[i].material.HasProperty("_Illum"))
                    {
                        glowTextures[i] = meshes[i].material.GetTexture("_Illum");
                    }
                }
            }
            normal = false;
            spec = false;
            final = false;
            lights = (Object.FindObjectsOfType(typeof(Light)) as Light[]);
        }
        if (final)
        {
            final = false;
            SetFinal();
        }
        if (normal)
        {
            normal = false;
            SetGreyNormal();
        }
        if (spec)
        {
            spec = false;
            SetSpecNormal();
        }
    }

    private void OnGUI()
    {
        GUILayout.BeginVertical();
        if (GUILayout.Button("Final"))
        {
            SetFinal();
        }
        if (GUILayout.Button("Grey Diff + Normal Map"))
        {
            SetGreyNormal();
        }
        if (GUILayout.Button("Spec + Normal Map"))
        {
            SetSpecNormal();
        }
        string text = (!matColorsOn) ? "Material Colours Off" : "Material Colours On";
        if (GUILayout.Button(text))
        {
            ToggleMatColours();
        }
        text = ((!specColorsOn) ? "Spec Colours Off" : "Spec Colours On");
        if (GUILayout.Button(text))
        {
            ToggleSpecColours();
        }
        text = ((!specDataOn) ? "Spec Data Off" : "Spec Data On");
        if (GUILayout.Button(text))
        {
            ToggleSpecData();
        }
        if (GUILayout.Button(skies[selectedSky].gameObject.name))
        {
            SwitchSky();
        }
        string text2 = (!lightOn) ? "Lights Off" : "Lights On";
        if (GUILayout.Button(text2))
        {
            ToggleLights();
        }
        if ((bool)camPlacer && GUILayout.Button("Switch Cam"))
        {
            camPlacer.next = true;
        }
        if (environments != null && environments.Length > 0 && GUILayout.Button("Switch Layout"))
        {
            environments[curEnv].gameObject.SetActive(value: false);
            curEnv = ++curEnv % environments.Length;
            environments[curEnv].gameObject.SetActive(value: true);
        }
        if (rotatingLight != null && GUILayout.Button("Switch Rotating Light"))
        {
            rotatingLight.SetActive(!rotatingLight.activeSelf);
        }
        GUILayout.EndVertical();
    }

    private void ToggleMatColours()
    {
        matColorsOn = !matColorsOn;
        if (meshes == null)
        {
            return;
        }
        for (int i = 0; i < meshes.Length; i++)
        {
            if (matColorsOn)
            {
                meshes[i].material.SetColor("_Color", colors[i]);
                meshes[i].material.SetColor("_Color2", colors2[i]);
                meshes[i].material.SetColor("_Color3", colors3[i]);
            }
            else
            {
                meshes[i].material.SetColor("_Color", Color.white);
                meshes[i].material.SetColor("_Color2", Color.white);
                meshes[i].material.SetColor("_Color3", Color.white);
            }
        }
    }

    private void ToggleSpecColours()
    {
        specColorsOn = !specColorsOn;
        if (meshes == null)
        {
            return;
        }
        for (int i = 0; i < meshes.Length; i++)
        {
            if (specColorsOn)
            {
                meshes[i].material.SetColor("_SpecColor", specColors[i]);
                meshes[i].material.SetColor("_SpecColor2", specColors2[i]);
                meshes[i].material.SetColor("_SpecColor3", specColors3[i]);
            }
            else
            {
                meshes[i].material.SetColor("_SpecColor", Color.white);
                meshes[i].material.SetColor("_SpecColor2", Color.white);
                meshes[i].material.SetColor("_SpecColor3", Color.white);
            }
        }
    }

    private void ToggleSpecData()
    {
        specDataOn = !specDataOn;
        if (meshes == null)
        {
            return;
        }
        for (int i = 0; i < meshes.Length; i++)
        {
            if (specDataOn)
            {
                if (meshes[i].material.HasProperty("_SpecInt"))
                {
                    meshes[i].material.SetFloat("_SpecInt", intensity[i]);
                }
                if (meshes[i].material.HasProperty("_SpecInt2"))
                {
                    meshes[i].material.SetFloat("_SpecInt2", intensity2[i]);
                }
                if (meshes[i].material.HasProperty("_SpecInt3"))
                {
                    meshes[i].material.SetFloat("_SpecInt3", intensity3[i]);
                }
                if (meshes[i].material.HasProperty("_Shininess"))
                {
                    meshes[i].material.SetFloat("_Shininess", sharpness[i]);
                }
                if (meshes[i].material.HasProperty("_Shininess2"))
                {
                    meshes[i].material.SetFloat("_Shininess2", sharpness2[i]);
                }
                if (meshes[i].material.HasProperty("_Shininess3"))
                {
                    meshes[i].material.SetFloat("_Shininess3", sharpness3[i]);
                }
                if (meshes[i].material.HasProperty("_Fresnel"))
                {
                    meshes[i].material.SetFloat("_Fresnel", fresnel[i]);
                }
                if (meshes[i].material.HasProperty("_Fresnel2"))
                {
                    meshes[i].material.SetFloat("_Fresnel2", fresnel2[i]);
                }
                if (meshes[i].material.HasProperty("_Fresnel3"))
                {
                    meshes[i].material.SetFloat("_Fresnel3", fresnel3[i]);
                }
            }
            else
            {
                if (meshes[i].material.HasProperty("_SpecInt"))
                {
                    meshes[i].material.SetFloat("_SpecInt", 1f);
                }
                if (meshes[i].material.HasProperty("_SpecInt2"))
                {
                    meshes[i].material.SetFloat("_SpecInt2", 1f);
                }
                if (meshes[i].material.HasProperty("_SpecInt3"))
                {
                    meshes[i].material.SetFloat("_SpecInt3", 1f);
                }
                if (meshes[i].material.HasProperty("_Shininess"))
                {
                    meshes[i].material.SetFloat("_Shininess", 4f);
                }
                if (meshes[i].material.HasProperty("_Shininess2"))
                {
                    meshes[i].material.SetFloat("_Shininess2", 4f);
                }
                if (meshes[i].material.HasProperty("_Shininess3"))
                {
                    meshes[i].material.SetFloat("_Shininess3", 4f);
                }
                if (meshes[i].material.HasProperty("_Fresnel"))
                {
                    meshes[i].material.SetFloat("_Fresnel", 0f);
                }
                if (meshes[i].material.HasProperty("_Fresnel2"))
                {
                    meshes[i].material.SetFloat("_Fresnel2", 0f);
                }
                if (meshes[i].material.HasProperty("_Fresnel3"))
                {
                    meshes[i].material.SetFloat("_Fresnel3", 0f);
                }
            }
        }
    }

    private void SwitchSky()
    {
        if (skies[selectedSky].gameObject.transform.parent != null && skies[selectedSky].gameObject.transform.parent.gameObject.name.Contains("cloud"))
        {
            skies[selectedSky].gameObject.transform.parent.gameObject.SetActive(value: false);
        }
        if (++selectedSky >= skies.Count)
        {
            selectedSky = 0;
        }
        SkyManager.Get().GlobalSky = skies[selectedSky];
        if (skies[selectedSky].gameObject.transform.parent != null && skies[selectedSky].gameObject.transform.parent.gameObject.name.Contains("cloud"))
        {
            skies[selectedSky].gameObject.transform.parent.gameObject.SetActive(value: true);
        }
    }

    private void ToggleLights()
    {
        lightOn = !lightOn;
        for (int i = 0; i < lights.Length; i++)
        {
            lights[i].enabled = lightOn;
        }
    }

    private void SetGreyNormal()
    {
        PandoraDebug.LogInfo("GreyNormal");
        if (meshes == null)
        {
            return;
        }
        for (int i = 0; i < meshes.Length; i++)
        {
            if ((bool)diffTextures[i])
            {
                meshes[i].material.SetTexture("_MainTex", greyTex);
            }
            if ((bool)diffTextures2[i])
            {
                meshes[i].material.SetTexture("_MainTex2", greyTex);
            }
            if ((bool)diffTextures3[i])
            {
                meshes[i].material.SetTexture("_MainTex3", greyTex);
            }
            if ((bool)specTextures[i])
            {
                meshes[i].material.SetTexture("_SpecTex", blackTex);
            }
            if ((bool)specTextures2[i])
            {
                meshes[i].material.SetTexture("_SpecTex2", blackTex);
            }
            if ((bool)specTextures3[i])
            {
                meshes[i].material.SetTexture("_SpecTex3", blackTex);
            }
            if ((bool)glowTextures[i])
            {
                meshes[i].material.SetTexture("_Illum", blackSpecTex);
            }
        }
    }

    private void SetSpecNormal()
    {
        PandoraDebug.LogInfo("SpecNormal");
        if (meshes == null)
        {
            return;
        }
        for (int i = 0; i < meshes.Length; i++)
        {
            if ((bool)diffTextures[i])
            {
                meshes[i].material.SetTexture("_MainTex", blackTex);
            }
            if ((bool)diffTextures2[i])
            {
                meshes[i].material.SetTexture("_MainTex2", blackTex);
            }
            if ((bool)diffTextures3[i])
            {
                meshes[i].material.SetTexture("_MainTex3", blackTex);
            }
            if ((bool)specTextures[i])
            {
                meshes[i].material.SetTexture("_SpecTex", specTextures[i]);
            }
            if ((bool)specTextures2[i])
            {
                meshes[i].material.SetTexture("_SpecTex2", specTextures2[i]);
            }
            if ((bool)specTextures3[i])
            {
                meshes[i].material.SetTexture("_SpecTex3", specTextures3[i]);
            }
            if ((bool)glowTextures[i])
            {
                meshes[i].material.SetTexture("_Illum", blackSpecTex);
            }
        }
    }

    private void SetFinal()
    {
        PandoraDebug.LogInfo("Final");
        if (meshes == null)
        {
            return;
        }
        for (int i = 0; i < meshes.Length; i++)
        {
            if ((bool)diffTextures[i])
            {
                meshes[i].material.SetTexture("_MainTex", diffTextures[i]);
            }
            if ((bool)diffTextures2[i])
            {
                meshes[i].material.SetTexture("_MainTex2", diffTextures2[i]);
            }
            if ((bool)diffTextures3[i])
            {
                meshes[i].material.SetTexture("_MainTex3", diffTextures3[i]);
            }
            if ((bool)specTextures[i])
            {
                meshes[i].material.SetTexture("_SpecTex", specTextures[i]);
            }
            if ((bool)specTextures2[i])
            {
                meshes[i].material.SetTexture("_SpecTex2", specTextures2[i]);
            }
            if ((bool)specTextures3[i])
            {
                meshes[i].material.SetTexture("_SpecTex3", specTextures3[i]);
            }
            if ((bool)glowTextures[i])
            {
                meshes[i].material.SetTexture("_Illum", glowTextures[i]);
            }
        }
    }
}
