using UnityEngine;

[ExecuteInEditMode]
public class PointLightFallOff : MonoBehaviour
{
    public AnimationCurve fallOffCurve;

    public Texture2D fallOffTexture;

    public bool generate;

    private void Start()
    {
        GenerateTexture();
    }

    private void Update()
    {
        if (generate)
        {
            generate = false;
            GenerateTexture();
        }
    }

    private void GenerateTexture()
    {
        fallOffTexture = new Texture2D(256, 256, TextureFormat.ARGB32, mipmap: false, linear: true);
        fallOffTexture.filterMode = FilterMode.Bilinear;
        fallOffTexture.wrapMode = TextureWrapMode.Clamp;
        for (int i = 0; i < fallOffTexture.height; i++)
        {
            for (int j = 0; j < fallOffTexture.width; j++)
            {
                float time = ((float)j + 0.5f) / (float)fallOffTexture.width;
                float num = fallOffCurve.Evaluate(time);
                Color color = new Color(num, num, num, num);
                fallOffTexture.SetPixel(j, i, color);
            }
        }
        fallOffTexture.Apply();
        Shader.SetGlobalTexture("_LookUpTexture", fallOffTexture);
    }
}
