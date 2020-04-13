using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIUnitSlot : MonoBehaviour
{
    [HideInInspector]
    public ToggleEffects slot;

    public Image icon;

    public Image unitTypeIcon;

    public GameObject subIconObject;

    public Image subIcon;

    public Text subIconText;

    public GameObject subIconObject2;

    public Image subIcon2;

    public Text subIconText2;

    public GameObject icnUnspentSkill;

    public bool isImpressive;

    public Unit currentUnitAtSlot;

    public int slotTypeIndex;

    [HideInInspector]
    public CanvasGroup canvasGroup;

    private Action<int, Unit, bool> overCallback;

    private Action<int, Unit, bool> selectedCallback;

    private Action<int, Unit, bool> confirmedCallback;

    public List<GameObject> impressiveLinks;

    public bool isLocked;

    private void Awake()
    {
        slot = GetComponent<ToggleEffects>();
        canvasGroup = GetComponent<CanvasGroup>();
        slot.onSelect.AddListener(OnSelect);
        slot.onAction.AddListener(OnConfirm);
        slot.onPointerEnter.AddListener(OnOver);
    }

    private void OnOver()
    {
        if (overCallback != null)
        {
            overCallback(slotTypeIndex, currentUnitAtSlot, isImpressive);
        }
    }

    private void OnConfirm()
    {
        if (confirmedCallback != null)
        {
            confirmedCallback(slotTypeIndex, currentUnitAtSlot, isImpressive);
        }
    }

    private void OnSelect()
    {
        if (selectedCallback != null)
        {
            selectedCallback(slotTypeIndex, currentUnitAtSlot, isImpressive);
        }
    }

    public void Set(Unit unit, int index, Action<int, Unit, bool> over, Action<int, Unit, bool> selected, Action<int, Unit, bool> confirmed, bool showStatusIcon = true)
    {
        Set(index, over, selected, confirmed);
        currentUnitAtSlot = unit;
        if (unit != null)
        {
            ((Graphic)icon).set_color(Color.white);
            icon.set_overrideSprite(unit.GetIcon());
            if ((UnityEngine.Object)(object)unitTypeIcon != null)
            {
                switch (unit.GetUnitTypeId())
                {
                    case UnitTypeId.LEADER:
                        ((Behaviour)(object)unitTypeIcon).enabled = true;
                        unitTypeIcon.set_sprite(PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("icn_leader", cached: true));
                        break;
                    case UnitTypeId.HERO_1:
                    case UnitTypeId.HERO_2:
                    case UnitTypeId.HERO_3:
                        ((Behaviour)(object)unitTypeIcon).enabled = true;
                        unitTypeIcon.set_sprite(PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("icn_heroes", cached: true));
                        break;
                    case UnitTypeId.IMPRESSIVE:
                        ((Behaviour)(object)unitTypeIcon).enabled = true;
                        unitTypeIcon.set_sprite(PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("icn_impressive", cached: true));
                        break;
                    default:
                        ((Behaviour)(object)unitTypeIcon).enabled = false;
                        break;
                }
            }
            if (unit.GetActiveStatus() != 0)
            {
                if ((UnityEngine.Object)(object)subIcon != null)
                {
                    subIconObject.SetActive(showStatusIcon);
                    subIcon.set_sprite(unit.GetActiveStatusIcon());
                    ((Graphic)subIcon).set_color(unit.GetActiveStatusIconColor());
                    int activeStatusUnits = unit.GetActiveStatusUnits();
                    subIconText.set_text((activeStatusUnits != 0) ? activeStatusUnits.ToConstantString() : string.Empty);
                }
                if ((UnityEngine.Object)(object)subIcon2 != null)
                {
                    subIcon2.set_sprite(unit.GetSecondActiveStatusIcon());
                    ((Graphic)subIcon2).set_color(unit.GetSecondActiveStatusIconColor());
                    subIconObject2.SetActive(showStatusIcon && subIcon2.get_sprite() != null);
                    int secondActiveStatusUnits = unit.GetSecondActiveStatusUnits();
                    subIconText2.set_text((secondActiveStatusUnits != 0) ? secondActiveStatusUnits.ToConstantString() : string.Empty);
                }
            }
            else
            {
                if (subIconObject != null)
                {
                    subIconObject.SetActive(value: false);
                    subIconText.set_text(string.Empty);
                }
                if (subIconObject2 != null)
                {
                    subIconObject2.SetActive(value: false);
                    subIconText2.set_text(string.Empty);
                }
            }
            if (icnUnspentSkill != null)
            {
                icnUnspentSkill.gameObject.SetActive(unit.UnspentSkill > 0);
            }
        }
        else
        {
            ((Graphic)icon).set_color(Constant.GetColor(ConstantId.COLOR_GOLD));
            icon.set_overrideSprite((Sprite)null);
            if ((UnityEngine.Object)(object)unitTypeIcon != null)
            {
                ((Behaviour)(object)unitTypeIcon).enabled = false;
            }
            if ((UnityEngine.Object)(object)subIcon != null)
            {
                subIconObject.SetActive(value: false);
                subIconText.set_text(string.Empty);
            }
            if ((UnityEngine.Object)(object)subIcon2 != null)
            {
                subIconObject2.SetActive(value: false);
                subIconText2.set_text(string.Empty);
            }
        }
    }

    public void Set(int index, Action<int, Unit, bool> over, Action<int, Unit, bool> selected, Action<int, Unit, bool> confirmed)
    {
        slotTypeIndex = index;
        currentUnitAtSlot = null;
        overCallback = over;
        selectedCallback = selected;
        confirmedCallback = confirmed;
        base.gameObject.SetActive(value: true);
        icon.set_overrideSprite((Sprite)null);
        if ((UnityEngine.Object)(object)unitTypeIcon != null)
        {
            ((Behaviour)(object)unitTypeIcon).enabled = false;
        }
        if (subIconObject != null)
        {
            subIconObject.SetActive(value: false);
        }
        if (subIconObject2 != null)
        {
            subIconObject2.SetActive(value: false);
        }
        if (icnUnspentSkill != null)
        {
            icnUnspentSkill.gameObject.SetActive(value: false);
        }
        isLocked = false;
    }

    public void Deactivate()
    {
        ((Selectable)slot.toggle).set_interactable(false);
        if (icnUnspentSkill != null)
        {
            icnUnspentSkill.gameObject.SetActive(value: false);
        }
        canvasGroup.alpha = 0.33f;
    }

    public void Activate()
    {
        ((Selectable)slot.toggle).set_interactable(true);
        canvasGroup.alpha = 1f;
    }

    public void Lock(Sprite lockIcon)
    {
        ((Graphic)icon).set_color(Color.white);
        icon.set_overrideSprite(lockIcon);
        if ((UnityEngine.Object)(object)unitTypeIcon != null)
        {
            ((Behaviour)(object)unitTypeIcon).enabled = false;
        }
        if ((UnityEngine.Object)(object)subIcon != null)
        {
            subIconObject.SetActive(value: false);
            subIconText.set_text(string.Empty);
        }
        if ((UnityEngine.Object)(object)subIcon2 != null)
        {
            subIconText2.set_text(string.Empty);
            subIconObject2.SetActive(value: false);
        }
        isLocked = true;
        Deactivate();
    }

    public void ShowImpressiveLinks(bool show)
    {
        if (impressiveLinks != null)
        {
            for (int i = 0; i < impressiveLinks.Count; i++)
            {
                impressiveLinks[i].SetActive(show);
            }
        }
    }
}
