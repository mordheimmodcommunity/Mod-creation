using UnityEngine;
using UnityEngine.UI;

public class FlyingTarget : FlyingText
{
	public Color enemyColor = Color.red;

	public Image icon;

	public Slider hp;

	public Slider damage;

	public void Play(UnitController unitCtrlr, UnitController target)
	{
		Vector3 position = target.BonesTr[BoneId.RIG_HEAD].position;
		position.y = target.CapsuleHeight;
		Play(position);
		if (target.Imprint.State == MapImprintStateId.VISIBLE)
		{
			icon.set_sprite(target.Imprint.visibleTexture);
			((Graphic)icon).set_color((!target.IsPlayed()) ? enemyColor : Color.white);
		}
		else if (target.Imprint.State == MapImprintStateId.LOST)
		{
			icon.set_sprite(target.Imprint.lostTexture);
		}
		((Component)(object)hp).gameObject.SetActive(value: true);
		((Component)(object)damage).gameObject.SetActive(value: true);
		Slider obj = damage;
		float minValue = 0f;
		hp.set_minValue(minValue);
		obj.set_minValue(minValue);
		Slider obj2 = damage;
		minValue = target.unit.Wound;
		hp.set_maxValue(minValue);
		obj2.set_maxValue(minValue);
		int minDamage = unitCtrlr.CurrentAction.GetMinDamage();
		int maxDamage = unitCtrlr.CurrentAction.GetMaxDamage();
		hp.set_value((float)target.unit.CurrentWound);
		damage.set_value((float)target.unit.CurrentWound - (float)(maxDamage + minDamage) / 2f);
	}

	public void Play(UnitController unitCtrlr, Destructible target)
	{
		Vector3 position = target.transform.position + Vector3.up * 1.75f;
		Play(position);
		if (target.Imprint.State == MapImprintStateId.VISIBLE)
		{
			icon.set_sprite(target.Imprint.visibleTexture);
			((Graphic)icon).set_color(enemyColor);
		}
		((Component)(object)hp).gameObject.SetActive(value: true);
		((Component)(object)damage).gameObject.SetActive(value: true);
		Slider obj = damage;
		float minValue = 0f;
		hp.set_minValue(minValue);
		obj.set_minValue(minValue);
		Slider obj2 = damage;
		minValue = target.Data.Wounds;
		hp.set_maxValue(minValue);
		obj2.set_maxValue(minValue);
		int minDamage = unitCtrlr.CurrentAction.GetMinDamage();
		int maxDamage = unitCtrlr.CurrentAction.GetMaxDamage();
		hp.set_value((float)target.CurrentWounds);
		damage.set_value((float)target.CurrentWounds - (float)(maxDamage + minDamage) / 2f);
	}
}
