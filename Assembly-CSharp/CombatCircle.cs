using UnityEngine;

public class CombatCircle : MonoBehaviour
{
    public MeshCollider meshCollider;

    public GameObject enemy;

    public GameObject enemyEngaged;

    public GameObject friendly;

    public GameObject friendlyEngaged;

    private Renderer[] fxRenderers;

    public void Init(bool isEnemy, bool isEngaged, bool currentUnitIsPlayed, bool isLarge, float currentUnitRadius, float height)
    {
        GameObject gameObject = isEnemy ? ((!isEngaged) ? enemy : enemyEngaged) : ((!isEngaged) ? friendly : friendlyEngaged);
        enemy.SetActive(enemy == gameObject && currentUnitIsPlayed);
        enemyEngaged.SetActive(enemyEngaged == gameObject && currentUnitIsPlayed);
        friendly.SetActive(friendly == gameObject && currentUnitIsPlayed);
        friendlyEngaged.SetActive(friendlyEngaged == gameObject && currentUnitIsPlayed);
        float @float = Constant.GetFloat((!isLarge) ? ConstantId.MELEE_RANGE_NORMAL : ConstantId.MELEE_RANGE_LARGE);
        gameObject.transform.localScale = Vector3.one * @float;
        fxRenderers = gameObject.GetComponentsInChildren<Renderer>();
        float num = (@float - currentUnitRadius) * 2f;
        Vector3 localScale = new Vector3(num, height, num);
        meshCollider.transform.localScale = localScale;
        meshCollider.isTrigger = true;
        meshCollider.transform.localPosition = new Vector3(0f, height, 0f);
    }

    public void SetAlpha(float a)
    {
        if (fxRenderers == null)
        {
            return;
        }
        for (int i = 0; i < fxRenderers.Length; i++)
        {
            if (fxRenderers[i].material.HasProperty("_Color"))
            {
                Color color = fxRenderers[i].material.GetColor("_Color");
                color.a = a;
                fxRenderers[i].material.SetColor("_Color", color);
            }
        }
    }
}
