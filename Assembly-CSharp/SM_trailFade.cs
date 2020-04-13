using UnityEngine;

public class SM_trailFade : MonoBehaviour
{
    public float fadeInTime = 0.1f;

    public float stayTime = 1f;

    public float fadeOutTime = 0.7f;

    public TrailRenderer thisTrail;

    private float timeElapsed;

    private float timeElapsedLast;

    private float percent;

    private void Start()
    {
        if (fadeInTime < 0.01f)
        {
            fadeInTime = 0.01f;
        }
        percent = timeElapsed / fadeInTime;
    }

    private void Update()
    {
        timeElapsed += Time.deltaTime;
        Color color = thisTrail.material.GetColor("_TintColor");
        if (timeElapsed <= fadeInTime)
        {
            percent = timeElapsed / fadeInTime;
            color.a = percent;
            thisTrail.material.SetColor("_TintColor", color);
        }
        if (timeElapsed > fadeInTime && timeElapsed < fadeInTime + stayTime)
        {
            color.a = 1f;
            thisTrail.material.SetColor("_TintColor", color);
        }
        if (timeElapsed >= fadeInTime + stayTime && timeElapsed < fadeInTime + stayTime + fadeOutTime)
        {
            timeElapsedLast += Time.deltaTime;
            percent = 1f - timeElapsedLast / fadeOutTime;
            color.a = percent;
            thisTrail.material.SetColor("_TintColor", color);
        }
    }
}
