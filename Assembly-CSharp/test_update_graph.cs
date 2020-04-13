using Pathfinding;
using UnityEngine;

public class test_update_graph : MonoBehaviour
{
    public bool walkable;

    private bool oldWalkable;

    private void Start()
    {
        oldWalkable = !walkable;
    }

    private void Update()
    {
        if (oldWalkable != walkable)
        {
            oldWalkable = walkable;
            GraphUpdateObject graphUpdateObject = new GraphUpdateObject(base.gameObject.GetComponent<Renderer>().bounds);
            graphUpdateObject.modifyWalkability = true;
            graphUpdateObject.setWalkability = walkable;
            AstarPath.active.UpdateGraphs(graphUpdateObject);
        }
    }
}
