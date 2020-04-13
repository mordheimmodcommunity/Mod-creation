using UnityEngine;

public class TNAutoCreate : MonoBehaviour
{
    public GameObject prefab;

    public bool persistent;

    private void Start()
    {
        TNManager.Create(prefab, base.transform.position, base.transform.rotation, persistent);
        Object.Destroy(base.gameObject);
    }
}
