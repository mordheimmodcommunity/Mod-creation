using UnityEngine;

public class KGFObject : MonoBehaviour
{
    public KGFDelegate EventOnAwake = new KGFDelegate();

    public KGFDelegate EventOnDestroy = new KGFDelegate();

    protected virtual void Awake()
    {
        KGFAccessor.AddKGFObject(this);
        EventOnAwake.Trigger(this);
        KGFAwake();
    }

    private void OnDestroy()
    {
        EventOnDestroy.Trigger(this);
        KGFAccessor.RemoveKGFObject(this);
        KGFDestroy();
    }

    protected virtual void KGFAwake()
    {
    }

    protected virtual void KGFDestroy()
    {
    }
}
