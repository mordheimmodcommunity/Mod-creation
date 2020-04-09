using System.Collections.Generic;
using UnityEngine;

public class HideoutMission : ICheapState
{
	private HideoutCamAnchor camAnchor;

	private Dictionary<int, GameObject> pawns;

	private List<ScoutPriceData> priceDatas;

	private int selectedMission = -1;

	private Warband warband;

	private bool missionConfirm;

	private WarbandTabsModule warbandTabs;

	private bool once = true;

	public HideoutMission(HideoutManager mng, HideoutCamAnchor anchor)
	{
		camAnchor = anchor;
		pawns = new Dictionary<int, GameObject>();
		for (int i = 0; i <= 3; i++)
		{
			string text = "map_pawn_" + ((PrimaryObjectiveTypeId)i).ToString();
			int index = i;
			PandoraDebug.LogDebug("Map Pawn = " + text);
			PandoraSingleton<AssetBundleLoader>.Instance.LoadAssetAsync<GameObject>("Assets/prefabs/environments/props/maps/", AssetBundleId.PROPS, text + ".prefab", delegate(Object go)
			{
				pawns[index] = (GameObject)go;
			});
		}
	}

	void ICheapState.Update()
	{
	}

	public void Destroy()
	{
	}

	public void Enter(int iFrom)
	{
		warband = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband;
		priceDatas = PandoraSingleton<DataFactory>.Instance.InitData<ScoutPriceData>("warband_rank", warband.Rank.ToString());
		missionConfirm = false;
		PandoraSingleton<HideoutManager>.Instance.CamManager.ClearLookAtFocus();
		PandoraSingleton<HideoutManager>.Instance.CamManager.CancelTransition();
		PandoraSingleton<HideoutManager>.Instance.CamManager.dummyCam.transform.position = camAnchor.transform.position;
		PandoraSingleton<HideoutManager>.Instance.CamManager.dummyCam.transform.rotation = camAnchor.transform.rotation;
		PandoraSingleton<HideoutManager>.Instance.CamManager.SetDOFTarget(camAnchor.dofTarget, 0f);
		PandoraSingleton<HideoutTabManager>.Instance.ActivateLeftTabModules(true, ModuleId.WARBAND_SHEET, ModuleId.MISSION);
		WarbandSheetModule moduleLeft = PandoraSingleton<HideoutTabManager>.Instance.GetModuleLeft<WarbandSheetModule>(ModuleId.WARBAND_SHEET);
		moduleLeft.Set(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband);
		PandoraSingleton<HideoutTabManager>.Instance.ActivateRightTabModules(false, ModuleId.TREASURY);
		PandoraSingleton<HideoutTabManager>.Instance.ActivateCenterTabModules(ModuleId.WARBAND_TABS, ModuleId.TITLE, ModuleId.NOTIFICATION);
		warbandTabs = PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<WarbandTabsModule>(ModuleId.WARBAND_TABS);
		warbandTabs.Setup(PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<TitleModule>(ModuleId.TITLE));
		warbandTabs.SetCurrentTab(HideoutManager.State.MISSION);
		warbandTabs.Refresh();
		PandoraSingleton<HideoutTabManager>.Instance.GetModuleLeft<MissionModule>(ModuleId.MISSION).Setup(OnScoutButton);
		PandoraSingleton<HideoutTabManager>.Instance.button1.gameObject.SetActive(value: true);
		PandoraSingleton<HideoutTabManager>.Instance.button1.SetAction("cancel", "go_to_camp", 0, negative: false, PandoraSingleton<HideoutTabManager>.Instance.icnBack);
		PandoraSingleton<HideoutTabManager>.Instance.button1.OnAction(delegate
		{
			PandoraSingleton<HideoutManager>.Instance.StateMachine.ChangeState(0);
		}, mouseOnly: false);
		PandoraSingleton<HideoutTabManager>.Instance.button2.gameObject.SetActive(value: false);
		PandoraSingleton<HideoutTabManager>.Instance.button3.gameObject.SetActive(value: false);
		PandoraSingleton<HideoutTabManager>.Instance.button4.gameObject.SetActive(value: false);
		RefreshMissionsDifficulty();
		MenuNode mapNode = PandoraSingleton<HideoutManager>.Instance.mapNode;
		mapNode.SetContent(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.map.gameObject);
		PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.map.transform.localPosition = Vector3.zero;
		PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.map.transform.localScale = Vector3.one;
		PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.map.Clear();
		for (int i = 0; i < PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.map.nodes.Count; i++)
		{
			PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.map.nodes[i].gameObject.SetActive(value: false);
		}
		AddPawns(0);
		if (PandoraSingleton<HideoutManager>.Instance.missions.Count == 0)
		{
			PandoraSingleton<HideoutManager>.Instance.messagePopup.Show("missions_unavailable_title", "missions_unavailable_desc", delegate
			{
				PandoraSingleton<HideoutManager>.Instance.StateMachine.ChangeState(0);
			});
			PandoraSingleton<HideoutManager>.Instance.messagePopup.HideCancelButton();
		}
		else
		{
			PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.map.Activate(CampNodeSelecteded, Dis1NodeSelecteded, Dis2NodeSelecteded, NodeUnselect, NodeConfirmed, PandoraInput.InputLayer.NORMAL);
		}
		once = true;
	}

