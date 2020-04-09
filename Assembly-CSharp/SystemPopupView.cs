using System;

public class SystemPopupView : ConfirmationPopupView
{
	private bool inMission;

	protected override void Awake()
	{
		base.Awake();
		isSystem = true;
	}

	public override void Show(Action<bool> callback, bool hideButtons = false, bool hideCancel = false)
	{
		base.Show(callback, hideButtons, hideCancel);
		if (PandoraSingleton<PandoraInput>.Instance.GetCurrentState() == PandoraInput.States.MISSION)
		{
			inMission = true;
			PandoraSingleton<PandoraInput>.Instance.SetCurrentState(PandoraInput.States.MENU, showMouse: true);
		}
		PandoraSingleton<PandoraInput>.Instance.SetActive(active: true);
	}

	public override void Hide()
	{
		base.Hide();
		if (inMission)
		{
			inMission = false;
			PandoraSingleton<PandoraInput>.Instance.SetCurrentState(PandoraInput.States.MISSION, showMouse: false);
		}
	}
}
