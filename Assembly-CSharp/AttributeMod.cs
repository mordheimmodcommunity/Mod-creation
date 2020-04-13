using System;
using System.Text;
using UnityEngine;

public struct AttributeMod
{
    public enum Type
    {
        NONE,
        BASE,
        ATTRIBUTE,
        SKILL,
        ITEM,
        ENCHANTMENT,
        BUFF,
        DEBUFF,
        TEMP,
        INJURY,
        MUTATION
    }

    public const string TEMP_ATTRIBUTE_MODIFIER = "temp";

    private const string COUNT_TXT = " ({0})";

    private string reason;

    private string effect;

    public int modifier;

    private int? maxModifier;

    private bool isPercent;

    private bool showValue;

    public bool isEnemy;

    public Type type;

    public int count;

    private string generatedText;

    public AttributeMod(Type type, string reason, int modifier, string effect = null, bool isPercent = false, bool isNegative = false)
    {
        this.reason = reason;
        this.effect = effect;
        this.modifier = ((!isNegative) ? modifier : (-modifier));
        this.isPercent = isPercent;
        maxModifier = null;
        isEnemy = false;
        showValue = true;
        this.type = type;
        count = 1;
        generatedText = null;
    }

    public AttributeMod(Type type, string reason, int minModifier, int maxModifier)
    {
        this.reason = reason;
        effect = null;
        modifier = minModifier;
        this.maxModifier = maxModifier;
        isPercent = false;
        isEnemy = false;
        showValue = true;
        this.type = type;
        count = 1;
        generatedText = null;
    }

    public AttributeMod(string reason)
    {
        this = new AttributeMod(Type.NONE, reason, 0);
        showValue = false;
    }

    public string GetText(bool forcePercent)
    {
        if (!showValue)
        {
            return reason;
        }
        if (!string.IsNullOrEmpty(generatedText))
        {
            return generatedText;
        }
        StringBuilder stringBuilder = PandoraUtils.StringBuilder;
        if (isEnemy)
        {
            stringBuilder.Append("<color=#d75b5dff>");
        }
        if (!maxModifier.HasValue && reason != "attribute_base_roll")
        {
            if (modifier > 0)
            {
                stringBuilder.Append('+');
            }
            else if (modifier < 0)
            {
                stringBuilder.Append('-');
            }
        }
        stringBuilder.Append(Mathf.Abs(modifier).ToConstantString());
        if (maxModifier.HasValue)
        {
            stringBuilder.Append('-').Append(maxModifier.Value.ToConstantString());
        }
        if (forcePercent || isPercent || reason.EndsWith("perc", StringComparison.OrdinalIgnoreCase))
        {
            stringBuilder.Append('%');
        }
        if (effect != null)
        {
            stringBuilder.Append(' ').Append(effect);
            stringBuilder.Append(" (").Append(reason).Append(')');
        }
        else
        {
            stringBuilder.Append(' ').Append(reason);
        }
        if (count > 1)
        {
            stringBuilder.AppendFormat(" ({0})", count.ToConstantString());
        }
        if (isEnemy)
        {
            stringBuilder.Append("</color>");
        }
        generatedText = stringBuilder.ToString();
        return generatedText;
    }

    public AttributeMod Negate()
    {
        modifier = -modifier;
        return this;
    }

    public AttributeMod SetEffect(string effect)
    {
        this.effect = effect;
        return this;
    }

    public AttributeMod SetIsPercent(bool value)
    {
        isPercent = value;
        return this;
    }

    public AttributeMod SetEnemyMod(bool value)
    {
        isEnemy = value;
        return this;
    }

    public bool IsTemp()
    {
        return reason == "temp";
    }

    public bool IsSame(AttributeMod mod)
    {
        return effect == mod.effect && isPercent == mod.isPercent && isEnemy == mod.isEnemy && reason == mod.reason && type == mod.type;
    }
}
