using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class UnitStatsGroup : MonoBehaviour
{
	public AttributeId attributeId;

	public Image icon;

	public Text text;

	public Image buff;

	public Image debuff;

	public bool percent;

	private Unit currentUnit;

	private void Awake()
	{
		PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.UNIT_ATTRIBUTES_CHANGED, OnAttributesChanged);
	}

	private void OnAttributesChanged()
	{
		UnitController unitController = PandoraSingleton<NoticeManager>.Instance.Parameters[0] as UnitController;
		if (unitController != null && unitController.unit == currentUnit)
		{
			Set(currentUnit);
		}
	}

	public void Set(Unit unit)
	{
		currentUnit = unit;
		int attribute = unit.GetAttribute(attributeId);
		int num = 0;
		bool enabled = false;
		bool enabled2 = false;
		List<AttributeMod> orNull = unit.attributeModifiers.GetOrNull(attributeId);
		if (orNull != null)
		{
			for (int i = 0; i < orNull.Count; i++)
			{
				AttributeMod attributeMod = orNull[i];
				if (attributeMod.type == AttributeMod.Type.BUFF)
				{
					enabled = true;
					int num2 = num;
					AttributeMod attributeMod2 = orNull[i];
					num = num2 + attributeMod2.modifier;
					continue;
				}
				AttributeMod attributeMod3 = orNull[i];
				if (attributeMod3.type == AttributeMod.Type.DEBUFF)
				{
					enabled2 = true;
					int num3 = num;
					AttributeMod attributeMod4 = orNull[i];
					num = num3 + attributeMod4.modifier;
				}
			}
		}
		StringBuilder stringBuilder = PandoraUtils.StringBuilder;
		if (num == 0)
		{
			stringBuilder.Append(attribute.ToConstantString());
			if (percent)
			{
				stringBuilder.Append('%');
			}
		}
		else
		{
			if (num > 0)
			{
				stringBuilder.Append(PandoraSingleton<LocalizationManager>.Instance.GetStringById("color_green"));
			}
			else
			{
				stringBuilder.Append(PandoraSingleton<LocalizationManager>.Instance.GetStringById("color_red"));
			}
			stringBuilder.Append(attribute.ToConstantString());
			if (percent)
			{
				stringBuilder.Append('%');
			}
			stringBuilder.Append(PandoraSingleton<LocalizationManager>.Instance.GetStringById("color_end"));
			stringBuilder.Append('(');
			if (num > 0)
			{
				stringBuilder.Append('+');
			}
			stringBuilder.Append(num.ToConstantString());
			stringBuilder.Append(')');
		}
		text.set_text(stringBuilder.ToString());
		((Behaviour)(object)buff).enabled = enabled;
		((Behaviour)(object)debuff).enabled = enabled2;
	}

	private void LateUpdate()
	{
	}
}
