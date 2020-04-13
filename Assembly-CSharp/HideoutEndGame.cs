using System.Collections.Generic;
using UnityEngine;

public class HideoutEndGame : ICheapState
{
    private enum EndState
    {
        NONE,
        SELECT_UNIT,
        OOA,
        INJURY,
        XP,
        DEAD,
        NEXT_UNIT,
        WAIT
    }

    private HideoutCamAnchor camAnchor;

    private List<UnitMenuController> warbandUnits;

    private ActionButtonModule actionButtonModule;

    private EndState state;

    private EndState prevState;

    private UIModule currentMod;

    private int currentUnit = -1;

    private List<KeyValuePair<int, int>> unitList = new List<KeyValuePair<int, int>>();

    private bool canAdvance;

    public HideoutEndGame(HideoutManager mng, HideoutCamAnchor anchor)
    {
        camAnchor = anchor;
    }

    void ICheapState.Update()
    {
        MissionEndDataSave endMission = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave().endMission;
        if (endMission.isSkirmish)
        {
            PandoraSingleton<HideoutManager>.Instance.StateMachine.ChangeState(0);
            return;
        }
        UnitMenuController unitMenuController = null;
        switch (state)
        {
            case EndState.SELECT_UNIT:
                {
                    PandoraDebug.LogDebug("SELECT_UNIT");
                    prevState = EndState.SELECT_UNIT;
                    unitMenuController = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.unitCtrlrs[unitList[currentUnit].Value];
                    unitMenuController.Hide(hide: false);
                    if (endMission.units[unitList[currentUnit].Key].status == UnitStateId.OUT_OF_ACTION)
                    {
                        unitMenuController.animator.SetInteger(AnimatorIds.unit_state, 2);
                        unitMenuController.animator.Play(AnimatorIds.kneeling_stunned, -1, (float)PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0.0, 1.0));
                    }
                    PandoraSingleton<HideoutManager>.Instance.CamManager.dummyCam.transform.position = unitMenuController.transform.position + unitMenuController.transform.forward * 4f;
                    PandoraSingleton<HideoutManager>.Instance.CamManager.dummyCam.transform.Translate(0f, 1.5f, 0f);
                    PandoraSingleton<HideoutManager>.Instance.CamManager.dummyCam.transform.LookAt(unitMenuController.transform.position + new Vector3(0f, 1f, 0f));
                    PandoraSingleton<HideoutTabManager>.Instance.ActivateLeftTabModules(true, ModuleId.UNIT_SHEET, ModuleId.UNIT_STATS);
                    int xpGained = GetXpGained(endMission, unitMenuController);
                    UnitSheetModule moduleLeft = PandoraSingleton<HideoutTabManager>.Instance.GetModuleLeft<UnitSheetModule>(ModuleId.UNIT_SHEET);
                    moduleLeft.SetInteractable(interactable: false);
                    moduleLeft.RefreshAttributes(unitMenuController.unit);
                    moduleLeft.RemoveDisplayedXp(xpGained);
                    UnitStatsModule moduleLeft2 = PandoraSingleton<HideoutTabManager>.Instance.GetModuleLeft<UnitStatsModule>(ModuleId.UNIT_STATS);
                    moduleLeft2.RefreshStats(unitMenuController.unit);
                    moduleLeft2.SetInteractable(interactable: false);
                    state = EndState.OOA;
                    break;
                }
            case EndState.OOA:
                PandoraDebug.LogDebug("OOA");
                prevState = EndState.OOA;
                state = EndState.WAIT;
                unitMenuController = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.unitCtrlrs[unitList[currentUnit].Value];
                if (endMission.VictoryType == VictoryTypeId.LOSS && endMission.units[unitList[currentUnit].Key].status == UnitStateId.OUT_OF_ACTION)
                {
                    PandoraSingleton<HideoutTabManager>.Instance.ActivateLeftTabModule(true, ModuleId.SLIDE_OOA);
                    currentMod = PandoraSingleton<HideoutTabManager>.Instance.GetModuleLeft<EndGameOoaModule>(ModuleId.SLIDE_OOA);
                    ((EndGameOoaModule)currentMod).Set(endMission.units[unitList[currentUnit].Key], unitMenuController.unit);
                    ShowContinueButton(currentUnit, unitList.Count);
                }
                else
                {
                    currentMod = null;
                    state = EndState.INJURY;
                }
                break;
            case EndState.INJURY:
                PandoraDebug.LogDebug("INJURY");
                prevState = EndState.INJURY;
                state = EndState.WAIT;
                unitMenuController = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.unitCtrlrs[unitList[currentUnit].Value];
                if (endMission.units[unitList[currentUnit].Key].injuries.Count > 0)
                {
                    PandoraSingleton<HideoutTabManager>.Instance.ActivateLeftTabModule(false, ModuleId.SLIDE_OOA);
                    PandoraSingleton<HideoutTabManager>.Instance.ActivateLeftTabModule(true, ModuleId.SLIDE_INJURY);
                    currentMod = PandoraSingleton<HideoutTabManager>.Instance.GetModuleLeft<EndGameInjuryModule>(ModuleId.SLIDE_INJURY);
                    ((EndGameInjuryModule)currentMod).Setup(endMission.units[unitList[currentUnit].Key]);
                    if (endMission.units[unitList[currentUnit].Key].dead)
                    {
                        unitMenuController.PlayDefState(AttackResultId.HIT, 0, UnitStateId.OUT_OF_ACTION);
                        prevState = EndState.XP;
                        ShowContinueButton(currentUnit, unitList.Count);
                        break;
                    }
                    unitMenuController.LaunchAction(UnitActionId.NONE, success: true, UnitStateId.NONE);
                    if (endMission.units[unitList[currentUnit].Key].isMaxRank)
                    {
                        ShowNextUnitButton(currentUnit, unitList.Count);
                    }
                    else
                    {
                        ShowContinueButton(currentUnit, unitList.Count);
                    }
                }
                else
                {
                    currentMod = null;
                    state = EndState.XP;
                }
                break;
            case EndState.XP:
                PandoraDebug.LogDebug("XP");
                prevState = EndState.DEAD;
                state = EndState.WAIT;
                unitMenuController = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.unitCtrlrs[unitList[currentUnit].Value];
                if (!endMission.units[unitList[currentUnit].Key].dead && !endMission.units[unitList[currentUnit].Key].isMaxRank)
                {
                    PandoraSingleton<HideoutTabManager>.Instance.ActivateLeftTabModule(false, ModuleId.SLIDE_INJURY);
                    PandoraSingleton<HideoutTabManager>.Instance.ActivateLeftTabModule(true, ModuleId.SLIDE_XP);
                    currentMod = PandoraSingleton<HideoutTabManager>.Instance.GetModuleLeft<EndGameXPModule>(ModuleId.SLIDE_XP);
                    ((EndGameXPModule)currentMod).Set(endMission.units[unitList[currentUnit].Key], unitMenuController.unit);
                    int xpGained = GetXpGained(endMission, unitMenuController);
                    UnitSheetModule moduleLeft = PandoraSingleton<HideoutTabManager>.Instance.GetModuleLeft<UnitSheetModule>(ModuleId.UNIT_SHEET);
                    moduleLeft.AddXp(xpGained);
                    ShowNextUnitButton(currentUnit, unitList.Count);
                }
                else
                {
                    currentMod = null;
                    state = EndState.NEXT_UNIT;
                }
                break;
            case EndState.DEAD:
                {
                    PandoraDebug.LogDebug("DEAD");
                    prevState = EndState.DEAD;
                    state = EndState.WAIT;
                    unitMenuController = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.unitCtrlrs[unitList[currentUnit].Value];
                    PandoraSingleton<HideoutTabManager>.Instance.ActivateLeftTabModule(false, ModuleId.SLIDE_INJURY);
                    InjuryId injuryId = unitMenuController.unit.UnitSave.injuries[unitMenuController.unit.UnitSave.injuries.Count - 1];
                    PandoraSingleton<HideoutManager>.Instance.messagePopup.Show("injury_name_" + injuryId, "injury_retirement_desc_" + injuryId, delegate
                    {
                        canAdvance = true;
                        Advance();
                    });
                    PandoraSingleton<HideoutManager>.Instance.messagePopup.HideCancelButton();
                    PandoraSingleton<HideoutManager>.Instance.SaveChanges();
                    currentMod = null;
                    ShowNextUnitButton(currentUnit, unitList.Count);
                    PandoraSingleton<Pan>.Instance.Narrate("unit_death" + PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(3, 5));
                    break;
                }
            case EndState.NEXT_UNIT:
                PandoraDebug.LogDebug("NEXT_UNIT");
                if (currentUnit != -1)
                {
                    unitMenuController = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.unitCtrlrs[unitList[currentUnit].Value];
                    unitMenuController.Hide(hide: true);
                }
                else if (!PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave().endMission.isSkirmish)
                {
                    for (int i = 0; i < PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.unitCtrlrs.Count; i++)
                    {
                        PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.unitCtrlrs[i].Hide(hide: true);
                    }
                }
                currentUnit++;
                prevState = EndState.NEXT_UNIT;
                PandoraSingleton<HideoutTabManager>.Instance.ActivateLeftTabModule(false, ModuleId.SLIDE_OOA);
                PandoraSingleton<HideoutTabManager>.Instance.ActivateLeftTabModule(false, ModuleId.SLIDE_INJURY);
                PandoraSingleton<HideoutTabManager>.Instance.ActivateLeftTabModule(false, ModuleId.SLIDE_XP);
                ShowContinueButton(currentUnit, unitList.Count);
                if (currentUnit >= unitList.Count)
                {
                    PandoraSingleton<HideoutManager>.Instance.StateMachine.ChangeState(6);
                    state = EndState.WAIT;
                }
                else
                {
                    state = EndState.SELECT_UNIT;
                }
                break;
            case EndState.WAIT:
                canAdvance = true;
                break;
        }
    }

    public void Destroy()
    {
    }

    public void Enter(int iFrom)
    {
        PandoraSingleton<HideoutManager>.Instance.CamManager.ClearLookAtFocus();
        PandoraSingleton<HideoutManager>.Instance.CamManager.CancelTransition();
        PandoraSingleton<HideoutManager>.Instance.CamManager.dummyCam.transform.position = camAnchor.transform.position;
        PandoraSingleton<HideoutManager>.Instance.CamManager.dummyCam.transform.rotation = camAnchor.transform.rotation;
        PandoraSingleton<HideoutManager>.Instance.CamManager.SetDOFTarget(camAnchor.dofTarget, 0f);
        PandoraSingleton<HideoutTabManager>.Instance.ActivateRightTabModules(false, ModuleId.TREASURY);
        PandoraSingleton<HideoutTabManager>.Instance.ActivateCenterTabModules(ModuleId.TITLE, ModuleId.ACTION_BUTTON);
        actionButtonModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<ActionButtonModule>(ModuleId.ACTION_BUTTON);
        actionButtonModule.Refresh(string.Empty, string.Empty, "menu_continue", Advance);
        PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<TitleModule>(ModuleId.TITLE).Set("menu_mission_report");
        PandoraSingleton<HideoutTabManager>.Instance.GetModuleRight<TreasuryModule>(ModuleId.TREASURY).Refresh(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave());
        MenuNodeGroup warbandNodeGroup = PandoraSingleton<HideoutManager>.Instance.warbandNodeGroup;
        PandoraSingleton<HideoutManager>.Instance.warbandNodeWagon.SetContent(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.wagon);
        unitList = new List<KeyValuePair<int, int>>();
        MissionEndDataSave endMission = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave().endMission;
        CleanUpMissionWagon();
        for (int i = 0; i < PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.unitCtrlrs.Count; i++)
        {
            UnitMenuController unitMenuController = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.unitCtrlrs[i];
            if (!unitMenuController.unit.Active)
            {
                continue;
            }
            MissionEndUnitSave missionEndUnitSave = null;
            for (int j = 0; j < endMission.units.Count; j++)
            {
                if (endMission.units[j].isPlayed && unitMenuController.unit.UnitSave.warbandSlotIndex == endMission.units[j].unitSave.warbandSlotIndex)
                {
                    missionEndUnitSave = endMission.units[j];
                    PandoraDebug.LogDebug("Adding Unit from MissionEndData");
                    if (endMission.isSkirmish)
                    {
                        PandoraSingleton<HideoutManager>.Instance.Progressor.UpdateUnitStats(missionEndUnitSave, unitMenuController.unit);
                        break;
                    }
                    warbandNodeGroup.nodes[unitMenuController.unit.UnitSave.warbandSlotIndex].SetContent(unitMenuController, (!unitMenuController.unit.IsImpressive) ? null : warbandNodeGroup.nodes[unitMenuController.unit.UnitSave.warbandSlotIndex + 1]);
                    unitMenuController.SwitchWeapons(UnitSlotId.SET1_MAINHAND);
                    unitList.Add(new KeyValuePair<int, int>(j, i));
                    PandoraSingleton<HideoutManager>.Instance.Progressor.EndGameUnitProgress(missionEndUnitSave, unitMenuController.unit);
                    break;
                }
            }
            if (!endMission.isSkirmish)
            {
                if (missionEndUnitSave != null && !missionEndUnitSave.dead)
                {
                    unitMenuController.RefreshBodyParts();
                    unitMenuController.RefreshEquipments();
                }
                unitMenuController.Hide(hide: true);
                unitMenuController.SwitchWeapons(UnitSlotId.SET1_MAINHAND);
                if (missionEndUnitSave != null && missionEndUnitSave.status == UnitStateId.OUT_OF_ACTION)
                {
                    unitMenuController.animator.SetInteger(AnimatorIds.unit_state, 2);
                    unitMenuController.animator.Play(AnimatorIds.kneeling_stunned, -1, (float)PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0.0, 1.0));
                }
                else
                {
                    unitMenuController.animator.Play(AnimatorIds.idle, -1, (float)PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0.0, 1.0));
                }
            }
        }
        if (endMission.isSkirmish)
        {
            PandoraSingleton<HideoutManager>.Instance.Progressor.UpdateWarbandStats(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband);
        }
        else
        {
            if (endMission.wagonItems != null)
            {
                float @float = Constant.GetFloat(ConstantId.WYRDSTONE_WEIGHT);
                int priceSold = PandoraSingleton<DataFactory>.Instance.InitData<ItemData>(130).PriceSold;
                int priceSold2 = PandoraSingleton<DataFactory>.Instance.InitData<ItemData>(208).PriceSold;
                int priceSold3 = PandoraSingleton<DataFactory>.Instance.InitData<ItemData>(209).PriceSold;
                List<ItemSave> items = endMission.wagonItems.GetItems();
                for (int num = items.Count - 1; num >= 0; num--)
                {
                    if (items[num].id == 554 || items[num].id == 555)
                    {
                        for (int k = 0; k < items[num].amount; k++)
                        {
                            ItemSave unclaimedRecipe = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetUnclaimedRecipe((ItemId)items[num].id);
                            items.Add(unclaimedRecipe);
                            PandoraSingleton<HideoutManager>.Instance.WarbandChest.AddItem(unclaimedRecipe);
                        }
                        items.RemoveAt(num);
                    }
                    else
                    {
                        PandoraSingleton<HideoutManager>.Instance.WarbandChest.AddItem(items[num]);
                        if (items[num].id == 130)
                        {
                            PandoraSingleton<Hephaestus>.Instance.IncrementStat(Hephaestus.StatId.WYRDSTONE_GATHER, (int)((float)(items[num].amount * priceSold) * @float));
                        }
                        else if (items[num].id == 208)
                        {
                            PandoraSingleton<Hephaestus>.Instance.IncrementStat(Hephaestus.StatId.WYRDSTONE_GATHER, (int)((float)(items[num].amount * priceSold2) * @float));
                        }
                        else if (items[num].id == 209)
                        {
                            PandoraSingleton<Hephaestus>.Instance.IncrementStat(Hephaestus.StatId.WYRDSTONE_GATHER, (int)((float)(items[num].amount * priceSold3) * @float));
                        }
                    }
                }
            }
            PandoraSingleton<HideoutManager>.Instance.Progressor.EndGameWarbandProgress(endMission, PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband);
            PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.TRANSITION_DONE, OnTransitionDone);
            PandoraSingleton<HideoutTabManager>.Instance.ActivateLeftTabModules(false);
        }
        PandoraSingleton<HideoutManager>.Instance.SaveChanges();
        PandoraSingleton<HideoutTabManager>.Instance.button1.gameObject.SetActive(value: false);
        PandoraSingleton<HideoutTabManager>.Instance.button2.gameObject.SetActive(value: false);
        PandoraSingleton<HideoutTabManager>.Instance.button3.gameObject.SetActive(value: false);
        PandoraSingleton<HideoutTabManager>.Instance.button4.gameObject.SetActive(value: false);
        currentUnit = -1;
        state = EndState.WAIT;
        prevState = EndState.NEXT_UNIT;
    }

    public void Exit(int iTo)
    {
        MissionEndDataSave endMission = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave().endMission;
        PandoraSingleton<HideoutManager>.Instance.warbandNodeGroup.Deactivate();
        List<UnitMenuController> list = new List<UnitMenuController>();
        List<UnitMenuController> list2 = new List<UnitMenuController>();
        if (endMission.isSkirmish)
        {
            return;
        }
        for (int i = 0; i < unitList.Count; i++)
        {
            UnitMenuController unitMenuController = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.unitCtrlrs[unitList[i].Value];
            if (endMission.units[unitList[i].Key].dead)
            {
                InjuryId injuryId = unitMenuController.unit.UnitSave.injuries[unitMenuController.unit.UnitSave.injuries.Count - 1];
                if (injuryId == InjuryId.DEAD)
                {
                    list2.Add(unitMenuController);
                }
                else
                {
                    list.Add(unitMenuController);
                }
            }
        }
        for (int j = 0; j < list.Count; j++)
        {
            InjuryId data = list[j].unit.UnitSave.injuries[list[j].unit.UnitSave.injuries.Count - 1];
            PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Disband(list[j].unit, EventLogger.LogEvent.RETIREMENT, (int)data);
        }
        for (int k = 0; k < list2.Count; k++)
        {
            InjuryId data2 = list2[k].unit.UnitSave.injuries[list2[k].unit.UnitSave.injuries.Count - 1];
            PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Disband(list2[k].unit, EventLogger.LogEvent.DEATH, (int)data2);
        }
    }

    private void CleanUpMissionWagon()
    {
        MissionEndDataSave endMission = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave().endMission;
        List<ItemSave> items = endMission.wagonItems.GetItems();
        for (int num = items.Count - 1; num >= 0; num--)
        {
            ItemData itemData = PandoraSingleton<DataFactory>.Instance.InitData<ItemData>(items[num].id);
            if (itemData.ItemTypeId == ItemTypeId.QUEST_ITEM)
            {
                items.RemoveAt(num);
            }
        }
        if (endMission.VictoryType == VictoryTypeId.LOSS && (endMission.crushed || endMission.missionSave.VictoryTypeId == 2))
        {
            endMission.wagonItems.Clear();
        }
    }

    public void FixedUpdate()
    {
    }

    private void ShowNextUnitButton(int unitNum, int unitTotal)
    {
        string desc = PandoraSingleton<LocalizationManager>.Instance.GetStringById("unit_name_warriors") + " " + (unitNum + 1).ToString() + "/" + unitTotal.ToString();
        if (HasNextUnit())
        {
            actionButtonModule.Refresh(string.Empty, desc, "end_game_next_unit", Advance);
        }
        else
        {
            ShowContinueButton(desc);
        }
    }

    private void ShowContinueButton(int unitNum, int unitTotal)
    {
        string desc = PandoraSingleton<LocalizationManager>.Instance.GetStringById("unit_name_warriors") + " " + (unitNum + 1).ToString() + "/" + unitTotal.ToString();
        ShowContinueButton(desc);
    }

    private void ShowContinueButton(string desc = "")
    {
        actionButtonModule.Refresh(string.Empty, desc, "menu_continue", Advance);
    }

    private bool HasNextUnit()
    {
        return currentUnit + 1 < unitList.Count;
    }

    private void Advance()
    {
        if (canAdvance)
        {
            canAdvance = false;
            if (currentMod != null && currentMod.EndShow())
            {
                state = prevState + 1;
            }
            else if (currentMod == null)
            {
                state = EndState.NEXT_UNIT;
            }
        }
    }

    private void OnTransitionDone()
    {
        PandoraSingleton<NoticeManager>.Instance.RemoveListener(Notices.TRANSITION_DONE, OnTransitionDone);
        canAdvance = true;
        Advance();
        actionButtonModule.Refresh(string.Empty, string.Empty, "end_game_next_unit", Advance);
        actionButtonModule.SetFocus();
    }

    private int GetXpGained(MissionEndDataSave endMission, UnitMenuController ctrlr)
    {
        int num = 0;
        for (int i = 0; i < endMission.units[unitList[currentUnit].Key].XPs.Count; i++)
        {
            num += endMission.units[unitList[currentUnit].Key].XPs[i].Key;
        }
        return Mathf.Max(-ctrlr.unit.Xp, num);
    }
}
