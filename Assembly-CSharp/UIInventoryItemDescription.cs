using UnityEngine;
using UnityEngine.UI;

public class UIInventoryItemDescription : MonoBehaviour
{
    public UIInventoryItemAttribute damageGroup;

    public UIInventoryItemAttribute speedGroup;

    public UIInventoryItemAttribute typeGroup;

    public UIInventoryItemAttribute rangeGroup;

    public UIInventoryItemAttribute armorGroup;

    public UIInventoryItemAttribute ratingGroup;

    public Text description;

    public UIInventoryRune runeBlock;

    public Text cantEquipReason;

    public void Set(Item item, UnitSlotId? unitSlotId = null)
    {
        if (item.Id == ItemId.NONE)
        {
            base.gameObject.SetActive(value: false);
            return;
        }
        base.gameObject.SetActive(value: true);
        if (item.DamageMin != 0 && item.DamageMax != 0)
        {
            damageGroup.Set($"{item.DamageMin}-{item.DamageMax}");
        }
        else
        {
            damageGroup.gameObject.SetActive(value: false);
        }
        if (item.SpeedData.Id != 0)
        {
            speedGroup.Set(item.SpeedData.Speed.ToString("+0;-0;0"));
        }
        else
        {
            speedGroup.gameObject.SetActive(value: false);
        }
        if (item.TypeData.Id != 0)
        {
            if (item.UnitSlots != null && item.UnitSlots.Count > 0)
            {
                switch (item.UnitSlots[0].UnitSlotId)
                {
                    case UnitSlotId.HELMET:
                        typeGroup.icon.set_overrideSprite(PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("item_slot/helmet", cached: true));
                        break;
                    case UnitSlotId.ARMOR:
                        typeGroup.icon.set_overrideSprite(PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("item_slot/armor", cached: true));
                        break;
                    case UnitSlotId.SET1_MAINHAND:
                    case UnitSlotId.SET1_OFFHAND:
                    case UnitSlotId.SET2_MAINHAND:
                    case UnitSlotId.SET2_OFFHAND:
                        if (item.IsTwoHanded)
                        {
                            typeGroup.icon.set_overrideSprite(PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("item_slot_two_handed", cached: true));
                        }
                        else
                        {
                            typeGroup.icon.set_overrideSprite(PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("item_slot_one_handed", cached: true));
                        }
                        break;
                }
                typeGroup.Set(PandoraSingleton<LocalizationManager>.Instance.GetStringById("item_type_" + item.TypeData.Name));
            }
            else
            {
                typeGroup.gameObject.SetActive(value: false);
            }
        }
        else
        {
            typeGroup.gameObject.SetActive(value: false);
        }
        if (item.RangeMax != 0)
        {
            rangeGroup.Set(item.RangeMax.ToConstantString());
        }
        else
        {
            rangeGroup.gameObject.SetActive(value: false);
        }
        if (item.ArmorAbsorption != 0)
        {
            armorGroup.Set(item.ArmorAbsorption.ToConstantString());
        }
        else
        {
            armorGroup.gameObject.SetActive(value: false);
        }
        if (ratingGroup != null)
        {
            int rating = item.GetRating();
            if (rating != 0)
            {
                ratingGroup.Set(rating.ToConstantString());
            }
            else
            {
                ratingGroup.gameObject.SetActive(value: false);
            }
        }
        string localizedDescription = item.GetLocalizedDescription(unitSlotId);
        if (string.IsNullOrEmpty(localizedDescription))
        {
            ((Component)(object)description).gameObject.SetActive(value: false);
        }
        else
        {
            ((Component)(object)description).gameObject.SetActive(value: true);
            description.set_text(localizedDescription);
        }
        if ((Object)(object)cantEquipReason != null)
        {
            ((Component)(object)cantEquipReason).gameObject.SetActive(value: false);
        }
        if (runeBlock != null)
        {
            runeBlock.Set(item);
        }
    }
}
