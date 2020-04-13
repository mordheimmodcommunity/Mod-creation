using System.Collections.Generic;
using UnityEngine;

public static class KGFGlobals
{
    public static string GetObjectPath(this GameObject theObject)
    {
        List<string> list = new List<string>();
        Transform transform = theObject.transform;
        do
        {
            list.Add(transform.name);
            transform = transform.parent;
        }
        while (transform != null);
        list.Reverse();
        return string.Join("/", list.ToArray());
    }
}
