using UnityEngine;

namespace Flowmap
{
    public static class Primitives
    {
        private static Mesh planeMesh;

        public static Mesh PlaneMesh
        {
            get
            {
                if (!planeMesh)
                {
                    planeMesh = new Mesh();
                    planeMesh.name = "Plane";
                    planeMesh.vertices = new Vector3[4]
                    {
                        new Vector3(-0.5f, 0f, -0.5f),
                        new Vector3(0.5f, 0f, -0.5f),
                        new Vector3(-0.5f, 0f, 0.5f),
                        new Vector3(0.5f, 0f, 0.5f)
                    };
                    planeMesh.uv = new Vector2[4]
                    {
                        new Vector2(0f, 0f),
                        new Vector2(1f, 0f),
                        new Vector2(0f, 1f),
                        new Vector2(1f, 1f)
                    };
                    planeMesh.normals = new Vector3[4]
                    {
                        Vector3.up,
                        Vector3.up,
                        Vector3.up,
                        Vector3.up
                    };
                    planeMesh.triangles = new int[6]
                    {
                        2,
                        1,
                        0,
                        3,
                        1,
                        2
                    };
                    planeMesh.tangents = new Vector4[4]
                    {
                        new Vector4(1f, 0f, 0f, 1f),
                        new Vector4(1f, 0f, 0f, 1f),
                        new Vector4(1f, 0f, 0f, 1f),
                        new Vector4(1f, 0f, 0f, 1f)
                    };
                    planeMesh.hideFlags = HideFlags.HideAndDontSave;
                }
                return planeMesh;
            }
        }
    }
}
