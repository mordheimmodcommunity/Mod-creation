using System.Collections.Generic;
using UnityEngine.UI;

public class UIUnitEnchantmentsController : UIUnitControllerChanged
{
	public UIUnitEnchantmentsContent content;

	private List<Enchantment> enchantments = new List<Enchantment>();

	public Text unitClass;

	private EnchantmentComparer enchantmentComparer;

	public bool showBuff;

	protected virtual void Awake()
	{
		PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.UNIT_ENCHANTMENTS_CHANGED, OnEnchantmentsChanged);
		enchantmentComparer = new EnchantmentComparer();
	}

	private void OnEnchantmentsChanged()
	{
		Unit unit = PandoraSingleton<NoticeManager>.Instance.Parameters[0] as Unit;
		if (base.CurrentUnitController != null && base.CurrentUnitController.unit == unit)
		{
			base.UpdateUnit = true;
		}
	}

	private void EnchantmentsChanged()
	{
		enchantments.Clear();
		if (!(base.CurrentUnitController != null))
		{
			return;
		}
		enchantments.AddRange(base.CurrentUnitController.unit.Enchantments);
		for (int i = 0; i < base.CurrentUnitController.unit.ActiveItems.Count; i++)
		{
			enchantments.AddRange(base.CurrentUnitController.unit.ActiveItems[i].Enchantments);
		}
		for (int j = 0; j < base.CurrentUnitController.unit.Injuries.Count; j++)
		{
			enchantments.AddRange(base.CurrentUnitController.unit.Injuries[j].Enchantments);
		}
		for (int k = 0; k < base.CurrentUnitController.unit.Mutations.Count; k++)
		{
			enchantments.AddRange(base.CurrentUnitController.unit.Mutations[k].Enchantments);
		}
		enchantments.Sort(enchantmentComparer);
		for (int l = 0; l < enchantments.Count; l++)
		{
			if (base.CurrentUnitController.CanShowEnchantment(enchantments[l], showBuff ? EffectTypeId.BUFF : EffectTypeId.DEBUFF))
			{
				content.Add(enchantments[l]);
			}
		}
		unitClass.set_text(base.CurrentUnitController.unit.LocalizedType);
		content.OnAddEnd();
	}

	protected override void OnUnitChanged()
	{
		EnchantmentsChanged();
	}
}
