public class UnitTabsModule : TabsModule
{
    private readonly SkillsShop skillShop = new SkillsShop();

    public override void Init()
    {
        base.Init();
        int num = 0;
        AddTabIcon(HideoutManager.State.UNIT_INFO, num++, null, "hideout_description", IsUnitInfoAvailable);
        AddTabIcon(HideoutManager.State.INVENTORY, num++, null, "hideout_title_inventory", IsInventoryAvailable);
        AddTabIcon(HideoutManager.State.SKILLS, num++, null, "hideout_menu_unit_skills", IsSkillsAvailable);
        AddTabIcon(HideoutManager.State.SPELLS, num++, null, "hideout_menu_unit_spells", IsSpellsAvailable);
        AddTabIcon(HideoutManager.State.CUSTOMIZATION, num++, null, "hideout_submenu_customization", IsCustomizationAvailable);
    }

    public bool IsUnitInfoAvailable(out string reason)
    {
        reason = string.Empty;
        return true;
    }

    public bool IsInventoryAvailable(out string reason)
    {
        reason = string.Empty;
        if (PandoraSingleton<HideoutManager>.Instance.IsTrainingOutsider)
        {
            reason = "na_hideout_training_outsider";
            return false;
        }
        switch (PandoraSingleton<HideoutManager>.Instance.currentUnit.unit.GetActiveStatus())
        {
            case UnitActiveStatusId.UPKEEP_NOT_PAID:
            case UnitActiveStatusId.TREATMENT_NOT_PAID:
            case UnitActiveStatusId.INJURED_AND_UPKEEP_NOT_PAID:
                reason = "na_hideout_upkeep_not_paid";
                return false;
            default:
                return true;
        }
    }

    public bool IsSkillsAvailable(out string reason)
    {
        reason = string.Empty;
        switch (PandoraSingleton<HideoutManager>.Instance.currentUnit.unit.GetActiveStatus())
        {
            case UnitActiveStatusId.UPKEEP_NOT_PAID:
            case UnitActiveStatusId.INJURED_AND_UPKEEP_NOT_PAID:
                reason = "na_hideout_upkeep_not_paid";
                return false;
            case UnitActiveStatusId.TREATMENT_NOT_PAID:
                reason = "na_hideout_treatment_not_paid";
                return false;
            case UnitActiveStatusId.INJURED:
                reason = "na_hideout_unit_injured";
                return false;
            default:
                return true;
        }
    }

    public bool IsSpellsAvailable(out string reason)
    {
        reason = string.Empty;
        switch (PandoraSingleton<HideoutManager>.Instance.currentUnit.unit.GetActiveStatus())
        {
            case UnitActiveStatusId.UPKEEP_NOT_PAID:
            case UnitActiveStatusId.INJURED_AND_UPKEEP_NOT_PAID:
                reason = "na_hideout_upkeep_not_paid";
                return false;
            case UnitActiveStatusId.TREATMENT_NOT_PAID:
                reason = "na_hideout_treatment_not_paid";
                return false;
            case UnitActiveStatusId.INJURED:
                reason = "na_hideout_unit_injured";
                return false;
            default:
                if (!skillShop.UnitHasSkillLine(PandoraSingleton<HideoutManager>.Instance.currentUnit.unit, SkillLineId.SPELL))
                {
                    reason = "na_hideout_spell";
                    return false;
                }
                return true;
        }
    }

    public bool IsCustomizationAvailable(out string reason)
    {
        reason = string.Empty;
        if (PandoraSingleton<HideoutManager>.Instance.IsTrainingOutsider)
        {
            reason = "na_hideout_training_outsider";
            return false;
        }
        switch (PandoraSingleton<HideoutManager>.Instance.currentUnit.unit.GetActiveStatus())
        {
            case UnitActiveStatusId.UPKEEP_NOT_PAID:
            case UnitActiveStatusId.INJURED_AND_UPKEEP_NOT_PAID:
                reason = "na_hideout_upkeep_not_paid";
                return false;
            case UnitActiveStatusId.TREATMENT_NOT_PAID:
                reason = "na_hideout_treatment_not_paid";
                return false;
            default:
                return true;
        }
    }
}