	public void Exit(int iTo)
	{
		WarbandSwapModule moduleCenter = PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<WarbandSwapModule>(ModuleId.SWAP);
		if (moduleCenter != null && moduleCenter.isActiveAndEnabled)
		{
			moduleCenter.ForceClose();
		}
		PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.map.Deactivate();
	}

	private void RefreshMissionsDifficulty()
	{
		for (int i = 0; i < PandoraSingleton<HideoutManager>.Instance.missions.Count; i++)
		{
			if (PandoraSingleton<HideoutManager>.Instance.missions[i].missionSave.isCampaign)
			{
				PandoraSingleton<HideoutManager>.Instance.missions[i].RefreshDifficulty(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetRating(), isProc: false);
			}
			else
			{
				PandoraSingleton<HideoutManager>.Instance.missions[i].RefreshDifficulty(PandoraSingleton<HideoutManager>.Instance.missions[i].missionSave.rating, isProc: true);
			}
		}
	}

	private void AddPawns(int startIndex)
	{
		PandoraDebug.LogDebug("Mission Counts = " + PandoraSingleton<HideoutManager>.Instance.missions.Count);
		for (int i = startIndex; i < PandoraSingleton<HideoutManager>.Instance.missions.Count; i++)
		{
			Mission mission = PandoraSingleton<HideoutManager>.Instance.missions[i];
			if (mission.missionSave.isCampaign)
			{
				PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.map.campaignNodes[mission.missionSave.mapPosition].gameObject.SetActive(value: true);
				PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.map.campaignNodes[mission.missionSave.mapPosition].SetContent(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.mapPawn);
			}
			else if (pawns.ContainsKey(mission.missionSave.objectiveTypeIds[0]))
			{
				if (mission.GetDistrictId() == DistrictId.MERCHANT)
				{
					PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.map.procNodesDis01[mission.missionSave.mapPosition].gameObject.SetActive(value: true);
					PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.map.procNodesDis01[mission.missionSave.mapPosition].SetContent(Object.Instantiate(pawns[mission.missionSave.objectiveTypeIds[0]]));
				}
				else if (mission.GetDistrictId() == DistrictId.NOBLE)
				{
					PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.map.procNodesDis02[mission.missionSave.mapPosition].gameObject.SetActive(value: true);
					PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.map.procNodesDis02[mission.missionSave.mapPosition].SetContent(Object.Instantiate(pawns[mission.missionSave.objectiveTypeIds[0]]));
				}
			}
			else
			{
				PandoraDebug.LogError("Pawns sholud already be loaded! HideoutMission::AddPawns");
			}
		}
	}

