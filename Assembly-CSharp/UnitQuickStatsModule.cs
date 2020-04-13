using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitQuickStatsModule : UIModule
{
    public Text strStat;

    public Image strStatDown;

    public Image strStatUp;

    public Text toughnessStat;

    public Image toughnessStatDown;

    public Image toughnessStatUp;

    public Text agilityStat;

    public Image agilityStatDown;

    public Image agilityStatUp;

    public Text leadershipStat;

    public Image leadershipStatDown;

    public Image leadershipStatUp;

    public Text intelligenceStat;

    public Image intelligenceStatDown;

    public Image intelligenceStatUp;

    public Text alertnessStat;

    public Image alertnessStatDown;

    public Image alertnessStatUp;

    public Text weaponSkillStat;

    public Image weaponSkillStatDown;

    public Image weaponSkillStatUp;

    public Text ballisticSkillStat;

    public Image ballisticSkillStatDown;

    public Image ballisticSkillStatUp;

    public Text accuracyStat;

    public Image accuracyStatDown;

    public Image accuracyStatUp;

    public Text movementStat;

    public Image movementStatDown;

    public Image movementStatUp;

    public Text moraleImpactStat;

    public Image moraleImpactStatDown;

    public Image moraleImpactStatUp;

    public Text moraleStat;

    public Image moraleStatDown;

    public Image moraleStatUp;

    public Text initiativeStat;

    public Image initiativeStatDown;

    public Image initiativeStatUp;

    public Text poisonResistStat;

    public Image poisonResistStatDown;

    public Image poisonResistStatUp;

    public Text magicResistStat;

    public Image magicResistStatDown;

    public Image magicResistStatUp;

    public Text rangeResistStat;

    public Image rangeResistStatDown;

    public Image rangeResistStatUp;

    public Text meleeResistStat;

    public Image meleeResistStatDown;

    public Image meleeResistStatUp;

    public Text critResistStat;

    public Image critResistStatDown;

    public Image critResistStatUp;

    public Text stunResistStat;

    public Image stunResistStatDown;

    public Image stunResistStatUp;

    public Text trapResistStat;

    public Image trapResistStatDown;

    public Image trapResistStatUp;

    public Text wyrdstoneResistStat;

    public Image wyrdstoneResistStatDown;

    public Image wyrdstoneResistStatUp;

    public UIInventoryItem primaryMainWeapon;

    public UIInventoryItem primaryOffHandWeapon;

    public GameObject secondaryWeaponGroup;

    public UIInventoryItem secondaryMainWeapon;

    public UIInventoryItem secondaryOffHandWeapon;

    public GameObject mutationGroup;

    public List<UIInventoryMutation> mutations;

    public List<UISkillItem> activeSkills;

    public List<UISkillItem> passiveSkills;

    public void RefreshStats(Unit unit, Unit comparedUnit = null)
    {
        strStat.set_text(unit.Strength.ToString());
        toughnessStat.set_text(unit.Toughness.ToString());
        agilityStat.set_text(unit.Agility.ToString());
        leadershipStat.set_text(unit.Leadership.ToString());
        intelligenceStat.set_text(unit.Intelligence.ToString());
        alertnessStat.set_text(unit.Alertness.ToString());
        weaponSkillStat.set_text(unit.WeaponSkill.ToString());
        ballisticSkillStat.set_text(unit.BallisticSkill.ToString());
        accuracyStat.set_text(unit.Accuracy.ToString());
        movementStat.set_text(unit.Movement.ToString());
        moraleImpactStat.set_text(unit.MoralImpact.ToString());
        moraleStat.set_text(unit.Moral.ToString());
        initiativeStat.set_text(unit.Initiative.ToString());
        poisonResistStat.set_text(unit.PoisonResistRoll.ToString());
        magicResistStat.set_text(unit.MagicResistance.ToString());
        rangeResistStat.set_text(unit.RangeResistance.ToString());
        meleeResistStat.set_text(unit.MeleeResistance.ToString());
        critResistStat.set_text(unit.CritResistance.ToString());
        stunResistStat.set_text(unit.StunResistRoll.ToString());
        trapResistStat.set_text(unit.TrapResistRoll.ToString());
        wyrdstoneResistStat.set_text(unit.WyrdstoneResistRoll.ToString());
        ((Component)(object)strStatDown).gameObject.SetActive(value: false);
        ((Component)(object)strStatUp).gameObject.SetActive(value: false);
        ((Component)(object)toughnessStatDown).gameObject.SetActive(value: false);
        ((Component)(object)toughnessStatUp).gameObject.SetActive(value: false);
        ((Component)(object)agilityStatDown).gameObject.SetActive(value: false);
        ((Component)(object)agilityStatUp).gameObject.SetActive(value: false);
        ((Component)(object)leadershipStatDown).gameObject.SetActive(value: false);
        ((Component)(object)leadershipStatUp).gameObject.SetActive(value: false);
        ((Component)(object)intelligenceStatDown).gameObject.SetActive(value: false);
        ((Component)(object)intelligenceStatUp).gameObject.SetActive(value: false);
        ((Component)(object)alertnessStatDown).gameObject.SetActive(value: false);
        ((Component)(object)alertnessStatUp).gameObject.SetActive(value: false);
        ((Component)(object)weaponSkillStatDown).gameObject.SetActive(value: false);
        ((Component)(object)weaponSkillStatUp).gameObject.SetActive(value: false);
        ((Component)(object)ballisticSkillStatDown).gameObject.SetActive(value: false);
        ((Component)(object)ballisticSkillStatUp).gameObject.SetActive(value: false);
        ((Component)(object)accuracyStatDown).gameObject.SetActive(value: false);
        ((Component)(object)accuracyStatUp).gameObject.SetActive(value: false);
        ((Component)(object)movementStatDown).gameObject.SetActive(value: false);
        ((Component)(object)movementStatUp).gameObject.SetActive(value: false);
        ((Component)(object)moraleImpactStatDown).gameObject.SetActive(value: false);
        ((Component)(object)moraleImpactStatUp).gameObject.SetActive(value: false);
        ((Component)(object)moraleStatDown).gameObject.SetActive(value: false);
        ((Component)(object)moraleStatUp).gameObject.SetActive(value: false);
        ((Component)(object)initiativeStatDown).gameObject.SetActive(value: false);
        ((Component)(object)initiativeStatUp).gameObject.SetActive(value: false);
        ((Component)(object)poisonResistStatDown).gameObject.SetActive(value: false);
        ((Component)(object)poisonResistStatUp).gameObject.SetActive(value: false);
        ((Component)(object)magicResistStatDown).gameObject.SetActive(value: false);
        ((Component)(object)magicResistStatUp).gameObject.SetActive(value: false);
        ((Component)(object)rangeResistStatDown).gameObject.SetActive(value: false);
        ((Component)(object)rangeResistStatUp).gameObject.SetActive(value: false);
        ((Component)(object)meleeResistStatDown).gameObject.SetActive(value: false);
        ((Component)(object)meleeResistStatUp).gameObject.SetActive(value: false);
        ((Component)(object)critResistStatDown).gameObject.SetActive(value: false);
        ((Component)(object)critResistStatUp).gameObject.SetActive(value: false);
        ((Component)(object)stunResistStatDown).gameObject.SetActive(value: false);
        ((Component)(object)stunResistStatUp).gameObject.SetActive(value: false);
        ((Component)(object)trapResistStatDown).gameObject.SetActive(value: false);
        ((Component)(object)trapResistStatUp).gameObject.SetActive(value: false);
        ((Component)(object)wyrdstoneResistStatDown).gameObject.SetActive(value: false);
        ((Component)(object)wyrdstoneResistStatUp).gameObject.SetActive(value: false);
        if (comparedUnit != null)
        {
            SetCompareIcon(unit.Strength, comparedUnit.Strength, ((Component)(object)strStatUp).gameObject, ((Component)(object)strStatDown).gameObject);
            SetCompareIcon(unit.Toughness, comparedUnit.Toughness, ((Component)(object)toughnessStatUp).gameObject, ((Component)(object)toughnessStatDown).gameObject);
            SetCompareIcon(unit.Agility, comparedUnit.Agility, ((Component)(object)agilityStatUp).gameObject, ((Component)(object)agilityStatDown).gameObject);
            SetCompareIcon(unit.Leadership, comparedUnit.Leadership, ((Component)(object)leadershipStatUp).gameObject, ((Component)(object)leadershipStatDown).gameObject);
            SetCompareIcon(unit.Intelligence, comparedUnit.Intelligence, ((Component)(object)intelligenceStatUp).gameObject, ((Component)(object)intelligenceStatDown).gameObject);
            SetCompareIcon(unit.Alertness, comparedUnit.Alertness, ((Component)(object)alertnessStatUp).gameObject, ((Component)(object)alertnessStatDown).gameObject);
            SetCompareIcon(unit.WeaponSkill, comparedUnit.WeaponSkill, ((Component)(object)weaponSkillStatUp).gameObject, ((Component)(object)weaponSkillStatDown).gameObject);
            SetCompareIcon(unit.BallisticSkill, comparedUnit.BallisticSkill, ((Component)(object)ballisticSkillStatUp).gameObject, ((Component)(object)ballisticSkillStatDown).gameObject);
            SetCompareIcon(unit.Accuracy, comparedUnit.Accuracy, ((Component)(object)accuracyStatUp).gameObject, ((Component)(object)accuracyStatDown).gameObject);
            SetCompareIcon(unit.Movement, comparedUnit.Movement, ((Component)(object)movementStatUp).gameObject, ((Component)(object)movementStatDown).gameObject);
            SetCompareIcon(unit.MoralImpact, comparedUnit.MoralImpact, ((Component)(object)moraleImpactStatDown).gameObject, ((Component)(object)moraleImpactStatUp).gameObject);
            SetCompareIcon(unit.Moral, comparedUnit.Moral, ((Component)(object)moraleStatUp).gameObject, ((Component)(object)moraleStatDown).gameObject);
            SetCompareIcon(unit.Initiative, comparedUnit.Initiative, ((Component)(object)initiativeStatUp).gameObject, ((Component)(object)initiativeStatDown).gameObject);
            SetCompareIcon(unit.PoisonResistRoll, comparedUnit.PoisonResistRoll, ((Component)(object)poisonResistStatUp).gameObject, ((Component)(object)poisonResistStatDown).gameObject);
            SetCompareIcon(unit.MagicResistance, comparedUnit.MagicResistance, ((Component)(object)magicResistStatUp).gameObject, ((Component)(object)magicResistStatDown).gameObject);
            SetCompareIcon(unit.RangeResistance, comparedUnit.RangeResistance, ((Component)(object)rangeResistStatUp).gameObject, ((Component)(object)rangeResistStatDown).gameObject);
            SetCompareIcon(unit.MeleeResistance, comparedUnit.MeleeResistance, ((Component)(object)meleeResistStatUp).gameObject, ((Component)(object)meleeResistStatDown).gameObject);
            SetCompareIcon(unit.CritResistance, comparedUnit.CritResistance, ((Component)(object)critResistStatUp).gameObject, ((Component)(object)critResistStatDown).gameObject);
            SetCompareIcon(unit.StunResistRoll, comparedUnit.StunResistRoll, ((Component)(object)stunResistStatUp).gameObject, ((Component)(object)stunResistStatDown).gameObject);
            SetCompareIcon(unit.TrapResistRoll, comparedUnit.TrapResistRoll, ((Component)(object)trapResistStatUp).gameObject, ((Component)(object)trapResistStatDown).gameObject);
            SetCompareIcon(unit.WyrdstoneResistRoll, comparedUnit.WyrdstoneResistRoll, ((Component)(object)wyrdstoneResistStatUp).gameObject, ((Component)(object)wyrdstoneResistStatDown).gameObject);
        }
        int activeWeaponSlot = (int)unit.ActiveWeaponSlot;
        primaryMainWeapon.Set(unit.Items[activeWeaponSlot]);
        primaryOffHandWeapon.Set(unit.Items[activeWeaponSlot + 1]);
        if (unit.CanSwitchWeapon())
        {
            secondaryWeaponGroup.gameObject.SetActive(value: true);
            int inactiveWeaponSlot = (int)unit.InactiveWeaponSlot;
            secondaryMainWeapon.Set(unit.Items[inactiveWeaponSlot]);
            secondaryOffHandWeapon.Set(unit.Items[inactiveWeaponSlot + 1]);
        }
        else
        {
            secondaryWeaponGroup.gameObject.SetActive(value: false);
        }
        for (int i = 0; i < activeSkills.Count; i++)
        {
            if (i < unit.ActiveSkills.Count)
            {
                activeSkills[i].gameObject.SetActive(value: true);
                activeSkills[i].Set(unit.ActiveSkills[i], canLearnSkill: false);
            }
            else
            {
                activeSkills[i].gameObject.SetActive(value: false);
            }
        }
        for (int j = 0; j < passiveSkills.Count; j++)
        {
            if (j < unit.PassiveSkills.Count)
            {
                passiveSkills[j].gameObject.SetActive(value: true);
                passiveSkills[j].Set(unit.PassiveSkills[j], canLearnSkill: false);
            }
            else
            {
                passiveSkills[j].gameObject.SetActive(value: false);
            }
        }
        if (unit.Mutations.Count > 0)
        {
            mutationGroup.gameObject.SetActive(value: true);
            for (int k = 0; k < mutations.Count; k++)
            {
                if (k < unit.Mutations.Count)
                {
                    mutations[k].gameObject.SetActive(value: true);
                    mutations[k].Set(unit.Mutations[k]);
                }
                else
                {
                    mutations[k].gameObject.SetActive(value: false);
                }
            }
        }
        else
        {
            mutationGroup.gameObject.SetActive(value: false);
        }
    }

    private void SetCompareIcon(int statValue, int comparedValue, GameObject compareBetter, GameObject compareWorse)
    {
        if (statValue > comparedValue)
        {
            compareBetter.SetActive(value: true);
        }
        else if (statValue < comparedValue)
        {
            compareWorse.SetActive(value: true);
        }
    }
}
