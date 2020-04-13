using UnityEngine;

public class Gizmo : MonoBehaviour
{
    public string iconName;

    public Vector3 offset = new Vector3(0f, 1f, 0f);

    private void OnDrawGizmos()
    {
        Gizmos.DrawIcon(base.transform.position + offset, iconName);
    }
}
