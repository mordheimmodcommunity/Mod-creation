using UnityEngine;

public class SM_randomRotation : MonoBehaviour
{
    public float rotationMaxX;

    public float rotationMaxY = 360f;

    public float rotationMaxZ;

    private void Start()
    {
        float xAngle = Random.Range(0f - rotationMaxX, rotationMaxX);
        float yAngle = Random.Range(0f - rotationMaxY, rotationMaxY);
        float zAngle = Random.Range(0f - rotationMaxZ, rotationMaxZ);
        base.transform.Rotate(xAngle, yAngle, zAngle);
    }
}
