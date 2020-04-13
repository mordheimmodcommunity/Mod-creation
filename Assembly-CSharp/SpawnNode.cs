using UnityEngine;

public class SpawnNode : MonoBehaviour
{
    public int types;

    [HideInInspector]
    public bool claimed;

    public MapImprint imprint;

    public SpawnNodeId overrideStyle = SpawnNodeId.START;

    public void ShowImprint(bool isMine)
    {
        if (imprint == null)
        {
            imprint = base.gameObject.AddComponent<MapImprint>();
        }
        imprint.Init("overview/deploy", "overview/deploy", alwaysVisible: true, (!isMine) ? MapImprintType.ENEMY_DEPLOYMENT : MapImprintType.PLAYER_DEPLOYMENT);
    }

    public bool IsOfType(SpawnNodeId id)
    {
        return (types & (1 << (int)(id - 1))) != 0;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawIcon(base.transform.position + new Vector3(0f, 2f, 0f), "start.png", allowScaling: true);
        float height = (!IsOfType(SpawnNodeId.IMPRESSIVE)) ? 1.6f : 2.18f;
        float num = (!IsOfType(SpawnNodeId.IMPRESSIVE)) ? 0.5f : 0.9f;
        PandoraUtils.DrawFacingGizmoCube(base.transform, height, num, num);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(base.transform.position, (!IsOfType(SpawnNodeId.IMPRESSIVE)) ? 2.15f : 2.7f);
    }
}
