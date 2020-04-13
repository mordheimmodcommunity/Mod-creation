using UnityEngine;
using UnityEngine.UI;

public class UIWheelDescription : MonoBehaviour
{
    public UIWheelController wheelController;

    public Image backgroundImage;

    public Image backgroundImageSmall;

    public Image titleBackgroundImage;

    public Image icon;

    public Text titleText;

    public Image mastery;

    public Text categoryText;

    public Text descriptionText;

    public Text nonAvailableText;

    public Sprite titleBgAvailable;

    public Sprite titleBgNonAvailable;

    public SkillDescModule skillDesc;

    public SpellDescModule spellDesc;

    public UIInventoryItemDescription itemDesc;

    public UIInventoryItem itemTitle;

    public void SetCurrentAction(WheelAction action)
    {
        ((Behaviour)(object)categoryText).enabled = true;
        ((Component)(object)backgroundImage).gameObject.SetActive(value: true);
        ((Component)(object)backgroundImageSmall).gameObject.SetActive(value: false);
        switch (action.category)
        {
            case UIWheelController.Category.BASE_ACTION:
            case UIWheelController.Category.ACTIVE_SKILL:
            case UIWheelController.Category.PASSIVE_SKILL:
            case UIWheelController.Category.STANCES:
                spellDesc.gameObject.SetActive(value: false);
                ((Component)(object)descriptionText).gameObject.SetActive(value: false);
                itemDesc.gameObject.SetActive(value: false);
                itemTitle.gameObject.SetActive(value: false);
                skillDesc.Set(action.action.skillData);
                break;
            case UIWheelController.Category.SPELLS:
                skillDesc.gameObject.SetActive(value: false);
                ((Component)(object)descriptionText).gameObject.SetActive(value: false);
                itemDesc.gameObject.SetActive(value: false);
                itemTitle.gameObject.SetActive(value: false);
                spellDesc.Set(action.action.skillData);
                break;
            case UIWheelController.Category.INVENTORY:
                ((Behaviour)(object)mastery).enabled = false;
                skillDesc.gameObject.SetActive(value: false);
                ((Component)(object)descriptionText).gameObject.SetActive(value: false);
                itemDesc.Set(action.item);
                itemTitle.Set(action.item);
                spellDesc.gameObject.SetActive(value: false);
                break;
        }
        if (action.action != null)
        {
            nonAvailableText.set_text(action.action.LocalizedNotAvailableReason);
        }
        else
        {
            nonAvailableText.set_text(string.Empty);
        }
        if (action.Available)
        {
            titleBackgroundImage.set_sprite(titleBgAvailable);
            ((Component)(object)nonAvailableText).gameObject.SetActive(value: false);
        }
        else
        {
            ((Component)(object)nonAvailableText).gameObject.SetActive(value: true);
            titleBackgroundImage.set_sprite(titleBgNonAvailable);
        }
    }

    public void SetCurrentCategory(UIWheelController.Category category)
    {
        ((Component)(object)nonAvailableText).gameObject.SetActive(value: false);
        icon.set_sprite(wheelController.innerWheelIcons[(int)category].icon.get_sprite());
        ((Graphic)titleText).set_color(Color.white);
        titleBackgroundImage.set_sprite(titleBgAvailable);
        ((Behaviour)(object)categoryText).enabled = false;
        ((Behaviour)(object)mastery).enabled = false;
        ((Component)(object)backgroundImage).gameObject.SetActive(value: false);
        ((Component)(object)backgroundImageSmall).gameObject.SetActive(value: true);
        if (category == UIWheelController.Category.INVENTORY)
        {
            Text obj = categoryText;
            string stringById = PandoraSingleton<LocalizationManager>.Instance.GetStringById("skill_category_title_" + category.ToLowerString(), wheelController.CurrentUnitController.unit.GetNumUsedItemSlot().ToConstantString(), wheelController.CurrentUnitController.unit.BackpackCapacity.ToConstantString());
            titleText.set_text(stringById);
            obj.set_text(stringById);
        }
        else
        {
            Text obj2 = categoryText;
            string stringById = PandoraSingleton<LocalizationManager>.Instance.GetStringById("skill_category_title_" + category.ToLowerString());
            titleText.set_text(stringById);
            obj2.set_text(stringById);
        }
        ((Component)(object)descriptionText).gameObject.SetActive(value: true);
        skillDesc.gameObject.SetActive(value: false);
        spellDesc.gameObject.SetActive(value: false);
        itemDesc.gameObject.SetActive(value: false);
        descriptionText.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("skill_category_desc_" + category.ToLowerString()));
    }
}
