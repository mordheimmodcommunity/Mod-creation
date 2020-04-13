using UnityEngine;

namespace Pathfinding.Voxels
{
    public class RasterizationMesh
    {
        public MeshFilter original;

        public int area;

        public Vector3[] vertices;

        public int[] triangles;

        public Bounds bounds;

        public Matrix4x4 matrix;

        public RasterizationMesh()
        {
        }

        public RasterizationMesh(Vector3[] vertices, int[] triangles, Bounds bounds)
        {
            matrix = Matrix4x4.identity;
            this.vertices = vertices;
            this.triangles = triangles;
            this.bounds = bounds;
            original = null;
            area = 0;
        }

        public RasterizationMesh(Vector3[] vertices, int[] triangles, Bounds bounds, Matrix4x4 matrix)
        {
            this.matrix = matrix;
            this.vertices = vertices;
            this.triangles = triangles;
            this.bounds = bounds;
            original = null;
            area = 0;
        }

        public void RecalculateBounds()
        {
            Bounds bounds = new Bounds(matrix.MultiplyPoint3x4(vertices[0]), Vector3.zero);
            for (int i = 1; i < vertices.Length; i++)
            {
                bounds.Encapsulate(matrix.MultiplyPoint3x4(vertices[i]));
            }
            this.bounds = bounds;
        }
    }
}
