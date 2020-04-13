using UnityEngine;

public abstract class BaseUIState : ICheapState
{
    protected GameObject View;

    protected Transform Transform;

    private Vector3 _startPosition;

    protected BaseUIState(GameObject view)
    {
        View = view;
        if (View != null)
        {
            Transform = View.transform;
            _startPosition = Transform.localPosition;
            View.SetActive(value: false);
        }
    }

    public abstract void Destroy();

    public abstract void InputAction();

    public abstract void InputCancel();

    public virtual void Enter(int iFrom)
    {
        if (View != null)
        {
            Transform.localPosition = Vector3.zero;
            View.SetActive(value: true);
        }
    }

    public virtual void Exit(int iTo)
    {
        if (View != null)
        {
            View.SetActive(value: false);
            Transform.localPosition = _startPosition;
        }
    }

    public virtual void Update()
    {
        if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("cancel") || PandoraSingleton<PandoraInput>.Instance.GetKeyUp("esc_cancel"))
        {
            InputCancel();
        }
        else if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("action"))
        {
            InputAction();
        }
    }

    public abstract void FixedUpdate();
}
