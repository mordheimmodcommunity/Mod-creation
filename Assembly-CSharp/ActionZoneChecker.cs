using UnityEngine;

public class ActionZoneChecker : MonoBehaviour
{
    public bool toDestroy;

    public void Check()
    {
        LayerMask layerMask = default(LayerMask);
        layerMask = ((1 << LayerMask.NameToLayer("environment")) | (1 << LayerMask.NameToLayer("ground")));
        RaycastHit hitInfo = default(RaycastHit);
        if (Physics.SphereCast(base.transform.position + new Vector3(0f, 0.7f, 0f), 0.45f, Vector3.down, out hitInfo, (int)layerMask) && (double)hitInfo.distance > 0.27)
        {
            toDestroy = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name != "climb_collider" && other.name != "action_zone" && other.name != "large_collision" && other.name != "action_collision" && other.name != "bounds" && other.name != "trigger" && other.name != "fx_ghost_wall_flame_01(clone)" && !other.name.Contains("collision_walk"))
        {
            toDestroy = true;
        }
    }
}
