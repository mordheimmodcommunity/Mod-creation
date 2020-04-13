using System;
using System.Text;
using UnityEngine;

public class UIStatIncrease : UIStat
{
    public int increase;

    public GameObject arrows;

    public ToggleEffects up;

    public ToggleEffects down;

    public Action<AttributeId, bool> statChangedCallback;

    private bool isShowArrows;

    protected override void Awake()
    {
        base.Awake();
        up.onAction.AddListener(IncreateStat);
        down.onAction.AddListener(DecreateStat);
    }

    public override void Refresh(Unit unit, bool showArrows, Action<AttributeId> statSelected, Action<AttributeId, bool> statChanged, Action<AttributeId> statUnselected)
    {
        isShowArrows = showArrows;
        base.Refresh(unit, showArrows, statSelected, statChanged, statUnselected);
        statChangedCallback = statChanged;
        RefreshArrows(unit);
    }

    public override void RefreshAttribute(Unit unit)
    {
        base.RefreshAttribute(unit);
        RefreshArrows(unit);
    }

    private void RefreshArrows(Unit unit)
    {
        if (isShowArrows && unit.CanRaiseAttribute(statId))
        {
            up.gameObject.SetActive(value: true);
        }
        else
        {
            up.gameObject.SetActive(value: false);
        }
        if (isShowArrows && unit.CanLowerAttribute(statId))
        {
            down.gameObject.SetActive(value: true);
        }
        else
        {
            down.gameObject.SetActive(value: false);
        }
    }

    private void IncreateStat()
    {
        statSelector.SetSelected();
        statChangedCallback(statId, arg2: true);
        statSelectedCallback(statId);
    }

    private void DecreateStat()
    {
        statSelector.SetSelected();
        statChangedCallback(statId, arg2: false);
        statSelectedCallback(statId);
    }

    protected override string GenerateStatsText(Unit unit, int stat, int? statMax)
    {
        int num = unit.GetBaseAttribute(statId) + unit.GetTempAttribute(statId);
        int attribute = unit.GetAttribute(statId);
        int baseAttribute = unit.GetBaseAttribute(unit.GetMaxAttribute(statId));
        int attribute2 = unit.GetAttribute(unit.GetMaxAttribute(statId));
        StringBuilder stringBuilder = PandoraUtils.StringBuilder;
        bool flag = unit.GetTempAttribute(statId) > 0;
        if (flag)
        {
            stringBuilder.AppendFormat("{0}<b>{1}</b>{2}", PandoraSingleton<LocalizationManager>.Instance.GetStringById("color_cyan"), attribute, PandoraSingleton<LocalizationManager>.Instance.GetStringById("color_end"));
        }
        else
        {
            stringBuilder.Append(attribute);
        }
        stringBuilder.Append(' ');
        if (num != attribute)
        {
            if (flag)
            {
                stringBuilder.AppendFormat("({0}<b>{1}</b>{2}) ", PandoraSingleton<LocalizationManager>.Instance.GetStringById("color_cyan"), num, PandoraSingleton<LocalizationManager>.Instance.GetStringById("color_end"));
            }
            else
            {
                stringBuilder.AppendFormat("({0}) ", num);
            }
        }
        stringBuilder.Append("/ ");
        stringBuilder.Append(attribute2);
        stringBuilder.Append(' ');
        if (baseAttribute != attribute2)
        {
            stringBuilder.AppendFormat("({0})", baseAttribute);
        }
        return stringBuilder.ToString();
    }
}
