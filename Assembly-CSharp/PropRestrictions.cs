using System.Collections.Generic;
using UnityEngine;

public class PropRestrictions
{
    public PropRestrictionJoinPropTypeData restrictionData;

    public List<GameObject> props;

    public PropRestrictions(PropRestrictionJoinPropTypeData data)
    {
        restrictionData = data;
        props = new List<GameObject>();
    }
}
