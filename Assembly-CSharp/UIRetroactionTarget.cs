using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIRetroactionTarget : MonoBehaviour
{
	public Text unitName;

	public Image unitIcon;

	public Image unitSubIcon;

	public Slider life;

	public Slider damage;

	public Text damageOrResult;

	public Text newStatus;

	public UIRetroactionResult check;

	public List<UIRetroactionResult> enchants;

	public RectTransform offset;

	private void Awake()
	{
		unitName.set_text(string.Empty);
		damageOrResult.set_text(string.Empty);
		newStatus.set_text(string.Empty);
	}

	public void AddEnchant(string name, EffectTypeId effectTypeId)
	{
		int num = 0;
		while (true)
		{
			if (num < enchants.Count)
			{
				if (string.IsNullOrEmpty(enchants[num].resultName.get_text()))
				{
					break;
				}
				num++;
				continue;
			}
			return;
		}
		if (effectTypeId == EffectTypeId.BUFF || effectTypeId == EffectTypeId.DEBUFF)
		{
			enchants[num].Set(name, effectTypeId == EffectTypeId.BUFF);
		}
		else
		{
			enchants[num].Set(name);
		}
	}

	public void Hide()
	{
		unitName.set_text(string.Empty);
		damageOrResult.set_text(string.Empty);
		newStatus.set_text(string.Empty);
		base.gameObject.SetActive(value: false);
		((Component)(object)life).gameObject.SetActive(value: false);
		((Behaviour)(object)unitIcon).enabled = true;
		check.Hide();
		for (int i = 0; i < enchants.Count; i++)
		{
			enchants[i].Hide();
		}
		DOTween.Kill((object)this, false);
	}

	public void SetDamage(Destructible dest, int startingWound, string damageText, bool critical)
	{
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Expected O, but got Unknown
		((Behaviour)(object)damageOrResult).enabled = false;
		if (critical)
		{
			damageOrResult.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("retro_critical", damageText));
		}
		else
		{
			damageOrResult.set_text(damageText);
		}
		((Component)(object)life).gameObject.SetActive(value: true);
		life.set_minValue(0f);
		life.set_maxValue((float)dest.Data.Wounds);
		life.set_value((float)startingWound);
		((Component)(object)damage).gameObject.SetActive(value: true);
		damage.set_minValue(0f);
		damage.set_maxValue((float)dest.Data.Wounds);
		damage.set_value((float)startingWound);
		Sequence val = TweenSettingsExtensions.Append(TweenSettingsExtensions.Append(TweenSettingsExtensions.SetTarget<Sequence>(DOTween.Sequence(), (object)this), (Tween)(object)ShortcutExtensions.DOValue(damage, (float)dest.CurrentWounds, 0.35f, true)), (Tween)(object)ShortcutExtensions.DOValue(life, (float)dest.CurrentWounds, 0.25f, true));
		if (dest.CurrentWounds <= 0)
		{
			TweenSettingsExtensions.OnComplete<Sequence>(val, (TweenCallback)(object)new TweenCallback(HideDamageBar));
		}
	}

	public void SetDamage(UnitController unit, int startingWound, string damageText, bool critical)
	{
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Expected O, but got Unknown
		((Behaviour)(object)damageOrResult).enabled = false;
		if (critical)
		{
			damageOrResult.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("retro_critical", damageText));
		}
		else
		{
			damageOrResult.set_text(damageText);
		}
		((Component)(object)life).gameObject.SetActive(value: true);
		life.set_minValue(0f);
		life.set_maxValue((float)unit.unit.Wound);
		life.set_value((float)startingWound);
		((Component)(object)damage).gameObject.SetActive(value: true);
		damage.set_minValue(0f);
		damage.set_maxValue((float)unit.unit.Wound);
		damage.set_value((float)startingWound);
		Sequence val = TweenSettingsExtensions.Append(TweenSettingsExtensions.Append(TweenSettingsExtensions.SetTarget<Sequence>(DOTween.Sequence(), (object)this), (Tween)(object)ShortcutExtensions.DOValue(damage, (float)unit.unit.CurrentWound, 0.35f, true)), (Tween)(object)ShortcutExtensions.DOValue(life, (float)unit.unit.CurrentWound, 0.25f, true));
		if (unit.unit.CurrentWound <= 0)
		{
			TweenSettingsExtensions.OnComplete<Sequence>(val, (TweenCallback)(object)new TweenCallback(HideDamageBar));
		}
	}

	private void HideDamageBar()
	{
		((Component)(object)damage).gameObject.SetActive(value: false);
	}

	public void SetOutcome(string effect, string damageEffect)
	{
		if (!string.IsNullOrEmpty(effect))
		{
			check.Set(effect);
		}
		if (!string.IsNullOrEmpty(damageEffect))
		{
			((Behaviour)(object)damageOrResult).enabled = false;
			damageOrResult.set_text(damageEffect);
		}
	}

	public void ShowOutcome()
	{
		if (!string.IsNullOrEmpty(damageOrResult.get_text()))
		{
			((Behaviour)(object)damageOrResult).enabled = true;
		}
	}

	public void SetTarget(Destructible dest)
	{
		base.gameObject.SetActive(value: true);
		unitName.set_text(dest.LocalizedName);
		if (dest.Owner == null)
		{
			((Graphic)unitIcon).set_color(Constant.GetColor(ConstantId.COLOR_ENEMY));
		}
		else if (dest.Owner.IsPlayed())
		{
			((Graphic)unitIcon).set_color(Constant.GetColor(ConstantId.COLOR_ALLY));
		}
		else if (dest.Owner.unit.IsMonster)
		{
			((Graphic)unitIcon).set_color(Constant.GetColor(ConstantId.COLOR_NEUTRAL));
		}
		else
		{
			((Graphic)unitIcon).set_color(Constant.GetColor(ConstantId.COLOR_ENEMY));
		}
		unitIcon.set_sprite(dest.imprintIcon);
		((Behaviour)(object)unitSubIcon).enabled = false;
		check.Hide();
		damageOrResult.set_text(string.Empty);
	}

	public void SetTarget(UnitController unit)
	{
		base.gameObject.SetActive(value: true);
		if (PandoraSingleton<MissionManager>.Instance.focusedUnit == unit)
		{
			unitName.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("retro_target_self"));
			((Graphic)unitIcon).set_color(Color.white);
		}
		else
		{
			unitName.set_text(unit.unit.Name);
		}
		if (unit.IsPlayed())
		{
			((Graphic)unitIcon).set_color(Constant.GetColor(ConstantId.COLOR_ALLY));
		}
		else if (unit.unit.IsMonster)
		{
			((Graphic)unitIcon).set_color(Constant.GetColor(ConstantId.COLOR_NEUTRAL));
		}
		else
		{
			((Graphic)unitIcon).set_color(Constant.GetColor(ConstantId.COLOR_ENEMY));
		}
		unitIcon.set_sprite(unit.unit.GetIcon());
		unitSubIcon.set_sprite(unit.unit.GetUnitTypeIcon());
		((Behaviour)(object)unitSubIcon).enabled = (unitSubIcon.get_sprite() != null);
		check.Hide();
		damageOrResult.set_text(string.Empty);
	}

	public void SetEnchantDamage(string enchant, int damage)
	{
		base.gameObject.SetActive(value: true);
		unitName.set_text(enchant);
		((Behaviour)(object)unitIcon).enabled = false;
		((Component)(object)life).gameObject.SetActive(value: false);
		check.Hide();
		((Behaviour)(object)damageOrResult).enabled = false;
		damageOrResult.set_text(damage.ToConstantString());
	}

	public void SetStatus(string status)
	{
		newStatus.set_text(status);
	}
}
