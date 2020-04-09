using UnityEngine;
using UnityEngine.UI;

public class UIInventoryItem : MonoBehaviour
{
	private const string quant = "x{0}";

	public Image icon;

	public Image enchantIcon;

	public Text title;

	public Text quantity;

	public GameObject sellSection;

	public Text cost;

	public Text rating;

	public GameObject sold;

	public Item item;

	public void Set(Item item, bool shop = false, bool buy = false, ItemId restrictedItemId = ItemId.NONE, bool flagSold = false)
	{
		this.item = item;
		icon.set_sprite(item.GetIcon());
		title.set_text(item.LocalizedName);
		if (item.Id == ItemId.NONE && restrictedItemId != 0)
		{
			Text obj = title;
			obj.set_text(obj.get_text() + "\n(" + Item.GetLocalizedName(restrictedItemId) + ")");
		}
		if (sold != null)
		{
			sold.gameObject.SetActive(item.Id != 0 && flagSold && !item.IsSold());
		}
		ItemQualityData itemQualityData = item.QualityData;
		if (item.IsRecipe)
		{
			if (item.TypeData.Id == ItemTypeId.RECIPE_ENCHANTMENT_NORMAL)
			{
				itemQualityData = PandoraSingleton<DataFactory>.Instance.InitData<ItemQualityData>(2);
			}
			else if (item.TypeData.Id == ItemTypeId.RECIPE_ENCHANTMENT_MASTERY)
			{
				itemQualityData = PandoraSingleton<DataFactory>.Instance.InitData<ItemQualityData>(3);
			}
		}
		Color color = PandoraUtils.StringToColor(itemQualityData.Color);
		((Graphic)title).set_color(color);
		((Graphic)icon).set_color(color);
		if ((Object)(object)enchantIcon != null)
		{
			Sprite runeIcon = item.GetRuneIcon();
			((Component)(object)enchantIcon).gameObject.SetActive(item.RuneMark != null || runeIcon != null);
			if (runeIcon != null)
			{
				enchantIcon.set_sprite(runeIcon);
			}
		}
		if ((Object)(object)quantity != null)
		{
			((Component)(object)quantity).gameObject.SetActive(!flagSold);
		}
		UpdateQuantity();
		if (sellSection != null)
		{
			sellSection.SetActive(shop);
		}
		if (shop && (Object)(object)cost != null)
		{
			int num = (!buy) ? PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetItemSellPrice(item) : PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetItemBuyPrice(item);
			cost.set_text(num.ToConstantString());
			if (buy && PandoraSingleton<HideoutManager>.Instance.WarbandChest.GetGold() < num)
			{
				((Graphic)cost).set_color(Color.red);
			}
		}
		if ((Object)(object)rating != null)
		{
			((Component)(object)rating).transform.parent.gameObject.SetActive(!flagSold);
			rating.set_text(item.GetRating().ToConstantString());
		}
	}

	public bool UpdateQuantity()
	{
		if ((Object)(object)quantity != null)
		{
			quantity.set_text((item.Id != ItemId.GOLD && item.Save.amount != 0) ? $"x{item.Save.amount}" : string.Empty);
		}
		return item.Save.amount > 0;
	}
}
