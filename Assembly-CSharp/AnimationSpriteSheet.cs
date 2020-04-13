using UnityEngine;

public class AnimationSpriteSheet : MonoBehaviour
{
    public int uvX = 4;

    public int uvY = 2;

    public float fps = 24f;

    private Renderer r;

    private void Awake()
    {
        r = GetComponent<Renderer>();
    }

    private void Update()
    {
        int num = Mathf.FloorToInt(Time.time * fps);
        num %= uvX * uvY;
        Vector2 scale = new Vector2(1f / (float)uvX, 1f / (float)uvY);
        int num2 = num % uvX;
        int num3 = num / uvX;
        Vector2 offset = new Vector2((float)num2 * scale.x, 1f - scale.y - (float)num3 * scale.y);
        r.material.SetTextureOffset("_MainTex", offset);
        r.material.SetTextureScale("_MainTex", scale);
    }
}
