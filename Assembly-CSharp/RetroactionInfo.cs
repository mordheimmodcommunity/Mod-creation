using System.Collections.Generic;

public class RetroactionInfo
{
    public UnitController unitCtrlr;

    public Destructible destructible;

    public UIRetroactionTarget playerUnitAction;

    public UIRetroactionTarget enemyUnitAction;

    private bool hasDamage;

    private bool damageShown;

    private string damageOrEffect;

    private bool isCritical;

    private string effect;

    private int startingWound;

    private int totalDamage;

    public List<Tuple<string, EffectTypeId, string>> enchants = new List<Tuple<string, EffectTypeId, string>>(8);

    private bool enchantShown;

    public UIRetroactionTarget RetroactionTarget => (!PandoraSingleton<MissionManager>.Instance.focusedUnit.IsPlayed()) ? enemyUnitAction : playerUnitAction;

    public void AddOutcome(string actionEffect, bool success, string damageEffect)
    {
        damageOrEffect = damageEffect;
        effect = actionEffect;
    }

    public void AddDamage(int damage, bool critical)
    {
        hasDamage = true;
        totalDamage += damage;
        damageOrEffect = (-totalDamage).ToConstantString();
        isCritical = critical;
    }

    public void AddEnchant(string enchantmentId, EffectTypeId effectTypeId, string effect)
    {
        bool flag = false;
        for (int i = 0; i < enchants.Count; i++)
        {
            if (enchants[i].Item1 == enchantmentId && enchants[i].Item2 == effectTypeId && enchants[i].Item3 == effect)
            {
                flag = true;
            }
        }
        if (!flag)
        {
            enchants.Add(new Tuple<string, EffectTypeId, string>(enchantmentId, effectTypeId, effect));
        }
    }

    public void Reset()
    {
        unitCtrlr = null;
        destructible = null;
        hasDamage = false;
        damageShown = false;
        damageOrEffect = null;
        isCritical = false;
        enchantShown = false;
        effect = null;
        playerUnitAction.Hide();
        enemyUnitAction.Hide();
        enchants.Clear();
        startingWound = 0;
        totalDamage = 0;
    }

    public void SetTarget(UnitController unitController)
    {
        unitCtrlr = unitController;
        startingWound = unitCtrlr.unit.CurrentWound;
        destructible = null;
    }

    public void SetTarget(Destructible dest)
    {
        unitCtrlr = null;
        destructible = dest;
        startingWound = destructible.CurrentWounds;
    }

    public void ShowTarget()
    {
        if (unitCtrlr != null)
        {
            RetroactionTarget.SetTarget(unitCtrlr);
        }
        else if (destructible != null)
        {
            RetroactionTarget.SetTarget(destructible);
        }
    }

    public void ShowEnchant()
    {
        if (!enchantShown)
        {
            enchantShown = true;
            for (int i = 0; i < enchants.Count; i++)
            {
                RetroactionTarget.AddEnchant(enchants[i].Item1, enchants[i].Item2);
            }
        }
    }

    public void ShowOutcome()
    {
        RetroactionTarget.SetOutcome(effect, damageOrEffect);
        if (hasDamage && !damageShown)
        {
            damageShown = true;
            if (destructible != null)
            {
                RetroactionTarget.SetDamage(destructible, startingWound, damageOrEffect, isCritical);
            }
            else if (unitCtrlr != null)
            {
                RetroactionTarget.SetDamage(unitCtrlr, startingWound, damageOrEffect, isCritical);
            }
        }
        RetroactionTarget.ShowOutcome();
    }

    public void ShowStatus(string status)
    {
        RetroactionTarget.SetStatus(status);
    }
}
