using UnityEngine;

public class HideoutCamp : ICheapState
{
	public enum NodeSlot
	{
		CAMP = -1,
		SHIPMENT,
		SHOP,
		LEADER,
		BANNER,
		DRAMATIS,
		WAGON,
		NEXT_DAY
	}

	private HideoutManager mngr;

	private HideoutCamAnchor camAnchor;

	private MenuNodeGroup nodeGroup;

	private CampSectionsModule campModule;

	private UnitMenuController leader;

	private WarbandTabsModule warbandTabs;

	private Color greenHighlightColor;

	private Color redHighlightColor;

	private bool once = true;

	public HideoutCamp(HideoutManager ctrl, HideoutCamAnchor anchor)
	{
		camAnchor = anchor;
		mngr = ctrl;
		greenHighlightColor = Constant.GetColor(ConstantId.COLOR_GREEN) / 2f;
		Color red = Color.red;
		red.a /= 4f;
		redHighlightColor = red;
	}

	void ICheapState.Destroy()
	{
	}

	void ICheapState.Enter(int iFrom)
	{
		PandoraSingleton<HideoutManager>.Instance.CamManager.ClearLookAtFocus();
		PandoraSingleton<HideoutManager>.Instance.CamManager.CancelTransition();
		mngr.CamManager.dummyCam.transform.position = camAnchor.transform.position;
		mngr.CamManager.dummyCam.transform.rotation = camAnchor.transform.rotation;
		PandoraSingleton<HideoutManager>.Instance.CamManager.SetDOFTarget(camAnchor.dofTarget, 0f);
		PandoraSingleton<HideoutTabManager>.Instance.ActivateLeftTabModules(false);
		PandoraSingleton<HideoutTabManager>.Instance.ActivateRightTabModules(false, ModuleId.TREASURY);
		PandoraSingleton<HideoutTabManager>.Instance.ActivateCenterTabModules(ModuleId.CAMP_SECTIONS, ModuleId.WARBAND_TABS, ModuleId.TITLE, ModuleId.NOTIFICATION);
		PandoraSingleton<HideoutTabManager>.Instance.button1.gameObject.SetActive(value: true);
		PandoraSingleton<HideoutTabManager>.Instance.button1.SetAction("cancel", "menu_options", 0, negative: false, PandoraSingleton<HideoutTabManager>.Instance.icnOptions);
		PandoraSingleton<HideoutTabManager>.Instance.button1.OnAction(ShowOptions, mouseOnly: false);
		PandoraSingleton<HideoutTabManager>.Instance.button2.gameObject.SetActive(value: false);
		PandoraSingleton<HideoutTabManager>.Instance.button3.gameObject.SetActive(value: false);
		PandoraSingleton<HideoutTabManager>.Instance.button4.gameObject.SetActive(value: false);
		warbandTabs = PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<WarbandTabsModule>(ModuleId.WARBAND_TABS);
		warbandTabs.Setup(PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<TitleModule>(ModuleId.TITLE));
		PandoraSingleton<HideoutTabManager>.Instance.GetModuleRight<TreasuryModule>(ModuleId.TREASURY).Refresh(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave());
		campModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<CampSectionsModule>(ModuleId.CAMP_SECTIONS);
		nodeGroup = PandoraSingleton<HideoutManager>.Instance.campNodeGroup;
		leader = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.GetLeader();
		if (leader != null)
		{
			nodeGroup.nodes[2].SetContent(leader);
			leader.SwitchWeapons(UnitSlotId.SET1_MAINHAND);
		}
		else
		{
			nodeGroup.nodes[2].SetContent(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.idol);
		}
		Cloth cloth = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.banner.GetComponentsInChildren<Cloth>(includeInactive: true)[0];
		cloth.enabled = false;
		nodeGroup.nodes[3].SetContent(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.banner);
		cloth.enabled = true;
		nodeGroup.nodes[5].SetContent(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.wagon);
		nodeGroup.nodes[0].SetContent(PandoraSingleton<HideoutManager>.Instance.GetShipmentNodeContent());
		nodeGroup.nodes[1].SetContent(PandoraSingleton<HideoutManager>.Instance.GetShopNodeContent());
		nodeGroup.nodes[6].SetContent(PandoraSingleton<HideoutManager>.Instance.GetNextDayNodeContent());
		UnitMenuController dramatis = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.GetDramatis();
		nodeGroup.nodes[4].SetContent(dramatis);
		dramatis.SwitchWeapons(UnitSlotId.SET1_MAINHAND);
		RefreshButtons();
		CheckTrophies();
		once = true;
	}

	void ICheapState.Exit(int iTo)
	{
		nodeGroup.Deactivate();
	}

