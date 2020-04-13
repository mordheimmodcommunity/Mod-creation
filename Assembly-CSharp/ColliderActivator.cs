using System.Collections.Generic;
using UnityEngine;

public class ColliderActivator : MonoBehaviour
{
    public static List<ColliderActivator> activators = new List<ColliderActivator>();

    private Collider col;

    public static void ActivateAll()
    {
        for (int i = 0; i < activators.Count; i++)
        {
            if (activators[i] != null)
            {
                if (activators[i].col != null)
                {
                    activators[i].col.enabled = true;
                }
                Object.Destroy(activators[i]);
            }
        }
        activators.Clear();
    }

    private void Awake()
    {
        col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
            activators.Add(this);
        }
        else
        {
            Object.Destroy(this);
        }
    }
}
