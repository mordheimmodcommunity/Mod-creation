using System;
using UnityEngine;
using UnityEngine.UI;

public class WarbandRankPopupView : MonoBehaviour
{
    public Text title;

    public SelectorGroup rank;

    protected Action<bool, int> _callback;

    private Vector3 _startPosition;

    public ButtonGroup confirmButton;

    public ButtonGroup cancelButton;

    protected bool isShow;

    protected bool init;

    public bool IsVisible => base.gameObject.activeSelf;

    protected virtual void Awake()
    {
    }

    protected virtual void Start()
    {
        if (!isShow)
        {
            base.gameObject.SetActive(value: false);
        }
    }

    protected virtual void Init()
    {
        init = true;
        if (confirmButton != null)
        {
            confirmButton.SetAction("action", "menu_confirm", 1);
            confirmButton.OnAction(Confirm, mouseOnly: false);
        }
        if (cancelButton != null)
        {
            cancelButton.SetAction("cancel", "menu_cancel", 1);
            cancelButton.OnAction(Cancel, mouseOnly: false);
        }
    }

    public virtual void Show(string titleId, string textId, Action<bool, int> callback, bool hideButtons = false)
    {
        title.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById(titleId));
        Show(callback, hideButtons);
    }

    public virtual void Show(Action<bool, int> callback, bool hideButtons = false)
    {
        isShow = true;
        _callback = callback;
        if (!init)
        {
            Init();
        }
        if (hideButtons)
        {
            if (confirmButton != null)
            {
                confirmButton.gameObject.SetActive(value: false);
            }
            if (cancelButton != null)
            {
                cancelButton.gameObject.SetActive(value: false);
            }
        }
        else
        {
            if (confirmButton != null)
            {
                confirmButton.gameObject.SetActive(value: true);
            }
            if (cancelButton != null)
            {
                cancelButton.gameObject.SetActive(value: true);
            }
        }
        base.gameObject.SetActive(value: true);
        PandoraSingleton<PandoraInput>.Instance.PushInputLayer(PandoraInput.InputLayer.POP_UP);
    }

    public virtual void Hide()
    {
        if (isShow)
        {
            isShow = false;
            base.gameObject.SetActive(value: false);
            PandoraSingleton<PandoraInput>.Instance.PopInputLayer(PandoraInput.InputLayer.POP_UP);
        }
    }

    public virtual void Confirm()
    {
        Hide();
        if (_callback != null)
        {
            _callback(arg1: true, (rank.CurSel != 0) ? 5 : 0);
        }
    }

    public virtual void Cancel()
    {
        Hide();
        if (_callback != null)
        {
            _callback(arg1: false, (rank.CurSel != 0) ? 5 : 0);
        }
    }
}
