using UnityEngine;
using UnityEngine.UI;

public class UIEngameMVUStats : MonoBehaviour
{
	public Image unitIcon;

	public Image unitTypeIcon;

	public Text unitName;

	public Text points;

	public GameObject[] categoryPointsGo;

	public Text[] categoryPoints;

	public void Set(UnitController unitController)
	{
		if (unitController == null)
		{
			((Behaviour)(object)unitIcon).enabled = false;
			((Behaviour)(object)unitName).enabled = false;
			((Behaviour)(object)points).enabled = false;
			for (int i = 0; i < categoryPointsGo.Length; i++)
			{
				categoryPointsGo[i].SetActive(value: false);
			}
			return;
		}
		unitIcon.set_sprite(unitController.unit.GetIcon());
		switch (unitController.unit.GetUnitTypeId())
		{
		case UnitTypeId.LEADER:
			((Behaviour)(object)unitTypeIcon).enabled = true;
			unitTypeIcon.set_sprite(PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("icn_leader", cached: true));
			break;
		case UnitTypeId.HERO_1:
		case UnitTypeId.HERO_2:
		case UnitTypeId.HERO_3:
			((Behaviour)(object)unitTypeIcon).enabled = true;
			unitTypeIcon.set_sprite(PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("icn_heroes", cached: true));
			break;
		case UnitTypeId.IMPRESSIVE:
			((Behaviour)(object)unitTypeIcon).enabled = true;
			unitTypeIcon.set_sprite(PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("icn_impressive", cached: true));
			break;
		default:
			((Behaviour)(object)unitTypeIcon).enabled = false;
			break;
		}
		unitName.set_text(unitController.unit.Name);
		points.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_mvu_pts", unitController.unit.GetAttribute(AttributeId.CURRENT_MVU).ToConstantString()));
		for (int j = 0; j < unitController.MVUptsPerCategory.Length; j++)
		{
			if (unitController.MVUptsPerCategory[j] != 0)
			{
				categoryPointsGo[j].SetActive(value: true);
				categoryPoints[j].set_text(unitController.MVUptsPerCategory[j].ToString("+#;-#"));
			}
			else
			{
				categoryPointsGo[j].SetActive(value: false);
			}
		}
	}
}
