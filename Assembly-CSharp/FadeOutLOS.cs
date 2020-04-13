using System.Collections.Generic;
using UnityEngine;

public class FadeOutLOS : MonoBehaviour
{
    private class FadeoutLOSInfo
    {
        public GameObject gameOjbect;

        public bool needFadeOut;

        public FadeoutLOSInfo(GameObject go)
        {
            gameOjbect = go;
            needFadeOut = false;
        }
    }

    public LayerMask layerMask = 0;

    public float radius = 0.2f;

    private List<Transform> targets = new List<Transform>();

    private List<FadeoutLOSInfo> fadedOutObjects = new List<FadeoutLOSInfo>();

    private Stack<FadeoutLOSInfo> unUsedLOSInfo = new Stack<FadeoutLOSInfo>();

    public void AddLayer(int layer)
    {
        layerMask.value |= 1 << layer;
    }

    public void RemoveLayer(int layer)
    {
        layerMask.value &= ~(1 << layer);
    }

    public void ClearTargets()
    {
        targets.Clear();
    }

    public void AddTarget(Transform target)
    {
        if (isInLayer(target) && target != null && targets.IndexOf(target) == -1)
        {
            targets.Add(target);
        }
    }

    public void ReplaceTarget(Transform target)
    {
        if (isInLayer(target) && target != null)
        {
            if (targets.Count == 0)
            {
                AddTarget(target);
            }
            else
            {
                targets[0] = target;
            }
        }
    }

    public bool isInLayer(Transform target)
    {
        if (target != null)
        {
            return ((1 << target.gameObject.layer) & layerMask.value) > 0;
        }
        return false;
    }

    private void LateUpdate()
    {
        if (targets.Count == 0 || targets[0] == null)
        {
            return;
        }
        for (int i = 0; i < fadedOutObjects.Count; i++)
        {
            fadedOutObjects[i].needFadeOut = false;
        }
        Vector3 vector = base.transform.position - base.transform.forward;
        for (int j = 0; j < targets.Count; j++)
        {
            Vector3 vector2 = targets[j].position + Vector3.up * 1.3f;
            float num = Vector3.Distance(vector, vector2);
            RaycastHit[] array = Physics.SphereCastAll(vector, radius, vector2 - vector, num - radius, layerMask.value);
            for (int k = 0; k < array.Length; k++)
            {
                bool flag = true;
                for (int l = 0; l < targets.Count; l++)
                {
                    if (array[k].collider == targets[l].gameObject.GetComponent<Collider>())
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag)
                {
                    AddLOSInfo(array[k].collider.gameObject);
                }
            }
        }
        for (int num2 = fadedOutObjects.Count - 1; num2 >= 0; num2--)
        {
            FadeoutLOSInfo fadeoutLOSInfo = fadedOutObjects[num2];
            fadeoutLOSInfo.gameOjbect.SendMessage("Fade", fadeoutLOSInfo.needFadeOut, SendMessageOptions.DontRequireReceiver);
            if (!fadeoutLOSInfo.needFadeOut)
            {
                fadedOutObjects.RemoveAt(num2);
                unUsedLOSInfo.Push(fadeoutLOSInfo);
            }
        }
    }

    private void AddLOSInfo(GameObject go)
    {
        FadeoutLOSInfo fadeoutLOSInfo = null;
        for (int i = 0; i < fadedOutObjects.Count; i++)
        {
            if (fadedOutObjects[i].gameOjbect == go)
            {
                fadeoutLOSInfo = fadedOutObjects[i];
            }
        }
        if (fadeoutLOSInfo == null && unUsedLOSInfo.Count > 0)
        {
            fadeoutLOSInfo = unUsedLOSInfo.Pop();
            fadeoutLOSInfo.gameOjbect = go;
            fadedOutObjects.Add(fadeoutLOSInfo);
        }
        if (fadeoutLOSInfo == null)
        {
            fadeoutLOSInfo = new FadeoutLOSInfo(go);
            fadedOutObjects.Add(fadeoutLOSInfo);
        }
        fadeoutLOSInfo.needFadeOut = true;
    }
}
