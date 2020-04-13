using UnityEngine.UI;

public class HideoutUnitInfo : BaseHideoutUnitState
{
    private UnitDescriptionModule unitDescModule;

    private StatusDescModule statusDescModule;

    private TreasuryModule treasuryModule;

    private CanvasGroupDisabler statusDescDisabler;

    private bool once = true;

    public HideoutUnitInfo(HideoutManager mng, HideoutCamAnchor anchor)
        : base(anchor, HideoutManager.State.UNIT_INFO)
    {
    }

    public override void Enter(int iFrom)
    {
        PandoraSingleton<HideoutTabManager>.Instance.ActivateRightTabModules(true, ModuleId.TREASURY, ModuleId.UNIT_DESC);
        if (PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.unitCtrlrs.Count > 1)
        {
            PandoraSingleton<HideoutTabManager>.Instance.ActivateCenterTabModules(ModuleId.CHARACTER_AREA, ModuleId.UNIT_TABS, ModuleId.TITLE, ModuleId.NEXT_UNIT, ModuleId.DESC, ModuleId.UNIT_DESC, ModuleId.NOTIFICATION);
            PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<NextUnitModule>(ModuleId.NEXT_UNIT).Setup();
        }
        else
        {
            PandoraSingleton<HideoutTabManager>.Instance.ActivateCenterTabModules(ModuleId.CHARACTER_AREA, ModuleId.UNIT_TABS, ModuleId.TITLE, ModuleId.DESC, ModuleId.UNIT_DESC, ModuleId.NOTIFICATION);
        }
        base.Enter(iFrom);
        unitDescModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleRight<UnitDescriptionModule>(ModuleId.UNIT_DESC);
        statusDescModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<StatusDescModule>(ModuleId.UNIT_DESC);
        if (PandoraSingleton<HideoutManager>.Instance.IsTrainingOutsider)
        {
            PandoraSingleton<HideoutTabManager>.Instance.ActivateCenterTabModule(false, ModuleId.UNIT_DESC);
        }
        else
        {
            statusDescModule.onPayUpkeep = PayUpkeep;
            statusDescModule.onPayTreatment = PayTreatment;
            statusDescModule.onFireUnit = FireUnit;
            statusDescDisabler = statusDescModule.GetComponent<CanvasGroupDisabler>();
            statusDescDisabler.enabled = true;
        }
        descModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<DescriptionModule>(ModuleId.DESC);
        descModule.gameObject.SetActive(value: false);
        treasuryModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleRight<TreasuryModule>(ModuleId.TREASURY);
        treasuryModule.Refresh(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave());
        characterCamModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<CharacterCameraAreaModule>(ModuleId.CHARACTER_AREA);
        characterCamModule.Init(camAnchor.transform.position);
        SelectUnit(PandoraSingleton<HideoutManager>.Instance.currentUnit);
        PandoraSingleton<HideoutTabManager>.Instance.DeactivateAllButtons();
        PandoraSingleton<HideoutTabManager>.Instance.button1.SetAction("cancel", "go_to_warband", 0, negative: false, PandoraSingleton<HideoutTabManager>.Instance.icnBack);
        PandoraSingleton<HideoutTabManager>.Instance.button1.OnAction(base.ReturnToWarband, mouseOnly: false);
        once = true;
    }

    public override void FixedUpdate()
    {
        if (once)
        {
            once = false;
            PandoraSingleton<HideoutManager>.Instance.ShowHideoutTuto(HideoutManager.HideoutTutoType.UNIT);
        }
    }

    private void Refresh()
    {
        statusDescModule.Refresh(currentUnit.unit);
        if (PandoraSingleton<HideoutManager>.Instance.IsTrainingOutsider)
        {
            unitDescModule.SetNav((Selectable)(object)statsModule.GetComponentInChildren<UIStatIncrease>().gameObject.GetComponent<Toggle>());
            statsModule.SetNav((Selectable)(object)statsModule.GetComponentInChildren<UIStatIncrease>().gameObject.GetComponent<Toggle>());
            RefreshUnitAttributes();
            unitDescModule.tabs[0].SetSelected(force: true);
        }
        else
        {
            unitDescModule.SetNav(statusDescModule.GetActiveButton());
            statsModule.SetNav(statusDescModule.GetActiveButton());
            statusDescModule.SetNav((Selectable)(object)statsModule.GetComponentInChildren<UIStatIncrease>().gameObject.GetComponent<Toggle>(), (Selectable)(object)unitDescModule.tabs[0].toggle);
            tabMod.Refresh();
            RefreshUnitAttributes();
            statusDescModule.SetFocus();
        }
    }

    public override void SelectUnit(UnitMenuController ctrlr)
    {
        base.SelectUnit(ctrlr);
        unitDescModule.Refresh(ctrlr.unit, showCost: false);
        Refresh();
    }

    public override bool CanIncreaseAttributes()
    {
        return true;
    }

    private void PayTreatment()
    {
        int unitTreatmentCost = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetUnitTreatmentCost(currentUnit.unit);
        if (PandoraSingleton<HideoutManager>.Instance.WarbandChest.GetGold() >= unitTreatmentCost)
        {
            PandoraSingleton<HideoutManager>.Instance.messagePopup.ShowLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("popup_pay_unit_treatment_title"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("popup_pay_unit_treatment_desc", unitTreatmentCost.ToString(), currentUnit.unit.Name, PandoraSingleton<HideoutManager>.Instance.WarbandChest.GetGold().ToString()), OnPayTreatmentPopup);
            return;
        }
        PandoraSingleton<HideoutManager>.Instance.messagePopup.Show("hideout_no_gold", "hideout_no_gold_upkeep", null);
        PandoraSingleton<HideoutManager>.Instance.messagePopup.HideCancelButton();
    }

