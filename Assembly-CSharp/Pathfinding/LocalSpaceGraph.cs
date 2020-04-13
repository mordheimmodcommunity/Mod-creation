using UnityEngine;

namespace Pathfinding
{
    [HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_local_space_graph.php")]
    public class LocalSpaceGraph : MonoBehaviour
    {
        protected Matrix4x4 originalMatrix;

        private void Start()
        {
            originalMatrix = base.transform.localToWorldMatrix;
        }

        public Matrix4x4 GetMatrix()
        {
            return base.transform.worldToLocalMatrix * originalMatrix;
        }
    }
}
