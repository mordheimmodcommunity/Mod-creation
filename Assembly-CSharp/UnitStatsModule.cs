using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitStatsModule : UIModule
{
    public HightlightAnimate highlight;

    public ToggleGroup toggleGroup;

    public Text martialStatPoints;

    public Text mentalStatPoints;

    public Text physicalStatPoints;

    public List<UIStat> stats;

    private Unit currentUnit;

    private Action onAttributeChanged;

    public void RefreshStats(Selectable right, Unit unit, Action<AttributeId> onAttributeSelected = null, Action attributeChanged = null, Action<AttributeId> onAttributeUnselected = null)
    {
        //IL_0048: Unknown result type (might be due to invalid IL or missing references)
        //IL_004d: Unknown result type (might be due to invalid IL or missing references)
        //IL_006c: Unknown result type (might be due to invalid IL or missing references)
        onAttributeChanged = attributeChanged;
        currentUnit = unit;
        RefreshUnspentPoints();
        for (int i = 0; i < stats.Count; i++)
        {
            if (stats[i].canGoRight)
            {
                Navigation navigation = ((Selectable)stats[i].statSelector.toggle).get_navigation();
                ((Navigation)(ref navigation)).set_selectOnRight(right);
                ((Selectable)stats[i].statSelector.toggle).set_navigation(navigation);
            }
            stats[i].Refresh(unit, showArrows: true, onAttributeSelected, OnStatChanged, onAttributeUnselected);
        }
    }

    public void RefreshStats(Unit unit, Action<AttributeId> onAttributeSelected = null, Action<AttributeId> onAttributeUnselected = null)
    {
        currentUnit = unit;
        RefreshUnspentPoints();
        for (int i = 0; i < stats.Count; i++)
        {
            stats[i].Refresh(unit, showArrows: false, onAttributeSelected, OnStatChanged, onAttributeUnselected);
        }
    }

    private void RefreshUnspentPoints()
    {
        if (currentUnit.UnspentMartial > 0)
        {
            ((Component)(object)martialStatPoints).gameObject.SetActive(value: true);
            martialStatPoints.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_msg_unspent_points", currentUnit.UnspentMartial.ToString()));
        }
        else
        {
            ((Component)(object)martialStatPoints).gameObject.SetActive(value: false);
        }
        if (currentUnit.UnspentMental > 0)
        {
            ((Component)(object)mentalStatPoints).gameObject.SetActive(value: true);
            mentalStatPoints.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_msg_unspent_points", currentUnit.UnspentMental.ToString()));
        }
        else
        {
            ((Component)(object)mentalStatPoints).gameObject.SetActive(value: false);
        }
        if (currentUnit.UnspentPhysical > 0)
        {
            ((Component)(object)physicalStatPoints).gameObject.SetActive(value: true);
            physicalStatPoints.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_msg_unspent_points", currentUnit.UnspentPhysical.ToString()));
        }
        else
        {
            ((Component)(object)physicalStatPoints).gameObject.SetActive(value: false);
        }
    }

    public void RefreshAttributes(Unit unit)
    {
        currentUnit = unit;
        RefreshUnspentPoints();
        for (int i = 0; i < stats.Count; i++)
        {
            stats[i].RefreshAttribute(unit);
        }
    }

    private void OnStatChanged(AttributeId attributeId, bool increase)
    {
        if (increase)
        {
            currentUnit.RaiseAttribute(attributeId);
        }
        else
        {
            currentUnit.LowerAttribute(attributeId);
        }
        RefreshAttributes(currentUnit);
        if (onAttributeChanged != null)
        {
            onAttributeChanged();
        }
    }

    private void OnEnable()
    {
        if ((UnityEngine.Object)(object)toggleGroup != null)
        {
            toggleGroup.SetAllTogglesOff();
        }
        highlight.Deactivate();
    }

    public void SelectStat(AttributeId attributeId)
    {
        int num = 0;
        while (true)
        {
            if (num < stats.Count)
            {
                if (stats[num].statId == attributeId)
                {
                    break;
                }
                num++;
                continue;
            }
            return;
        }
        stats[num].statSelector.SetOn();
    }

    public void SetNav(Selectable right)
    {
        //IL_0033: Unknown result type (might be due to invalid IL or missing references)
        //IL_0038: Unknown result type (might be due to invalid IL or missing references)
        //IL_0057: Unknown result type (might be due to invalid IL or missing references)
        for (int i = 0; i < stats.Count; i++)
        {
            if (stats[i].canGoRight)
            {
                Navigation navigation = ((Selectable)stats[i].statSelector.toggle).get_navigation();
                ((Navigation)(ref navigation)).set_selectOnRight(right);
                ((Selectable)stats[i].statSelector.toggle).set_navigation(navigation);
            }
        }
    }
}
