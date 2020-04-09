public abstract class UIUnitControllerChanged : CanvasGroupDisabler
{
	public UnitController CurrentUnitController
	{
		get;
		set;
	}

	public UnitController TargetUnitController
	{
		get;
		set;
	}

	public Destructible TargetDestructible
	{
		get;
		set;
	}

	public bool UpdateUnit
	{
		get;
		set;
	}

	public virtual void UnitChanged(UnitController unitController, UnitController targetUnitController, Destructible targetDestructible = null)
	{
		CurrentUnitController = unitController;
		TargetUnitController = targetUnitController;
		TargetDestructible = targetDestructible;
		UpdateUnit = true;
	}

	protected abstract void OnUnitChanged();

	protected virtual void LateUpdate()
	{
		if (UpdateUnit)
		{
			UpdateUnit = false;
			OnUnitChanged();
		}
	}
}