	void ICheapState.Update()
	{
	}

	void ICheapState.FixedUpdate()
	{
		if (!PandoraSingleton<HideoutManager>.Instance.transitionDone || !once || PandoraSingleton<PandoraInput>.Instance.CurrentInputLayer != 0)
		{
			return;
		}
		once = false;
		nodeGroup.Activate(NodeSelecteded, null, NodeConfirmed, PandoraInput.InputLayer.NORMAL, unselectOverOut: false);
		nodeGroup.SelectNode(nodeGroup.nodes[0]);
		PandoraSingleton<HideoutManager>.Instance.ShowHideoutTuto(HideoutManager.HideoutTutoType.CAMP);
		if (PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate == Constant.GetInt(ConstantId.CAL_DAY_START) && (PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Units.Count != 1 || PandoraSingleton<HideoutManager>.Instance.IsPostMission()))
		{
			if (!PandoraSingleton<HideoutManager>.Instance.IsPostMission())
			{
				PandoraSingleton<HideoutManager>.Instance.ShowHideoutTuto(HideoutManager.HideoutTutoType.CAMP_2);
			}
			else if (PandoraSingleton<HideoutManager>.Instance.IsPostMission())
			{
				PandoraSingleton<HideoutManager>.Instance.ShowHideoutTuto(HideoutManager.HideoutTutoType.CAMP_3);
			}
		}
	}

	public void RefreshButtons()
	{
		warbandTabs.SetCurrentTab(HideoutManager.State.CAMP);
		warbandTabs.Refresh();
		for (int i = 0; i < warbandTabs.icons.Count; i++)
		{
			TabIcon tabIcon = warbandTabs.icons[i];
			if (tabIcon.nodeSlot != NodeSlot.CAMP)
			{
				if (tabIcon.available)
				{
					nodeGroup.nodes[(int)tabIcon.nodeSlot].SetHighlightColor(greenHighlightColor);
				}
				else
				{
					nodeGroup.nodes[(int)tabIcon.nodeSlot].SetHighlightColor(redHighlightColor);
				}
			}
		}
		campModule.Setup(IconSelected, null, IconConfirmed);
	}

	private void CheckTrophies()
	{
		int amount = PandoraSingleton<HideoutManager>.Instance.WarbandChest.GetItem(ItemId.WYRDSTONE_FRAGMENT).amount;
		int amount2 = PandoraSingleton<HideoutManager>.Instance.WarbandChest.GetItem(ItemId.WYRDSTONE_SHARD).amount;
		int amount3 = PandoraSingleton<HideoutManager>.Instance.WarbandChest.GetItem(ItemId.WYRDSTONE_CLUSTER).amount;
		if (amount >= 100 && amount2 >= 100 && amount3 >= 100)
		{
			PandoraSingleton<Hephaestus>.Instance.UnlockAchievement(Hephaestus.TrophyId.WYRDSTONES);
		}
		PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.CheckUnitStatus();
	}

	private void NodeSelecteded(MenuNode node, int idx)
	{
		campModule.Refresh(idx);
	}

	private void NodeConfirmed(MenuNode node, int idx)
	{
		TabIcon tabIcon = warbandTabs.GetTabIcon((NodeSlot)idx);
		if (!tabIcon.available)
		{
			nodeGroup.SelectNode(node);
			return;
		}
		switch (idx)
		{
		case 2:
			PandoraSingleton<HideoutManager>.Instance.StateMachine.ChangeState(1);
			break;
		case 3:
			PandoraSingleton<HideoutManager>.Instance.StateMachine.ChangeState(3);
			break;
		case 5:
			PandoraSingleton<HideoutManager>.Instance.StateMachine.ChangeState(12);
			break;
		case 0:
			PandoraSingleton<HideoutManager>.Instance.StateMachine.ChangeState(9);
			break;
		case 4:
			PandoraSingleton<HideoutManager>.Instance.StateMachine.ChangeState(11);
			break;
		case 1:
			PandoraSingleton<HideoutManager>.Instance.StateMachine.ChangeState(10);
			break;
		case 6:
			PandoraSingleton<HideoutManager>.Instance.OnNextDay();
			break;
		}
	}

	private void IconSelected(int idx)
	{
		nodeGroup.SelectNode(nodeGroup.nodes[idx]);
	}

	private void IconConfirmed(int idx)
	{
		if (PandoraSingleton<PandoraInput>.Instance.CurrentInputLayer == 0)
		{
			NodeConfirmed(null, idx);
		}
	}

	private void ShowOptions()
	{
		PandoraSingleton<HideoutManager>.Instance.StateMachine.ChangeState(17);
	}
}
