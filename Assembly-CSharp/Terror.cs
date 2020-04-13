using UnityEngine;

public class Terror : ICheapState
{
    private UnitController unitCtrlr;

    private bool success;

    public Terror(UnitController ctrlr)
    {
        unitCtrlr = ctrlr;
    }

    void ICheapState.Destroy()
    {
    }

    void ICheapState.Enter(int iFrom)
    {
        unitCtrlr.SetFixed(fix: true);
        unitCtrlr.currentActionLabel = AttributeId.TERROR_ROLL.ToString();
        int terrorRoll = unitCtrlr.unit.TerrorRoll;
        success = unitCtrlr.unit.Roll(PandoraSingleton<MissionManager>.Instance.NetworkTyche, terrorRoll, AttributeId.TERROR_ROLL);
        unitCtrlr.recoveryTarget = ((terrorRoll <= unitCtrlr.recoveryTarget) ? unitCtrlr.recoveryTarget : terrorRoll);
        unitCtrlr.currentActionData.SetAction(PandoraSingleton<LocalizationManager>.Instance.BuildStringAndLocalize("skill_name_", AttributeId.TERROR_ROLL.ToString()), PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("action/terror", cached: true));
        PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.RETROACTION_TARGET_OUTCOME, unitCtrlr, string.Empty, success, (!success) ? PandoraSingleton<LocalizationManager>.Instance.GetStringById("retro_outcome_fail") : PandoraSingleton<LocalizationManager>.Instance.GetStringById("retro_outcome_success"));
        PandoraSingleton<MissionManager>.Instance.PlaySequence("moral_check", unitCtrlr, OnSeqDone);
    }

    void ICheapState.Exit(int iTo)
    {
    }

    void ICheapState.Update()
    {
    }

    void ICheapState.FixedUpdate()
    {
    }

    private void OnSeqDone()
    {
        unitCtrlr.StateMachine.ChangeState(9);
        PandoraSingleton<MissionManager>.Instance.GetCurrentUnit().CheckFear();
    }
}
