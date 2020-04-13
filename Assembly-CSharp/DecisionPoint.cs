using UnityEngine;

public class DecisionPoint : MonoBehaviour
{
    public DecisionPointId id;

    [HideInInspector]
    public float closestUnitSqrDist;

    private void OnDrawGizmos()
    {
        string str = "tga";
        switch (id)
        {
            case DecisionPointId.OVERWATCH:
                Gizmos.color = Color.red;
                break;
            case DecisionPointId.AMBUSH:
                Gizmos.color = Color.magenta;
                break;
            case DecisionPointId.PATROL:
                Gizmos.color = Color.cyan;
                break;
            case DecisionPointId.SPAWN:
                Gizmos.color = Color.green;
                str = "png";
                break;
            case DecisionPointId.FLY:
                Gizmos.color = Color.yellow;
                str = "png";
                break;
        }
        Gizmos.DrawIcon(base.transform.position + new Vector3(0f, 1f, 0f), id.ToString().ToLower() + "_point." + str, allowScaling: true);
        PandoraUtils.DrawFacingGizmoCube(base.transform, 1f, 0.5f, 0.5f);
    }

    public void Register()
    {
        if (Physics.Raycast(base.transform.position + base.transform.up * 2f, base.transform.up * -1f, 1.8f, LayerMaskManager.decisionMask) || Physics.Raycast(base.transform.position + base.transform.up + base.transform.right / 3f, base.transform.right * -1f, 1f, LayerMaskManager.decisionMask) || Physics.Raycast(base.transform.position + base.transform.up - base.transform.right / 3f, base.transform.right, 1f, LayerMaskManager.decisionMask) || Physics.Raycast(base.transform.position + base.transform.up + base.transform.forward / 3f, base.transform.forward * -1f, 1f, LayerMaskManager.decisionMask) || Physics.Raycast(base.transform.position + base.transform.up - base.transform.forward / 3f, base.transform.forward, 1f, LayerMaskManager.decisionMask) || !Physics.Raycast(base.transform.position + base.transform.up, base.transform.up * -1f, 1.5f, LayerMaskManager.decisionMask))
        {
            Object.DestroyImmediate(base.gameObject);
        }
        else
        {
            PandoraSingleton<MissionManager>.Instance.RegisterDecisionPoint(this);
        }
    }
}
