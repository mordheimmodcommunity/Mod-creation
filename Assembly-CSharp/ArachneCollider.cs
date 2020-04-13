using UnityEngine;

public class ArachneCollider : MonoBehaviour
{
    [SerializeField]
    private Vector3 center;

    public float radius;

    [HideInInspector]
    public Vector3 position;

    private void OnDrawGizmos()
    {
        position = base.transform.position + base.transform.rotation * center;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(position, radius);
    }

    private void Update()
    {
        position = base.transform.position + base.transform.rotation * center;
    }
}
