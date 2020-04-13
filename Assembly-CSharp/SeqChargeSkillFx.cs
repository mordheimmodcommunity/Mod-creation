using Prometheus;
using UnityEngine;
using WellFired;

[USequencerEvent("Mordheim/ChargeSkillFx")]
[USequencerFriendlyName("ChargeSkillFx")]
public class SeqChargeSkillFx : USEventBase
{
    public SeqChargeSkillFx()
        : this()
    {
    }

    public override void FireEvent()
    {
        UnitController ctrlr = PandoraSingleton<MissionManager>.Instance.focusedUnit;
        if (ctrlr.CurrentAction.fxData != null)
        {
            PandoraSingleton<Prometheus.Prometheus>.Instance.SpawnFx(ctrlr.CurrentAction.fxData.ChargeFx, (!ctrlr.CurrentAction.fxData.ChargeOnTarget || !(ctrlr.defenderCtrlr != null)) ? ctrlr : ctrlr.defenderCtrlr, null, delegate (GameObject fx)
            {
                if (fx != null)
                {
                    ctrlr.chargeFx = fx.GetComponent<OlympusFire>();
                }
            });
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
