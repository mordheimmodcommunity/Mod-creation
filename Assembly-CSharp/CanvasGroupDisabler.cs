using UnityEngine;

public class CanvasGroupDisabler : MonoBehaviour
{
    private bool isInit;

    private CanvasGroup canvasGroup;

    public CanvasGroup CanvasGroup
    {
        get
        {
            if (canvasGroup == null && !isInit)
            {
                isInit = true;
                canvasGroup = GetComponent<CanvasGroup>();
            }
            return canvasGroup;
        }
        set
        {
            canvasGroup = value;
        }
    }

    public bool IsVisible => CanvasGroup != null && Mathf.Approximately(CanvasGroup.alpha, 1f) && CanvasGroup.interactable;

    public virtual void OnEnable()
    {
        if (CanvasGroup != null)
        {
            CanvasGroup.alpha = 1f;
            CanvasGroup.interactable = true;
            CanvasGroup.blocksRaycasts = true;
        }
    }

    public virtual void OnDisable()
    {
        if (CanvasGroup != null)
        {
            CanvasGroup.alpha = 0f;
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;
        }
    }
}
