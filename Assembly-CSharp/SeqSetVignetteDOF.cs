using UnityEngine;
using WellFired;

[USequencerEvent("Mordheim/SetVignetteDOF")]
[USequencerFriendlyName("SetVignetteDOF")]
public class SeqSetVignetteDOF : USEventBase
{
    public float vignetteFadeTime = 1f;

    public float fovTime = 0.2f;

    public SequenceTargetId DOFTargetId;

    public bool setActive;

    public SeqSetVignetteDOF()
        : this()
    {
    }

    public override void FireEvent()
    {
        UnitController unitController = null;
        Transform target = null;
        switch (DOFTargetId)
        {
            case SequenceTargetId.FOCUSED_UNIT:
                unitController = PandoraSingleton<MissionManager>.Instance.focusedUnit;
                target = PandoraSingleton<MissionManager>.Instance.focusedUnit.transform;
                break;
            case SequenceTargetId.DEFENDER:
                unitController = PandoraSingleton<MissionManager>.Instance.focusedUnit.defenderCtrlr;
                target = PandoraSingleton<MissionManager>.Instance.focusedUnit.defenderCtrlr.transform;
                break;
            case SequenceTargetId.ACTION_ZONE:
                unitController = PandoraSingleton<MissionManager>.Instance.focusedUnit;
                target = PandoraSingleton<MissionManager>.Instance.focusedUnit.interactivePoint.transform;
                break;
            case SequenceTargetId.ACTION_DEST:
                unitController = PandoraSingleton<MissionManager>.Instance.focusedUnit;
                target = PandoraSingleton<MissionManager>.Instance.focusedUnit.activeActionDest.destination.transform;
                break;
        }
        if (!(PandoraSingleton<MissionManager>.Instance.OwnUnitInvolved(unitController, unitController.defenderCtrlr) == null))
        {
            if (setActive)
            {
                PandoraSingleton<MissionManager>.Instance.CamManager.TurnOnDOF(target);
            }
            else
            {
                PandoraSingleton<MissionManager>.Instance.CamManager.TurnOffDOF();
            }
            PandoraSingleton<MissionManager>.Instance.CamManager.ActivateOverlay(setActive, vignetteFadeTime);
            PandoraSingleton<MissionManager>.Instance.CamManager.SetFOV(60f, 0.2f);
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
