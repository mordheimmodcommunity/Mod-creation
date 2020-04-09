using UnityEngine.Events;
using UnityEngine.UI;

public class HideoutSmuggler : ICheapState
{
	private HideoutCamAnchor camAnchor;

	private TreasuryModule treasury;

	private SmugglerFactionOverviewModule factionsOverview;

	private SmugglerFactionShipmentModule factionShipment;

	private SmugglerFactionBonusModule factionBonus;

	private WarbandTabsModule warbandTabs;

	private bool once = true;

	public HideoutSmuggler(HideoutManager mng, HideoutCamAnchor anchor)
	{
		camAnchor = anchor;
	}

	void ICheapState.Destroy()
	{
	}

	void ICheapState.Enter(int iFrom)
	{
		PandoraSingleton<HideoutManager>.Instance.CamManager.ClearLookAtFocus();
		PandoraSingleton<HideoutManager>.Instance.CamManager.CancelTransition();
		PandoraSingleton<HideoutManager>.Instance.CamManager.dummyCam.transform.position = camAnchor.transform.position;
		PandoraSingleton<HideoutManager>.Instance.CamManager.dummyCam.transform.rotation = camAnchor.transform.rotation;
		PandoraSingleton<HideoutManager>.Instance.CamManager.SetDOFTarget(camAnchor.dofTarget, 0f);
		PandoraSingleton<HideoutTabManager>.Instance.ActivateLeftTabModules(false, ModuleId.SMUGGLER);
		PandoraSingleton<HideoutTabManager>.Instance.ActivateRightTabModules(false, ModuleId.SMUGGLER, ModuleId.TREASURY);
		PandoraSingleton<HideoutTabManager>.Instance.ActivateCenterTabModules(ModuleId.SMUGGLER, ModuleId.WARBAND_TABS, ModuleId.TITLE, ModuleId.NOTIFICATION);
		warbandTabs = PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<WarbandTabsModule>(ModuleId.WARBAND_TABS);
		warbandTabs.Setup(PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<TitleModule>(ModuleId.TITLE));
		warbandTabs.SetCurrentTab(HideoutManager.State.SHIPMENT);
		warbandTabs.Refresh();
		treasury = PandoraSingleton<HideoutTabManager>.Instance.GetModuleRight<TreasuryModule>(ModuleId.TREASURY);
		treasury.Refresh(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave());
		factionShipment = PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<SmugglerFactionShipmentModule>(ModuleId.SMUGGLER);
		Toggle[] componentsInChildren = factionShipment.GetComponentsInChildren<Toggle>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			((UnityEvent<bool>)(object)componentsInChildren[i].onValueChanged).AddListener((UnityAction<bool>)delegate(bool on)
			{
				if (on)
				{
					SetCenterPanelButtons();
				}
			});
		}
		factionBonus = PandoraSingleton<HideoutTabManager>.Instance.GetModuleRight<SmugglerFactionBonusModule>(ModuleId.SMUGGLER);
		factionsOverview = PandoraSingleton<HideoutTabManager>.Instance.GetModuleLeft<SmugglerFactionOverviewModule>(ModuleId.SMUGGLER);
		factionsOverview.Setup(OnFactionHighlighted, OnFactionConfirmed);
		SetFocusOnLeftPanel();
		ValidateFactionReputations();
		once = true;
	}

	void ICheapState.Exit(int iTo)
	{
	}

	void ICheapState.Update()
	{
	}

	void ICheapState.FixedUpdate()
	{
		if (once)
		{
			once = false;
			PandoraSingleton<HideoutManager>.Instance.ShowHideoutTuto(HideoutManager.HideoutTutoType.SMUGGLER);
		}
	}

	private void ValidateFactionReputations()
	{
		for (int i = 0; i < PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.factionCtrlrs.Count; i++)
		{
			FactionMenuController factionMenuController = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.factionCtrlrs[i];
			int rank = factionMenuController.Faction.Rank;
			int num = factionMenuController.AddReputation(0);
			if (rank != num)
			{
				PandoraSingleton<HideoutManager>.Instance.Progressor.IncrementFactionRank(factionMenuController, num);
			}
		}
	}

	private void OnFactionHighlighted(FactionMenuController faction)
	{
		factionShipment.OnLostFocus();
		factionShipment.Setup(faction, OnShipmentSent);
		factionBonus.Setup(faction);
	}

	private void OnFactionConfirmed(FactionMenuController faction)
	{
		factionsOverview.OnLostFocus();
		SetFocusOnCenterPanel();
	}

	private void OnShipmentSent()
	{
		factionsOverview.Refresh();
		SetFocusOnLeftPanel();
		treasury.Refresh(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave());
	}

	private void SetLeftPanelButtons()
	{
		PandoraSingleton<HideoutTabManager>.Instance.button1.gameObject.SetActive(value: true);
		PandoraSingleton<HideoutTabManager>.Instance.button1.SetAction("cancel", "go_to_camp", 0, negative: false, PandoraSingleton<HideoutTabManager>.Instance.icnBack);
		PandoraSingleton<HideoutTabManager>.Instance.button1.OnAction(delegate
		{
			PandoraSingleton<HideoutManager>.Instance.StateMachine.ChangeState(0);
		}, mouseOnly: false);
		if (PandoraSingleton<PandoraInput>.Instance.lastInputMode == PandoraInput.InputMode.JOYSTICK)
		{
			PandoraSingleton<HideoutTabManager>.Instance.button2.SetAction("action", "menu_confirm");
			PandoraSingleton<HideoutTabManager>.Instance.button2.OnAction(null, mouseOnly: false);
		}
		else
		{
			PandoraSingleton<HideoutTabManager>.Instance.button2.gameObject.SetActive(value: false);
		}
		PandoraSingleton<HideoutTabManager>.Instance.button3.gameObject.SetActive(value: false);
		PandoraSingleton<HideoutTabManager>.Instance.button4.gameObject.SetActive(value: false);
	}

	private void SetCenterPanelButtons()
	{
		PandoraSingleton<HideoutTabManager>.Instance.button1.gameObject.SetActive(value: true);
		PandoraSingleton<HideoutTabManager>.Instance.button1.SetAction("cancel", "menu_back", 0, negative: false, PandoraSingleton<HideoutTabManager>.Instance.icnBack);
		PandoraSingleton<HideoutTabManager>.Instance.button1.OnAction(delegate
		{
			SetFocusOnLeftPanel();
		}, mouseOnly: false);
		if (PandoraSingleton<PandoraInput>.Instance.lastInputMode == PandoraInput.InputMode.JOYSTICK)
		{
			PandoraSingleton<HideoutTabManager>.Instance.button2.SetAction("action", "menu_confirm");
			PandoraSingleton<HideoutTabManager>.Instance.button2.OnAction(null, mouseOnly: false);
		}
		else
		{
			PandoraSingleton<HideoutTabManager>.Instance.button2.gameObject.SetActive(value: false);
		}
		PandoraSingleton<HideoutTabManager>.Instance.button3.gameObject.SetActive(value: false);
		PandoraSingleton<HideoutTabManager>.Instance.button4.gameObject.SetActive(value: false);
	}

	private void SetFocusOnCenterPanel()
	{
		SetCenterPanelButtons();
		factionShipment.SetFocus();
	}

	private void SetFocusOnLeftPanel()
	{
		SetLeftPanelButtons();
		factionsOverview.SetFocus();
		factionShipment.OnLostFocus();
	}
}
