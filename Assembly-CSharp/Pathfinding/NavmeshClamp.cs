using UnityEngine;

namespace Pathfinding
{
    [HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_navmesh_clamp.php")]
    public class NavmeshClamp : MonoBehaviour
    {
        private GraphNode prevNode;

        private Vector3 prevPos;

        private void LateUpdate()
        {
            if (prevNode == null)
            {
                NNInfo nearest = AstarPath.active.GetNearest(base.transform.position);
                prevNode = nearest.node;
                prevPos = base.transform.position;
            }
            if (prevNode == null)
            {
                return;
            }
            if (prevNode != null)
            {
                IRaycastableGraph raycastableGraph = AstarData.GetGraph(prevNode) as IRaycastableGraph;
                if (raycastableGraph != null)
                {
                    if (raycastableGraph.Linecast(prevPos, base.transform.position, prevNode, out GraphHitInfo hit))
                    {
                        ref Vector3 point = ref hit.point;
                        Vector3 position = base.transform.position;
                        point.y = position.y;
                        Vector3 vector = VectorMath.ClosestPointOnLine(hit.tangentOrigin, hit.tangentOrigin + hit.tangent, base.transform.position);
                        Vector3 point2 = hit.point;
                        point2 += Vector3.ClampMagnitude((Vector3)hit.node.position - point2, 0.008f);
                        if (raycastableGraph.Linecast(point2, vector, hit.node, out hit))
                        {
                            ref Vector3 point3 = ref hit.point;
                            Vector3 position2 = base.transform.position;
                            point3.y = position2.y;
                            base.transform.position = hit.point;
                        }
                        else
                        {
                            Vector3 position3 = base.transform.position;
                            vector.y = position3.y;
                            base.transform.position = vector;
                        }
                    }
                    prevNode = hit.node;
                }
            }
            prevPos = base.transform.position;
        }
    }
}
