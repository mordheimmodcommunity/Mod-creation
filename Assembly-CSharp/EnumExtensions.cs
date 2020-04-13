using System;
using System.Globalization;

public static class EnumExtensions
{
    private static string[] skillIdValues;

    private static string[] attributeIdValues;

    public static string ToLowerString(this object value)
    {
        return value.ToString().ToLowerInvariant();
    }

    public static string ToLowerString(this SkillId skill)
    {
        return skill.ToSkillIdString();
    }

    public static string ToLowerString(this AttributeId attribute)
    {
        return attribute.ToAttributeIdString();
    }

    public static string ToIntString<TEnum>(this TEnum enumValue) where TEnum : struct, IComparable, IFormattable, IConvertible
    {
        return Constant.ToString(enumValue.ToInt32(CultureInfo.InvariantCulture));
    }

    public static string ToSkillIdString(this SkillId skill)
    {
        if (skillIdValues == null)
        {
            skillIdValues = new string[934];
            for (SkillId skillId = SkillId.NONE; skillId < SkillId.MAX_VALUE; skillId++)
            {
                skillIdValues[(int)skillId] = skillId.ToString().ToLowerInvariant();
            }
        }
        return skillIdValues[(int)skill];
    }

    public static string ToAttributeIdString(this AttributeId attribute)
    {
        if (attributeIdValues == null)
        {
            attributeIdValues = new string[152];
            for (AttributeId attributeId = AttributeId.NONE; attributeId < AttributeId.MAX_VALUE; attributeId++)
            {
                attributeIdValues[(int)attributeId] = attributeId.ToString().ToLowerInvariant();
            }
        }
        return attributeIdValues[(int)attribute];
    }
}
