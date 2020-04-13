using UnityEngine;
using WellFired;

[USequencerFriendlyName("PlaySound")]
[USequencerEvent("Mordheim/PlaySound")]
public class SeqPlaySound : USEventBase
{
    public SequenceTargetId targetId;

    public string soundName;

    public SeqPlaySound()
        : this()
    {
    }

    public override void FireEvent()
    {
        UnitController target = null;
        switch (targetId)
        {
            case SequenceTargetId.FOCUSED_UNIT:
                target = PandoraSingleton<MissionManager>.Instance.focusedUnit;
                break;
            case SequenceTargetId.DEFENDER:
                target = PandoraSingleton<MissionManager>.Instance.focusedUnit.defenderCtrlr;
                break;
        }
        PandoraSingleton<Pan>.Instance.GetSound(soundName, cache: true, delegate (AudioClip clip)
        {
            target.audioSource.clip = clip;
            target.audioSource.Play();
            target.audioSource.loop = false;
        });
    }

    public override void ProcessEvent(float runningTime)
    {
    }

    public override void EndEvent()
    {
        ((USEventBase)this).EndEvent();
    }
}
