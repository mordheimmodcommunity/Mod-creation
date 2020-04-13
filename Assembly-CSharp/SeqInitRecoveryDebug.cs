using UnityEngine;
using WellFired;

[USequencerEvent("Mordheim/InitRecoveryDebug")]
[USequencerFriendlyName("InitRecoveryDebug")]
public class SeqInitRecoveryDebug : USEventBase
{
    public AttributeId recoveryAttributedId;

    public int recoveryTarget;

    public int recoveryDamage;

    public bool recoverySuccess;

    public UnitStateId startingState;

    public SeqInitRecoveryDebug()
        : this()
    {
    }

    public override void FireEvent()
    {
        if (!(Application.loadedLevelName != "sequence"))
        {
            UnitController focusedUnit = PandoraSingleton<MissionManager>.Instance.focusedUnit;
            PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.GAME_WARBAND_MORAL, PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs, Constant.GetFloat(ConstantId.ROUT_RATIO_ALIVE));
            if (startingState == UnitStateId.STUNNED)
            {
                focusedUnit.animator.Play(AnimatorIds.kneeling_stunned);
                focusedUnit.animator.SetInteger(AnimatorIds.unit_state, 2);
                focusedUnit.unit.SetStatus(UnitStateId.STUNNED);
            }
            else
            {
                focusedUnit.animator.Play(AnimatorIds.idle);
                focusedUnit.animator.SetInteger(AnimatorIds.unit_state, 0);
                focusedUnit.unit.SetStatus(UnitStateId.NONE);
            }
            focusedUnit.recoveryTarget = recoveryTarget;
        }
    }

    public override void ProcessEvent(float runningTime)
    {
    }

    public override void EndEvent()
    {
        ((USEventBase)this).EndEvent();
    }
}
