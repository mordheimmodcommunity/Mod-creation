using System.Collections.Generic;
using UnityEngine;

public class HideoutWarband : WarbandManagementBaseState
{
    private MenuNodeGroup nodeGroup;

    private HideoutCamAnchor camAnchor;

    private WarbandTabsModule warbandTabs;

    private TreasuryModule treasuryModule;

    private WarbandOverviewUnitsModule unitOverviewModule;

    private WarbandSheetModule warbandSheet;

    private bool once = true;

    public HideoutWarband(HideoutManager mng, HideoutCamAnchor anchor)
        : base(mng, ModuleId.WARBAND_MANAGEMENT)
    {
        camAnchor = anchor;
        once = true;
    }

    public override void Enter(int iFrom)
    {
        PandoraSingleton<HideoutManager>.Instance.CamManager.ClearLookAtFocus();
        PandoraSingleton<HideoutManager>.Instance.CamManager.CancelTransition();
        hideoutMngr.CamManager.dummyCam.transform.position = camAnchor.transform.position;
        hideoutMngr.CamManager.dummyCam.transform.rotation = camAnchor.transform.rotation;
        PandoraSingleton<HideoutManager>.Instance.CamManager.SetDOFTarget(camAnchor.dofTarget, 0f);
        PandoraSingleton<HideoutTabManager>.Instance.ActivateLeftTabModules(true, ModuleId.WARBAND_SHEET);
        PandoraSingleton<HideoutTabManager>.Instance.ActivateCenterTabModules(ModuleId.WARBAND_TABS, ModuleId.TITLE, ModuleId.NOTIFICATION, ModuleId.WARBAND_MANAGEMENT);
        warbandSheet = PandoraSingleton<HideoutTabManager>.Instance.GetModuleLeft<WarbandSheetModule>(ModuleId.WARBAND_SHEET);
        warbandSheet.Set(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband);
        warbandTabs = PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<WarbandTabsModule>(ModuleId.WARBAND_TABS);
        warbandTabs.Setup(PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<TitleModule>(ModuleId.TITLE));
        warbandTabs.SetCurrentTab(HideoutManager.State.WARBAND);
        warbandTabs.Refresh();
        unitOverviewModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleLeft<WarbandOverviewUnitsModule>(ModuleId.WARBAND_OVERVIEW_UNITS);
        hideoutMngr.warbandNodeWagon.SetContent(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.wagon);
        nodeGroup = PandoraSingleton<HideoutManager>.Instance.warbandNodeGroup;
        nodeGroup.Deactivate();
        PlaceUnits();
        base.Enter(iFrom);
        treasuryModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleRight<TreasuryModule>(ModuleId.TREASURY);
    }

    public override void Exit(int iTo)
    {
        PandoraSingleton<HideoutTabManager>.Instance.DeactivateAllButtons();
        ClearUnits();
    }

    public override void FixedUpdate()
    {
        if (once)
        {
            once = false;
            PandoraSingleton<HideoutManager>.Instance.ShowHideoutTuto(HideoutManager.HideoutTutoType.MANAGEMENT);
        }
    }

