using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class CameraSetter : MonoBehaviour
{
    public bool needFog;

    public Color fogColor;

    public float fogDensity = 0.001f;

    public float startDistance;

    public float fogHeight;

    public float fogHeightDensity;

    public Texture2D amplifyLutTexture;

    public bool useLUTVolumes;

    public void SetCameraInfo(Camera mainCamera)
    {
        if (!mainCamera)
        {
            return;
        }
        RenderSettings.fog = needFog;
        if (needFog)
        {
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = Mathf.Max(0.001f, fogDensity);
        }
        GlobalFog component = mainCamera.GetComponent<GlobalFog>();
        if ((Object)(object)component != null)
        {
            ((Behaviour)(object)component).enabled = needFog;
            component.startDistance = startDistance;
            component.heightFog = true;
            float num = 0f;
            if (PandoraSingleton<MissionManager>.Exists() && PandoraSingleton<MissionManager>.Instance.mapOrigin != null)
            {
                Vector3 position = PandoraSingleton<MissionManager>.Instance.mapOrigin.transform.position;
                num = position.y;
            }
            component.height = fogHeight + num;
            component.heightDensity = fogHeightDensity;
        }
        if (!amplifyLutTexture)
        {
            return;
        }
        AmplifyColorEffect component2 = mainCamera.GetComponent<AmplifyColorEffect>();
        if ((bool)component2)
        {
            component2.LutTexture = amplifyLutTexture;
            if (useLUTVolumes)
            {
                component2.UseVolumes = useLUTVolumes;
            }
        }
    }
}
