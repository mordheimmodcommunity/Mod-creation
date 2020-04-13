using UnityEngine;
using UnityEngine.UI;

public class UIInventoryRune : MonoBehaviour
{
    public Image icon;

    public Image redSplatter;

    public Image cannotEnchant;

    public Text title;

    public Text quality;

    public Text cost;

    public Text desc;

    public Text rating;

    public Text notFound;

    public void Set(RuneMark rune)
    {
        icon.set_sprite(rune.GetIcon());
        if ((Object)(object)title != null)
        {
            title.set_text(rune.FullLocName);
        }
        if ((Object)(object)quality != null)
        {
            quality.set_text(rune.LocQuality);
        }
        if ((Object)(object)redSplatter != null)
        {
            ((Behaviour)(object)redSplatter).enabled = !string.IsNullOrEmpty(rune.Reason);
        }
        if ((Object)(object)cannotEnchant != null)
        {
            ((Behaviour)(object)cannotEnchant).enabled = !string.IsNullOrEmpty(rune.Reason);
        }
        if ((Object)(object)notFound != null)
        {
            if (string.IsNullOrEmpty(rune.Reason))
            {
                ((Behaviour)(object)notFound).enabled = false;
            }
            else
            {
                ((Behaviour)(object)notFound).enabled = true;
                notFound.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById(rune.Reason));
            }
        }
        if ((Object)(object)cost != null)
        {
            ((Behaviour)(object)cost).enabled = true;
            cost.set_text(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetRuneMarkBuyPrice(rune).ToConstantString());
            if (PandoraSingleton<HideoutManager>.Instance.WarbandChest.GetGold() < PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetRuneMarkBuyPrice(rune))
            {
                ((Graphic)cost).set_color(Color.red);
            }
        }
        if ((Object)(object)rating != null)
        {
            rating.set_text(rune.QualityItemTypeData.Rating.ToConstantString());
        }
        if ((Object)(object)desc != null)
        {
            desc.set_text(rune.LocDesc);
        }
    }

    public void Set(Item item)
    {
        Sprite runeIcon = item.GetRuneIcon();
        if (runeIcon == null)
        {
            base.gameObject.SetActive(value: false);
            return;
        }
        base.gameObject.SetActive(value: true);
        icon.set_sprite(runeIcon);
        if ((Object)(object)title != null)
        {
            title.set_text(item.GetRuneNameDesc());
        }
    }
}
