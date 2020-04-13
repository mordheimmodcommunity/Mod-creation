using UnityEngine;
using UnityEngine.UI;

public class WarbandSkillDescModule : UIModule
{
    public Image icon;

    public Image masteryIcon;

    public Text title;

    public Text desc;

    public GameObject masterySection;

    public Text masteryDesc;

    public virtual void Set(int idx, WarbandSkill skill)
    {
        base.gameObject.SetActive(value: true);
        icon.set_sprite(WarbandSkill.GetIcon(skill));
        title.set_text(WarbandSkill.LocName(skill));
        desc.set_text(WarbandSkill.LocDesc(skill));
        if (skill != null)
        {
            ((Behaviour)(object)masteryIcon).enabled = skill.IsMastery;
            WarbandSkillData skillMastery = skill.GetSkillMastery();
            if (skillMastery != null)
            {
                masterySection.SetActive(value: true);
                masteryDesc.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("warband_skill_desc_" + skill.Id.ToLowerString() + "_hideout"));
            }
            else
            {
                masterySection.SetActive(value: false);
            }
        }
        else
        {
            ((Behaviour)(object)masteryIcon).enabled = false;
            masterySection.SetActive(value: false);
        }
    }
}
