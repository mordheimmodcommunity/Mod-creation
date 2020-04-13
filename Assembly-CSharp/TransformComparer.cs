using System;
using System.Collections.Generic;
using UnityEngine;

public class TransformComparer : IComparer<Transform>
{
    public static TransformComparer Default = new TransformComparer();

    int IComparer<Transform>.Compare(Transform x, Transform y)
    {
        return string.Compare(x.name, y.name, StringComparison.OrdinalIgnoreCase);
    }
}
