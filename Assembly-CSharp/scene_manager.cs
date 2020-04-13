using mset;
using Prometheus;
using UnityEngine;

public class scene_manager : MonoBehaviour
{
    public Sky[] skies;

    public Light[] lights;

    public float[] LightIntensity;

    private bool background = true;

    private int currentSky;

    private bool showSpecular;

    private bool showNormal;

    private bool untextured;

    private bool showAmbienceObscurance;

    private bool showColorGrading;

    private bool showFilmToneMapping;

    private bool showDebugDiffuse;

    private bool showDebugSpecular;

    private float exposure;

    private float Camexposure;

    private float Diffexposure;

    private float Specexposure;

    private float glow;

    public Renderer[] meshes;

    private Texture[] diffTextures;

    private Texture[] specTextures;

    private Texture[] glowTextures;

    private Texture[] NormalTextures;

    public Texture2D greyTex;

    public Texture2D blackTex;

    public Texture2D normNeutro;

    private Filmic DaToneMapping;

    private AmplifyColorEffect DaColor;

    public bool showGUI = true;

    public Texture2D DiffCheck;

    public Texture2D ColCorrection;

    public Material[] materials;

    public Material[] RawDif;

    public Shader shaderDiff;

    private GameObject Imported;

    private bool rotate;

    public GameObject[] fogs;

    private void Start()
    {
        if (Imported == null)
        {
            Imported = GameObject.Find("Imported").gameObject;
        }
        Imported.GetComponent<Rotate>().enabled = false;
        meshes = (Object.FindObjectsOfType(typeof(Renderer)) as Renderer[]);
        lights = (Object.FindObjectsOfType(typeof(Light)) as Light[]);
        LightIntensity = new float[lights.Length];
        for (int i = 0; i < lights.Length; i++)
        {
            LightIntensity[i] = lights[i].intensity;
        }
        RenderSettings.ambientLight = Color.black;
        shaderDiff = Shader.Find("Diffuse");
        if (DaToneMapping == null && base.transform.GetComponent<Filmic>() != null)
        {
            DaToneMapping = base.transform.GetComponent<Filmic>();
        }
        if (DaColor == null && base.transform.GetComponent<AmplifyColorEffect>() != null)
        {
            DaColor = base.transform.GetComponent<AmplifyColorEffect>();
            DaColor.LutTexture = ColCorrection;
        }
        skies = (Object.FindObjectsOfType(typeof(Sky)) as Sky[]);
        exposure = skies[0].MasterIntensity;
        Camexposure = skies[0].CamExposure;
        Diffexposure = skies[0].DiffIntensity;
        Specexposure = skies[0].SpecIntensity;
        if (fogs.Length > 0)
        {
            GameObject[] array = fogs;
            foreach (GameObject gameObject in array)
            {
                gameObject.SetActive(value: false);
            }
        }
        currentSky = 0;
        for (int num = skies.Length - 1; num >= 0; num--)
        {
            setSky(num);
        }
        setBackground(background);
        greyTex = new Texture2D(16, 16);
        Color color = new Color(0.73f, 0.73f, 0.73f, 1f);
        Color[] pixels = greyTex.GetPixels();
        for (int k = 0; k < pixels.Length; k++)
        {
            pixels[k] = color;
        }
        greyTex.SetPixels(pixels);
        greyTex.Apply(updateMipmaps: true);
        blackTex = new Texture2D(16, 16);
        pixels = blackTex.GetPixels();
        Color color2 = new Color(0.1f, 0.1f, 0.1f, 1f);
        for (int l = 0; l < pixels.Length; l++)
        {
            pixels[l] = color2;
        }
        blackTex.SetPixels(pixels);
        blackTex.Apply(updateMipmaps: true);
        if (meshes != null)
        {
            diffTextures = new Texture[meshes.Length];
            specTextures = new Texture[meshes.Length];
            glowTextures = new Texture[meshes.Length];
            NormalTextures = new Texture[meshes.Length];
            materials = new Material[meshes.Length];
            RawDif = new Material[meshes.Length];
            for (int m = 0; m < meshes.Length; m++)
            {
                if (meshes[m].material.HasProperty("_MainTex"))
                {
                    diffTextures[m] = meshes[m].material.GetTexture("_MainTex");
                }
                if (meshes[m].material.HasProperty("_SpecTex"))
                {
                    specTextures[m] = meshes[m].material.GetTexture("_SpecTex");
                }
                if (meshes[m].material.HasProperty("_Illum"))
                {
                    glowTextures[m] = meshes[m].material.GetTexture("_Illum");
                }
                if (meshes[m].material.HasProperty("_BumpMap"))
                {
                    NormalTextures[m] = meshes[m].material.GetTexture("_BumpMap");
                }
                materials[m] = meshes[m].material;
                RawDif[m] = new Material(meshes[m].material);
                RawDif[m].shader = shaderDiff;
            }
        }
        setGrey(yes: false);
        setBlack(yes: false);
        setNormal(yes: false);
    }

