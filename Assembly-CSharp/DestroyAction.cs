using UnityEngine;

public class DestroyAction : MonoBehaviour
{
    public Object toDestroy;

    private void Awake()
    {
        if (toDestroy == null)
        {
            toDestroy = base.gameObject;
        }
    }

    public void Destroy()
    {
        if (toDestroy != null)
        {
            Object.Destroy(toDestroy);
        }
    }
}
