using System.Collections.Generic;
using UnityEngine;
using WellFired;

[USequencerEvent("Mordheim/EndSequence")]
[USequencerFriendlyName("EndSequence")]
public class SeqEndSequence : USEventBase
{
    private float vignetteFadeTime = 0.2f;

    public List<SeqFinisher> finishers;

    public SeqEndSequence()
        : this()
    {
    }

    public override void FireEvent()
    {
        if (!PandoraSingleton<SequenceManager>.Instance.isPlaying)
        {
            PandoraDebug.LogWarning("EndSequence FireEvent SequenceManager is not playing. This is most probably a second call to this event");
        }
        else if (finishers != null && finishers.Count > 0)
        {
            UnitController focusedUnit = PandoraSingleton<MissionManager>.Instance.focusedUnit;
            foreach (SeqFinisher finisher in finishers)
            {
                if (focusedUnit.unit.Data.UnitBaseId == finisher.id && (finisher.style == AnimStyleId.NONE || focusedUnit.unit.currentAnimStyleId == finisher.style))
                {
                    End();
                }
            }
        }
        else
        {
            End();
        }
    }

    public override void ProcessEvent(float runningTime)
    {
    }

    public override void EndEvent()
    {
        ((USEventBase)this).EndEvent();
    }

    private void End()
    {
        PandoraDebug.LogInfo("SeqEndSequence Ending Sequence " + ((!((Object)(object)PandoraSingleton<SequenceManager>.Instance.currentSeq != null)) ? "none" : ((Object)(object)PandoraSingleton<SequenceManager>.Instance.currentSeq).name), "SEQUENCE");
        PandoraSingleton<MissionManager>.Instance.CamManager.TurnOffDOF();
        PandoraSingleton<MissionManager>.Instance.CamManager.SetFOV(45f, 0.2f);
        PandoraSingleton<MissionManager>.Instance.CamManager.ActivateOverlay(active: false, vignetteFadeTime);
        PandoraSingleton<MissionManager>.Instance.CamManager.DeactivateBloodSplatter();
        PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.SEQUENCE_ENDED);
    }
}
