using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CampSectionsModule : UIModule
{
    private bool selecting;

    public Sprite lockIcon;

    public Sprite redSplatter;

    public List<ToggleEffects> icons;

    public Image playerProgressionIcon;

    public UIDescription description;

    public GameObject unspentSkillPoints;

    public GameObject unspentVeteranPoints;

    public GameObject unpaidUnits;

    public Text unpaidUnitsCost;

    public GameObject deliveryDue;

    public Text deliveryDueText;

    private WarbandTabsModule warbandTabs;

    private UnityAction<int> onSelectCallback;

    private UnityAction<int> onUnselectCallback;

    private UnityAction<int> onConfirmCallback;

    public override void Init()
    {
        base.Init();
        playerProgressionIcon.set_sprite(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.GetDramatis().unit.GetIcon());
        warbandTabs = PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<WarbandTabsModule>(ModuleId.WARBAND_TABS);
        warbandTabs.onIconEnter = OnTabEnter;
    }

    private void OnTabEnter(TabIcon tabIcon)
    {
        if (base.isActiveAndEnabled && !selecting && tabIcon.nodeSlot != HideoutCamp.NodeSlot.CAMP && ((Selectable)icons[(int)tabIcon.nodeSlot].toggle).get_interactable())
        {
            icons[(int)tabIcon.nodeSlot].SetOn();
        }
    }

    public void Setup(UnityAction<int> onSelect, UnityAction<int> onUnselect, UnityAction<int> onConfirm)
    {
        selecting = false;
        onConfirmCallback = onConfirm;
        onUnselectCallback = onUnselect;
        onSelectCallback = onSelect;
        description.gameObject.SetActive(value: false);
        for (int i = 0; i < warbandTabs.icons.Count; i++)
        {
            TabIcon tabIcon = warbandTabs.icons[i];
            if (tabIcon.nodeSlot != HideoutCamp.NodeSlot.CAMP)
            {
                int idx = (int)tabIcon.nodeSlot;
                ToggleEffects toggleEffects = icons[idx];
                toggleEffects.onSelect.RemoveAllListeners();
                toggleEffects.onSelect.AddListener(delegate
                {
                    OnSelect(idx);
                });
                toggleEffects.onUnselect.RemoveAllListeners();
                toggleEffects.onUnselect.AddListener(delegate
                {
                    OnUnselect(idx);
                });
                if (tabIcon.available)
                {
                    toggleEffects.onAction.RemoveAllListeners();
                    toggleEffects.onAction.AddListener(delegate
                    {
                        OnConfirm(idx);
                    });
                    toggleEffects.toColor[0].set_color(Color.white);
                    ((Selectable)toggleEffects.toggle).get_image().set_overrideSprite((Sprite)null);
                }
                else
                {
                    ((Selectable)toggleEffects.toggle).get_image().set_overrideSprite(lockIcon);
                    toggleEffects.toColor[0].set_color(Color.red);
                    toggleEffects.onAction.RemoveAllListeners();
                }
            }
        }
        Warband warband = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband;
        bool active = false;
        List<UnitMenuController> unitCtrlrs = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.unitCtrlrs;
        for (int j = 0; j < unitCtrlrs.Count; j++)
        {
            if (unitCtrlrs[j].unit.UnspentSkill > 0)
            {
                active = true;
            }
        }
        unspentSkillPoints.gameObject.SetActive(active);
        unspentVeteranPoints.gameObject.SetActive(warband.GetPlayerSkillsAvailablePoints() > 0);
        if (warband.GetTotalTreatmentOwned() > 0)
        {
            unpaidUnits.SetActive(value: true);
            unpaidUnitsCost.set_text(warband.GetTotalTreatmentOwned().ToString());
        }
        else if (warband.GetTotalUpkeepOwned() > 0)
        {
            unpaidUnits.SetActive(value: true);
            unpaidUnitsCost.set_text(warband.GetTotalUpkeepOwned().ToString());
        }
        else
        {
            unpaidUnits.SetActive(value: false);
        }
        Tuple<int, EventLogger.LogEvent, int> tuple = warband.Logger.FindLastEvent(EventLogger.LogEvent.SHIPMENT_LATE);
        if (tuple != null && tuple.Item1 > PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate)
        {
            deliveryDue.SetActive(value: true);
            deliveryDueText.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_days_left", (tuple.Item1 - PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate).ToString()));
        }
        else if (tuple != null && tuple.Item1 == PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate)
        {
            deliveryDue.SetActive(value: true);
            deliveryDueText.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_no_days_left"));
        }
        else
        {
            deliveryDue.SetActive(value: false);
        }
    }

    public void Refresh(int selectedIdx, bool selected = true)
    {
        icons[selectedIdx].toggle.set_isOn(selected);
    }

    public void OnSelect(int idx)
    {
        if (onSelectCallback != null)
        {
            onSelectCallback(idx);
        }
        TabIcon tabIcon = warbandTabs.GetTabIcon((HideoutCamp.NodeSlot)idx);
        if (!tabIcon.available)
        {
            description.gameObject.SetActive(value: true);
            description.Set(tabIcon.titleText, tabIcon.reason + "_camp");
        }
        else
        {
            description.gameObject.SetActive(value: false);
        }
        selecting = true;
        warbandTabs.OnTabIconEnter(tabIcon);
        selecting = false;
    }

    public void OnUnselect(int idx)
    {
        if (onUnselectCallback != null)
        {
            onUnselectCallback(idx);
        }
        warbandTabs.OnTabIconExit(warbandTabs.GetTabIcon((HideoutCamp.NodeSlot)idx));
    }

    public void OnConfirm(int idx)
    {
        if (onConfirmCallback != null)
        {
            onConfirmCallback(idx);
        }
    }
}
