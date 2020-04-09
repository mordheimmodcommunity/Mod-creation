using System;
using UnityEngine;
using UnityEngine.UI;

public class UIStat : MonoBehaviour
{
	public AttributeId statId;

	public ToggleEffects statSelector;

	public Text value;

	protected Action<AttributeId> statSelectedCallback;

	protected Action<AttributeId> statUnselectedCallback;

	public bool canGoRight = true;

	protected virtual void Awake()
	{
		if (statSelector != null)
		{
			statSelector.onSelect.AddListener(OnStatSelected);
			statSelector.onUnselect.AddListener(OnStatUnselected);
		}
	}

	public virtual void Refresh(Unit unit, bool showArrows, Action<AttributeId> statSelected, Action<AttributeId, bool> statChanged = null, Action<AttributeId> statUnselected = null)
	{
		statSelectedCallback = statSelected;
		statUnselectedCallback = statUnselected;
		RefreshAttribute(unit);
	}

	public virtual void RefreshAttribute(Unit unit)
	{
		AttributeId maxAttribute = unit.GetMaxAttribute(statId);
		if (maxAttribute == AttributeId.NONE)
		{
			value.set_text(GenerateStatsText(unit, unit.GetAttribute(statId), null));
		}
		else
		{
			value.set_text(GenerateStatsText(unit, unit.GetAttribute(statId), unit.GetAttribute(maxAttribute)));
		}
	}

	protected virtual string GenerateStatsText(Unit unit, int stat, int? statMax)
	{
		if (statMax.HasValue)
		{
			return PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_stat_max_value", stat.ToConstantString(), statMax.ToString());
		}
		AttributeData attributeData = PandoraSingleton<DataFactory>.Instance.InitData<AttributeData>((int)statId);
		if (unit.HasModifierType(statId, AttributeMod.Type.TEMP))
		{
			return PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_stat_increase_value", stat + ((!attributeData.IsPercent) ? string.Empty : "%"));
		}
		return stat + ((!attributeData.IsPercent) ? string.Empty : "%");
	}

	private void OnStatSelected()
	{
		if (statSelectedCallback != null)
		{
			statSelectedCallback(statId);
		}
	}

	private void OnStatUnselected()
	{
		if (statUnselectedCallback != null)
		{
			statUnselectedCallback(statId);
		}
	}

	public virtual bool HasChanges()
	{
		return false;
	}

	public virtual void ApplyChanges(Unit unit)
	{
	}

	public virtual void RevertChanges(Unit unit)
	{
	}
}
