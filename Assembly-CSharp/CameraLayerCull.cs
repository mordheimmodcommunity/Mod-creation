using UnityEngine;

public class CameraLayerCull : MonoBehaviour
{
    public bool update;

    public float[] distances = new float[32];

    private void Start()
    {
        base.gameObject.GetComponent<Camera>().layerCullDistances = distances;
        update = false;
    }

    private void Update()
    {
        if (update)
        {
            update = false;
            base.gameObject.GetComponent<Camera>().layerCullDistances = distances;
        }
    }
}