    private void setDiffuse(bool yes)
    {
        for (int i = 0; i < skies.Length; i++)
        {
            if ((bool)skies[i])
            {
                skies[i].DiffIntensity = ((!yes) ? 0f : 1f);
            }
        }
        if ((bool)SkyManager.Get().GlobalSky)
        {
            SkyManager.Get().GlobalSky.Apply();
        }
    }

    private void setSpecular(bool yes)
    {
        showSpecular = yes;
        for (int i = 0; i < skies.Length; i++)
        {
            if ((bool)skies[i])
            {
                skies[i].SpecIntensity = ((!yes) ? 0f : 1f);
            }
        }
        if ((bool)SkyManager.Get().GlobalSky)
        {
            SkyManager.Get().GlobalSky.Apply();
        }
    }

    private void setExposures(float val)
    {
        exposure = val;
        for (int i = 0; i < skies.Length; i++)
        {
            if ((bool)skies[i])
            {
                skies[i].MasterIntensity = val;
            }
        }
        SkyManager.Get().GlobalSky.Apply();
    }

    private void setCamExposures(float val)
    {
        Camexposure = val;
        for (int i = 0; i < skies.Length; i++)
        {
            if ((bool)skies[i])
            {
                skies[i].CamExposure = val;
            }
        }
        SkyManager.Get().GlobalSky.Apply();
    }

    private void setDiffExposures(float val)
    {
        Diffexposure = val;
        for (int i = 0; i < skies.Length; i++)
        {
            if ((bool)skies[i])
            {
                skies[i].DiffIntensity = val;
            }
        }
        SkyManager.Get().GlobalSky.Apply();
    }

    private void setSpecExposures(float val)
    {
        Specexposure = val;
        for (int i = 0; i < skies.Length; i++)
        {
            if ((bool)skies[i])
            {
                skies[i].SpecIntensity = val;
            }
        }
        SkyManager.Get().GlobalSky.Apply();
    }

    private void setSky(int index)
    {
        currentSky = index;
        skies[currentSky].Apply();
    }

    private void setBackground(bool yes)
    {
        background = yes;
        SkyManager.Get().ShowSkybox = yes;
        SkyManager.Get().GlobalSky.Apply();
    }

    private void setMaterials(bool yes)
    {
        if (meshes == null)
        {
            return;
        }
        for (int i = 0; i < meshes.Length; i++)
        {
            if (yes)
            {
                meshes[i].material = RawDif[i];
            }
            else
            {
                meshes[i].material = materials[i];
            }
        }
    }

    private void setGrey(bool yes)
    {
        if (meshes == null)
        {
            return;
        }
        for (int i = 0; i < meshes.Length; i++)
        {
            if (yes)
            {
                if ((bool)diffTextures[i])
                {
                    meshes[i].material.SetTexture("_MainTex", diffTextures[i]);
                }
            }
            else if ((bool)diffTextures[i])
            {
                meshes[i].material.SetTexture("_MainTex", greyTex);
            }
        }
    }

    private void setBlack(bool yes)
    {
        if (meshes == null)
        {
            return;
        }
        for (int i = 0; i < meshes.Length; i++)
        {
            if (yes)
            {
                if ((bool)specTextures[i])
                {
                    meshes[i].material.SetTexture("_SpecTex", specTextures[i]);
                }
                if ((bool)glowTextures[i])
                {
                    meshes[i].material.SetTexture("_Illum", glowTextures[i]);
                }
            }
            else
            {
                if ((bool)specTextures[i])
                {
                    meshes[i].material.SetTexture("_SpecTex", blackTex);
                }
                if ((bool)glowTextures[i])
                {
                    meshes[i].material.SetTexture("_Illum", blackTex);
                }
            }
        }
    }

    private void setNormal(bool yes)
    {
        if (meshes == null)
        {
            return;
        }
        for (int i = 0; i < meshes.Length; i++)
        {
            if (yes)
            {
                if ((bool)NormalTextures[i])
                {
                    meshes[i].material.SetTexture("_BumpMap", NormalTextures[i]);
                }
            }
            else if ((bool)NormalTextures[i])
            {
                meshes[i].material.SetTexture("_BumpMap", normNeutro);
            }
        }
    }

    private void SetDebugDiff(bool yes)
    {
        if (yes)
        {
            DaColor.enabled = showDebugDiffuse;
            DaColor.LutTexture = DiffCheck;
            RenderSettings.ambientLight = Color.white;
            untextured = true;
            SkyManager.Get().ShowSkybox = false;
            for (int i = 0; i < lights.Length; i++)
            {
                if ((bool)lights[i])
                {
                    lights[i].intensity = 0f;
                }
            }
            setMaterials(yes: true);
            return;
        }
        DaColor.LutTexture = ColCorrection;
        DaColor.enabled = showColorGrading;
        RenderSettings.ambientLight = Color.black;
        setGrey(untextured);
        for (int j = 0; j < lights.Length; j++)
        {
            if ((bool)lights[j])
            {
                lights[j].intensity = LightIntensity[j];
            }
        }
        setMaterials(yes: false);
    }

