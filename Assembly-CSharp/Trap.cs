using Prometheus;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Trap : TriggerPoint
{
	public TrapTypeId defaultType;

	public GameObject projectileStart;

	public bool forceInactive;

	public bool removeObjOnDestroy;

	public Sprite allyImprintIcon;

	public Sprite enemyImprintIcon;

	public MapImprintType enemyImprintType;

	private bool debugDis;

	public TrapEffectData EffectData
	{
		get;
		private set;
	}

	public int TeamIdx
	{
		get;
		set;
	}

	public MapImprint Imprint
	{
		get;
		private set;
	}

	public static void SpawnTrap(TrapTypeId trapTypeId, int teamIdx, Vector3 position, Quaternion rotation, Action cb = null, bool unload = true)
	{
		TrapTypeData typeData = PandoraSingleton<DataFactory>.Instance.InitData<TrapTypeData>((int)trapTypeId);
		List<TrapTypeJoinTrapData> list = PandoraSingleton<DataFactory>.Instance.InitData<TrapTypeJoinTrapData>("fk_trap_type_id", typeData.Id.ToIntString());
		string visual = list[PandoraSingleton<MissionManager>.Instance.NetworkTyche.Rand(0, list.Count)].TrapId.ToLowerString();
		PandoraSingleton<AssetBundleLoader>.Instance.LoadAssetAsync<GameObject>("Assets/prefabs/fx/", AssetBundleId.FX, visual + ".prefab", delegate(UnityEngine.Object go)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)go);
			if (unload)
			{
				SceneManager.UnloadScene(visual);
			}
			gameObject.transform.position = position;
			gameObject.transform.rotation = rotation;
			Trap component = gameObject.GetComponent<Trap>();
			uint nextRTGUID = PandoraSingleton<MissionManager>.Instance.GetNextRTGUID();
			component.Init(typeData, nextRTGUID);
			component.name += nextRTGUID;
			component.TeamIdx = teamIdx;
			component.SetImprint();
			PandoraSingleton<MissionManager>.Instance.triggerPoints.Add(component);
			PandoraSingleton<MissionManager>.Instance.MissionEndData.AddDynamicTrap(component);
			if (cb != null)
			{
				cb();
			}
		});
	}

	public void Init(TrapTypeData data, uint id)
	{
		defaultType = data.Id;
		List<TrapTypeJoinTrapEffectData> list = PandoraSingleton<DataFactory>.Instance.InitData<TrapTypeJoinTrapEffectData>("fk_trap_type_id", data.Id.ToIntString());
		int index = PandoraSingleton<MissionManager>.Instance.NetworkTyche.Rand(0, list.Count);
		TrapEffectId trapEffectId = list[index].TrapEffectId;
		EffectData = PandoraSingleton<DataFactory>.Instance.InitData<TrapEffectData>((int)trapEffectId);
		soundName = "trap";
		guid = id;
		TeamIdx = -1;
		Init();
	}

	public void SetImprint()
	{
		bool flag = PandoraSingleton<MissionManager>.Instance.GetMyWarbandCtrlr().teamIdx == TeamIdx;
		if ((flag && allyImprintIcon != null) || (!flag && enemyImprintIcon != null))
		{
			Imprint = base.gameObject.AddComponent<MapImprint>();
			Imprint.Init((!flag) ? enemyImprintIcon : allyImprintIcon, null, alwaysVisible: true, MapImprintType.TRAP, null, null, null, this);
		}
	}

	private void OnDestroy()
	{
		if (PandoraSingleton<MissionManager>.Exists())
		{
			int num = PandoraSingleton<MissionManager>.Instance.triggerPoints.IndexOf(this);
			if (num != -1 && PandoraSingleton<GameManager>.Instance.currentSave != null && !PandoraSingleton<MissionManager>.Instance.MissionEndData.missionSave.isTuto)
			{
				if (TeamIdx != -1)
				{
					PandoraSingleton<MissionManager>.Instance.MissionEndData.UpdateDynamicTrap(this);
				}
				else
				{
					PandoraSingleton<MissionManager>.Instance.MissionEndData.destroyedTraps.Add(guid);
				}
			}
		}
		if (Imprint != null)
		{
			UnityEngine.Object.Destroy(Imprint);
		}
		if (trigger != null)
		{
			UnityEngine.Object.Destroy(trigger.gameObject);
			trigger = null;
		}
		if (removeObjOnDestroy)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public override void Trigger(UnitController currentUnit)
	{
		if (!string.IsNullOrEmpty(EffectData.Fx))
		{
			PandoraSingleton<Prometheus.Prometheus>.Instance.SpawnFx(EffectData.Fx, projectileStart.transform, attached: true, null);
		}
		ActionOnUnit(currentUnit);
		if (EffectData.ZoneAoeId != 0)
		{
			ZoneAoe.Spawn(EffectData.ZoneAoeId, (float)EffectData.Radius, projectileStart.transform.position, currentUnit);
		}
		base.Trigger(currentUnit);
	}

	public override void ActionOnUnit(UnitController currentUnit)
	{
		currentUnit.Hit();
	}
}
