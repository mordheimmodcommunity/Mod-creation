using UnityEngine;
using UnityEngine.UI;

public class UIRetroactionAction : MonoBehaviour
{
    public Text unitName;

    public Text actionName;

    public Image actionIcon;

    public Image mastery;

    public RectTransform offset;

    public UIRetroactionResult result;

    private void Awake()
    {
        unitName.set_text(string.Empty);
        actionName.set_text(string.Empty);
    }

    public void Set(UnitController unitCtrlr)
    {
        unitName.set_text(unitCtrlr.unit.Name);
        actionName.set_text(unitCtrlr.currentActionData.name);
        ((Behaviour)(object)mastery).enabled = unitCtrlr.currentActionData.mastery;
        actionIcon.set_sprite(unitCtrlr.currentActionData.icon);
        if (actionIcon.get_sprite() == null)
        {
            actionIcon.set_sprite(unitCtrlr.unit.GetIcon());
        }
        result.gameObject.SetActive(value: false);
    }
}
