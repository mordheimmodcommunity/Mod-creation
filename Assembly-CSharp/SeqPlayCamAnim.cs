using System.Text;
using UnityEngine;
using WellFired;

[USequencerEvent("Mordheim/PlayCamAnim")]
[USequencerFriendlyName("PlayCamAnim")]
public class SeqPlayCamAnim : USEventBase
{
    public string clip;

    public SequenceTargetId targetId;

    public CamAnimTypeId typeId;

    public bool raceBound;

    public bool weaponSizeBound;

    public bool unitSizeBound;

    public bool atkResultBound;

    public bool checkClaws;

    public int variations;

    public SeqPlayCamAnim()
        : this()
    {
    }

    public override void FireEvent()
    {
        UnitController unitController = null;
        Transform transform = null;
        Transform transform2 = null;
        Transform transform3 = null;
        switch (targetId)
        {
            case SequenceTargetId.FOCUSED_UNIT:
                unitController = PandoraSingleton<MissionManager>.Instance.focusedUnit;
                transform = unitController.transform;
                if (unitController.defenderCtrlr != null)
                {
                    transform2 = unitController.defenderCtrlr.transform;
                }
                break;
            case SequenceTargetId.DEFENDER:
                unitController = PandoraSingleton<MissionManager>.Instance.focusedUnit.defenderCtrlr;
                transform = unitController.transform;
                break;
            case SequenceTargetId.ACTION_ZONE:
                unitController = PandoraSingleton<MissionManager>.Instance.focusedUnit;
                transform = unitController.interactivePoint.transform;
                transform3 = unitController.interactivePoint.cameraAnchor;
                break;
            case SequenceTargetId.ACTION_DEST:
                unitController = PandoraSingleton<MissionManager>.Instance.focusedUnit;
                transform = unitController.activeActionDest.destination.transform;
                break;
        }
        StringBuilder stringBuilder = new StringBuilder();
        if (raceBound)
        {
            string camBase = unitController.unit.BaseData.CamBase;
            stringBuilder.Append(camBase);
            stringBuilder.Append("_");
        }
        if (checkClaws && unitController.Equipments[(int)unitController.unit.ActiveWeaponSlot] != null && unitController.Equipments[(int)unitController.unit.ActiveWeaponSlot].Item.Id == ItemId.FIGHTING_CLAWS)
        {
            stringBuilder.Append("cl_");
        }
        if (unitSizeBound || (weaponSizeBound && unitController.unit.Data.UnitSizeId == UnitSizeId.LARGE))
        {
            UnitSizeData unitSizeData = PandoraSingleton<DataFactory>.Instance.InitData<UnitSizeData>((int)unitController.unit.Data.UnitSizeId);
            string size = unitSizeData.Size;
            stringBuilder.Append(size);
            stringBuilder.Append("_");
        }
        else if (weaponSizeBound)
        {
            string size2 = unitController.Equipments[(int)unitController.unit.ActiveWeaponSlot].Item.StyleData.Size;
            stringBuilder.Append(size2);
            stringBuilder.Append("_");
        }
        stringBuilder.Append(clip);
        if (atkResultBound)
        {
            stringBuilder.Append((unitController.attackResultId == AttackResultId.PARRY) ? "_critical" : ((!unitController.criticalHit) ? "_success" : "_critical"));
        }
        stringBuilder.Append("_cam");
        if (variations != 0)
        {
            int value = PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0, variations) + 1;
            stringBuilder.Append("_0");
            stringBuilder.Append(value);
        }
        else
        {
            stringBuilder.Append("_01");
        }
        string text = stringBuilder.ToString();
        UnitController unitController2 = PandoraSingleton<MissionManager>.Instance.OwnUnitInvolved(unitController, unitController.defenderCtrlr);
        if (unitController2 != null && unitController.defenderCtrlr == null && !text.Contains("search"))
        {
            if (transform != unitController2.transform)
            {
                PandoraSingleton<MissionManager>.Instance.CamManager.LookAtFocus(transform, overrideCurrentTarget: false);
            }
            PandoraSingleton<MissionManager>.Instance.CamManager.SwitchToCam(CameraManager.CameraType.CHARACTER, unitController2.transform, transition: true, force: true, transform == unitController2.transform, unitController2.unit.Data.UnitSizeId == UnitSizeId.LARGE);
        }
        else if (unitController2 != null && text.Contains("search"))
        {
            if (transform3 != null)
            {
                PandoraSingleton<MissionManager>.Instance.CamManager.LookAtFocus(transform, overrideCurrentTarget: false);
                PandoraSingleton<MissionManager>.Instance.CamManager.SwitchToCam(CameraManager.CameraType.CONSTRAINED, unitController2.transform, transition: true, force: true, clearFocus: false, unitController2.unit.Data.UnitSizeId == UnitSizeId.LARGE);
                ConstrainedCamera currentCam = PandoraSingleton<MissionManager>.Instance.CamManager.GetCurrentCam<ConstrainedCamera>();
                currentCam.SetOrigins(transform3);
            }
            else
            {
                PandoraSingleton<MissionManager>.Instance.CamManager.LookAtFocus(transform, overrideCurrentTarget: false);
                PandoraSingleton<MissionManager>.Instance.CamManager.SwitchToCam(CameraManager.CameraType.SEMI_CONSTRAINED, unitController2.transform, transition: true, force: true, clearFocus: false, unitController2.unit.Data.UnitSizeId == UnitSizeId.LARGE);
            }
        }
        else if (unitController2 != null)
        {
            PandoraSingleton<MissionManager>.Instance.CamManager.LookAtFocus((!(unitController2.transform == transform2)) ? transform2 : unitController.transform, overrideCurrentTarget: false);
            PandoraSingleton<MissionManager>.Instance.CamManager.SwitchToCam(CameraManager.CameraType.SEMI_CONSTRAINED, unitController2.transform, transition: true, force: true, clearFocus: false, unitController2.unit.Data.UnitSizeId == UnitSizeId.LARGE);
        }
        if (transform2 != null)
        {
            PandoraSingleton<MissionManager>.Instance.CamManager.AddLOSTarget(transform2);
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
