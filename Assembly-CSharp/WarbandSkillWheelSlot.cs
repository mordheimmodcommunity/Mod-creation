using UnityEngine;

public class WarbandSkillWheelSlot : BaseWheelSlot<WarbandSkill>
{
    protected override Sprite GetIcon()
    {
        return WarbandSkill.GetIcon(skillData);
    }

    protected override bool IsMastery()
    {
        return skillData != null && skillData.IsMastery;
    }
}
