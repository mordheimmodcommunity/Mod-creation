using UnityEngine;

public class Stupidity : ICheapState
{
    private UnitController unitCtrlr;

    private bool stupidSuccess;

    public Stupidity(UnitController ctrlr)
    {
        unitCtrlr = ctrlr;
    }

    void ICheapState.Destroy()
    {
    }

    void ICheapState.Enter(int iFrom)
    {
        unitCtrlr.SetFixed(fix: true);
        PandoraDebug.LogInfo("Stupidity Enter: unit is engaged : " + unitCtrlr.Engaged + " has condition : " + unitCtrlr.unit.HasEnchantment(EnchantmentTypeId.MENTAL_CONDITION_STUPIDITY));
        if (unitCtrlr.unit.HasEnchantment(EnchantmentTypeId.MENTAL_CONDITION_STUPIDITY) && !unitCtrlr.Engaged)
        {
            int stupidityRoll = unitCtrlr.unit.StupidityRoll;
            stupidSuccess = unitCtrlr.unit.Roll(PandoraSingleton<MissionManager>.Instance.NetworkTyche, stupidityRoll, AttributeId.STUPIDITY_ROLL);
            unitCtrlr.recoveryTarget = stupidityRoll;
            unitCtrlr.currentActionData.SetAction(PandoraSingleton<LocalizationManager>.Instance.GetStringById("skill_name_stupidity_roll"), PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("action/stupidity", cached: true));
            PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.RETROACTION_TARGET_OUTCOME, unitCtrlr, string.Empty, stupidSuccess, (!stupidSuccess) ? PandoraSingleton<LocalizationManager>.Instance.GetStringById("retro_outcome_fail") : PandoraSingleton<LocalizationManager>.Instance.GetStringById("retro_outcome_success"));
            if (!stupidSuccess)
            {
                unitCtrlr.unit.SetAttribute(AttributeId.CURRENT_STRATEGY_POINTS, 0);
            }
            PandoraSingleton<MissionManager>.Instance.PlaySequence((!stupidSuccess) ? "stupidity" : "moral_check", unitCtrlr, OnSeqDone);
        }
        else
        {
            NextState();
        }
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
        if (!stupidSuccess)
        {
            unitCtrlr.SkillRPC(339);
        }
        else
        {
            NextState();
        }
    }

    private void NextState()
    {
        unitCtrlr.nextState = UnitController.State.START_MOVE;
    }
}
