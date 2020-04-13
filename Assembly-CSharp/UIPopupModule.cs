using UnityEngine;

public class UIPopupModule : MonoBehaviour
{
    public PopupModuleId popupModuleId;

    protected CanvasGroup canvas;

    public bool initialized
    {
        get;
        private set;
    }

    protected virtual void Awake()
    {
        canvas = GetComponent<CanvasGroup>();
    }

    public virtual void Init()
    {
        initialized = true;
    }

    public void SetInteractable(bool interactable)
    {
        if (canvas != null)
        {
            canvas.interactable = interactable;
            canvas.blocksRaycasts = interactable;
        }
    }
}
