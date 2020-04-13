using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Dissolver : MonoBehaviour
{
    public float dissolveSpeed = 2f;

    private readonly List<Renderer> dissolveRenderers = new List<Renderer>();

    private float dissolve;

    private UnityAction onDissolved;

    public bool Ressolved => dissolve == 0f;

    public bool Dissolved => dissolve == 1f;

    public bool Dissolving => dissolve != 1f && dissolve != 0f;

    public void Hide(bool hide, bool force = false, UnityAction onDissolved = null)
    {
        this.onDissolved = onDissolved;
        dissolveRenderers.Clear();
        GetComponentsInChildren(includeInactive: true, dissolveRenderers);
        StopCoroutine("Dissolve");
        if (!hide)
        {
            for (int i = 0; i < dissolveRenderers.Count; i++)
            {
                Renderer renderer = dissolveRenderers[i];
                renderer.enabled = true;
            }
        }
        if (force || dissolveSpeed == 0f || dissolveRenderers.Count == 0)
        {
            dissolve = (hide ? 1 : 0);
            for (int j = 0; j < dissolveRenderers.Count; j++)
            {
                Renderer renderer2 = dissolveRenderers[j];
                if (renderer2 != null)
                {
                    Material[] materials = renderer2.materials;
                    for (int k = 0; k < materials.Length; k++)
                    {
                        materials[k].SetFloat("_Dissolve", dissolve);
                    }
                    renderer2.enabled = !hide;
                }
            }
            CallBack();
        }
        else if (base.gameObject.activeSelf)
        {
            StartCoroutine("Dissolve", hide);
        }
    }

    private IEnumerator Dissolve(bool hide)
    {
        float target = hide ? 1 : 0;
        while (dissolve != target)
        {
            dissolve += (float)((dissolve < target) ? 1 : (-1)) * dissolveSpeed * Time.deltaTime;
            dissolve = Mathf.Clamp(dissolve, 0f, 1f);
            for (int k = 0; k < dissolveRenderers.Count; k++)
            {
                Renderer rend = dissolveRenderers[k];
                if (rend != null)
                {
                    Material[] materials = rend.materials;
                    for (int i = 0; i < materials.Length; i++)
                    {
                        materials[i].SetFloat("_Dissolve", dissolve);
                    }
                }
            }
            yield return null;
        }
        if (hide && dissolve == target)
        {
            for (int j = 0; j < dissolveRenderers.Count; j++)
            {
                if (dissolveRenderers[j] != null)
                {
                    dissolveRenderers[j].enabled = false;
                }
            }
        }
        yield return null;
        CallBack();
    }

    private void CallBack()
    {
        if (onDissolved != null)
        {
            onDissolved();
        }
    }
}
