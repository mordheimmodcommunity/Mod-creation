using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class StatusDescModule : UIModule
{
    public Image statusImage;

    public Text statusTitle;

    public Text statusText;

    public Text statusCost;

    public Image costImage;

    public ButtonGroup btnAction;

    public ButtonGroup btnFire;

    public Sprite upkeepAndTreatmentIcon;

    public Sprite upkeepIcon;

    public Sprite treatmentIcon;

    public Sprite injuredIcon;

    public Sprite trainingIcon;

    public Sprite availableIcon;

    public UnityAction onPayUpkeep;

    public UnityAction onPayTreatment;

    public UnityAction onFireUnit;

    public void Refresh(Unit unit)
    {
        int @int = Constant.GetInt(ConstantId.UPKEEP_DAYS_WITHOUT_PAY);
        int num = 0;
        Tuple<int, EventLogger.LogEvent, int> tuple = unit.Logger.FindLastEvent(EventLogger.LogEvent.NO_TREATMENT);
        if (tuple != null && tuple.Item1 > PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate)
        {
            num = tuple.Item1 - PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate;
        }
        btnFire.SetAction(string.Empty, "hideout_btn_fire_unit");
        btnFire.OnAction(onFireUnit, mouseOnly: false);
        switch (unit.GetActiveStatus())
        {
            case UnitActiveStatusId.INJURED_AND_UPKEEP_NOT_PAID:
                statusImage.set_sprite(upkeepAndTreatmentIcon);
                statusTitle.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("unit_status_name_injured_and_upkeep_not_paid"));
                statusText.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("unit_status_desc_injured_and_upkeep_not_paid", unit.UnitSave.injuredTime.ToString(), unit.GetUpkeepOwned().ToString(), (@int - unit.GetUpkeepMissedDays()).ToString()));
                btnAction.SetDisabled(disabled: false);
                ((Component)(object)costImage).gameObject.SetActive(value: true);
                statusCost.set_text(unit.GetUpkeepOwned().ToString());
                btnAction.SetAction(string.Empty, "hideout_pay");
                btnAction.OnAction(onPayUpkeep, mouseOnly: false);
                btnAction.effects.enabled = true;
                btnFire.SetDisabled();
                break;
            case UnitActiveStatusId.UPKEEP_NOT_PAID:
                statusImage.set_sprite(upkeepIcon);
                statusTitle.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("unit_status_name_upkeep_not_paid"));
                statusText.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("unit_status_desc_upkeep_not_paid", unit.GetUpkeepOwned().ToString(), (@int - unit.GetUpkeepMissedDays()).ToString()));
                btnAction.SetDisabled(disabled: false);
                ((Component)(object)costImage).gameObject.SetActive(value: true);
                statusCost.set_text(unit.GetUpkeepOwned().ToString());
                btnAction.SetAction(string.Empty, "hideout_pay");
                btnAction.OnAction(onPayUpkeep, mouseOnly: false);
                btnAction.effects.enabled = true;
                btnFire.SetDisabled();
                break;
            case UnitActiveStatusId.TREATMENT_NOT_PAID:
                statusImage.set_sprite(treatmentIcon);
                statusTitle.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("unit_status_name_treatment_not_paid"));
                statusText.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("unit_status_desc_treatment_not_paid", PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetUnitTreatmentCost(unit).ToString(), num.ToString()));
                btnAction.SetDisabled(disabled: false);
                ((Component)(object)costImage).gameObject.SetActive(value: true);
                statusCost.set_text(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetUnitTreatmentCost(unit).ToString());
                btnAction.SetAction(string.Empty, "hideout_pay_treatment");
                btnAction.OnAction(onPayTreatment, mouseOnly: false);
                btnAction.effects.enabled = true;
                btnFire.SetDisabled();
                break;
            case UnitActiveStatusId.INJURED:
                statusImage.set_sprite(injuredIcon);
                statusTitle.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("unit_status_name_injured"));
                statusText.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("unit_status_desc_injured", unit.UnitSave.injuredTime.ToString()));
                btnAction.SetAction(string.Empty, "hideout_pay");
                btnAction.SetDisabled();
                ((Component)(object)costImage).gameObject.SetActive(value: false);
                statusCost.set_text(string.Empty);
                btnFire.SetDisabled(disabled: false);
                break;
            case UnitActiveStatusId.IN_TRAINING:
                statusImage.set_sprite(trainingIcon);
                statusTitle.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("unit_status_name_in_training"));
                statusText.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("unit_status_desc_in_training", unit.UnitSave.trainingTime.ToString()));
                btnAction.SetAction(string.Empty, "hideout_pay");
                btnAction.SetDisabled();
                ((Component)(object)costImage).gameObject.SetActive(value: false);
                btnFire.SetDisabled();
                statusCost.set_text(string.Empty);
                break;
            case UnitActiveStatusId.AVAILABLE:
                statusImage.set_sprite(availableIcon);
                statusTitle.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("unit_status_name_available"));
                statusText.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("unit_status_desc_available"));
                btnAction.SetAction(string.Empty, "hideout_pay");
                btnAction.SetDisabled();
                ((Component)(object)costImage).gameObject.SetActive(value: false);
                statusCost.set_text(string.Empty);
                btnFire.SetDisabled(disabled: false);
                break;
        }
        if (PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave().lateShipmentCount >= Constant.GetInt(ConstantId.MAX_SHIPMENT_FAIL))
        {
            btnAction.gameObject.SetActive(value: false);
            btnFire.gameObject.SetActive(value: false);
        }
    }

    public void Refresh(Warband wb)
    {
        btnFire.SetAction(string.Empty, "hideout_disband");
        btnFire.OnAction(onFireUnit, mouseOnly: false);
        int totalUpkeepOwned = wb.GetTotalUpkeepOwned();
        int totalTreatmentOwned = wb.GetTotalTreatmentOwned();
        if (totalTreatmentOwned > 0)
        {
            statusImage.set_sprite(treatmentIcon);
            statusTitle.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("warband_status_name_treatment_required"));
            statusText.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("warband_status_desc_treatment_required", totalTreatmentOwned.ToString()));
            btnFire.SetDisabled();
            btnAction.SetDisabled(disabled: false);
            ((Component)(object)costImage).gameObject.SetActive(value: true);
            statusCost.set_text(totalTreatmentOwned.ToString());
            btnAction.SetAction(string.Empty, "hideout_pay_treatment");
            btnAction.OnAction(onPayTreatment, mouseOnly: false);
            btnAction.effects.enabled = true;
        }
        else if (totalUpkeepOwned > 0)
        {
            statusImage.set_sprite(upkeepIcon);
            statusTitle.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("warband_status_name_upkeep_not_paid"));
            statusText.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("warband_status_desc_upkeep_not_paid", totalUpkeepOwned.ToString()));
            btnFire.SetDisabled();
            btnAction.SetDisabled(disabled: false);
            ((Component)(object)costImage).gameObject.SetActive(value: true);
            statusCost.set_text(totalUpkeepOwned.ToString());
            btnAction.SetAction(string.Empty, "hideout_pay");
            btnAction.OnAction(onPayUpkeep, mouseOnly: false);
            btnAction.effects.enabled = true;
        }
        else
        {
            statusImage.set_sprite(availableIcon);
            statusTitle.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("warband_status_name_normal"));
            statusText.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("warband_status_desc_normal"));
            btnAction.SetAction(string.Empty, "hideout_pay");
            btnAction.SetDisabled();
            ((Component)(object)costImage).gameObject.SetActive(value: false);
            statusCost.set_text(string.Empty);
            btnFire.SetDisabled(disabled: false);
        }
        if (PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave().lateShipmentCount >= Constant.GetInt(ConstantId.MAX_SHIPMENT_FAIL))
        {
            btnAction.gameObject.SetActive(value: false);
            btnFire.gameObject.SetActive(value: false);
        }
    }

    public void SetFocus()
    {
        if (btnAction.IsInteractable())
        {
            btnAction.SetSelected(force: true);
        }
        else
        {
            btnFire.SetSelected(force: true);
        }
    }

    public Selectable GetActiveButton()
    {
        if (btnAction.IsInteractable())
        {
            return (Selectable)(object)btnAction.effects.toggle;
        }
        return (Selectable)(object)btnFire.effects.toggle;
    }

    private void OnDisable()
    {
        btnAction.effects.toggle.set_isOn(false);
        btnFire.effects.toggle.set_isOn(false);
    }

    private void Update()
    {
    }

    public void SetNav(Selectable left, Selectable right)
    {
        //IL_0020: Unknown result type (might be due to invalid IL or missing references)
        //IL_0025: Unknown result type (might be due to invalid IL or missing references)
        //IL_0083: Unknown result type (might be due to invalid IL or missing references)
        //IL_00a9: Unknown result type (might be due to invalid IL or missing references)
        //IL_00ae: Unknown result type (might be due to invalid IL or missing references)
        //IL_010c: Unknown result type (might be due to invalid IL or missing references)
        if (btnFire.IsInteractable())
        {
            Navigation navigation = ((Selectable)btnFire.effects.toggle).get_navigation();
            ((Navigation)(ref navigation)).set_mode((Mode)4);
            ((Navigation)(ref navigation)).set_selectOnLeft(left);
            ((Navigation)(ref navigation)).set_selectOnRight(right);
            ((Navigation)(ref navigation)).set_selectOnUp((Selectable)null);
            ((Navigation)(ref navigation)).set_selectOnDown((Selectable)(object)((!btnAction.IsInteractable()) ? null : btnAction.effects.toggle));
            ((Selectable)btnFire.effects.toggle).set_navigation(navigation);
        }
        if (btnAction.IsInteractable())
        {
            Navigation navigation2 = ((Selectable)btnAction.effects.toggle).get_navigation();
            ((Navigation)(ref navigation2)).set_mode((Mode)4);
            ((Navigation)(ref navigation2)).set_selectOnLeft(left);
            ((Navigation)(ref navigation2)).set_selectOnRight(right);
            ((Navigation)(ref navigation2)).set_selectOnDown((Selectable)null);
            ((Navigation)(ref navigation2)).set_selectOnUp((Selectable)(object)((!btnFire.IsInteractable()) ? null : btnFire.effects.toggle));
            ((Selectable)btnAction.effects.toggle).set_navigation(navigation2);
        }
    }
}
