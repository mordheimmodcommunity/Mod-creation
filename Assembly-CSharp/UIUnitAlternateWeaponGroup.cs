using UnityEngine;
using UnityEngine.UI;

public class UIUnitAlternateWeaponGroup : MonoBehaviour
{
	public Image icon;

	public Text nameText;

	public Text damage;

	public void Set(Item item)
	{
		if (item.Id != 0)
		{
			((Behaviour)(object)icon).enabled = true;
			icon.set_sprite(item.GetIcon());
			((Behaviour)(object)nameText).enabled = true;
			nameText.set_text(item.LocalizedName);
			if ((Object)(object)damage != null)
			{
				((Behaviour)(object)damage).enabled = true;
				if (item.DamageMin == 0 && item.DamageMax == 0)
				{
					damage.set_text(string.Empty);
				}
				else
				{
					damage.set_text(PandoraUtils.StringBuilder.Append(item.DamageMin.ToConstantString()).Append('-').Append(item.DamageMax.ToConstantString())
						.ToString());
				}
			}
		}
		else
		{
			((Behaviour)(object)icon).enabled = false;
			((Behaviour)(object)nameText).enabled = false;
			if ((Object)(object)damage != null)
			{
				((Behaviour)(object)damage).enabled = false;
			}
		}
	}
}
