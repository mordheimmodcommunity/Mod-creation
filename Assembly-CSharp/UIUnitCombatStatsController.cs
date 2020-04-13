using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIUnitCombatStatsController : UIUnitControllerChanged
{
    public Text nameText;

    public Image icon;

    public Image iconStar;

    public Text armor;

    public Text damage;

    public Text critChance;

    public Text nbBuffs;

    public Text nbDebuffs;

    public Text wounds;

    public List<Image> offensePoints;

    public List<Image> strategyPoints;

    public Sprite offensePointPreview;

    public Sprite offensePointConsumed;

    public Sprite strategyPointPreview;

    public Sprite strategyPointConsumed;

    public Slider hpBar;

    public Slider damageBar;

    private bool update;

    public bool UpdateAction
    {
        get;
        set;
    }

    protected virtual void Awake()
    {
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.UNIT_ATTRIBUTES_CHANGED, OnAttributesChanged);
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.CURRENT_UNIT_ACTION_CHANGED, OnActionChanged);
    }

    private void OnActionChanged()
    {
        UnitController y = PandoraSingleton<NoticeManager>.Instance.Parameters[0] as UnitController;
        if (base.CurrentUnitController != null && base.CurrentUnitController == y && base.CurrentUnitController.IsPlayed())
        {
            ActionStatus actionStatus = PandoraSingleton<NoticeManager>.Instance.Parameters[1] as ActionStatus;
            if (actionStatus != null)
            {
                damageBar.set_value((float)(base.CurrentUnitController.unit.CurrentWound - actionStatus.skillData.WoundsCostMin));
            }
            else
            {
                damageBar.set_value((float)base.CurrentUnitController.unit.CurrentWound);
            }
        }
    }

    private void OnAttributesChanged()
    {
        Unit unit = PandoraSingleton<NoticeManager>.Instance.Parameters[0] as Unit;
        if (base.CurrentUnitController != null && base.CurrentUnitController.unit == unit)
        {
            base.UpdateUnit = true;
            return;
        }
        UnitController y = PandoraSingleton<NoticeManager>.Instance.Parameters[0] as UnitController;
        if (base.CurrentUnitController != null && base.CurrentUnitController == y)
        {
            base.UpdateUnit = true;
        }
    }

    public float GetAverageDamage()
    {
        if (base.TargetUnitController != null && base.TargetUnitController.CurrentAction != null)
        {
            return (float)(base.TargetUnitController.CurrentAction.GetMinDamage() + base.TargetUnitController.CurrentAction.GetMinDamage()) / 2f;
        }
        return 0f;
    }

    public void AttributeChangedDestructible()
    {
        Destructible targetDestructible = base.TargetDestructible;
        nameText.set_text(targetDestructible.LocalizedName);
        icon.set_sprite(targetDestructible.imprintIcon);
        ((Behaviour)(object)iconStar).enabled = false;
        armor.set_text("0");
        damage.set_text("0-0");
        critChance.set_text("0%");
        nbBuffs.set_text("0");
        nbDebuffs.set_text("0");
        wounds.set_text($"{targetDestructible.CurrentWounds}/{targetDestructible.Data.Wounds}");
        hpBar.set_maxValue((float)targetDestructible.Data.Wounds);
        hpBar.set_minValue(0f);
        hpBar.set_value(Mathf.Max(0f, (float)targetDestructible.CurrentWounds - GetAverageDamage()));
        damageBar.set_maxValue((float)targetDestructible.Data.Wounds);
        damageBar.set_minValue(0f);
        damageBar.set_value((float)Mathf.Max(0, targetDestructible.CurrentWounds));
        for (int i = 0; i < offensePoints.Count; i++)
        {
            ((Behaviour)(object)offensePoints[i]).enabled = false;
        }
        for (int j = 0; j < strategyPoints.Count; j++)
        {
            ((Behaviour)(object)strategyPoints[j]).enabled = false;
        }
    }

    public void AttributesChanged()
    {
        Unit unit = base.CurrentUnitController.unit;
        nameText.set_text(unit.Name);
        icon.set_sprite(unit.GetIcon());
        switch (unit.GetUnitTypeId())
        {
            case UnitTypeId.LEADER:
                ((Behaviour)(object)iconStar).enabled = true;
                iconStar.set_sprite(PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("icn_leader", cached: true));
                break;
            case UnitTypeId.HERO_1:
            case UnitTypeId.HERO_2:
            case UnitTypeId.HERO_3:
                ((Behaviour)(object)iconStar).enabled = true;
                iconStar.set_sprite(PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("icn_heroes", cached: true));
                break;
            case UnitTypeId.IMPRESSIVE:
                ((Behaviour)(object)iconStar).enabled = true;
                iconStar.set_sprite(PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("icn_impressive", cached: true));
                break;
            default:
                ((Behaviour)(object)iconStar).enabled = false;
                break;
        }
        armor.set_text(Mathf.Clamp(unit.ArmorAbsorptionPerc, 0, Constant.GetInt(ConstantId.MAX_ROLL)).ToConstantPercString());
        damage.set_text(PandoraUtils.StringBuilder.Append(unit.GetWeaponDamageMin(null).ToConstantString()).Append('-').Append(unit.GetWeaponDamageMax(null).ToConstantString())
            .ToString());
        critChance.set_text(Mathf.Clamp((!unit.HasRange()) ? unit.CriticalMeleeAttemptRoll : unit.CriticalRangeAttemptRoll, 0, Constant.GetInt(ConstantId.MAX_ROLL)).ToConstantPercString());
        nbBuffs.set_text(base.CurrentUnitController.GetEffectTypeCount(EffectTypeId.BUFF).ToConstantString());
        nbDebuffs.set_text(base.CurrentUnitController.GetEffectTypeCount(EffectTypeId.DEBUFF).ToConstantString());
        wounds.set_text(PandoraUtils.StringBuilder.Append(unit.CurrentWound.ToConstantString()).Append('/').Append(unit.Wound.ToConstantString())
            .ToString());
        hpBar.set_maxValue((float)unit.Wound);
        hpBar.set_minValue(0f);
        hpBar.set_value(Mathf.Max(0f, (float)unit.CurrentWound - GetAverageDamage()));
        damageBar.set_maxValue((float)unit.Wound);
        damageBar.set_minValue(0f);
        damageBar.set_value((float)Mathf.Max(0, unit.CurrentWound));
        for (int i = 0; i < offensePoints.Count; i++)
        {
            if (i < unit.OffensePoints)
            {
                ((Behaviour)(object)offensePoints[i]).enabled = true;
                if (i >= unit.CurrentOffensePoints - unit.tempOffensePoints)
                {
                    offensePoints[i].set_overrideSprite(offensePointConsumed);
                }
                else if (base.CurrentUnitController.CurrentAction != null && i >= unit.CurrentOffensePoints - unit.tempOffensePoints - base.CurrentUnitController.CurrentAction.OffensePoints)
                {
                    offensePoints[i].set_overrideSprite(offensePointPreview);
                }
                else
                {
                    offensePoints[i].set_overrideSprite((Sprite)null);
                }
            }
            else
            {
                ((Behaviour)(object)offensePoints[i]).enabled = false;
            }
        }
        for (int j = 0; j < strategyPoints.Count; j++)
        {
            if (j < unit.StrategyPoints)
            {
                ((Behaviour)(object)strategyPoints[j]).enabled = true;
                if (j >= unit.CurrentStrategyPoints - unit.tempStrategyPoints)
                {
                    strategyPoints[j].set_overrideSprite(strategyPointConsumed);
                }
                else if (base.CurrentUnitController.CurrentAction != null && j >= unit.CurrentStrategyPoints - unit.tempStrategyPoints - base.CurrentUnitController.CurrentAction.StrategyPoints)
                {
                    strategyPoints[j].set_overrideSprite(strategyPointPreview);
                }
                else
                {
                    strategyPoints[j].set_overrideSprite((Sprite)null);
                }
            }
            else
            {
                ((Behaviour)(object)strategyPoints[j]).enabled = false;
            }
        }
    }

    protected override void OnUnitChanged()
    {
        if (base.TargetDestructible != null)
        {
            AttributeChangedDestructible();
        }
        else if (base.CurrentUnitController != null)
        {
            AttributesChanged();
        }
    }
}
