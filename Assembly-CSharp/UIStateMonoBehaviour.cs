using UnityEngine;

public abstract class UIStateMonoBehaviour<TStateMachine> : MonoBehaviour, IState where TStateMachine : IStateMachine
{
	public CanvasGroup canvasGroup;

	private Transform cachedTransform;

	private Vector3 lastPosition;

	public TStateMachine StateMachine
	{
		get;
		private set;
	}

	public abstract int StateId
	{
		get;
	}

	public virtual void Awake()
	{
		if (canvasGroup == null)
		{
			canvasGroup = GetComponent<CanvasGroup>();
		}
		cachedTransform = canvasGroup.transform;
		lastPosition = cachedTransform.localPosition;
		StateMachine = GetComponentInParent<TStateMachine>();
		StateMachine.Register(StateId, this);
	}

	protected virtual void Start()
	{
		Show(visible: false);
	}

	public abstract void StateEnter();

	public virtual void StateUpdate()
	{
		if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("cancel") || PandoraSingleton<PandoraInput>.Instance.GetKeyUp("esc_cancel"))
		{
			OnInputCancel();
		}
		else if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("action"))
		{
			OnInputAction(isMouse: false);
		}
		else if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("mouse_click"))
		{
			OnInputAction(isMouse: true);
		}
	}

	public abstract void StateExit();

	public virtual void OnInputAction(bool isMouse)
	{
	}

	public virtual void OnInputCancel()
	{
	}

	public void Show(bool visible)
	{
		canvasGroup.gameObject.SetActive(visible);
		canvasGroup.interactable = visible;
		cachedTransform.localPosition = ((!visible) ? lastPosition : Vector3.zero);
		canvasGroup.interactable = visible;
	}
}
