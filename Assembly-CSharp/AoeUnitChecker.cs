using UnityEngine;

public class AoeUnitChecker
{
    public UnitController ctrlr;

    public Vector3 lastPos;

    public AoeUnitChecker(UnitController ctrlr, Vector3 lastPos)
    {
        this.ctrlr = ctrlr;
        this.lastPos = lastPos;
    }
}
