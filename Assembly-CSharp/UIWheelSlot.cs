using UnityEngine;
using UnityEngine.UI;

public class UIWheelSlot : MonoBehaviour
{
    [HideInInspector]
    public ToggleEffects slot;

    public Image icon;

    public Image subIcon;

    public ToggleEffects defaultUpItem;

    public ToggleEffects defaultDownItem;

    public ToggleEffects defaultRightItem;

    public ToggleEffects defaultLeftItem;

    public ToggleEffects altUpItem;

    public ToggleEffects altDownItem;

    public ToggleEffects altRightItem;

    public ToggleEffects altLeftItem;

    private void Awake()
    {
        slot = GetComponent<ToggleEffects>();
    }

    public void SetLeftSelectable(ToggleEffects left)
    {
        if (defaultLeftItem == null)
        {
            defaultLeftItem = left;
        }
        else
        {
            altLeftItem = left;
        }
    }

    public void RefreshNavigation()
    {
        //IL_000b: Unknown result type (might be due to invalid IL or missing references)
        //IL_0010: Unknown result type (might be due to invalid IL or missing references)
        //IL_0080: Unknown result type (might be due to invalid IL or missing references)
        Navigation navigation = ((Selectable)slot.toggle).get_navigation();
        ((Navigation)(ref navigation)).set_selectOnUp(SetSelectable(defaultUpItem, altUpItem));
        ((Navigation)(ref navigation)).set_selectOnDown(SetSelectable(defaultDownItem, altDownItem));
        ((Navigation)(ref navigation)).set_selectOnRight(SetSelectable(defaultRightItem, altRightItem));
        ((Navigation)(ref navigation)).set_selectOnLeft(SetSelectable(defaultLeftItem, altLeftItem));
        ((Selectable)slot.toggle).set_navigation(navigation);
    }

    private Selectable SetSelectable(ToggleEffects defaultItem, ToggleEffects altItem)
    {
        if (defaultItem != null && defaultItem.isActiveAndEnabled && ((Selectable)defaultItem.toggle).get_interactable())
        {
            return (Selectable)(object)defaultItem.toggle;
        }
        if (altItem != null && altItem.isActiveAndEnabled && ((Selectable)altItem.toggle).get_interactable())
        {
            return (Selectable)(object)altItem.toggle;
        }
        return null;
    }
}
