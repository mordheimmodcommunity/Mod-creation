public class HideoutOptions : ICheapState
{
	private HideoutManager mngr;

	private OptionsManager optionsMan;

	public HideoutOptions(HideoutManager manager)
	{
		mngr = manager;
		optionsMan = mngr.optionsPanel.GetComponentInChildren<OptionsManager>();
		optionsMan.onCloseOptionsMenu = OnCloseOptions;
		optionsMan.onQuitGame = OnBackToMenu;
		optionsMan.SetBackButtonLoc("go_to_camp");
		optionsMan.SetQuitButtonLoc("menu_back_main_menu", string.Empty);
		optionsMan.HideAltQuitOption();
		optionsMan.HideSaveAndQuitOption();
		mngr.optionsPanel.SetActive(value: false);
	}

	void ICheapState.Destroy()
	{
	}

	void ICheapState.Enter(int iFrom)
	{
		PandoraSingleton<PandoraInput>.Instance.PushInputLayer(PandoraInput.InputLayer.MENU);
		PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<TitleModule>(ModuleId.TITLE).gameObject.SetActive(value: false);
		PandoraSingleton<HideoutTabManager>.Instance.GetModuleRight<TreasuryModule>(ModuleId.TREASURY).gameObject.SetActive(value: false);
		PandoraSingleton<HideoutTabManager>.Instance.DeactivateAllButtons();
		mngr.optionsPanel.SetActive(value: true);
		optionsMan.OnShow();
	}

	void ICheapState.Exit(int iTo)
	{
		optionsMan.OnHide();
		mngr.optionsPanel.SetActive(value: false);
		PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<TitleModule>(ModuleId.TITLE).gameObject.SetActive(value: true);
		PandoraSingleton<HideoutTabManager>.Instance.GetModuleRight<TreasuryModule>(ModuleId.TREASURY).gameObject.SetActive(value: true);
		PandoraSingleton<PandoraInput>.Instance.PopInputLayer(PandoraInput.InputLayer.MENU);
	}

	void ICheapState.Update()
	{
	}

	void ICheapState.FixedUpdate()
	{
	}

	private void OnCloseOptions()
	{
		mngr.StateMachine.ChangeState(0);
	}

	private void OnBackToMenu(bool isAlt)
	{
		PandoraSingleton<HideoutManager>.Instance.messagePopup.Show("com_btn_quit_to_main", "com_quit_to_main_menu", OnBackPopup);
	}

	private void OnBackPopup(bool ok)
	{
		if (ok)
		{
			mngr.SaveChanges();
			SceneLauncher.Instance.LaunchScene(SceneLoadingId.OPTIONS_QUIT_GAME);
		}
	}
}
