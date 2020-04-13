using UnityEngine;

public class SkillWheelSlot : BaseWheelSlot<SkillData>
{
    protected override Sprite GetIcon()
    {
        if (skillData != null)
        {
            return SkillHelper.GetIcon(skillData);
        }
        return null;
    }

    protected override bool IsMastery()
    {
        if (skillData != null)
        {
            return SkillHelper.IsMastery(skillData);
        }
        return false;
    }
}
