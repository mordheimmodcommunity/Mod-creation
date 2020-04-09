using UnityEngine;
using UnityEngine.UI;

public class FlyingOverview : FlyingText
{
	public Color enemyColor = Color.red;

	public Color allyColor = Color.blue;

	public Color neutralColor = Color.magenta;

	public Image bgEnemy;

	public Image bgPlayer;

	public Image bgNeutral;

	public Image bgCurrentPlayer;

	public Image bgSelected;

	public Image bgWyrdstone;

	public Image bgSearchPoint;

	public Image bgBeacon;

	public Image bgTrap;

	public Image icon;

	public Image icon_ooa;

	public Image iconLeader;

	public Image iconBounty;

	public Image iconLost;

	public Image iconIdol;

	public Image iconSearched;

	public GameObject orientation;

	public Slider hp;

	public Slider damage;

	[HideInInspector]
	public MapImprint imprint;

	private void Awake()
	{
		orientation.SetActive(value: false);
	}

	public void Set(MapImprint imprint, bool clamp, bool selected)
	{
		this.imprint = imprint;
		Vector3 lastKnownPos = imprint.lastKnownPos;
		lastKnownPos.y += 2.75f;
		Play(lastKnownPos);
		switch (imprint.imprintType)
		{
		case MapImprintType.BEACON:
			base.transform.SetParent(PandoraSingleton<FlyingTextManager>.Instance.beaconContainer);
			break;
		case MapImprintType.UNIT:
			base.transform.SetParent(PandoraSingleton<FlyingTextManager>.Instance.unitContainer);
			if (imprint.UnitCtrlr != null && imprint.UnitCtrlr != PandoraSingleton<MissionManager>.Instance.GetCurrentUnit())
			{
				base.transform.SetAsFirstSibling();
			}
			break;
		case MapImprintType.PLAYER_DEPLOYMENT:
		case MapImprintType.ENEMY_DEPLOYMENT:
			base.transform.SetParent(PandoraSingleton<FlyingTextManager>.Instance.deploymentContainter);
			break;
		case MapImprintType.PLAYER_WAGON:
		case MapImprintType.ENEMY_WAGON:
		case MapImprintType.INTERACTIVE_POINT:
		case MapImprintType.WYRDSTONE:
		case MapImprintType.TRAP:
		case MapImprintType.DESTRUCTIBLE:
			base.transform.SetParent(PandoraSingleton<FlyingTextManager>.Instance.miscContainer);
			if (imprint.imprintType != MapImprintType.ENEMY_WAGON && imprint.imprintType != MapImprintType.PLAYER_WAGON)
			{
				base.transform.SetAsFirstSibling();
			}
			break;
		default:
			PandoraDebug.LogWarning("Unknown imprint type while setting imprint in FlyingOverview::Set", "UI", imprint);
			break;
		}
		bool flag = imprint.State == MapImprintStateId.VISIBLE || imprint.State == MapImprintStateId.LOST;
		if (((Behaviour)(object)icon).enabled != flag)
		{
			((Behaviour)(object)icon).enabled = flag;
		}
		icon.set_sprite(imprint.visibleTexture);
		if (imprint.UnitCtrlr != null)
		{
			((Graphic)icon).set_color(imprint.UnitCtrlr.IsPlayed() ? allyColor : ((!imprint.UnitCtrlr.unit.IsMonster) ? enemyColor : neutralColor));
		}
		else if (imprint.imprintType == MapImprintType.PLAYER_DEPLOYMENT)
		{
			((Graphic)icon).set_color(allyColor);
		}
		else if (imprint.imprintType == MapImprintType.ENEMY_DEPLOYMENT)
		{
			((Graphic)icon).set_color(enemyColor);
		}
		else
		{
			((Graphic)icon).set_color(Color.white);
		}
		if (imprint.UnitCtrlr != null)
		{
			switch (imprint.UnitCtrlr.unit.GetUnitTypeId())
			{
			case UnitTypeId.LEADER:
				((Behaviour)(object)iconLeader).enabled = true;
				iconLeader.set_sprite(PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("icn_leader", cached: true));
				break;
			case UnitTypeId.HERO_1:
			case UnitTypeId.HERO_2:
			case UnitTypeId.HERO_3:
				((Behaviour)(object)iconLeader).enabled = true;
				iconLeader.set_sprite(PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("icn_heroes", cached: true));
				break;
			case UnitTypeId.IMPRESSIVE:
				((Behaviour)(object)iconLeader).enabled = true;
				iconLeader.set_sprite(PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("icn_impressive", cached: true));
				break;
			default:
				((Behaviour)(object)iconLeader).enabled = false;
				break;
			}
		}
		else
		{
			((Behaviour)(object)iconLeader).enabled = false;
		}
		bool flag2 = imprint.imprintType == MapImprintType.TRAP && imprint.Trap != null && PandoraSingleton<MissionManager>.Instance.GetMyWarbandCtrlr().teamIdx == imprint.Trap.TeamIdx;
		((Behaviour)(object)iconBounty).enabled = (imprint.UnitCtrlr != null && !imprint.UnitCtrlr.IsPlayed() && imprint.UnitCtrlr.IsBounty());
		((Behaviour)(object)iconLost).enabled = (imprint.UnitCtrlr != null && imprint.State == MapImprintStateId.LOST);
		iconIdol.set_sprite(imprint.idolTexture);
		((Behaviour)(object)iconIdol).enabled = (imprint.idolTexture != null);
		((Behaviour)(object)iconSearched).enabled = (imprint.Search != null && imprint.Search.wasSearched);
		((Graphic)iconSearched).set_color((!((Behaviour)(object)iconSearched).enabled || !imprint.Search.IsEmpty()) ? Color.white : Color.red);
		((Behaviour)(object)bgNeutral).enabled = (((Behaviour)(object)icon).enabled && imprint.UnitCtrlr != null && imprint.UnitCtrlr.unit.IsMonster);
		((Behaviour)(object)bgEnemy).enabled = ((((Behaviour)(object)icon).enabled && imprint.UnitCtrlr != null && !imprint.UnitCtrlr.IsPlayed() && !((Behaviour)(object)bgNeutral).enabled) || imprint.imprintType == MapImprintType.ENEMY_WAGON || imprint.imprintType == MapImprintType.ENEMY_DEPLOYMENT);
		((Behaviour)(object)bgPlayer).enabled = (((Behaviour)(object)icon).enabled && ((imprint.UnitCtrlr != null && imprint.UnitCtrlr.IsPlayed()) || imprint.imprintType == MapImprintType.PLAYER_WAGON || imprint.imprintType == MapImprintType.PLAYER_DEPLOYMENT));
		((Behaviour)(object)bgCurrentPlayer).enabled = (((Behaviour)(object)icon).enabled && imprint.UnitCtrlr != null && PandoraSingleton<MissionManager>.Instance.GetCurrentUnit() == imprint.UnitCtrlr);
		((Behaviour)(object)bgSelected).enabled = selected;
		((Behaviour)(object)bgWyrdstone).enabled = (((Behaviour)(object)icon).enabled && (imprint.imprintType == MapImprintType.WYRDSTONE || (!flag2 && imprint.imprintType == MapImprintType.TRAP && imprint.Trap.enemyImprintType == MapImprintType.WYRDSTONE)));
		((Behaviour)(object)bgSearchPoint).enabled = (((Behaviour)(object)icon).enabled && (imprint.imprintType == MapImprintType.INTERACTIVE_POINT || imprint.imprintType == MapImprintType.DESTRUCTIBLE || (!flag2 && imprint.imprintType == MapImprintType.TRAP && imprint.Trap.enemyImprintType == MapImprintType.INTERACTIVE_POINT)));
		((Behaviour)(object)bgBeacon).enabled = (((Behaviour)(object)icon).enabled && imprint.imprintType == MapImprintType.BEACON);
		((Behaviour)(object)bgTrap).enabled = (((Behaviour)(object)icon).enabled && imprint.imprintType == MapImprintType.TRAP && flag2);
		if (((Behaviour)(object)bgBeacon).enabled)
		{
			((Graphic)icon).set_color(((Graphic)bgBeacon).get_color());
		}
		orientation.SetActive(imprint.UnitCtrlr != null && imprint.State == MapImprintStateId.VISIBLE && PandoraSingleton<MissionManager>.Instance.GetCurrentUnit() == imprint.UnitCtrlr);
		clamped = (orientation.activeSelf || clamp);
		if (imprint.State == MapImprintStateId.VISIBLE && (imprint.UnitCtrlr != null || imprint.Destructible != null))
		{
			int num = (!(imprint.UnitCtrlr != null)) ? imprint.Destructible.Data.Wounds : imprint.UnitCtrlr.unit.Wound;
			int num2 = (!(imprint.UnitCtrlr != null)) ? imprint.Destructible.CurrentWounds : imprint.UnitCtrlr.unit.CurrentWound;
			((Behaviour)(object)icon_ooa).enabled = ((!(imprint.UnitCtrlr != null)) ? (imprint.Destructible.CurrentWounds == 0) : (imprint.UnitCtrlr.unit.Status == UnitStateId.OUT_OF_ACTION));
			((Component)(object)hp).gameObject.SetActive(value: true);
			((Component)(object)damage).gameObject.SetActive(value: true);
			if (num2 > 0)
			{
				damage.get_fillRect().gameObject.SetActive(value: true);
				hp.get_fillRect().gameObject.SetActive(value: true);
				((Behaviour)(object)hp).enabled = true;
				Slider obj = damage;
				float minValue = 0f;
				hp.set_minValue(minValue);
				obj.set_minValue(minValue);
				Slider obj2 = damage;
				minValue = num;
				hp.set_maxValue(minValue);
				obj2.set_maxValue(minValue);
				hp.set_value((float)num2);
				damage.set_value((float)num2);
			}
			else
			{
				damage.get_fillRect().gameObject.SetActive(value: false);
				hp.get_fillRect().gameObject.SetActive(value: false);
			}
		}
		else
		{
			((Behaviour)(object)icon_ooa).enabled = false;
			((Component)(object)hp).gameObject.SetActive(value: false);
			((Component)(object)damage).gameObject.SetActive(value: false);
		}
	}

	public override void Deactivate()
	{
		base.Deactivate();
	}

	private void Update()
	{
		if (orientation.activeSelf)
		{
			Transform transform = orientation.transform;
			Vector3 eulerAngles = PandoraSingleton<MissionManager>.Instance.CamManager.transform.rotation.eulerAngles;
			float y = eulerAngles.y;
			Vector3 eulerAngles2 = imprint.UnitCtrlr.transform.rotation.eulerAngles;
			transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, y - eulerAngles2.y));
		}
	}
}