    private void PlaceUnits()
    {
        for (int i = 0; i < nodeGroup.nodes.Count; i++)
        {
            nodeGroup.nodes[i].RemoveContent();
            nodeGroup.nodes[i].Hide();
        }
        List<UnitMenuController> unitCtrlrs = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.unitCtrlrs;
        for (int j = 0; j < unitCtrlrs.Count; j++)
        {
            UnitMenuController unitMenuController = unitCtrlrs[j];
            int warbandSlotIndex = unitMenuController.unit.UnitSave.warbandSlotIndex;
            if (warbandSlotIndex >= 0 && warbandSlotIndex < 20)
            {
                if (nodeGroup.nodes[warbandSlotIndex].IsOccupied())
                {
                    PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.FindSuitableSlot(unitMenuController.unit, checkCurrent: false);
                }
                if (warbandSlotIndex == 5 && unitMenuController.unit.IsImpressive)
                {
                    nodeGroup.nodes[1].SetContent(unitMenuController);
                }
                else
                {
                    nodeGroup.nodes[warbandSlotIndex].SetContent(unitMenuController, (!unitMenuController.unit.IsImpressive) ? null : nodeGroup.nodes[warbandSlotIndex + 1]);
                }
                unitMenuController.SwitchWeapons(UnitSlotId.SET1_MAINHAND);
                unitMenuController.animator.Play(AnimatorIds.idle, -1, (float)PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0.0, 1.0));
            }
        }
    }

    public void ClearUnits()
    {
        for (int i = 0; i < nodeGroup.nodes.Count; i++)
        {
            nodeGroup.nodes[i].RemoveContent();
        }
    }

    protected override void OnEmptyNodeConfirmed(bool isImpressive)
    {
        base.OnEmptyNodeConfirmed(isImpressive);
        if (hideoutMngr.WarbandCtrlr.Warband.CanHireMoreUnit(isImpressive))
        {
            HireUnit();
        }
    }

    protected override void OnIdolSelected()
    {
        base.OnIdolSelected();
        PandoraSingleton<HideoutTabManager>.Instance.ActivateLeftTabModules(true, ModuleId.WARBAND_SHEET, ModuleId.WARBAND_OVERVIEW_UNITS, ModuleId.WARBAND_NEXT_RANK_PREVIEW);
        warbandSheet.Set(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband);
        unitOverviewModule.Set(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband);
        PandoraSingleton<HideoutTabManager>.Instance.GetModuleLeft<WarbandRankBonusPreviewModule>(ModuleId.WARBAND_NEXT_RANK_PREVIEW).Set(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband);
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

    protected override void OnIdolConfirmed()
    {
        base.OnIdolConfirmed();
        PandoraSingleton<HideoutManager>.Instance.StateMachine.ChangeState(18);
    }

    protected override void OnUnitSelected(Unit unit)
    {
        base.OnUnitSelected(unit);
        if (unit != null)
        {
            UnitMenuController unitMenuController = hideoutMngr.GetUnitMenuController(unit);
            hideoutMngr.CamManager.SetDOFTarget(unitMenuController.gameObject.transform, 0f);
        }
        GameObject gameObject = null;
        if (unit != null)
        {
            UnitMenuController unitMenuController2 = PandoraSingleton<HideoutManager>.Instance.GetUnitMenuController(unit);
            if (unitMenuController2 != null)
            {
                gameObject = unitMenuController2.gameObject;
            }
        }
        if (gameObject != null)
        {
            int num = 0;
            while (true)
            {
                if (num < nodeGroup.nodes.Count)
                {
                    if (nodeGroup.nodes[num].IsContent(gameObject))
                    {
                        break;
                    }
                    num++;
                    continue;
                }
                return;
            }
            nodeGroup.SelectNode(nodeGroup.nodes[num]);
            nodeGroup.nodes[num].highlightable.ReinitMaterials();
        }
        else
        {
            nodeGroup.UnSelectCurrentNode();
        }
    }

    protected override void OnEmptyNodeSelected(int slotIndex, bool isImpressive)
    {
        nodeGroup.UnSelectCurrentNode();
        PandoraSingleton<HideoutTabManager>.Instance.ActivateLeftTabModules(true, ModuleId.WARBAND_SHEET, ModuleId.AVAILABLE_UNITS);
        PandoraSingleton<HideoutTabManager>.Instance.button1.gameObject.SetActive(value: true);
        PandoraSingleton<HideoutTabManager>.Instance.button1.SetAction("cancel", "go_to_camp", 0, negative: false, PandoraSingleton<HideoutTabManager>.Instance.icnBack);
        PandoraSingleton<HideoutTabManager>.Instance.button1.OnAction(delegate
        {
            PandoraSingleton<HideoutManager>.Instance.StateMachine.ChangeState(0);
        }, mouseOnly: false);
        if (PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.CanHireMoreUnit(isImpressive))
        {
            PandoraSingleton<HideoutTabManager>.Instance.GetModuleLeft<AvailableUnitsModule>(ModuleId.AVAILABLE_UNITS).Set(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.GetHireableUnits(slotIndex, isImpressive), canHire: true);
        }
        else
        {
            PandoraSingleton<HideoutTabManager>.Instance.GetModuleLeft<AvailableUnitsModule>(ModuleId.AVAILABLE_UNITS).Set(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.GetHireableUnits(slotIndex, isImpressive), canHire: false);
        }
        PandoraSingleton<HideoutTabManager>.Instance.button2.gameObject.SetActive(value: false);
        PandoraSingleton<HideoutTabManager>.Instance.button3.gameObject.SetActive(value: false);
        PandoraSingleton<HideoutTabManager>.Instance.button4.gameObject.SetActive(value: false);
    }

    protected override void OnHiredSwordsConfirmed()
    {
        base.OnHiredSwordsConfirmed();
        if (PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.GetHiredSwordUnits().Count > 0)
        {
            hideoutMngr.currentWarbandSlotIdx = -1;
            hideoutMngr.StateMachine.ChangeState(14);
        }
    }

    protected override void OnHiredSwordsSelected()
    {
        PandoraSingleton<HideoutTabManager>.Instance.ActivateLeftTabModules(true, ModuleId.WARBAND_SHEET, ModuleId.AVAILABLE_UNITS);
        PandoraSingleton<HideoutTabManager>.Instance.button1.gameObject.SetActive(value: true);
        PandoraSingleton<HideoutTabManager>.Instance.button1.SetAction("cancel", "go_to_camp", 0, negative: false, PandoraSingleton<HideoutTabManager>.Instance.icnBack);
        PandoraSingleton<HideoutTabManager>.Instance.button1.OnAction(delegate
        {
            PandoraSingleton<HideoutManager>.Instance.StateMachine.ChangeState(0);
        }, mouseOnly: false);
        PandoraSingleton<HideoutTabManager>.Instance.GetModuleLeft<AvailableUnitsModule>(ModuleId.AVAILABLE_UNITS).Set(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.GetHiredSwordUnits(), canHire: true);
        PandoraSingleton<HideoutTabManager>.Instance.button2.gameObject.SetActive(value: false);
        PandoraSingleton<HideoutTabManager>.Instance.button3.gameObject.SetActive(value: false);
        PandoraSingleton<HideoutTabManager>.Instance.button4.gameObject.SetActive(value: false);
    }

    protected override void SetupDefaultButtons()
    {
        PandoraSingleton<HideoutTabManager>.Instance.button1.gameObject.SetActive(value: true);
        PandoraSingleton<HideoutTabManager>.Instance.button1.SetAction("cancel", "go_to_camp", 0, negative: false, PandoraSingleton<HideoutTabManager>.Instance.icnBack);
        PandoraSingleton<HideoutTabManager>.Instance.button1.OnAction(delegate
        {
            PandoraSingleton<HideoutManager>.Instance.StateMachine.ChangeState(0);
        }, mouseOnly: false);
        PandoraSingleton<HideoutTabManager>.Instance.button2.gameObject.SetActive(value: false);
        PandoraSingleton<HideoutTabManager>.Instance.button3.gameObject.SetActive(value: false);
    }

    private void HireUnit()
    {
        hideoutMngr.currentWarbandSlotIdx = currentUnitSlotIndex;
        hideoutMngr.currentWarbandSlotHireImpressive = currentUnitIsImpressive;
        hideoutMngr.StateMachine.ChangeState(14);
    }

    protected override void HideSwap(bool confirm)
    {
        base.HideSwap(confirm);
        PandoraSingleton<HideoutTabManager>.Instance.ActivateLeftTabModules(true, ModuleId.WARBAND_SHEET);
        PandoraSingleton<HideoutTabManager>.Instance.ActivateCenterTabModules(ModuleId.WARBAND_TABS, ModuleId.TITLE, ModuleId.NOTIFICATION, ModuleId.WARBAND_MANAGEMENT);
        PlaceUnits();
    }

    protected override void OnUnitConfirmed(Unit unit)
    {
        hideoutMngr.currentUnit = PandoraSingleton<HideoutManager>.Instance.GetUnitMenuController(unit);
        hideoutMngr.StateMachine.ChangeState(15);
    }
}