    private void SetDebugSpec()
    {
        setNormal(yes: true);
        setBlack(yes: true);
        for (int i = 0; i < meshes.Length; i++)
        {
            if ((bool)diffTextures[i])
            {
                meshes[i].material.SetTexture("_MainTex", blackTex);
            }
        }
    }

    private void EnableRotation(bool yes)
    {
        if (yes)
        {
            Imported.GetComponent<Rotate>().enabled = true;
        }
        else
        {
            Imported.GetComponent<Rotate>().enabled = false;
        }
    }

    private void OnGUI()
    {
        Rect pixelRect = GetComponent<Camera>().pixelRect;
        pixelRect.y = GetComponent<Camera>().pixelRect.height * 0.87f;
        pixelRect.height = GetComponent<Camera>().pixelRect.height * 0.06f;
        GUI.color = Color.white;
        if (!showGUI)
        {
            return;
        }
        GUILayout.BeginArea(new Rect(0f, Screen.height / 4, 250f, Screen.height));
        bool flag = showNormal;
        showNormal = GUILayout.Toggle(showNormal, "Normal Map");
        if (flag != showNormal)
        {
            setNormal(showNormal);
        }
        if (!showDebugSpecular)
        {
            flag = untextured;
            untextured = GUILayout.Toggle(untextured, "Diffuse/Grey");
            setGrey(untextured);
        }
        flag = showSpecular;
        showSpecular = GUILayout.Toggle(showSpecular, "Specular");
        if (flag != showSpecular)
        {
            setBlack(showSpecular);
        }
        flag = showAmbienceObscurance;
        showAmbienceObscurance = GUILayout.Toggle(showAmbienceObscurance, "Ambience Obscurance");
        flag = showColorGrading;
        showColorGrading = GUILayout.Toggle(showColorGrading, "AmplifyColor");
        if (flag != showColorGrading)
        {
            DaColor.enabled = showColorGrading;
            DaColor.LutTexture = ColCorrection;
        }
        flag = showFilmToneMapping;
        showFilmToneMapping = GUILayout.Toggle(showFilmToneMapping, "Film tonemapping");
        if (flag != showFilmToneMapping)
        {
            DaToneMapping.enabled = showFilmToneMapping;
        }
        bool flag2 = showDebugDiffuse;
        showDebugDiffuse = GUILayout.Toggle(showDebugDiffuse, "Diffuse debugging");
        if (flag2 != showDebugDiffuse)
        {
            SetDebugDiff(showDebugDiffuse);
        }
        bool flag3 = showDebugSpecular;
        showDebugSpecular = GUILayout.Toggle(showDebugSpecular, "Spec debugging");
        if (flag3 != showDebugSpecular)
        {
            SetDebugSpec();
        }
        flag = rotate;
        rotate = GUILayout.Toggle(rotate, "Rotate model");
        if (flag != rotate)
        {
            EnableRotation(rotate);
        }
        GUILayout.EndArea();
        GUILayout.BeginArea(new Rect(Screen.width - 100, Screen.height / 4, 250f, Screen.height));
        flag = background;
        background = GUILayout.Toggle(background, "Skybox");
        if (flag != background)
        {
            setBackground(background);
        }
        for (int i = 0; i < skies.Length; i++)
        {
            if (GUILayout.Button(skies[i].name, GUILayout.Width(100f)))
            {
                skies[i].Apply();
            }
        }
        GameObject[] array = fogs;
        foreach (GameObject gameObject in array)
        {
            gameObject.SetActive(GUILayout.Toggle(gameObject.activeSelf, gameObject.name));
        }
        GUILayout.Label("Master: " + Mathf.CeilToInt(exposure * 100f) + "%");
        float value = Mathf.Sqrt(exposure);
        value = GUILayout.HorizontalSlider(value, 0f, 2f, GUILayout.Width(100f));
        exposure = value * value;
        setExposures(exposure);
        GUILayout.Label("CamExp: " + Mathf.CeilToInt(Camexposure * 100f) + "%");
        float value2 = Mathf.Sqrt(Camexposure);
        value2 = GUILayout.HorizontalSlider(value2, 0f, 2f, GUILayout.Width(100f));
        Camexposure = value2 * value2;
        setCamExposures(Camexposure);
        GUILayout.Label("Diff: " + Mathf.CeilToInt(Diffexposure * 100f) + "%");
        float value3 = Mathf.Sqrt(Diffexposure);
        value3 = GUILayout.HorizontalSlider(value3, 0f, 2f, GUILayout.Width(100f));
        Diffexposure = value3 * value3;
        setDiffExposures(Diffexposure);
        GUILayout.Label("Spec: " + Mathf.CeilToInt(Specexposure * 100f) + "%");
        float value4 = Mathf.Sqrt(Specexposure);
        value4 = GUILayout.HorizontalSlider(value4, 0f, 2f, GUILayout.Width(100f));
        Specexposure = value4 * value4;
        setSpecExposures(Specexposure);
        GUILayout.EndArea();
    }
}
