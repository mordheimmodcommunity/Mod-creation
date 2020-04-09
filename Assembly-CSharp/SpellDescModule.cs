using UnityEngine.UI;

public class SpellDescModule : SkillDescModule
{
	public Text castingMod;

	public Text curseMod;

	public Text curseTitle;

	public override void Set(SkillData skillData, string reason = null)
	{
		base.Set(skillData, reason);
		bool flag = ((!PandoraSingleton<HideoutManager>.Exists()) ? null : PandoraSingleton<HideoutManager>.Instance.currentUnit.unit)?.HasSkillOrSpell(skillData.Id) ?? true;
		if (SkillHelper.IsMastery(skillData) && !flag)
		{
			SkillData skill = SkillHelper.GetSkill(skillData.SkillIdPrerequiste);
			skillType.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("skill_" + skill.SpellTypeId.ToLowerString()));
			castingMod.set_text(SkillHelper.GetLocalizedCasting(skill));
			curseTitle.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById((skill.SpellTypeId != SpellTypeId.ARCANE) ? "skill_wrath_title" : "skill_curse_title"));
			curseMod.set_text(SkillHelper.GetLocalizedCurse(skill));
		}
		else
		{
			skillType.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("skill_" + skillData.SpellTypeId.ToLowerString()));
			castingMod.set_text(SkillHelper.GetLocalizedCasting(skillData));
			curseTitle.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById((skillData.SpellTypeId != SpellTypeId.ARCANE) ? "skill_wrath_title" : "skill_curse_title"));
			curseMod.set_text(SkillHelper.GetLocalizedCurse(skillData));
		}
	}
}
