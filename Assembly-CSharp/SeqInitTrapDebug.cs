using System.Collections.Generic;
using UnityEngine;
using WellFired;

[USequencerFriendlyName("InitTrapDebug")]
[USequencerEvent("Mordheim/InitTrapDebug")]
public class SeqInitTrapDebug : USEventBase
{
    public TrapNode trapNode;

    public AttackResultId resultId;

    public UnitStateId state;

    public string actionLabel;

    public string atkFlyingLabel;

    private Trap trap;

    public SeqInitTrapDebug()
        : this()
    {
    }

    public override void FireEvent()
    {
        if (!(Application.loadedLevelName != "sequence"))
        {
            UnitController focusedUnit = PandoraSingleton<MissionManager>.Instance.focusedUnit;
            if (trap != null)
            {
                Object.Destroy(trap.gameObject);
            }
            if (trapNode != null)
            {
                trapNode.gameObject.SetActive(value: false);
                TrapTypeData trapTypeData = PandoraSingleton<DataFactory>.Instance.InitData<TrapTypeData>((int)trapNode.typeId);
                List<TrapTypeJoinTrapData> list = PandoraSingleton<DataFactory>.Instance.InitData<TrapTypeJoinTrapData>("fk_trap_type_id", trapTypeData.Id.ToIntString());
                int index = PandoraSingleton<MissionManager>.Instance.NetworkTyche.Rand(0, list.Count);
                TrapId trapId = list[index].TrapId;
                string name = PandoraSingleton<DataFactory>.Instance.InitData<TrapData>((int)trapId).Name;
                GameObject original = null;
                GameObject gameObject = Object.Instantiate(original);
                gameObject.transform.position = trapNode.transform.position;
                gameObject.transform.rotation = trapNode.transform.rotation;
                trap = gameObject.GetComponent<Trap>();
                trap.Init(trapTypeData, 0u);
                trap.trigger.SetActive(value: false);
                focusedUnit.activeTrigger = trap;
            }
            focusedUnit.attackResultId = resultId;
            focusedUnit.unit.SetStatus(state);
            focusedUnit.currentActionLabel = actionLabel;
            focusedUnit.flyingLabel = PandoraSingleton<LocalizationManager>.Instance.GetStringById(atkFlyingLabel);
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
