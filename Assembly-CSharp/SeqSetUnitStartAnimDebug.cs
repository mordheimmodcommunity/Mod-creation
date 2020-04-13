using UnityEngine;
using WellFired;

[USequencerFriendlyName("SetUnitStartAnimDebug")]
[USequencerEvent("Mordheim/SetUnitStartAnimDebug")]
public class SeqSetUnitStartAnimDebug : USEventBase
{
    public string animState;

    public SeqSetUnitStartAnimDebug()
        : this()
    {
    }

    public override void FireEvent()
    {
        if (!(Application.loadedLevelName != "sequence"))
        {
            UnitController focusedUnit = PandoraSingleton<MissionManager>.Instance.focusedUnit;
            focusedUnit.SetFixed(fix: false);
            focusedUnit.GetComponent<Rigidbody>().isKinematic = true;
            focusedUnit.animator.Play(Animator.StringToHash(animState));
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
