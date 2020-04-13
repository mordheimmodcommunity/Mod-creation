using UnityEngine;
using UnityEngine.UI;

public class WarbandOverviewUnitsModule : UIModule
{
    public Text message;

    public Text activeUnits;

    public Text unpaidUnits;

    public Text learningUnits;

    public Text inTreatment;

    public Text injuredUnits;

    public Text leadersAvailable;

    public void Set(Warband warband)
    {
        int num = 0;
        int num2 = 0;
        int num3 = 0;
        int num4 = 0;
        int num5 = 0;
        int num6 = 0;
        for (int i = 0; i < warband.Units.Count; i++)
        {
            switch (warband.Units[i].GetActiveStatus())
            {
                case UnitActiveStatusId.AVAILABLE:
                    num++;
                    if (warband.Units[i].IsLeader)
                    {
                        num6++;
                    }
                    break;
                case UnitActiveStatusId.IN_TRAINING:
                    num3++;
                    break;
                case UnitActiveStatusId.INJURED_AND_UPKEEP_NOT_PAID:
                    num2++;
                    num4++;
                    break;
                case UnitActiveStatusId.INJURED:
                    num4++;
                    break;
                case UnitActiveStatusId.TREATMENT_NOT_PAID:
                    num5++;
                    break;
                case UnitActiveStatusId.UPKEEP_NOT_PAID:
                    num2++;
                    break;
                default:
                    PandoraDebug.LogWarning("Unknown status in WarbandUnitsHistoryModule.Set", "UI", this);
                    break;
            }
        }
        if (num < Constant.GetInt(ConstantId.MIN_MISSION_UNITS))
        {
            message.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_msg_mission_warning_not_enough_units"));
            ((Component)(object)message).gameObject.SetActive(value: true);
        }
        else
        {
            ((Component)(object)message).gameObject.SetActive(value: false);
        }
        activeUnits.set_text(num.ToString());
        unpaidUnits.set_text(num2.ToString());
        learningUnits.set_text(num3.ToString());
        inTreatment.set_text(num4.ToString());
        injuredUnits.set_text(num5.ToString());
        leadersAvailable.set_text(num6.ToString());
    }
}
