using UnityEngine;

public class AnimatorPlayerEvents : MonoBehaviour
{
    private AnimStyleId weaponStyle;

    protected string lastShout;

    private UnitMenuController unitMenuCtrlr;

    private UnitMenuController unitCtrlr
    {
        get
        {
            if (unitMenuCtrlr == null)
            {
                unitMenuCtrlr = GetComponent<UnitMenuController>();
            }
            return unitMenuCtrlr;
        }
    }

    public void EventSheathe()
    {
        if (PandoraSingleton<AnimatorPlayer>.Instance.sheated)
        {
            PandoraSingleton<AnimatorPlayer>.Instance.weaponStyle = weaponStyle;
            PandoraSingleton<AnimatorPlayer>.Instance.EquipWeapon();
            return;
        }
        weaponStyle = PandoraSingleton<AnimatorPlayer>.Instance.weaponStyle;
        PandoraSingleton<AnimatorPlayer>.Instance.weaponStyle = AnimStyleId.NONE;
        PandoraSingleton<AnimatorPlayer>.Instance.EquipWeapon();
        PandoraSingleton<AnimatorPlayer>.Instance.sheated = true;
    }

    public void EventHurt(int variation)
    {
    }

    public void EventAvoid(int variation)
    {
    }

    public void EventParry()
    {
    }

    public void EventFx(string fxName)
    {
    }

    public void EventSound(string soundName)
    {
    }

    public void EventSoundFoot(string soundName)
    {
    }

    public void EventDisplayDamage()
    {
    }

    public void EventDisplayActionOutcome()
    {
    }

    public void EventDisplayStatusOutcome()
    {
    }

    public void EventTrail(int active)
    {
    }

    public void EventShoot(int idx)
    {
    }

    public void EventSpellStart()
    {
    }

    public void EventSpellShoot()
    {
    }

    public void EventAttachProj(int variation)
    {
    }

    public void EventReloadWeapons(int slot)
    {
    }

    public void EventWeaponAim()
    {
    }

    public void EventTp()
    {
    }

    public void EventShowStaff(int state)
    {
    }

    public void EventShout(string soundName)
    {
        GetParamSound(ref soundName);
        GetRandomSound(ref soundName, ref lastShout);
        PlaySound(ref soundName);
    }

    private void GetParamSound(ref string soundName)
    {
        string text;
        while (true)
        {
            if (soundName.IndexOf("(") == -1)
            {
                return;
            }
            int num = soundName.IndexOf("(");
            int num2 = soundName.IndexOf(")");
            text = soundName.Substring(num + 1, num2 - num - 1);
            string text2 = string.Empty;
            switch (text)
            {
                default:
                    {
                        int num3;
                        if (num3 == 1)
                        {
                            if (PandoraSingleton<MissionManager>.Instance.focusedUnit != null && PandoraSingleton<MissionManager>.Instance.focusedUnit.CurrentAction != null)
                            {
                                text2 = PandoraSingleton<MissionManager>.Instance.focusedUnit.CurrentAction.skillData.Name;
                            }
                        }
                        else
                        {
                            PandoraDebug.LogWarning("Unsupported Sound Param " + text, "SOUND");
                        }
                        break;
                    }
                case "unit":
                    text2 = unitCtrlr.unit.Data.Name;
                    break;
            }
            if (text2 == string.Empty)
            {
                break;
            }
            soundName = soundName.Substring(0, num) + text2.ToLower() + soundName.Substring(num2 + 1, soundName.Length - num2 - 1);
        }
        PandoraDebug.LogWarning("No value found for parameter (" + text + ") current soundName: " + soundName, "SOUND");
    }

    private void GetRandomSound(ref string soundName, ref string lastSoundPlayed)
    {
        int num = int.Parse(soundName.Substring(soundName.Length - 1));
        string arg = soundName.Substring(0, soundName.Length - 1);
        int num2 = PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(1, num + 1);
        string a = arg + num2;
        if (a == lastSoundPlayed)
        {
            num2 = ++num2 % (num + 1);
            if (num2 == 0)
            {
                num2++;
            }
        }
        lastSoundPlayed = arg + num2;
        soundName = lastSoundPlayed;
    }

    private void PlaySound(ref string name)
    {
        PandoraSingleton<Pan>.Instance.GetSound(name, cache: true, delegate (AudioClip clip)
        {
            if (clip != null && unitCtrlr.audioSource != null)
            {
                unitCtrlr.audioSource.PlayOneShot(clip);
            }
        });
    }
}
