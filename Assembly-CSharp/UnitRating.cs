using System.Collections.Generic;

public class UnitRating
{
	public Unit unit;

	public int rating;

	public List<SkillData> skillsData;

	public int[] baseAttributes = new int[9];

	public int[] maxAttributes = new int[9];

	public void UpdateBaseAttributes()
	{
		baseAttributes[0] = unit.GetBaseAttribute(AttributeId.STRENGTH);
		baseAttributes[1] = unit.GetBaseAttribute(AttributeId.TOUGHNESS);
		baseAttributes[2] = unit.GetBaseAttribute(AttributeId.AGILITY);
		baseAttributes[3] = unit.GetBaseAttribute(AttributeId.LEADERSHIP);
		baseAttributes[4] = unit.GetBaseAttribute(AttributeId.INTELLIGENCE);
		baseAttributes[5] = unit.GetBaseAttribute(AttributeId.ALERTNESS);
		baseAttributes[6] = unit.GetBaseAttribute(AttributeId.WEAPON_SKILL);
		baseAttributes[7] = unit.GetBaseAttribute(AttributeId.BALLISTIC_SKILL);
		baseAttributes[8] = unit.GetBaseAttribute(AttributeId.ACCURACY);
	}

	public void UpdateMaxAttributes()
	{
		maxAttributes[0] = unit.GetBaseAttribute(AttributeId.STRENGTH_MAX);
		maxAttributes[1] = unit.GetBaseAttribute(AttributeId.TOUGHNESS_MAX);
		maxAttributes[2] = unit.GetBaseAttribute(AttributeId.AGILITY_MAX);
		maxAttributes[3] = unit.GetBaseAttribute(AttributeId.LEADERSHIP_MAX);
		maxAttributes[4] = unit.GetBaseAttribute(AttributeId.INTELLIGENCE_MAX);
		maxAttributes[5] = unit.GetBaseAttribute(AttributeId.ALERTNESS_MAX);
		maxAttributes[6] = unit.GetBaseAttribute(AttributeId.WEAPON_SKILL_MAX);
		maxAttributes[7] = unit.GetBaseAttribute(AttributeId.BALLISTIC_SKILL_MAX);
		maxAttributes[8] = unit.GetBaseAttribute(AttributeId.ACCURACY_MAX);
	}
}
