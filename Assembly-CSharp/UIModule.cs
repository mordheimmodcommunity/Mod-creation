using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIModule : UIPopupModule
{
    public ModuleId moduleId;

    protected Queue<GameObject> showQueue;

    protected override void Awake()
    {
        base.Awake();
        showQueue = new Queue<GameObject>();
    }

    public void StartShow(float wait)
    {
        StartCoroutine("ShowNext", wait);
    }

    public bool EndShow()
    {
        if (showQueue.Count > 0)
        {
            StopCoroutine("ShowNext");
            while (showQueue.Count > 0)
            {
                showQueue.Dequeue().SetActive(value: true);
            }
            return false;
        }
        return true;
    }

    public IEnumerator ShowNext(float wait)
    {
        while (showQueue.Count > 0)
        {
            yield return new WaitForSeconds(wait);
            showQueue.Dequeue().SetActive(value: true);
        }
    }
}