	private void OnScoutButton()
	{
		PandoraSingleton<HideoutTabManager>.Instance.ActivatePopupModules(PopupBgSize.SMALL, PopupModuleId.POPUP_GENERIC_ANCHOR, PopupModuleId.POPUP_SCOUT);
		List<UIPopupModule> modulesPopup = PandoraSingleton<HideoutTabManager>.Instance.GetModulesPopup<UIPopupModule>(new PopupModuleId[2]
		{
			PopupModuleId.POPUP_GENERIC_ANCHOR,
			PopupModuleId.POPUP_SCOUT
		});
		modulesPopup[0].GetComponent<ConfirmationPopupView>().Show("hideout_mission_scout_title", "hideout_mission_scout_desc", OnScoutConfirm);
		WarbandSave warbandSave = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave();
		int scoutCost = warband.GetScoutCost(priceDatas[warbandSave.scoutsSent]);
		modulesPopup[1].gameObject.GetComponent<UIDescription>().SetLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("skill_cost_title"), scoutCost.ToString());
	}

	private void OnScoutConfirm(bool isConfirm)
	{
		if (isConfirm)
		{
			WarbandSave warbandSave = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave();
			PandoraSingleton<HideoutManager>.Instance.WarbandChest.RemoveGold(warband.GetScoutCost(priceDatas[warbandSave.scoutsSent]));
			warbandSave.scoutsSent++;
			Mission miss = PandoraSingleton<HideoutManager>.Instance.AddProcMission(boost: true);
			PandoraSingleton<HideoutManager>.Instance.SaveChanges();
			AddPawns(PandoraSingleton<HideoutManager>.Instance.missions.Count - 1);
			PandoraSingleton<HideoutTabManager>.Instance.GetModuleLeft<MissionModule>(ModuleId.MISSION).RefreshScoutButton();
			PandoraSingleton<HideoutTabManager>.Instance.GetModuleRight<TreasuryModule>(ModuleId.TREASURY).Refresh(warbandSave);
			ShowScoutedMission(miss);
		}
	}

	private void ShowScoutedMission(Mission miss)
	{
		PandoraSingleton<HideoutTabManager>.Instance.ActivatePopupModules(PopupBgSize.SMALL, PopupModuleId.POPUP_GENERIC_ANCHOR, PopupModuleId.POPUP_SCOUTED_MISSION);
		List<UIPopupModule> modulesPopup = PandoraSingleton<HideoutTabManager>.Instance.GetModulesPopup<UIPopupModule>(new PopupModuleId[2]
		{
			PopupModuleId.POPUP_GENERIC_ANCHOR,
			PopupModuleId.POPUP_SCOUTED_MISSION
		});
		PandoraDebug.LogDebug("module length = " + modulesPopup.Count);
		ConfirmationPopupView component = modulesPopup[0].GetComponent<ConfirmationPopupView>();
		component.Show("hideout_mission_found_title", "hideout_mission_found_desc", null);
		component.HideCancelButton();
		WarbandSave warbandSave = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave();
		modulesPopup[1].gameObject.GetComponent<MissionDescModule>().SetupShort(miss);
	}

	private void CampNodeSelecteded(MenuNode node, int idx)
	{
		PandoraDebug.LogDebug("Node Selected");
		selectedMission = 0;
		SelectMission(PandoraSingleton<HideoutManager>.Instance.missions[0]);
	}

	private void Dis1NodeSelecteded(MenuNode node, int idx)
	{
		PandoraDebug.LogDebug("Node Selected");
		int num = 0;
		while (true)
		{
			if (num < PandoraSingleton<HideoutManager>.Instance.missions.Count)
			{
				if (!PandoraSingleton<HideoutManager>.Instance.missions[num].missionSave.isCampaign && PandoraSingleton<HideoutManager>.Instance.missions[num].GetDistrictId() == DistrictId.MERCHANT && PandoraSingleton<HideoutManager>.Instance.missions[num].missionSave.mapPosition == idx)
				{
					break;
				}
				num++;
				continue;
			}
			return;
		}
		selectedMission = num;
		SelectMission(PandoraSingleton<HideoutManager>.Instance.missions[num]);
	}

	private void Dis2NodeSelecteded(MenuNode node, int idx)
	{
		int num = 0;
		while (true)
		{
			if (num < PandoraSingleton<HideoutManager>.Instance.missions.Count)
			{
				if (!PandoraSingleton<HideoutManager>.Instance.missions[num].missionSave.isCampaign && PandoraSingleton<HideoutManager>.Instance.missions[num].GetDistrictId() == DistrictId.NOBLE && PandoraSingleton<HideoutManager>.Instance.missions[num].missionSave.mapPosition == idx)
				{
					break;
				}
				num++;
				continue;
			}
			return;
		}
		selectedMission = num;
		SelectMission(PandoraSingleton<HideoutManager>.Instance.missions[num]);
	}

	private void NodeConfirmed(MenuNode node, int idx)
	{
		ConfirmMission(selectedMission);
	}

	private void NodeUnselect(MenuNode node, int idx)
	{
		PandoraDebug.LogDebug("Node Unselected");
		if (!missionConfirm)
		{
			PandoraSingleton<HideoutTabManager>.Instance.ActivateCenterTabModule(false, ModuleId.MISSION_DESC);
		}
	}

	private void SelectMission(Mission miss)
	{
		if (!missionConfirm)
		{
			PandoraSingleton<HideoutTabManager>.Instance.ActivateCenterTabModule(true, ModuleId.MISSION_DESC);
			PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<MissionDescModule>(ModuleId.MISSION_DESC).Setup(miss);
		}
	}

	private void ConfirmMission(int selectedMiss)
	{
		missionConfirm = true;
		PandoraSingleton<HideoutTabManager>.Instance.ActivateCenterTabModules(ModuleId.SWAP);
		PandoraSingleton<HideoutTabManager>.Instance.ActivateLeftTabModules(false);
		PandoraSingleton<HideoutTabManager>.Instance.ActivateRightTabModules(false);
		WarbandSwapModule moduleCenter = PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<WarbandSwapModule>(ModuleId.SWAP);
		moduleCenter.Set(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband, OnMissionConfirm, isMission: true, PandoraSingleton<HideoutManager>.Instance.missions[selectedMission].missionSave.isCampaign, isContest: false, null, pushLayer: true);
	}

	private void OnMissionConfirm(bool isConfirm)
	{
		if (isConfirm)
		{
			PandoraDebug.LogDebug("Selected Mission = " + selectedMission + " Missions Count = " + PandoraSingleton<HideoutManager>.Instance.missions.Count);
			MissionSave missionSave = PandoraSingleton<HideoutManager>.Instance.missions[selectedMission].missionSave;
			if (!missionSave.isCampaign && (missionSave.ratingId == 3 || missionSave.ratingId == 4))
			{
				bool flag = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave().lastMissionAmbushed;
				if (!flag)
				{
					int num = PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0, 100);
					if (num < Constant.GetInt(ConstantId.AMBUSH_MISSION_PERC))
					{
						Mission value = Mission.GenerateAmbushMission(PandoraSingleton<HideoutManager>.Instance.missions[selectedMission]);
						PandoraSingleton<HideoutManager>.Instance.missions[selectedMission] = value;
						flag = true;
					}
				}
				else
				{
					flag = false;
				}
				PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave().lastMissionAmbushed = flag;
				PandoraSingleton<HideoutManager>.Instance.SaveChanges();
			}
			PandoraSingleton<HideoutManager>.Instance.missions[selectedMission].missionSave.autoDeploy = !PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<WarbandSwapModule>(ModuleId.SWAP).DeployRequested;
			PandoraSingleton<MissionStartData>.Instance.Clear();
			PandoraSingleton<PandoraInput>.Instance.SetActive(active: false);
			PandoraSingleton<GameManager>.Instance.StartCoroutine(PandoraSingleton<MissionStartData>.Instance.SetMissionFull(PandoraSingleton<HideoutManager>.Instance.missions[selectedMission], PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr, delegate
			{
				if (PandoraSingleton<HideoutManager>.Instance.missions[selectedMission].missionSave.isCampaign)
				{
					SceneLauncher.Instance.LaunchScene(SceneLoadingId.LAUNCH_MISSION_CAMPAIGN);
				}
				else
				{
					SceneLauncher.Instance.LaunchScene(SceneLoadingId.LAUNCH_MISSION);
				}
			}));
		}
		else
		{
			missionConfirm = false;
			PandoraSingleton<HideoutTabManager>.Instance.ActivateCenterTabModules(ModuleId.WARBAND_TABS, ModuleId.TITLE, ModuleId.NOTIFICATION);
			PandoraSingleton<HideoutTabManager>.Instance.ActivateLeftTabModules(true, ModuleId.WARBAND_SHEET, ModuleId.TITLE, ModuleId.MISSION);
			PandoraSingleton<HideoutTabManager>.Instance.ActivateRightTabModules(false, ModuleId.TREASURY);
			PandoraSingleton<HideoutTabManager>.Instance.GetModuleLeft<MissionModule>(ModuleId.MISSION).Setup(OnScoutButton);
			PandoraSingleton<HideoutTabManager>.Instance.button1.gameObject.SetActive(value: true);
			PandoraSingleton<HideoutTabManager>.Instance.button1.SetAction("cancel", "go_to_camp", 0, negative: false, PandoraSingleton<HideoutTabManager>.Instance.icnBack);
			PandoraSingleton<HideoutTabManager>.Instance.button1.OnAction(delegate
			{
				PandoraSingleton<HideoutManager>.Instance.StateMachine.ChangeState(0);
			}, mouseOnly: false);
			PandoraSingleton<HideoutTabManager>.Instance.button2.gameObject.SetActive(value: false);
			PandoraSingleton<HideoutTabManager>.Instance.button3.gameObject.SetActive(value: false);
			PandoraSingleton<HideoutTabManager>.Instance.button4.gameObject.SetActive(value: false);
		}
	}

	public void FixedUpdate()
	{
		if (!once)
		{
			return;
		}
		once = false;
		List<MenuNode> nodes = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.map.nodes;
		for (int i = 0; i < nodes.Count; i++)
		{
			if (nodes[i].IsSelectable && nodes[i].gameObject.activeSelf)
			{
				PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.map.SelectNode(nodes[i]);
				break;
			}
		}
		PandoraSingleton<HideoutManager>.Instance.ShowHideoutTuto(HideoutManager.HideoutTutoType.CAMPAIGN);
	}
}
