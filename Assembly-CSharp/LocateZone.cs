using UnityEngine;

public class LocateZone : MonoBehaviour
{
    public uint guid;

    public Bounds ColliderBounds
    {
        get;
        private set;
    }

    private void Start()
    {
        Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
        Renderer[] array = componentsInChildren;
        foreach (Renderer renderer in array)
        {
            renderer.enabled = false;
        }
        Collider componentInChildren = GetComponentInChildren<Collider>();
        if (componentInChildren != null)
        {
            ColliderBounds = componentInChildren.bounds;
        }
        PandoraSingleton<MissionManager>.Instance.RegisterLocateZone(this);
    }
}
