using System.Collections.Generic;
using UnityEngine;

public class DynamicChargeCircle : DynamicCircle
{
    public bool fitToEnv = true;

    private List<Vector3> displayPoints = new List<Vector3>();

    private Mesh cylinderMesh;

    public override void Init()
    {
        base.Init();
        capsuleMinHeight = 0.6f;
        heightTreshold = 0.5f;
        angleIteration = 60;
        envHeight = 0.5f;
        pointsTreshold = 0.45f;
        collisionPointsDistMin = 0.0001f;
        cylinderMesh = new Mesh();
        GetComponent<MeshFilter>().mesh = cylinderMesh;
    }

    public void Set(float chargeDist, float radius)
    {
        sphereRadius = radius;
        capsuleMinHeight = radius + 0.4f;
        DetectCollisions(chargeDist, chargeDist, base.transform.parent.transform.rotation, flatEnv: true, -0.2f, ref displayPoints);
        CreateCylinderOutlineMesh(cylinderMesh, displayPoints, -0.1f, 1.5f);
    }
}