    private void OnPayTreatmentPopup(bool confirm)
    {
        if (confirm)
        {
            PandoraSingleton<HideoutManager>.Instance.WarbandChest.RemoveGold(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetUnitTreatmentCost(currentUnit.unit));
            treasuryModule.Refresh(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave());
            currentUnit.unit.TreatmentPaid();
            PandoraSingleton<HideoutManager>.Instance.SaveChanges();
            Refresh();
        }
    }

    private void PayUpkeep()
    {
        int upkeepOwned = currentUnit.unit.GetUpkeepOwned();
        if (PandoraSingleton<HideoutManager>.Instance.WarbandChest.GetGold() >= upkeepOwned)
        {
            PandoraSingleton<HideoutManager>.Instance.messagePopup.ShowLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("popup_pay_unit_upkeep_title"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("popup_pay_unit_upkeep_desc", upkeepOwned.ToString(), currentUnit.unit.Name, PandoraSingleton<HideoutManager>.Instance.WarbandChest.GetGold().ToString()), OnPayUpkeepPopup);
            return;
        }
        PandoraSingleton<HideoutManager>.Instance.messagePopup.Show("hideout_no_gold", "hideout_no_gold_upkeep", null);
        PandoraSingleton<HideoutManager>.Instance.messagePopup.HideCancelButton();
    }

    private void OnPayUpkeepPopup(bool confirm)
    {
        if (confirm)
        {
            PandoraSingleton<HideoutManager>.Instance.WarbandChest.RemoveGold(currentUnit.unit.GetUpkeepOwned());
            treasuryModule.Refresh(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave());
            currentUnit.unit.PayUpkeepOwned();
            PandoraSingleton<HideoutManager>.Instance.SaveChanges();
            Refresh();
        }
    }

    private void FireUnit()
    {
        string key = "popup_fire_unit_desc";
        if (currentUnit.unit.IsLeader)
        {
            bool flag = false;
            for (int i = 0; i < PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.Units.Count; i++)
            {
                if (PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.Units[i].IsLeader && PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.Units[i] != currentUnit.unit)
                {
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                key = "popup_fire_unit_last_leader_desc";
            }
        }
        PandoraSingleton<HideoutManager>.Instance.messagePopup.ShowLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("popup_fire_unit_title"), PandoraSingleton<LocalizationManager>.Instance.GetStringById(key, currentUnit.unit.Name), OnFireUnitPopup);
        PandoraSingleton<HideoutManager>.Instance.messagePopup.cancelButton.effects.toggle.set_isOn(true);
    }

    private void OnFireUnitPopup(bool confirm)
    {
        if (!confirm)
        {
            return;
        }
        for (int i = 0; i < currentUnit.unit.Injuries.Count; i++)
        {
            if (currentUnit.unit.Injuries[i].Data.Id == InjuryId.SEVERED_LEG || currentUnit.unit.Injuries[i].Data.Id == InjuryId.SEVERED_ARM)
            {
                PandoraSingleton<Hephaestus>.Instance.UnlockAchievement(Hephaestus.TrophyId.INJURED_FIRE);
            }
        }
        PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Disband(currentUnit.unit, EventLogger.LogEvent.FIRE, 0);
        PandoraSingleton<HideoutManager>.Instance.SaveChanges();
        ReturnToWarband();
    }

    public override Selectable ModuleLeftOnRight()
    {
        if (PandoraSingleton<HideoutManager>.Instance.IsTrainingOutsider)
        {
            return (Selectable)(object)unitDescModule.tabs[0].toggle;
        }
        if (statusDescModule.btnAction.isActiveAndEnabled)
        {
            return (Selectable)(object)statusDescModule.btnAction.effects.toggle;
        }
        if (statusDescModule.btnFire.isActiveAndEnabled)
        {
            return (Selectable)(object)statusDescModule.btnFire.effects.toggle;
        }
        return (Selectable)(object)unitDescModule.tabs[0].toggle;
    }

    protected override void ShowDescription(string title, string desc)
    {
        base.ShowDescription(title, desc);
        statusDescDisabler.enabled = false;
        PandoraSingleton<HideoutTabManager>.Instance.button1.SetAction("cancel", "go_to_warband", 0, negative: false, PandoraSingleton<HideoutTabManager>.Instance.icnBack);
        PandoraSingleton<HideoutTabManager>.Instance.button1.OnAction(base.ReturnToWarband, mouseOnly: false);
        SetupAttributeButtons(PandoraSingleton<HideoutTabManager>.Instance.button2, PandoraSingleton<HideoutTabManager>.Instance.button3, PandoraSingleton<HideoutTabManager>.Instance.button4);
        PandoraSingleton<HideoutTabManager>.Instance.button5.gameObject.SetActive(value: false);
    }

    protected override void HideStatsDesc()
    {
        base.HideStatsDesc();
        statusDescDisabler.enabled = true;
        if (PandoraSingleton<HideoutManager>.Instance.StateMachine.GetActiveStateId() == 15)
        {
            PandoraSingleton<HideoutTabManager>.Instance.DeactivateAllButtons();
            PandoraSingleton<HideoutTabManager>.Instance.button1.SetAction("cancel", "go_to_warband", 0, negative: false, PandoraSingleton<HideoutTabManager>.Instance.icnBack);
            PandoraSingleton<HideoutTabManager>.Instance.button1.OnAction(base.ReturnToWarband, mouseOnly: false);
        }
    }
}
