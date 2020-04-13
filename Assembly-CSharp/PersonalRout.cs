using UnityEngine;

public class PersonalRout : ICheapState
{
    private UnitController unitCtrlr;

    private bool success;

    public PersonalRout(UnitController ctrlr)
    {
        unitCtrlr = ctrlr;
    }

    void ICheapState.Destroy()
    {
    }

    void ICheapState.Enter(int iFrom)
    {
        unitCtrlr.SetFixed(fix: true);
        success = true;
        unitCtrlr.currentActionLabel = AttributeId.ALL_ALONE_ROLL.ToString();
        int allAloneRoll = unitCtrlr.unit.AllAloneRoll;
        success = unitCtrlr.unit.Roll(PandoraSingleton<MissionManager>.Instance.NetworkTyche, allAloneRoll, AttributeId.ALL_ALONE_ROLL);
        unitCtrlr.currentActionData.SetAction(PandoraSingleton<LocalizationManager>.Instance.GetStringById("skill_name_all_alone_roll"), PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("action/all_alone", cached: true));
        PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.RETROACTION_TARGET_OUTCOME, unitCtrlr, string.Empty, success, (!success) ? PandoraSingleton<LocalizationManager>.Instance.GetStringById("retro_outcome_fail") : PandoraSingleton<LocalizationManager>.Instance.GetStringById("retro_outcome_success"));
        unitCtrlr.recoveryTarget = allAloneRoll;
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
        if (success || !unitCtrlr.CanDisengage())
        {
            unitCtrlr.CheckTerror();
            return;
        }
        unitCtrlr.fleeDistanceMultiplier = Constant.GetFloat(ConstantId.ALL_ALONE_MOVEMENT_MULTIPLIER);
        unitCtrlr.StateMachine.ChangeState(40);
    }
}
