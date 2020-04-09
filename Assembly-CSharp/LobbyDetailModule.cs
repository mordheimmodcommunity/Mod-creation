using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LobbyDetailModule : UIModule
{
	private delegate string GetLoc();

	private const int TURN_TIMER_MAX_VALUE = 180;

	private const int DELPOY_TIMER_MAX_VALUE = 180;

	private const int BACKTRACKING_MAX_VALUE = 10;

	private const int TURN_TIMER_INCREASE = 15;

	private const int ROUT_THRESHOLD_INCREASE = 5;

	[SerializeField]
	private Text lobbyName;

	[SerializeField]
	private SelectorGroup privacy;

	[SerializeField]
	private SelectorGroup AI;

	[SerializeField]
	private SelectorGroup AIWarbandType;

	[SerializeField]
	private SelectorGroup roaming;

	[SerializeField]
	private SelectorGroup backtracking;

	[SerializeField]
	private SelectorGroup turnTimer;

	[SerializeField]
	private SelectorGroup deployTimer;

	[SerializeField]
	private SelectorGroup routThreshold;

	[SerializeField]
	private Text mapName;

	[SerializeField]
	public ScrollGroup mapList;

	[SerializeField]
	private GameObject mapItem;

	[SerializeField]
	private SelectorGroup timeOfDay;

	[SerializeField]
	private SelectorGroup gameplay;

	[SerializeField]
	private SelectorGroup fow;

	[SerializeField]
	private SelectorGroup deployment;

	[SerializeField]
	private SelectorGroup autodeploy;

	[SerializeField]
	private Text player1;

	[SerializeField]
	private Text player2;

	[SerializeField]
	private SelectorGroup position1;

	[SerializeField]
	private SelectorGroup position2;

	[SerializeField]
	private SelectorGroup gameType;

	[SerializeField]
	private Text player1Obj;

	[SerializeField]
	private Text player2Obj;

	[SerializeField]
	private SelectorGroup objective1;

	[SerializeField]
	private SelectorGroup objective2;

	public ButtonGroup inviteButton;

	public ButtonGroup swapButton;

	public ButtonGroup launchButton;

	public ButtonGroup displayProfile;

	public Sprite swapIcon;

	public Sprite launchIcon;

	private int currentMapIndex = -1;

	private SkirmishMap skirmishMap;

	private SkirmishManager skirmishManager;

	private Hephaestus.LobbyPrivacy lobbyPrivacy;

	public override void Init()
	{
		base.Init();
		skirmishMap = null;
		skirmishManager = PandoraSingleton<SkirmishManager>.Instance;
		mapList.Setup(mapItem, hideBarIfEmpty: false);
		mapList.gameObject.AddComponent<EventTrigger>().AddUnityEvent((EventTriggerType)1, delegate
		{
			ToggleCurrentMap();
		});
		currentMapIndex = 0;
		position1.id = 0;
		position2.id = 1;
		objective1.id = 0;
		objective2.id = 1;
		inviteButton.SetAction(string.Empty, "lobby_invite_friend");
		inviteButton.OnAction(OpenInviteInterface, mouseOnly: false);
	}

	public void SetLobbyData(Lobby data, bool isHost)
	{
		if (isHost)
		{
			SetHostLobby(data);
		}
		else
		{
			SetClientLobby(data);
		}
	}

	private void SetHostLobby(Lobby data)
	{
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0716: Unknown result type (might be due to invalid IL or missing references)
		//IL_071b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0731: Unknown result type (might be due to invalid IL or missing references)
		lobbyName.set_text(data.name);
		privacy.transform.parent.gameObject.SetActive(value: true);
		lobbyPrivacy = data.privacy;
		string key = "lobby_privacy_" + data.privacy.ToLowerString();
		string stringById = PandoraSingleton<LocalizationManager>.Instance.GetStringById(key);
		privacy.selections.Clear();
		privacy.selections.Add(stringById);
		privacy.SetCurrentSel(0);
		privacy.SetButtonsVisible(show: false);
		AI.selections.Clear();
		AI.SetButtonsVisible(PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isSkirmish);
		if (data.privacy != Hephaestus.LobbyPrivacy.OFFLINE)
		{
			AI.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_title_player"));
			SetInviteButtonVisible(visible: true);
		}
		if (PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isSkirmish)
		{
			AI.onValueChanged = OnSetAI;
			List<ProcMissionRatingData> list = PandoraSingleton<DataFactory>.Instance.InitData<ProcMissionRatingData>();
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Id != 0)
				{
					AI.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_title_ai_" + list[i].Id));
				}
			}
		}
		AI.SetCurrentSel(0);
		Selectable component = (Selectable)(object)privacy.transform.parent.gameObject.GetComponent<Toggle>();
		Navigation navigation = ((Selectable)AI.selection).get_navigation();
		((Navigation)(ref navigation)).set_selectOnUp(component);
		((Selectable)AI.selection).set_navigation(navigation);
		AIWarbandType.selections.Clear();
		List<WarbandData> list2 = PandoraSingleton<DataFactory>.Instance.InitData<WarbandData>("basic", "1");
		for (int j = 0; j < list2.Count; j++)
		{
			AIWarbandType.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById("com_wb_type_" + list2[j].Name.ToLowerInvariant()));
		}
		AIWarbandType.SetCurrentSel(0);
		if (data.privacy == Hephaestus.LobbyPrivacy.OFFLINE)
		{
			AIWarbandType.SetButtonsVisible(show: true);
		}
		else
		{
			AIWarbandType.SetButtonsVisible(show: false);
			((Component)(object)AIWarbandType.selection).GetComponent<Text>().set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("unit_name_none"));
		}
		roaming.selections.Clear();
		roaming.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_title_random"));
		List<UnitId> roamingUnitIds = PandoraSingleton<MissionStartData>.Instance.CurrentMission.GetRoamingUnitIds();
		for (int k = 0; k < roamingUnitIds.Count; k++)
		{
			roaming.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById("unit_name_" + roamingUnitIds[k]));
		}
		roaming.SetCurrentSel(1);
		roaming.SetButtonsVisible(show: true);
		backtracking.selections.Clear();
		backtracking.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_no_backtracking"));
		for (int l = 1; l <= 10; l++)
		{
			backtracking.selections.Add(l.ToString());
		}
		backtracking.SetCurrentSel(0);
		backtracking.SetButtonsVisible(show: true);
		turnTimer.selections.Clear();
		for (int m = 0; m <= 180; m += 15)
		{
			turnTimer.selections.Add((m != 0) ? m.ToString() : PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_no_timer"));
		}
		turnTimer.SetCurrentSel(0);
		turnTimer.SetButtonsVisible(show: true);
		deployTimer.selections.Clear();
		for (int m = 0; m <= 180; m += 15)
		{
			deployTimer.selections.Add((m != 0) ? m.ToString() : PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_no_timer"));
		}
		deployTimer.SetCurrentSel(0);
		deployTimer.SetButtonsVisible(show: true);
		routThreshold.selections.Clear();
		float @float = Constant.GetFloat(ConstantId.ROUT_RATIO_ALIVE);
		for (int n = 0; n <= 95; n += 5)
		{
			routThreshold.selections.Add(n.ToConstantString() + "%");
		}
		routThreshold.SetCurrentSel((int)(@float * 100f / 5f));
		routThreshold.SetButtonsVisible(show: true);
		autodeploy.selections.Clear();
		autodeploy.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_off"));
		autodeploy.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_on"));
		autodeploy.SetButtonsVisible(show: true);
		autodeploy.SetCurrentSel(0);
		mapList.ClearList();
		mapName.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_title_random"));
		component = (Selectable)(object)routThreshold.transform.parent.gameObject.GetComponent<Toggle>();
		Selectable component2 = (Selectable)(object)gameplay.transform.parent.gameObject.GetComponent<Toggle>();
		GameObject gameObject = mapList.AddToList(component, component2);
		Image component3 = gameObject.GetComponent<Image>();
		component3.set_sprite(Resources.Load<Sprite>("maps/img_map_random"));
		gameObject.GetComponent<ToggleEffects>().onAction.AddListener(delegate
		{
			OnMapSelect(0);
		});
		for (int num = 0; num < skirmishManager.skirmishMaps.Count; num++)
		{
			gameObject = mapList.AddToList(component, component2);
			component3 = gameObject.GetComponent<Image>();
			component3.set_sprite(Resources.Load<Sprite>("maps/img_map_" + skirmishManager.skirmishMaps[num].mapData.Name));
			int index = num + 1;
			gameObject.GetComponent<ToggleEffects>().onAction.AddListener(delegate
			{
				OnMapSelect(index);
			});
		}
		inviteButton.gameObject.SetActive(value: true);
		component2 = (Selectable)(object)swapButton.gameObject.GetComponent<Toggle>();
		navigation = ((Selectable)objective2.selection).get_navigation();
		((Navigation)(ref navigation)).set_selectOnDown(component2);
		((Selectable)objective2.selection).set_navigation(navigation);
		roaming.onValueChanged = OnRoamingChange;
		AIWarbandType.onValueChanged = OnAiWarbandChanged;
		backtracking.onValueChanged = OnBacktrackingChange;
		turnTimer.onValueChanged = OnTurnTimerChange;
		deployTimer.onValueChanged = OnDeployTimerChanged;
		routThreshold.onValueChanged = OnRoutThresholdChanged;
		timeOfDay.onValueChanged = OnTimeofDayChange;
		gameplay.onValueChanged = OnGameplayChange;
		deployment.onValueChanged = OnDeploymentChange;
		autodeploy.onValueChanged = OnAutodeployChange;
		position1.onValueChanged = OnPositionChange;
		position2.onValueChanged = OnPositionChange;
		gameType.onValueChanged = OnGameTypeChange;
		objective1.onValueChanged = OnObjectiveChange;
		objective2.onValueChanged = OnObjectiveChange;
		RefreshPlayerNames();
		currentMapIndex = -1;
		OnMapSelect(0);
		privacy.GetComponentInParent<ToggleEffects>().SetSelected(force: true);
	}

	private void SetClientLobby(Lobby data)
	{
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0904: Unknown result type (might be due to invalid IL or missing references)
		//IL_0919: Unknown result type (might be due to invalid IL or missing references)
		Mission currentMission = PandoraSingleton<MissionStartData>.Instance.CurrentMission;
		lobbyName.set_text(data.name);
		privacy.transform.parent.gameObject.SetActive(value: false);
		AI.SetButtonsVisible(show: false);
		AI.selections.Clear();
		AI.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_title_player"));
		AI.SetCurrentSel(0);
		Selectable component = (Selectable)(object)objective2.transform.parent.gameObject.GetComponent<Toggle>();
		Navigation navigation = ((Selectable)AI.selection).get_navigation();
		((Navigation)(ref navigation)).set_selectOnUp(component);
		((Selectable)AI.selection).set_navigation(navigation);
		AIWarbandType.selections.Clear();
		AIWarbandType.SetButtonsVisible(show: false);
		AIWarbandType.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById("com_wb_type_" + ((WarbandId)data.warbandId).ToLowerString()));
		AIWarbandType.SetCurrentSel(0);
		roaming.selections.Clear();
		roaming.SetButtonsVisible(show: false);
		roaming.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById((!currentMission.missionSave.randomRoaming) ? ("unit_name_" + (UnitId)currentMission.missionSave.roamingUnitId) : "lobby_title_random"));
		roaming.SetCurrentSel(0);
		backtracking.selections.Clear();
		backtracking.SetButtonsVisible(show: false);
		backtracking.selections.Add((currentMission.missionSave.beaconLimit != 0) ? currentMission.missionSave.beaconLimit.ToString() : PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_no_backtracking"));
		backtracking.SetCurrentSel(0);
		turnTimer.selections.Clear();
		turnTimer.SetButtonsVisible(show: false);
		turnTimer.selections.Add((currentMission.missionSave.turnTimer != 0) ? currentMission.missionSave.turnTimer.ToString() : PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_no_timer"));
		turnTimer.SetCurrentSel(0);
		deployTimer.selections.Clear();
		deployTimer.SetButtonsVisible(show: false);
		deployTimer.selections.Add((currentMission.missionSave.deployTimer != 0) ? currentMission.missionSave.deployTimer.ToString() : PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_no_timer"));
		deployTimer.SetCurrentSel(0);
		routThreshold.selections.Clear();
		routThreshold.SetButtonsVisible(show: false);
		routThreshold.selections.Add(currentMission.missionSave.routThreshold * 100f + "%");
		routThreshold.SetCurrentSel(0);
		MissionMapId mapId = currentMission.GetMapId();
		mapName.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById((mapId == MissionMapId.NONE) ? "lobby_title_random" : (mapId.ToLowerString() + "_name")));
		component = (Selectable)(object)turnTimer.transform.parent.gameObject.GetComponent<Toggle>();
		Selectable component2 = (Selectable)(object)gameplay.transform.parent.gameObject.GetComponent<Toggle>();
		GameObject gameObject;
		if (mapList.items.Count != 1)
		{
			mapList.ClearList();
			gameObject = mapList.AddToList(component, component2);
		}
		else
		{
			gameObject = mapList.items[0];
		}
		Image component3 = gameObject.GetComponent<Image>();
		component3.set_sprite(Resources.Load<Sprite>("maps/img_map_" + ((mapId == MissionMapId.NONE) ? "random" : mapId.ToLowerString())));
		RealignOnNextFrame(0);
		timeOfDay.SetButtonsVisible(show: false);
		timeOfDay.selections.Clear();
		string empty = string.Empty;
		string skyName = currentMission.GetSkyName();
		if (skyName.Contains("day"))
		{
			empty = PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_title_sky_day");
			empty = empty + " " + skyName[skyName.Length - 1];
		}
		else if (skyName.Contains("night"))
		{
			empty = PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_title_sky_night");
			empty = empty + " " + skyName[skyName.Length - 1];
		}
		else
		{
			empty = PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_title_random");
		}
		timeOfDay.selections.Add(empty);
		timeOfDay.SetCurrentSel(0);
		gameplay.SetButtonsVisible(show: false);
		gameplay.selections.Clear();
		gameplay.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById((!currentMission.missionSave.randomGameplay) ? ("lobby_title_" + currentMission.GetMapGameplayId()) : "lobby_title_random"));
		gameplay.SetCurrentSel(0);
		deployment.SetButtonsVisible(show: false);
		deployment.selections.Clear();
		DeploymentScenarioId deploymentScenarioId = currentMission.GetDeploymentScenarioId();
		deployment.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById((deploymentScenarioId == DeploymentScenarioId.NONE) ? "lobby_title_random" : ("lobby_title_" + deploymentScenarioId.ToLowerString())));
		deployment.SetCurrentSel(0);
		autodeploy.SetButtonsVisible(show: false);
		autodeploy.selections.Clear();
		autodeploy.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById((!currentMission.missionSave.autoDeploy) ? "menu_off" : "menu_on"));
		autodeploy.SetCurrentSel(0);
		position1.SetButtonsVisible(show: false);
		position1.selections.Clear();
		DeploymentId deploymentId = currentMission.GetDeploymentId(0);
		position1.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById((deploymentId == DeploymentId.NONE) ? "lobby_title_random" : ("lobby_title_" + deploymentId.ToLowerString())));
		position1.SetCurrentSel(0);
		position2.SetButtonsVisible(show: false);
		position2.selections.Clear();
		deploymentId = currentMission.GetDeploymentId(1);
		position2.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById((deploymentId == DeploymentId.NONE) ? "lobby_title_random" : ("lobby_title_" + deploymentId.ToLowerString())));
		position2.SetCurrentSel(0);
		gameType.SetButtonsVisible(show: false);
		gameType.selections.Clear();
		gameType.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById((!currentMission.HasObjectives()) ? "lobby_title_battleground_only" : "lobby_title_extra_objectives"));
		gameType.SetCurrentSel(0);
		objective1.SetButtonsVisible(show: false);
		objective1.selections.Clear();
		objective1.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById((!PandoraSingleton<MissionStartData>.Instance.CurrentMission.HasObjectives()) ? "lobby_title_no_extra_objectives" : ((!currentMission.IsObjectiveRandom(0)) ? ("lobby_title_" + currentMission.GetObjectiveTypeId(0).ToString()) : "lobby_title_random")));
		objective1.SetCurrentSel(0);
		objective2.SetButtonsVisible(show: false);
		objective2.selections.Clear();
		objective2.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById((!PandoraSingleton<MissionStartData>.Instance.CurrentMission.HasObjectives()) ? "lobby_title_no_extra_objectives" : ((!currentMission.IsObjectiveRandom(1)) ? ("lobby_title_" + currentMission.GetObjectiveTypeId(1).ToString()) : "lobby_title_random")));
		objective2.SetCurrentSel(0);
		inviteButton.gameObject.SetActive(value: false);
		component2 = (Selectable)(object)AI.transform.parent.gameObject.GetComponent<Toggle>();
		navigation = ((Selectable)objective2.selection).get_navigation();
		((Navigation)(ref navigation)).set_selectOnDown(component2);
		((Selectable)objective2.selection).set_navigation(navigation);
		SetInviteButtonVisible(visible: false);
		launchButton.SetDisabled(disabled: false);
		launchButton.SetSelected(force: true);
		ForceMapReselect();
	}

	public void RefreshPlayerNames()
	{
		if (PandoraSingleton<MissionStartData>.Instance.FightingWarbands.Count > 0)
		{
			player1.set_text(PandoraSingleton<MissionStartData>.Instance.FightingWarbands[0].PlayerName);
			player1Obj.set_text(PandoraSingleton<MissionStartData>.Instance.FightingWarbands[0].PlayerName);
		}
		else
		{
			player1.set_text(PandoraSingleton<Hephaestus>.Instance.GetUserName());
			player1Obj.set_text(PandoraSingleton<Hephaestus>.Instance.GetUserName());
		}
		if (PandoraSingleton<MissionStartData>.Instance.FightingWarbands.Count > 1)
		{
			player2.set_text(PandoraSingleton<MissionStartData>.Instance.FightingWarbands[1].PlayerName);
			player2Obj.set_text(PandoraSingleton<MissionStartData>.Instance.FightingWarbands[1].PlayerName);
		}
		else
		{
			player2.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_title_player") + " 2");
			player2Obj.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_title_player") + " 2");
		}
	}

	public void LockAI(bool lockButtons)
	{
		if (PandoraSingleton<Hephaestus>.Instance.Lobby.privacy != Hephaestus.LobbyPrivacy.OFFLINE)
		{
			AI.SetButtonsVisible(!lockButtons);
		}
	}

	private void OnSetAI(int id, int index)
	{
		if (index != 0 || PandoraSingleton<Hephaestus>.Instance.Lobby.privacy == Hephaestus.LobbyPrivacy.OFFLINE)
		{
			int ratingId = index + ((PandoraSingleton<Hephaestus>.Instance.Lobby.privacy == Hephaestus.LobbyPrivacy.OFFLINE) ? 1 : 0);
			PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.ratingId = ratingId;
			if (PandoraSingleton<MissionStartData>.Instance.FightingWarbands.Count == 1)
			{
				PandoraSingleton<SkirmishManager>.Instance.AddAIOpponent();
			}
			else
			{
				PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.SKIRMISH_OPPONENT_READY);
			}
			SetInviteButtonVisible(visible: false);
			AIWarbandType.SetButtonsVisible(show: true);
		}
		else
		{
			PandoraSingleton<SkirmishManager>.Instance.RemoveOpponent();
			SetInviteButtonVisible(visible: true);
			AIWarbandType.SetButtonsVisible(show: false);
			((Component)(object)AIWarbandType.selection).GetComponent<Text>().set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("unit_name_none"));
		}
	}

	public void SetWarbandType(WarbandId wbId)
	{
		List<WarbandData> list = PandoraSingleton<DataFactory>.Instance.InitData<WarbandData>("basic", "1");
		int num = 0;
		while (true)
		{
			if (num < list.Count)
			{
				if (list[num].Id == wbId)
				{
					break;
				}
				num++;
				continue;
			}
			return;
		}
		AIWarbandType.SetCurrentSel(num);
	}

	private void OnAiWarbandChanged(int id, int index)
	{
		List<WarbandData> list = PandoraSingleton<DataFactory>.Instance.InitData<WarbandData>("basic", "1");
		PandoraSingleton<SkirmishManager>.Instance.RemoveOpponent();
		PandoraSingleton<SkirmishManager>.Instance.AddAIOpponent(list[index]);
	}

	public void SetInviteButtonVisible(bool visible)
	{
		if (!visible && EventSystem.get_current().get_currentSelectedGameObject() == inviteButton.gameObject)
		{
			swapButton.SetSelected(force: true);
		}
		inviteButton.gameObject.SetActive(visible && PandoraSingleton<Hephaestus>.Instance.Lobby.privacy != Hephaestus.LobbyPrivacy.OFFLINE);
	}

	public void SetLaunchButtonVisible(bool visible, bool disabled)
	{
		if (!visible && EventSystem.get_current().get_currentSelectedGameObject() == launchButton.gameObject)
		{
			swapButton.SetSelected(force: true);
		}
		if (disabled)
		{
			launchButton.SetDisabled(!visible);
		}
		else
		{
			launchButton.gameObject.SetActive(visible);
		}
	}

	private void OnTurnTimerChange(int id, int index)
	{
		PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.turnTimer = index * 15;
		PandoraSingleton<MissionStartData>.Instance.SendMission(clearWarbands: false);
	}

	private void OnDeployTimerChanged(int id, int index)
	{
		PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.deployTimer = index * 15;
		PandoraSingleton<MissionStartData>.Instance.SendMission(clearWarbands: false);
	}

	private void OnRoutThresholdChanged(int id, int index)
	{
		PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.routThreshold = (float)(index * 5) / 100f;
		PandoraSingleton<MissionStartData>.Instance.SendMission(clearWarbands: false);
	}

	private void OnRoamingChange(int id, int index)
	{
		if (index == 0)
		{
			PandoraSingleton<MissionStartData>.Instance.CurrentMission.SetRandomRoaming(PandoraSingleton<GameManager>.Instance.LocalTyche);
		}
		else
		{
			PandoraSingleton<MissionStartData>.Instance.CurrentMission.SetRoamingUnit(index - 1);
		}
		PandoraSingleton<MissionStartData>.Instance.SendMission(clearWarbands: false);
	}

	private void OnBacktrackingChange(int id, int index)
	{
		PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.beaconLimit = index;
		PandoraSingleton<MissionStartData>.Instance.SendMission(clearWarbands: false);
	}

	private void ToggleCurrentMap()
	{
		if (PandoraSingleton<Hermes>.Instance.IsHost())
		{
			mapList.items[currentMapIndex].SetSelected();
		}
	}

	public void ForceMapReselect()
	{
		if (base.gameObject.activeSelf)
		{
			StartCoroutine(RealignOnNextFrame(currentMapIndex));
		}
	}

	private IEnumerator RealignOnNextFrame(int index)
	{
		yield return null;
		mapList.RealignList(isOn: true, index, force: true);
	}

	private void OnMapSelect(int index)
	{
		if (currentMapIndex != index)
		{
			mapList.items[index].SetSelected();
			mapList.items[index].GetComponent<ToggleEffects>().SetOn();
			StartCoroutine(RealignOnNextFrame(index));
			currentMapIndex = index;
			PandoraDebug.LogDebug("Map Selected = " + currentMapIndex);
			PandoraSingleton<Hephaestus>.Instance.SetLobbyData("map", currentMapIndex.ToLowerString());
			if (currentMapIndex == 0)
			{
				skirmishMap = null;
				mapName.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_title_random"));
				PandoraSingleton<MissionStartData>.Instance.RegenerateMission();
				PandoraDebug.LogInfo("Generating random map", "SKIRMISH");
			}
			else
			{
				skirmishMap = skirmishManager.skirmishMaps[currentMapIndex - 1];
				mapName.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById(skirmishMap.mapData.Name + "_name"));
				PandoraDebug.LogInfo("Generating map : " + skirmishMap.mapData.Id, "SKIRMISH");
				PandoraSingleton<MissionStartData>.Instance.RegenerateMission(skirmishMap.mapData.Id);
			}
			SetTimeOfDayChoices(index);
			SetGameplayChoices(index);
			SetDeploymentChoices(index);
		}
	}

	private void SetTimeOfDayChoices(int mapIndex)
	{
		timeOfDay.selections.Clear();
		timeOfDay.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_title_random"));
		if (mapIndex != 0)
		{
			for (int i = 0; i < skirmishMap.layouts.Count; i++)
			{
				string empty = string.Empty;
				string name = skirmishMap.layouts[i].Name;
				empty = ((!name.Contains("day")) ? PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_title_sky_night") : PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_title_sky_day"));
				char c = name[name.Length - 1];
				if (!char.IsDigit(c))
				{
					c = '1';
				}
				empty = empty + " " + c;
				timeOfDay.selections.Add(empty);
			}
		}
		timeOfDay.SetButtonsVisible(mapIndex != 0);
		timeOfDay.SetCurrentSel(0);
	}

	private void OnTimeofDayChange(int id, int index)
	{
		PandoraDebug.LogDebug("timeofDay cursel = " + timeOfDay.CurSel);
		PandoraDebug.LogDebug("OnTimeofDayChange index = " + index);
		MissionMapLayoutId missionMapLayoutId = MissionMapLayoutId.NONE;
		bool flag = false;
		if (index == 0)
		{
			flag = true;
			missionMapLayoutId = skirmishMap.layouts[PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0, skirmishMap.layouts.Count)].Id;
		}
		else
		{
			missionMapLayoutId = skirmishMap.layouts[index - 1].Id;
		}
		PandoraDebug.LogInfo("Changing Map Layout :" + missionMapLayoutId + " random : " + flag, "SKIRMISH");
		PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.randomLayout = flag;
		PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.mapLayoutId = (int)missionMapLayoutId;
		PandoraSingleton<MissionStartData>.Instance.SendMission(clearWarbands: false);
	}

	private void SetGameplayChoices(int mapIndex)
	{
		gameplay.selections.Clear();
		if (mapIndex != 0 && skirmishMap.gameplays.Count > 0)
		{
			gameplay.SetButtonsVisible(show: true);
			gameplay.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_title_random"));
			for (int i = 0; i < skirmishMap.gameplays.Count; i++)
			{
				gameplay.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_title_" + skirmishMap.gameplays[i].Name));
			}
		}
		else
		{
			gameplay.SetButtonsVisible(show: false);
			gameplay.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_title_no_gameplay"));
		}
		gameplay.SetCurrentSel(0);
	}

	private void OnGameplayChange(int id, int index)
	{
		PandoraDebug.LogDebug("Gameplay cursel = " + gameplay.CurSel);
		PandoraDebug.LogDebug("OnGameplayChanged index = " + index);
		MissionMapGameplayId missionMapGameplayId = MissionMapGameplayId.NONE;
		bool flag = index == 0;
		missionMapGameplayId = ((!flag) ? skirmishMap.gameplays[index - 1].Id : skirmishMap.gameplays[PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0, skirmishMap.gameplays.Count)].Id);
		PandoraDebug.LogInfo("Changing Map Gameplay :" + missionMapGameplayId + " random : " + flag, "SKIRMISH");
		PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.randomGameplay = flag;
		PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.mapGameplayId = (int)missionMapGameplayId;
		PandoraSingleton<MissionStartData>.Instance.SendMission(clearWarbands: false);
	}

	private void SetDeploymentChoices(int mapIndex)
	{
		deployment.selections.Clear();
		deployment.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_title_random"));
		if (mapIndex != 0)
		{
			for (int i = 0; i < skirmishMap.deployments.Count; i++)
			{
				string key = "lobby_title_" + skirmishMap.deployments[i].scenarioData.Name;
				deployment.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById(key));
			}
		}
		deployment.SetButtonsVisible(mapIndex != 0);
		deployment.SetCurrentSel(0);
		SetPositionsChoices(0);
	}

	private void OnDeploymentChange(int id, int index)
	{
		DeploymentScenarioId scenarioId = (index != 0) ? skirmishMap.deployments[index - 1].scenarioData.Id : DeploymentScenarioId.NONE;
		PandoraSingleton<MissionStartData>.Instance.RegenerateMission(skirmishMap.mapData.Id, scenarioId, keepLayout: true);
		SetPositionsChoices(index);
	}

	private void OnAutodeployChange(int id, int index)
	{
		PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.autoDeploy = (index == 1);
		PandoraSingleton<MissionStartData>.Instance.SendMission(clearWarbands: false);
	}

	private void SetPositionsChoices(int deployIndex)
	{
		position1.selections.Clear();
		position1.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_title_random"));
		position2.selections.Clear();
		position2.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_title_random"));
		if (deployIndex != 0)
		{
			for (int i = 0; i < skirmishMap.deployments[deployIndex - 1].slots.Count; i++)
			{
				position1.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_title_" + skirmishMap.deployments[deployIndex - 1].slots[i].Name));
				position2.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_title_" + skirmishMap.deployments[deployIndex - 1].slots[i].Name));
			}
		}
		position1.SetButtonsVisible(deployIndex != 0);
		position1.SetCurrentSel(0);
		position2.SetButtonsVisible(deployIndex != 0);
		position2.SetCurrentSel(0);
		SetGameTypeChoices();
	}

	private void OnPositionChange(int id, int index)
	{
		if (index == 0)
		{
			PandoraSingleton<MissionStartData>.Instance.CurrentMission.SetRandomDeploySlots(PandoraSingleton<GameManager>.Instance.LocalTyche);
			position1.SetCurrentSel(0);
			position2.SetCurrentSel(0);
		}
		else
		{
			int num = index - 1;
			PandoraSingleton<MissionStartData>.Instance.CurrentMission.SetDeploySlots(id, num);
			if (id == 0)
			{
				position2.SetCurrentSel((num + 1) % 2 + 1);
			}
			else
			{
				position1.SetCurrentSel((num + 1) % 2 + 1);
			}
		}
		if (PandoraSingleton<MissionStartData>.Instance.CurrentMission.HasObjectives())
		{
			PandoraSingleton<MissionStartData>.Instance.CurrentMission.RandomizeObjectives(PandoraSingleton<GameManager>.Instance.LocalTyche);
		}
		PandoraSingleton<MissionStartData>.Instance.SendMission(clearWarbands: false);
		SetObjectiveChoices();
	}

	private void SetGameTypeChoices()
	{
		gameType.selections.Clear();
		gameType.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_title_battleground_only"));
		gameType.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_title_extra_objectives"));
		gameType.SetCurrentSel(PandoraSingleton<MissionStartData>.Instance.CurrentMission.HasObjectives() ? 1 : 0);
		gameType.SetButtonsVisible(show: true);
		SetObjectiveChoices();
	}

	private void OnGameTypeChange(int id, int index)
	{
		if (index == 0)
		{
			PandoraSingleton<MissionStartData>.Instance.CurrentMission.ClearObjectives();
		}
		else
		{
			PandoraSingleton<MissionStartData>.Instance.CurrentMission.RandomizeObjectives(PandoraSingleton<GameManager>.Instance.LocalTyche);
		}
		PandoraSingleton<MissionStartData>.Instance.SendMission(clearWarbands: false);
		SetObjectiveChoices();
	}

	private void SetObjectiveChoices()
	{
		bool flag = PandoraSingleton<MissionStartData>.Instance.CurrentMission.HasObjectives();
		objective1.selections.Clear();
		objective2.selections.Clear();
		if (!flag)
		{
			objective1.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_title_no_extra_objectives"));
			objective2.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_title_no_extra_objectives"));
		}
		else
		{
			SetPlayerObjectiveChoices(objective1, 0);
			SetPlayerObjectiveChoices(objective2, 1);
		}
		objective1.SetButtonsVisible(flag && deployment.CurSel != 0);
		objective1.SetCurrentSel(0);
		objective2.SetButtonsVisible(flag && deployment.CurSel != 0);
		objective2.SetCurrentSel(0);
	}

	private void SetPlayerObjectiveChoices(SelectorGroup objectiveGroup, int idx)
	{
		objectiveGroup.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_title_random"));
		List<PrimaryObjectiveTypeId> availableObjectiveTypes = PandoraSingleton<MissionStartData>.Instance.CurrentMission.GetAvailableObjectiveTypes(idx);
		for (int i = 0; i < availableObjectiveTypes.Count; i++)
		{
			objectiveGroup.selections.Add(PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_title_" + availableObjectiveTypes[i].ToLowerString()));
		}
	}

	private void OnObjectiveChange(int id, int index)
	{
		if (index == 0)
		{
			PandoraSingleton<MissionStartData>.Instance.CurrentMission.SetRandomObjective(PandoraSingleton<GameManager>.Instance.LocalTyche, id);
		}
		else
		{
			PandoraSingleton<MissionStartData>.Instance.CurrentMission.SetObjective(id, index - 1);
		}
		PandoraSingleton<MissionStartData>.Instance.SendMission(clearWarbands: false);
	}

	public void OpenInviteInterface()
	{
		PandoraSingleton<Hephaestus>.Instance.InviteFriends();
	}

	public void LinkDescriptions(UnityAction<string, string> onSelect, UnityAction<string, string> onSelectLocalized)
	{
		if (privacy.transform.parent.gameObject.activeSelf)
		{
			SetDesc(privacy, onSelect, GetPrivacy);
		}
		SetDesc(AI, onSelect, GetPlayerTypeLoc);
		SetDesc(AIWarbandType, onSelect, GetAiTypeLoc);
		SetDesc(roaming, onSelect, GetRoamingDesc);
		SetDesc(backtracking, onSelect, GetBacktrackingLoc);
		SetDesc(turnTimer, onSelect, GetTurnTimerLoc);
		SetDesc(deployTimer, onSelect, GetDeployTimerLoc);
		SetDesc(routThreshold, onSelect, GetRoutThresholdLoc);
		foreach (GameObject item in mapList.items)
		{
			item.GetComponent<ToggleEffects>().onSelect.AddListener(delegate
			{
				onSelect(GetMapLoc() + "_name", GetMapLoc() + "_desc");
			});
		}
		SetDesc(timeOfDay, onSelect, GetMapLayoutLoc);
		SetDesc(gameplay, onSelect, GetMapGameplayLoc);
		SetDesc(deployment, onSelect, GetDeploymentScenarLoc);
		SetDesc(autodeploy, onSelect, GetAutodeployLoc);
		SetDesc(position1, onSelect, GetDeploySlot1Loc);
		SetDesc(position2, onSelect, GetDeploySlot2Loc);
		SetDesc(gameType, onSelect, GetGameTypeLoc);
		SetDesc(objective1, onSelect, GetObj1Loc);
		SetDesc(objective2, onSelect, GetObj2Loc);
	}

	private string GetPrivacy()
	{
		return lobbyPrivacy.ToLowerString();
	}

	private string GetPlayerTypeLoc()
	{
		return (AI.CurSel != 0) ? "ai" : "player";
	}

	private string GetAiTypeLoc()
	{
		return "opponent_warband";
	}

	private string GetRoamingDesc()
	{
		return "roaming";
	}

	private string GetBacktrackingLoc()
	{
		return "backtracking";
	}

	private string GetTurnTimerLoc()
	{
		return "timer";
	}

	private string GetDeployTimerLoc()
	{
		return "deploy_timer";
	}

	private string GetRoutThresholdLoc()
	{
		return "rout_threshold";
	}

	private string GetMapLoc()
	{
		MissionMapId mapId = PandoraSingleton<MissionStartData>.Instance.CurrentMission.GetMapId();
		return (mapId != 0) ? mapId.ToLowerString() : "lobby_title_random";
	}

	private string GetMapLayoutLoc()
	{
		if (PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.randomLayout)
		{
			return "random";
		}
		MissionMapLayoutData missionMapLayoutData = PandoraSingleton<DataFactory>.Instance.InitData<MissionMapLayoutData>((int)PandoraSingleton<MissionStartData>.Instance.CurrentMission.GetMapLayoutId());
		if (missionMapLayoutData.Name.Contains("day"))
		{
			return "sky_day";
		}
		return "sky_night";
	}

	private string GetMapGameplayLoc()
	{
		MissionMapGameplayData missionMapGameplayData = PandoraSingleton<DataFactory>.Instance.InitData<MissionMapGameplayData>((int)PandoraSingleton<MissionStartData>.Instance.CurrentMission.GetMapGameplayId());
		return (!PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.randomGameplay) ? missionMapGameplayData.Name : "random";
	}

	private string GetDeploymentScenarLoc()
	{
		return (!PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.randomDeployment) ? PandoraSingleton<MissionStartData>.Instance.CurrentMission.GetDeploymentScenarioId().ToLowerString() : "random";
	}

	private string GetAutodeployLoc()
	{
		return (!PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.autoDeploy) ? "manualdeploy" : "autodeploy";
	}

	private string GetDeploySlot1Loc()
	{
		return (!PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.randomSlots) ? PandoraSingleton<MissionStartData>.Instance.CurrentMission.GetDeploymentId(0).ToLowerString() : "random";
	}

	private string GetDeploySlot2Loc()
	{
		return (!PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.randomSlots) ? PandoraSingleton<MissionStartData>.Instance.CurrentMission.GetDeploymentId(1).ToLowerString() : "random";
	}

	private string GetGameTypeLoc()
	{
		return (!PandoraSingleton<MissionStartData>.Instance.CurrentMission.HasObjectives()) ? "battleground_only" : "extra_objectives";
	}

	private string GetObj1Loc()
	{
		if (!PandoraSingleton<MissionStartData>.Instance.CurrentMission.HasObjectives())
		{
			return "no_extra_objectives";
		}
		return (!PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.randomObjectives[0]) ? ((PrimaryObjectiveTypeId)PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.objectiveTypeIds[0]).ToLowerString() : "random";
	}

	private string GetObj2Loc()
	{
		if (!PandoraSingleton<MissionStartData>.Instance.CurrentMission.HasObjectives())
		{
			return "no_extra_objectives";
		}
		return (!PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.randomObjectives[1]) ? ((PrimaryObjectiveTypeId)PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.objectiveTypeIds[1]).ToLowerString() : "random";
	}

	private void SetDesc(SelectorGroup group, UnityAction<string, string> onSelect, GetLoc loc, string title = "lobby_title_", string desc = "lobby_desc_")
	{
		UnityAction action = delegate
		{
			onSelect(title + loc(), desc + loc());
		};
		group.GetComponentInParent<ToggleEffects>().onSelect.AddListener(action);
		group.onValueChanged = (SelectorGroup.OnValueChanged)Delegate.Combine(group.onValueChanged, (SelectorGroup.OnValueChanged)delegate
		{
			action();
		});
	}
}
