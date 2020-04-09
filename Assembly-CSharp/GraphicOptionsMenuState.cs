public class GraphicOptionsMenuState : ICheapState
{
	private OptionsMenuState controller;

	private int selectionX;

	private int selectionY;

	public GraphicOptionsMenuState(OptionsMenuState ctrlr)
	{
		controller = ctrlr;
	}

	void ICheapState.Destroy()
	{
	}

	void ICheapState.Enter(int iFrom)
	{
		PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.INPUT_CANCEL, InputCancel);
		selectionX = 0;
		selectionY = 0;
		UpdateSelection(0, 0);
	}

	void ICheapState.Exit(int iTo)
	{
		PandoraSingleton<NoticeManager>.Instance.RemoveListener(Notices.INPUT_CANCEL, InputCancel);
	}

	void ICheapState.Update()
	{
		if (PandoraSingleton<PandoraInput>.Instance.GetKeyDown("v"))
		{
			UpdateSelection(0, -1);
		}
		else if (PandoraSingleton<PandoraInput>.Instance.GetNegKeyDown("v"))
		{
			UpdateSelection(0, 1);
		}
		else if (PandoraSingleton<PandoraInput>.Instance.GetKeyDown("h"))
		{
			UpdateSelection(-1, 0);
		}
		else if (PandoraSingleton<PandoraInput>.Instance.GetNegKeyDown("h"))
		{
			UpdateSelection(1, 0);
		}
		else if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("action"))
		{
			InputAction();
		}
		else if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("cancel"))
		{
			InputCancel();
		}
	}

	void ICheapState.FixedUpdate()
	{
	}

	private void InputAction()
	{
		MenuConfig.OptionsGraphics optionsGraphics = MenuConfig.menuOptionsGfx[selectionX][selectionY];
		if (optionsGraphics == MenuConfig.OptionsGraphics.REVERT)
		{
		}
	}

	private void InputCancel()
	{
		controller.GoTo(OptionsMenuState.State.BROWSE);
	}

	private void UpdateSelection(int x, int y)
	{
		selectionX += x;
		if (selectionX >= MenuConfig.menuOptionsGfx.Length)
		{
			selectionX = 0;
		}
		else if (selectionX < 0)
		{
			selectionX = MenuConfig.menuOptionsGfx.Length - 1;
		}
		selectionY += y;
		if (selectionY >= MenuConfig.menuOptionsGfx[selectionX].Length)
		{
			selectionY = 0;
		}
		else if (selectionY < 0)
		{
			selectionY = MenuConfig.menuOptionsGfx[selectionX].Length - 1;
		}
		PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.MENU_GRAPHICS_SELECTED, MenuConfig.menuOptionsGfx[selectionX][selectionY]);
	}
}
