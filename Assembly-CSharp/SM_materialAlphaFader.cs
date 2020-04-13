using UnityEngine;

public class SM_materialAlphaFader : MonoBehaviour
{
    public float fadeSpeed = 1f;

    public float beginTintAlpha = 0.5f;

    private Renderer r;

    private void Awake()
    {
        r = GetComponent<Renderer>();
    }

    private void Update()
    {
        beginTintAlpha -= Time.deltaTime * fadeSpeed;
        r.material.SetColor("_TintColor", new Color(1f, 1f, 1f, beginTintAlpha));
    }
}
