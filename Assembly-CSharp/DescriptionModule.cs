using UnityEngine;

public class DescriptionModule : UIModule
{
    public GameObject descPrefab;

    [HideInInspector]
    public UIDescription desc;

    public override void Init()
    {
        base.Init();
        PandoraDebug.LogDebug("Description Module Init");
        GameObject gameObject = Object.Instantiate(descPrefab);
        gameObject.transform.SetParent(base.transform, worldPositionStays: false);
        desc = gameObject.GetComponent<UIDescription>();
    }

    public void Show(bool visible)
    {
        desc.gameObject.SetActive(visible);
    }
}
