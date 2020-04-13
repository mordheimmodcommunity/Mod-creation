using UnityEngine;

public class Billboard : ExternalUpdator
{
    public override void ExternalUpdate()
    {
        if (!(Camera.main == null))
        {
            Vector3 position = Camera.main.transform.position;
            Vector3 position2 = cachedTransform.position;
            position.y = position2.y;
            cachedTransform.LookAt(position, Vector3.up);
        }
    }
}
