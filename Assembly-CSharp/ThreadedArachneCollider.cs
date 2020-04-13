using UnityEngine;

public struct ThreadedArachneCollider
{
    public Vector3 position;

    public float radius;

    public float sqRadius;

    public ThreadedArachneCollider(ArachneCollider collider)
    {
        position = collider.position;
        radius = collider.radius;
        sqRadius = radius * radius;
    }
}
