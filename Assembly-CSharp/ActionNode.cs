using UnityEngine;

public class ActionNode : MonoBehaviour
{
    public GameObject zone;

    public void Init()
    {
        zone = Object.Instantiate(zone);
        zone.transform.parent = base.gameObject.transform.parent;
        zone.transform.position = base.gameObject.transform.position;
        zone.transform.rotation = base.gameObject.transform.rotation;
        Object.Destroy(base.gameObject);
    }

    private void OnDrawGizmos()
    {
        if (!(zone == null))
        {
            Gizmos.color = Color.cyan;
            if (zone.name == "action_zone3")
            {
                Gizmos.DrawWireCube(base.transform.position + new Vector3(0f, 3.5f, 0f), new Vector3(2f, 1f, 2f));
                Gizmos.DrawWireCube(base.transform.position + new Vector3(0f, 0.5f, 0f) + base.transform.forward * 1.35f, new Vector3(2f, 1f, 2f));
            }
            else if (zone.name == "action_zone6")
            {
                Gizmos.DrawWireCube(base.transform.position + new Vector3(0f, 6.5f, 0f), new Vector3(2f, 1f, 2f));
                Gizmos.DrawWireCube(base.transform.position + new Vector3(0f, 3.5f, 0f) + base.transform.forward * 1.35f, new Vector3(2f, 1f, 2f));
                Gizmos.DrawWireCube(base.transform.position + new Vector3(0f, 0.5f, 0f) + base.transform.forward * 1.35f, new Vector3(2f, 1f, 2f));
            }
            else
            {
                Gizmos.DrawWireCube(base.transform.position + new Vector3(0f, 9.5f, 0f), new Vector3(2f, 1f, 2f));
                Gizmos.DrawWireCube(base.transform.position + new Vector3(0f, 6.5f, 0f) + base.transform.forward * 1.35f, new Vector3(2f, 1f, 2f));
                Gizmos.DrawWireCube(base.transform.position + new Vector3(0f, 3.5f, 0f) + base.transform.forward * 1.35f, new Vector3(2f, 1f, 2f));
                Gizmos.DrawWireCube(base.transform.position + new Vector3(0f, 0.5f, 0f) + base.transform.forward * 1.35f, new Vector3(2f, 1f, 2f));
            }
        }
    }
}
