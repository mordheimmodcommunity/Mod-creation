using System.Collections;
using UnityEngine;

public class DistortFade : MonoBehaviour
{
    public float timeToFade;

    public float fadeTime;

    public float fadeFrom;

    public float fadeTo;

    private float elapsed;

    private float fade;

    private void Awake()
    {
        elapsed = 0f;
        if (!GetComponent<Renderer>().sharedMaterial.HasProperty("_BumpAmt"))
        {
            Object.DestroyImmediate(this);
        }
        else
        {
            GetComponent<Renderer>().sharedMaterial.SetFloat("_BumpAmt", fadeFrom);
        }
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        if (elapsed >= timeToFade)
        {
            StartCoroutine(FadeAlphaFactor());
        }
    }

    private IEnumerator FadeAlphaFactor()
    {
        fade = GetComponent<Renderer>().sharedMaterial.GetFloat("_BumpAmt");
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime;
            fade = Mathf.SmoothStep(t: t / fadeTime, from: fade, to: fadeTo);
            GetComponent<Renderer>().sharedMaterial.SetFloat("_BumpAmt", fade);
            yield return 0;
        }
    }
}
