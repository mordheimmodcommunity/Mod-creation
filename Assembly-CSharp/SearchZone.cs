using System.Collections.Generic;
using UnityEngine;

public class SearchZone : MonoBehaviour
{
    public SearchZoneId type;

    [HideInInspector]
    public bool claimed;

    private List<Bounds> bounds;

    private void Start()
    {
        bounds = new List<Bounds>();
        for (int i = 0; i < base.transform.childCount; i++)
        {
            Transform child = base.transform.GetChild(i);
            if (child.name == "bounds")
            {
                Bounds item = child.GetComponent<Renderer>().bounds;
                bounds.Add(item);
            }
        }
        SearchZoneId searchZoneId = type;
        if ((searchZoneId == SearchZoneId.WYRDSTONE_CONCENTRATION || searchZoneId == SearchZoneId.WYRDSTONE_CLUSTER) && bounds.Count == 0)
        {
            PandoraDebug.LogError("SearchZone with no bounds", "DEPLOY", this);
        }
    }

    public bool Contains(Vector3 pos)
    {
        for (int i = 0; i < bounds.Count; i++)
        {
            if (bounds[i].Contains(pos))
            {
                return true;
            }
        }
        return false;
    }
}
