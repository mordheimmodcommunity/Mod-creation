using Prometheus;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ZoneAoe : MonoBehaviour
{
	private const float BASE_HEIGHT = 1f;

	public bool autoInit;

	public bool indestructible;

	public bool once;

	public bool autoDestroyOnFxDone;

	public ZoneAoeId zoneAoeId;

	public string fxName;

	private ZoneAoeData data;

	private List<ZoneAoeEnchantmentData> enchantmentsData;

	private List<AoeUnitChecker> unitsToCheck = new List<AoeUnitChecker>();

	private float radius;

	private float height;

	private Vector3 offset = Vector3.zero;

	private readonly List<UnitController> affectedUnits = new List<UnitController>();

	private List<UnitController> allies;

	private List<UnitController> enemies;

	public int durationLeft;

	private bool usedOnce;

	private readonly List<Bounds> boxBounds = new List<Bounds>();

	private OlympusFire fx;

	private bool fxPresent;

	[HideInInspector]
	public uint guid;

	[HideInInspector]
	public Destructible destructible;

	public bool Initialized
	{
		get;
		private set;
	}

	public UnitController Owner
	{
		get;
		private set;
	}

	public string Name => data.Name;

	public static ZoneAoe Spawn(UnitController ctrlr, ActionStatus action, Action<ZoneAoe> cb = null)
	{
		if (action != null && action.ZoneAoeId != 0)
		{
			Spawn(action.ZoneAoeId, action.Radius, ctrlr.currentSpellTargetPosition, ctrlr, register: true, cb);
		}
		return null;
	}

	public static void Spawn(ZoneAoeId zoneId, float radius, Vector3 position, UnitController caster, bool register = true, Action<ZoneAoe> cb = null)
	{
		PandoraSingleton<AssetBundleLoader>.Instance.LoadAssetAsync<GameObject>("Assets/prefabs/zone_aoe/", AssetBundleId.FX, zoneId.ToLowerString() + ".prefab", delegate(UnityEngine.Object prefab)
		{
			if (!(prefab == null))
			{
				GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)prefab);
				gameObject.transform.position = position;
				gameObject.transform.rotation = Quaternion.identity;
				ZoneAoe component = gameObject.GetComponent<ZoneAoe>();
				component.Init(zoneId, caster, radius);
				int damageMin = 0;
				int damageMax = 0;
				component.GetDamages(out damageMin, out damageMax);
				if (damageMax > 0)
				{
					for (int i = 0; i < PandoraSingleton<MissionManager>.Instance.triggerPoints.Count; i++)
					{
						if (PandoraSingleton<MissionManager>.Instance.triggerPoints[i] is Destructible)
						{
							Destructible destructible = (Destructible)PandoraSingleton<MissionManager>.Instance.triggerPoints[i];
							if (destructible.IsInRange(position, component.radius))
							{
								int damage = PandoraSingleton<MissionManager>.Instance.NetworkTyche.Rand(damageMin, damageMax);
								destructible.ApplyDamage(damage);
								destructible.Hit(caster);
							}
						}
					}
				}
				if (register)
				{
					PandoraSingleton<MissionManager>.Instance.MissionEndData.UpdateAoe(component.guid, caster.uid, component.zoneAoeId, radius, component.durationLeft, position);
				}
				if (cb != null)
				{
					cb(component);
				}
			}
		});
	}

	public void AutoInit()
	{
		if (autoInit)
		{
			autoInit = false;
			Activate();
			usedOnce = false;
		}
		else
		{
			Deactivate();
		}
	}

	private void Update()
	{
		if (!Initialized)
		{
			return;
		}
		if (autoDestroyOnFxDone && fxPresent && fx == null)
		{
			autoDestroyOnFxDone = false;
			fxPresent = false;
			Deactivate();
		}
		for (int num = unitsToCheck.Count - 1; num >= 0; num--)
		{
			if (unitsToCheck[num].ctrlr.transform.position != unitsToCheck[num].lastPos)
			{
				if (CheckUnit(unitsToCheck[num].ctrlr, network: true))
				{
					unitsToCheck.RemoveAt(num);
				}
				else
				{
					unitsToCheck[num].lastPos = unitsToCheck[num].ctrlr.transform.position;
				}
			}
		}
	}

	public void Init(ZoneAoeId zoneId, UnitController caster, float radius, float height = 1f)
	{
		zoneAoeId = zoneId;
		PandoraSingleton<MissionManager>.Instance.RegisterZoneAoe(this);
		data = PandoraSingleton<DataFactory>.Instance.InitData<ZoneAoeData>((int)zoneId);
		durationLeft = data.Duration;
		enchantmentsData = PandoraSingleton<DataFactory>.Instance.InitData<ZoneAoeEnchantmentData>("fk_zone_aoe_id", ((int)zoneId).ToConstantString());
		enchantmentsData.Sort(new ZoneEnchantComparer());
		Owner = caster;
		if (Owner != null)
		{
			Owner.GetAlliesEnemies(out allies, out enemies);
		}
		this.radius = radius;
		this.height = height;
		BoxCollider[] componentsInChildren = GetComponentsInChildren<BoxCollider>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Bounds bounds = componentsInChildren[i].bounds;
			boxBounds.Add(bounds);
		}
		Initialized = true;
		List<UnitController> allAliveUnits = PandoraSingleton<MissionManager>.Instance.GetAllAliveUnits();
		if (allAliveUnits != null)
		{
			for (int j = 0; j < allAliveUnits.Count; j++)
			{
				CheckUnit(allAliveUnits[j], network: false);
			}
		}
		if (!string.IsNullOrEmpty(fxName))
		{
			PandoraSingleton<Prometheus.Prometheus>.Instance.SpawnFx(fxName, base.transform, attached: true, null);
		}
		if (autoDestroyOnFxDone)
		{
			fx = GetComponentInChildren<OlympusFire>();
			fxPresent = (fx != null);
			Collider componentInChildren = GetComponentInChildren<Collider>();
			componentInChildren.enabled = false;
		}
		guid = PandoraSingleton<MissionManager>.Instance.GetNextRTGUID();
	}

	public void Activate()
	{
		if (once && usedOnce)
		{
			return;
		}
		usedOnce = true;
		base.gameObject.SetActive(value: true);
		float num = 0f;
		float num2 = 1f;
		if (boxBounds.Count == 0)
		{
			Collider componentInChildren = GetComponentInChildren<Collider>();
			if (componentInChildren != null)
			{
				Bounds bounds = componentInChildren.bounds;
				Vector3 extents = bounds.extents;
				num = extents.x;
				Vector3 size = bounds.size;
				num2 = Mathf.Max(size.y, 0.5f);
				Vector3 center = bounds.center;
				float x = center.x;
				Vector3 min = bounds.min;
				float y = min.y;
				Vector3 center2 = bounds.center;
				offset = new Vector3(x, y, center2.z) - base.transform.position;
			}
		}
		Init(zoneAoeId, null, num, num2);
	}

	public void Deactivate()
	{
		if (!once || !usedOnce)
		{
			usedOnce = true;
			PandoraDebug.LogInfo("Destroying Aoe Zone : " + zoneAoeId);
			unitsToCheck.Clear();
			for (int num = affectedUnits.Count - 1; num >= 0; num--)
			{
				Trigger(affectedUnits[num], entry: false, network: false);
			}
			affectedUnits.Clear();
			Initialized = false;
			base.gameObject.SetActive(value: false);
		}
	}

	public void EnterZone(UnitController ctrlr)
	{
		if (!Initialized || !base.gameObject.activeSelf || ctrlr == null || (!PandoraSingleton<MissionManager>.Instance.IsCurrentPlayer() && PandoraSingleton<MissionManager>.Instance.transitionDone))
		{
			return;
		}
		PandoraDebug.LogInfo("Entering Aoe Zone : " + zoneAoeId + " for unit " + ctrlr.name);
		bool flag = false;
		for (int i = 0; i < unitsToCheck.Count; i++)
		{
			if (unitsToCheck[i].ctrlr == ctrlr)
			{
				flag = true;
			}
		}
		if (!flag)
		{
			unitsToCheck.Add(new AoeUnitChecker(ctrlr, Vector3.zero));
		}
	}

	public void ExitZone(UnitController ctrlr)
	{
		if (Initialized && base.gameObject.activeSelf && !(ctrlr == null) && (PandoraSingleton<MissionManager>.Instance.IsCurrentPlayer() || !PandoraSingleton<MissionManager>.Instance.transitionDone) && !IsUnitInside(ctrlr))
		{
			RemoveUnit(ctrlr, sendTrigger: true);
		}
	}

	public void RemoveUnit(UnitController ctrlr, bool sendTrigger)
	{
		for (int i = 0; i < unitsToCheck.Count; i++)
		{
			if (unitsToCheck[i].ctrlr == ctrlr)
			{
				unitsToCheck.RemoveAt(i);
				PandoraDebug.LogInfo("Exiting Aoe Zone: " + zoneAoeId + " Removed from target to check : " + ctrlr.name);
				return;
			}
		}
		if (sendTrigger)
		{
			for (int num = affectedUnits.Count - 1; num >= 0; num--)
			{
				if (affectedUnits[num] == ctrlr)
				{
					Trigger(ctrlr, entry: false, network: true);
					PandoraDebug.LogInfo("Exiting Aoe Zone: " + zoneAoeId + " Removed affected units : " + ctrlr.name);
					break;
				}
			}
		}
		int num2 = affectedUnits.Count - 1;
		while (true)
		{
			if (num2 >= 0)
			{
				if (affectedUnits[num2] == ctrlr)
				{
					break;
				}
				num2--;
				continue;
			}
			return;
		}
		affectedUnits.RemoveAt(num2);
	}

	public void AddToAffected(UnitController unitCtrlr)
	{
		if (affectedUnits.IndexOf(unitCtrlr) == -1)
		{
			affectedUnits.Add(unitCtrlr);
		}
	}

	public bool CheckUnit(UnitController ctrlr, bool network)
	{
		if (!ctrlr.IsInFriendlyZone && IsUnitInside(ctrlr))
		{
			Trigger(ctrlr, entry: true, network);
			return true;
		}
		return false;
	}

	public void CheckEnterOrExitUnit(UnitController ctrlr, bool network)
	{
		if (IsUnitInside(ctrlr))
		{
			Trigger(ctrlr, entry: true, network);
		}
		else if (IsUnitAffected(ctrlr))
		{
			Trigger(ctrlr, entry: false, network);
		}
	}

	private bool IsUnitInside(UnitController ctrlr)
	{
		if (boxBounds.Count > 0)
		{
			for (int i = 0; i < boxBounds.Count; i++)
			{
				if (boxBounds[i].Contains(ctrlr.transform.position))
				{
					return true;
				}
			}
		}
		else if (ctrlr.isInsideCylinder(base.transform.position + offset, radius, height, base.transform.up))
		{
			return true;
		}
		return false;
	}

	private void Trigger(UnitController ctrlr, bool entry, bool network)
	{
		if (entry)
		{
			if (IsUnitAffected(ctrlr))
			{
				return;
			}
			affectedUnits.Add(ctrlr);
		}
		ctrlr.SendZoneAoeCross(this, entry, network);
	}

	private bool IsUnitAffected(UnitController ctrlr)
	{
		return affectedUnits.IndexOf(ctrlr) != -1;
	}

	public EffectTypeId GetEnterEffectType()
	{
		for (int i = 0; i < enchantmentsData.Count; i++)
		{
			if (enchantmentsData[i].ZoneTriggerId == ZoneTriggerId.ENTER)
			{
				EnchantmentData enchantmentData = PandoraSingleton<DataFactory>.Instance.InitData<EnchantmentData>((int)enchantmentsData[i].EnchantmentId);
				return enchantmentData.EffectTypeId;
			}
		}
		return EffectTypeId.NONE;
	}

	public void ApplyEnchantments(UnitController ctrlr, bool entry)
	{
		bool flag = Owner == null || Owner == ctrlr;
		bool flag2 = Owner == null || allies == null || allies.IndexOf(ctrlr) != -1;
		bool flag3 = Owner == null || enemies == null || enemies.IndexOf(ctrlr) != -1;
		PandoraDebug.LogInfo("Unit is affected by AoeZone :  " + zoneAoeId + " for unit " + ctrlr.name + " self=" + flag + " ally=" + flag2 + " enemy=" + flag3);
		for (int i = 0; i < enchantmentsData.Count; i++)
		{
			if (!entry && enchantmentsData[i].ZoneTriggerId == ZoneTriggerId.INSIDE && ((enchantmentsData[i].TargetSelf && flag) || (enchantmentsData[i].TargetAlly && flag2) || (enchantmentsData[i].TargetEnemy && flag3)))
			{
				ctrlr.unit.RemoveEnchantment(enchantmentsData[i].EnchantmentId, (!(Owner != null)) ? ctrlr.unit : Owner.unit);
			}
			if (base.gameObject.activeSelf && (((enchantmentsData[i].ZoneTriggerId == ZoneTriggerId.ENTER || enchantmentsData[i].ZoneTriggerId == ZoneTriggerId.INSIDE) && entry) || (enchantmentsData[i].ZoneTriggerId == ZoneTriggerId.EXIT && !entry)) && ((enchantmentsData[i].TargetSelf && flag) || (enchantmentsData[i].TargetAlly && flag2) || (enchantmentsData[i].TargetEnemy && flag3)))
			{
				ctrlr.unit.AddEnchantment(enchantmentsData[i].EnchantmentId, (!(Owner != null)) ? ctrlr.unit : Owner.unit, original: false, updateAttributes: false);
			}
		}
	}

	public void UpdateDuration()
	{
		if (durationLeft <= 0)
		{
			return;
		}
		durationLeft--;
		if (durationLeft == 0)
		{
			Deactivate();
			PandoraSingleton<MissionManager>.Instance.MissionEndData.ClearAoe(guid);
			if (destructible != null && destructible.destroyOnAoeDestroy)
			{
				destructible.Deactivate();
			}
		}
		else
		{
			PandoraSingleton<MissionManager>.Instance.MissionEndData.UpdateAoe(guid, Owner.uid, zoneAoeId, radius, durationLeft, base.transform.position);
		}
	}

	public void GetDamages(out int damageMin, out int damageMax)
	{
		damageMin = 0;
		damageMax = 0;
		for (int i = 0; i < enchantmentsData.Count; i++)
		{
			if (enchantmentsData[i].ZoneTriggerId == ZoneTriggerId.ENTER)
			{
				EnchantmentData enchantmentData = PandoraSingleton<DataFactory>.Instance.InitData<EnchantmentData>((int)enchantmentsData[i].EnchantmentId);
				damageMin += enchantmentData.DamageMin;
				damageMax += enchantmentData.DamageMax;
			}
		}
	}
}
