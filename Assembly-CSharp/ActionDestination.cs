using Pathfinding;
using System;
using UnityEngine;

[Serializable]
public class ActionDestination
{
    public UnitActionId actionId = UnitActionId.CLIMB_3M;

    public ActionZone destination;

    public GameObject fx;

    public NodeLink2 navLink;

    public ParticleSystem[] particles;
}
