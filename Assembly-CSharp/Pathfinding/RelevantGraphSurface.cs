using UnityEngine;

namespace Pathfinding
{
    [HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_relevant_graph_surface.php")]
    [AddComponentMenu("Pathfinding/Navmesh/RelevantGraphSurface")]
    public class RelevantGraphSurface : MonoBehaviour
    {
        private static RelevantGraphSurface root;

        public float maxRange = 1f;

        private RelevantGraphSurface prev;

        private RelevantGraphSurface next;

        private Vector3 position;

        public Vector3 Position => position;

        public RelevantGraphSurface Next => next;

        public RelevantGraphSurface Prev => prev;

        public static RelevantGraphSurface Root => root;

        public void UpdatePosition()
        {
            position = base.transform.position;
        }

        private void OnEnable()
        {
            UpdatePosition();
            if (root == null)
            {
                root = this;
                return;
            }
            next = root;
            root.prev = this;
            root = this;
        }

        private void OnDisable()
        {
            if (root == this)
            {
                root = next;
                if (root != null)
                {
                    root.prev = null;
                }
            }
            else
            {
                if (prev != null)
                {
                    prev.next = next;
                }
                if (next != null)
                {
                    next.prev = prev;
                }
            }
            prev = null;
            next = null;
        }

        public static void UpdateAllPositions()
        {
            RelevantGraphSurface relevantGraphSurface = root;
            while (relevantGraphSurface != null)
            {
                relevantGraphSurface.UpdatePosition();
                relevantGraphSurface = relevantGraphSurface.Next;
            }
        }

        public static void FindAllGraphSurfaces()
        {
            RelevantGraphSurface[] array = Object.FindObjectsOfType(typeof(RelevantGraphSurface)) as RelevantGraphSurface[];
            for (int i = 0; i < array.Length; i++)
            {
                array[i].OnDisable();
                array[i].OnEnable();
            }
        }

        public void OnDrawGizmos()
        {
            Gizmos.color = new Color(19f / 85f, 211f / 255f, 46f / 255f, 0.4f);
            Gizmos.DrawLine(base.transform.position - Vector3.up * maxRange, base.transform.position + Vector3.up * maxRange);
        }

        public void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(19f / 85f, 211f / 255f, 46f / 255f);
            Gizmos.DrawLine(base.transform.position - Vector3.up * maxRange, base.transform.position + Vector3.up * maxRange);
        }
    }
}
