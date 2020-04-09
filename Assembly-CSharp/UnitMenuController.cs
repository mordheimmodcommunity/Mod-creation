using HighlightingSystem;
using Prometheus;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Animator))]
public class UnitMenuController : MonoBehaviour
{
	private const string BODY_PARTS_FOLDER = "Assets/prefabs/characters/";

	private const string MATERIAL_FOLDER = "Assets/3d_assets/characters/";

	private const int MAX_DEFAULT_SKIN_COLOR_ITERATION = 3;

	[HideInInspector]
	public Unit unit;

	private SkinnedMeshCombiner skinnedMeshCombiner;

	protected ShaderSetter shaderSetter;

	[HideInInspector]
	public Animator animator;

	protected UnitActionId currentActionId;

	public SequenceInfo seqData;

	[HideInInspector]
	protected List<Cloth> cloths = new List<Cloth>();

	protected List<WeaponTrail> bodyPartTrails = new List<WeaponTrail>();

	public Dissolver dissolver;

	private bool visible = true;

	public AudioSource audioSource;

	protected string lastFoot;

	protected string lastShout;

	private int asyncQueued;

	private int totalQueued;

	private int totalLoaded;

	private Action bodyPartsLoadCB;

	protected Action finishedLoad;

	private GameObject statusFx;

	private readonly List<Renderer> tempItemsRenderers = new List<Renderer>();

	public List<ItemController> Equipments
	{
		get;
		protected set;
	}

	public Highlighter Highlight
	{
		get;
		protected set;
	}

	public Dictionary<BoneId, Transform> BonesTr
	{
		get;
		private set;
	}

	public static IEnumerator LoadUnitPrefabAsync(Unit unit, Action<GameObject> callback, Action finishedLoading)
	{
		string unitBase = unit.Data.UnitBaseId.ToString().ToLower();
		string prefab = "prefabs/characters/" + unitBase + "_menu";
		float startTime = Time.realtimeSinceStartup;
		ResourceRequest res = Resources.LoadAsync<GameObject>(prefab);
		yield return res;
		GameObject instance = (GameObject)UnityEngine.Object.Instantiate(res.asset);
		instance.name = unitBase + "_" + unit.Id + "_" + unit.Name;
		instance.AddComponent<AnimatorPlayerEvents>();
		UnitMenuController ctrlr = instance.GetComponent<UnitMenuController>();
		ctrlr.asyncQueued = 0;
		ctrlr.totalLoaded = 0;
		ctrlr.finishedLoad = finishedLoading;
		ctrlr.SetCharacter(unit);
		yield return null;
		ctrlr.LaunchBodyPartsLoading(ctrlr.FinishLoadingBodyParts);
		callback(instance);
	}

	protected virtual void Awake()
	{
		skinnedMeshCombiner = GetComponent<SkinnedMeshCombiner>();
		shaderSetter = GetComponent<ShaderSetter>();
		animator = GetComponent<Animator>();
		animator.stabilizeFeet = true;
		dissolver = GetComponent<Dissolver>();
		if (dissolver == null)
		{
			dissolver = base.gameObject.AddComponent<Dissolver>();
		}
		Highlight = GetComponent<Highlighter>();
		if ((UnityEngine.Object)(object)Highlight == null)
		{
			Highlight = base.gameObject.AddComponent<Highlighter>();
		}
		Highlight.seeThrough = false;
		if (GetComponent<AudioSource>() == null)
		{
			StartCoroutine(LoadSoundBaseAsync());
		}
	}

	private IEnumerator LoadSoundBaseAsync()
	{
		yield return null;
		ResourceRequest req = Resources.LoadAsync("prefabs/sound_base");
		yield return req;
		GameObject go = (GameObject)UnityEngine.Object.Instantiate(req.asset);
		go.transform.SetParent(base.transform);
		go.transform.localPosition = Vector3.zero;
		go.transform.localRotation = Quaternion.identity;
		audioSource = go.GetComponent<AudioSource>();
		audioSource.loop = false;
		audioSource.playOnAwake = false;
	}

	protected virtual void LateUpdate()
	{
		if (IsAnimating())
		{
			animator.SetInteger(AnimatorIds.action, 0);
			animator.SetInteger(AnimatorIds.atkResult, 0);
			animator.SetFloat(AnimatorIds.emoteVariation, 0f);
		}
	}

	public void MergeNoAtlas()
	{
		if (skinnedMeshCombiner != null)
		{
			skinnedMeshCombiner.MergeNoAtlas();
		}
	}

	public void SetCharacter(Unit unitInfo)
	{
		unit = unitInfo;
		InitializeBones();
		InitBodyTrails();
		InstantiateAllEquipment();
	}

	public void InitializeBones()
	{
		BonesTr = new Dictionary<BoneId, Transform>();
		List<BoneData> list = PandoraSingleton<DataFactory>.Instance.InitData<BoneData>();
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Transform child = base.transform.GetChild(i);
			for (int j = 0; j < list.Count; j++)
			{
				if (child.name == list[j].Name)
				{
					BonesTr[list[j].Id] = child;
					child.gameObject.SetLayerRecursively(LayerMask.NameToLayer("cloth"));
					break;
				}
			}
		}
	}

	public void InitCloth()
	{
		cloths.Clear();
		base.gameObject.GetComponentsInChildren(includeInactive: true, cloths);
	}

	public void InitBodyTrails()
	{
		bodyPartTrails.Clear();
		foreach (BodyPart value in unit.bodyParts.Values)
		{
			for (int i = 0; i < value.relatedGO.Count; i++)
			{
				value.relatedGO[i].SetActive(value: true);
				bodyPartTrails.AddRange(value.relatedGO[i].GetComponentsInChildren<WeaponTrail>());
			}
		}
		for (int j = 0; j < bodyPartTrails.Count; j++)
		{
			bodyPartTrails[j].Emit(activate: false);
		}
	}

	public void SetModelVariation(BodyPartId bodyPart, int index, bool reload = true)
	{
		KeyValuePair<int, int> value = new KeyValuePair<int, int>(index, unit.UnitSave.customParts[bodyPart].Value);
		unit.UnitSave.customParts[bodyPart] = value;
		unit.bodyParts[bodyPart].SetVariation(index);
		if (bodyPart == BodyPartId.ARML || bodyPart == BodyPartId.ARMR)
		{
			SetModelVariation((bodyPart != BodyPartId.ARML) ? BodyPartId.HANDR : BodyPartId.HANDL, index, reload: false);
		}
		if (reload)
		{
			LoadBodyParts();
		}
	}

	public void SetBodyPartColor(BodyPartId bodyPart, int color)
	{
		KeyValuePair<int, int> value = new KeyValuePair<int, int>(unit.UnitSave.customParts[bodyPart].Key, color);
		unit.UnitSave.customParts[bodyPart] = value;
		unit.bodyParts[bodyPart].SetColorOverride(color);
		LoadBodyParts();
		if (bodyPart == BodyPartId.BODY && unit.bodyParts.ContainsKey(BodyPartId.GEAR_BODY))
		{
			SetBodyPartColor(BodyPartId.GEAR_BODY, color);
		}
		if (bodyPart == BodyPartId.ARMR || bodyPart == BodyPartId.ARML)
		{
			SetBodyPartColor((bodyPart != BodyPartId.ARMR) ? BodyPartId.HANDL : BodyPartId.HANDR, color);
		}
	}

	public void SetColorPreset(ColorPresetId presetId)
	{
		int num = (int)presetId << 8;
		foreach (BodyPart value2 in unit.bodyParts.Values)
		{
			KeyValuePair<int, int> value = new KeyValuePair<int, int>(unit.UnitSave.customParts[value2.Id].Key, num);
			unit.UnitSave.customParts[value2.Id] = value;
			value2.SetColorPreset(num);
		}
		LoadBodyParts();
	}

	public void SetSkinColor(string color)
	{
		unit.UnitSave.skinColor = color;
		foreach (BodyPart value in unit.bodyParts.Values)
		{
			value.SetSkinColor(color);
		}
		LoadBodyParts();
	}

	public void LoadBodyParts()
	{
		LaunchBodyPartsLoading(OnBodyPartCustomization);
	}

	private void OnBodyPartCustomization()
	{
		for (int i = 0; i < Equipments.Count; i++)
		{
			if (Equipments[i] != null && Equipments[i].gameObject != null)
			{
				Equipments[i].gameObject.SetActive(value: true);
			}
		}
		animator.Rebind();
		foreach (BodyPart value in unit.bodyParts.Values)
		{
			for (int j = 0; j < value.relatedGO.Count; j++)
			{
				value.relatedGO[j].SetActive(value: true);
			}
		}
		if (shaderSetter != null)
		{
			shaderSetter.ApplyShaderParams();
		}
		if (skinnedMeshCombiner != null)
		{
			skinnedMeshCombiner.AttachAttachers();
		}
		SwitchWeapons(UnitSlotId.SET1_MAINHAND);
		Hide(hide: false);
		PandoraSingleton<AssetBundleLoader>.Instance.UnloadScenes();
	}

	public void LaunchBodyPartsLoading(Action callback, bool noLOD = true)
	{
		PandoraSingleton<GameManager>.Instance.StartCoroutine(LoadBodyPartsAsync(callback, noLOD));
	}

	public IEnumerator LoadBodyPartsAsync(Action callback, bool noLOD = false)
	{
		bodyPartsLoadCB = callback;
		for (int i = 0; i < Equipments.Count; i++)
		{
			if (Equipments[i] != null && Equipments[i].gameObject != null)
			{
				Equipments[i].gameObject.SetActive(value: false);
			}
		}
		bool loading = false;
		ItemTypeId preferredItemTypeId = ItemTypeId.NONE;
		Item bodyItem = null;
		if (unit.bodyParts.ContainsKey(BodyPartId.BODY))
		{
			bodyItem = unit.bodyParts[BodyPartId.BODY].GetRelatedItem();
		}
		if (bodyItem != null)
		{
			preferredItemTypeId = bodyItem.TypeData.Id;
		}
		foreach (KeyValuePair<BodyPartId, BodyPart> p in unit.bodyParts)
		{
			BodyPart part = p.Value;
			if (part.AssetNeedReload)
			{
				part.DestroyRelatedGO();
				string partName = part.GetAsset(preferredItemTypeId);
				if (string.IsNullOrEmpty(partName))
				{
					part.SetColorOverride(0);
					partName = part.GetAsset(preferredItemTypeId);
				}
				part.AssetNeedReload = false;
				if (!part.Empty)
				{
					LoadBodyPart(bpId: (int)p.Key, assetBundle: part.AssetBundle, partName: partName, noLOD: noLOD);
					loading = true;
				}
			}
		}
		if (!loading && asyncQueued == 0 && bodyPartsLoadCB != null)
		{
			bodyPartsLoadCB();
			bodyPartsLoadCB = null;
		}
		yield return null;
	}

	public float GetBodypartPercentLoaded()
	{
		if (totalQueued == 0)
		{
			return 0f;
		}
		return (float)totalLoaded / (float)totalQueued;
	}

	public void LoadBodyPart(string assetBundle, string partName, int bpId, bool noLOD)
	{
		asyncQueued++;
		totalQueued++;
		PandoraSingleton<AssetBundleLoader>.Instance.LoadSceneAssetAsync(partName, assetBundle, delegate
		{
			GameObject go2 = SceneManager.GetSceneByName(partName).GetRootGameObjects()[0];
			asyncQueued--;
			totalLoaded++;
			OnBodyPartLoaded(go2, bpId, noLOD);
		});
	}

	public void OnBodyPartLoaded(GameObject go, int bpId, bool noLOD)
	{
		BodyPart LambdaPart = unit.bodyParts[(BodyPartId)bpId];
		if (skinnedMeshCombiner == null || base.gameObject == null)
		{
			return;
		}
		GameObject gameObject;
		if (go == null)
		{
			PandoraDebug.LogWarning("Couldn't find asset in asset bundle " + LambdaPart.AssetBundle + " ,for body part " + LambdaPart.Id);
			gameObject = new GameObject();
			gameObject.transform.position = skinnedMeshCombiner.transform.position;
			gameObject.transform.rotation = skinnedMeshCombiner.transform.rotation;
		}
		else
		{
			gameObject = (GameObject)UnityEngine.Object.Instantiate(go, skinnedMeshCombiner.transform.position, skinnedMeshCombiner.transform.rotation);
		}
		gameObject.SetLayerRecursively(LayerMask.NameToLayer("characters"));
		gameObject.transform.SetParent(skinnedMeshCombiner.transform, worldPositionStays: true);
		LambdaPart.relatedGO = skinnedMeshCombiner.AttachGameObject(gameObject, noLOD);
		if (LambdaPart.MutationId != 0)
		{
			List<MutationFxData> list = PandoraSingleton<DataFactory>.Instance.InitData<MutationFxData>(new string[2]
			{
				"fk_mutation_id",
				"fk_unit_id"
			}, new string[2]
			{
				((int)LambdaPart.MutationId).ToString(),
				((int)unit.Id).ToString()
			});
			if (list != null && list.Count > 0)
			{
				PandoraSingleton<Prometheus.Prometheus>.Instance.SpawnFx(list[0].Asset, this, null, delegate(GameObject fx)
				{
					if (fx != null)
					{
						LambdaPart.relatedGO.Add(fx);
					}
				});
			}
		}
		for (int i = 0; i < LambdaPart.relatedGO.Count; i++)
		{
			LambdaPart.relatedGO[i].SetActive(value: false);
		}
		if (asyncQueued == 0 && bodyPartsLoadCB != null)
		{
			bodyPartsLoadCB();
			bodyPartsLoadCB = null;
		}
		LambdaPart.AssetNeedReload = false;
	}

	public void FinishLoadingBodyParts()
	{
		foreach (BodyPart value in unit.bodyParts.Values)
		{
			for (int i = 0; i < value.relatedGO.Count; i++)
			{
				value.relatedGO[i].SetActive(value: true);
			}
		}
		for (int j = 0; j < Equipments.Count; j++)
		{
			if (Equipments[j] != null && Equipments[j].gameObject != null)
			{
				Equipments[j].gameObject.SetActive(value: true);
			}
		}
		Hide(!visible, !visible);
		animator.Rebind();
		if (shaderSetter != null)
		{
			shaderSetter.ApplyShaderParams();
		}
		if (skinnedMeshCombiner != null)
		{
			skinnedMeshCombiner.AttachAttachers();
		}
		SwitchWeapons(UnitSlotId.SET1_MAINHAND);
		InitCloth();
		if ((UnityEngine.Object)(object)Highlight != null)
		{
			Highlight.ReinitMaterials();
		}
		if (finishedLoad != null)
		{
			finishedLoad();
		}
	}

	public void RefreshBodyParts()
	{
		unit.ResetBodyPart();
		LoadBodyParts();
	}

	public bool IsAnimating()
	{
		int fullPathHash = animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
		return fullPathHash != AnimatorIds.idle && fullPathHash != AnimatorIds.kneeling_stunned && fullPathHash != AnimatorIds.climb_sheathe && fullPathHash != AnimatorIds.jump_sheathe && fullPathHash != AnimatorIds.leap_sheathe && fullPathHash != AnimatorIds.search_idle;
	}

	public void SetAnimStyle()
	{
		animator.SetFloat(AnimatorIds.type, (float)unit.currentAnimStyleId);
		animator.SetInteger(AnimatorIds.style, (int)unit.currentAnimStyleId);
	}

	public void LaunchAction(UnitActionId id, bool success, UnitStateId stateId, int variation = 0)
	{
		currentActionId = id;
		if (seqData.action > 0)
		{
			animator.SetInteger(AnimatorIds.action, seqData.action);
			animator.SetBool(AnimatorIds.actionSuccess, seqData.actionSuccess);
			animator.SetInteger(AnimatorIds.unit_state, seqData.unitState);
			animator.SetFloat(AnimatorIds.emoteVariation, seqData.emoteVariation);
			animator.SetInteger(AnimatorIds.variation, seqData.attackVariation);
			seqData.action = 0;
			seqData.actionSuccess = false;
			seqData.unitState = 0;
			seqData.emoteVariation = 0;
			seqData.attackVariation = 0;
		}
		else
		{
			animator.SetInteger(AnimatorIds.action, (int)id);
			animator.SetBool(AnimatorIds.actionSuccess, success);
			animator.SetInteger(AnimatorIds.unit_state, (int)stateId);
			animator.SetInteger(AnimatorIds.variation, variation);
			animator.SetFloat(AnimatorIds.emoteVariation, 0f);
		}
	}

	public void PlayDefState(AttackResultId resultId, int variation, UnitStateId stateId)
	{
		animator.SetInteger(AnimatorIds.atkResult, (int)resultId);
		animator.SetInteger(AnimatorIds.variation, variation);
		animator.SetInteger(AnimatorIds.unit_state, (int)stateId);
		SetStatusFX();
	}

	public void PlayBuffDebuff(EffectTypeId effectId)
	{
		if (effectId == EffectTypeId.BUFF || effectId == EffectTypeId.DEBUFF)
		{
			animator.SetInteger(AnimatorIds.action, 40);
			animator.SetInteger(AnimatorIds.variation, (effectId != EffectTypeId.BUFF) ? 1 : 0);
		}
	}

	public virtual void SetAnimSpeed(float speed)
	{
		animator.SetFloat(AnimatorIds.speed, speed);
	}

	public void SetStatusFX()
	{
		if (statusFx != null)
		{
			UnityEngine.Object.Destroy(statusFx);
		}
		UnitStateData unitStateData = PandoraSingleton<DataFactory>.Instance.InitData<UnitStateData>((int)unit.Status);
		if (!string.IsNullOrEmpty(unitStateData.Fx))
		{
			PandoraSingleton<Prometheus.Prometheus>.Instance.SpawnFx(unitStateData.Fx, unit, delegate(GameObject fx)
			{
				statusFx = fx;
			});
		}
	}

	public bool HasClose()
	{
		return Equipments[(int)unit.ActiveWeaponSlot] != null && Equipments[(int)unit.ActiveWeaponSlot].Item.TypeData != null && !Equipments[(int)unit.ActiveWeaponSlot].Item.TypeData.IsRange;
	}

	public bool HasRange()
	{
		return Equipments[(int)unit.ActiveWeaponSlot] != null && Equipments[(int)unit.ActiveWeaponSlot].Item.TypeData != null && Equipments[(int)unit.ActiveWeaponSlot].Item.TypeData.IsRange;
	}

	public bool IsAltClose()
	{
		return Equipments[(int)unit.InactiveWeaponSlot] != null && Equipments[(int)unit.InactiveWeaponSlot].Item.TypeData != null && !Equipments[(int)unit.InactiveWeaponSlot].Item.TypeData.IsRange;
	}

	public bool IsAltRange()
	{
		return Equipments[(int)unit.InactiveWeaponSlot] != null && Equipments[(int)unit.InactiveWeaponSlot].Item.TypeData != null && Equipments[(int)unit.InactiveWeaponSlot].Item.TypeData.IsRange;
	}

	public bool CanSwitchWeapon()
	{
		return unit.CanSwitchWeapon();
	}

	public void SwitchWeapons(UnitSlotId nextWeaponSlot)
	{
		for (int i = 2; i <= 5; i++)
		{
			if ((i != (int)nextWeaponSlot || i != (int)(nextWeaponSlot + 1)) && Equipments[i] != null)
			{
				Equipments[i].gameObject.SetActive(value: false);
			}
		}
		unit.ActiveWeaponSlot = nextWeaponSlot;
		AnimStyleId currentAnimStyleId = AnimStyleId.NONE;
		if (Equipments[(int)nextWeaponSlot] != null)
		{
			Equipments[(int)nextWeaponSlot].gameObject.SetActive(value: true);
			Equipments[(int)nextWeaponSlot].Unsheathe(BonesTr);
			currentAnimStyleId = Equipments[(int)nextWeaponSlot].Item.StyleData.Id;
			if (Equipments[(int)(nextWeaponSlot + 1)] != null)
			{
				Equipments[(int)(nextWeaponSlot + 1)].gameObject.SetActive(value: true);
				Equipments[(int)(nextWeaponSlot + 1)].Unsheathe(BonesTr, offhand: true);
				if (Equipments[(int)(nextWeaponSlot + 1)].Item.TypeData.Id == ItemTypeId.MELEE_1H && Equipments[(int)(nextWeaponSlot + 1)].Item.StyleData.Id != 0)
				{
					currentAnimStyleId = AnimStyleId.DUAL_WIELD;
				}
				else if (Equipments[(int)(nextWeaponSlot + 1)].Item.TypeData.Id == ItemTypeId.SHIELD)
				{
					currentAnimStyleId = Equipments[(int)nextWeaponSlot].Item.StyleData.Id + 1;
				}
			}
		}
		unit.currentAnimStyleId = currentAnimStyleId;
		SetAnimStyle();
	}

	protected void InstantiateAllEquipment()
	{
		Equipments = new List<ItemController>();
		for (int i = 0; i < unit.Items.Count; i++)
		{
			Equipments.Add(null);
		}
		RefreshEquipments();
		SwitchWeapons(UnitSlotId.SET1_MAINHAND);
	}

	public List<ItemSave> EquipItem(UnitSlotId slot, ItemId itemId)
	{
		return EquipItem(slot, new ItemSave(itemId));
	}

	private void ShowWeapons()
	{
		Hide(hide: false, force: true);
	}

	public List<ItemSave> EquipItem(UnitSlotId slot, ItemSave itemSave)
	{
		List<Item> list = unit.EquipItem(slot, itemSave);
		UnitSlotData unitSlotData = PandoraSingleton<DataFactory>.Instance.InitData<UnitSlotData>((int)slot);
		switch (unitSlotData.UnitSlotTypeId)
		{
		case UnitSlotTypeId.BODY_PART:
			HideAndRefreshBodyParts();
			break;
		case UnitSlotTypeId.EQUIPMENT:
			RefreshEquipments(ShowWeapons);
			break;
		}
		List<ItemSave> list2 = new List<ItemSave>();
		for (int i = 0; i < list.Count; i++)
		{
			list2.Add(list[i].Save);
		}
		if ((bool)(UnityEngine.Object)(object)Highlight)
		{
			Highlight.ReinitMaterials();
		}
		return list2;
	}

	private void HideAndRefreshBodyParts()
	{
		Hide(hide: true);
		RefreshBodyParts();
	}

	public void RefreshEquipments(Action callback = null)
	{
		if (callback != null)
		{
			bodyPartsLoadCB = callback;
		}
		for (int i = 0; i < 6; i++)
		{
			UnitSlotData unitSlotData = PandoraSingleton<DataFactory>.Instance.InitData<UnitSlotData>(i);
			if (unitSlotData.UnitSlotTypeId != UnitSlotTypeId.EQUIPMENT)
			{
				continue;
			}
			int index2 = i;
			if (unit.Items[i - 1].IsPaired)
			{
				index2 = i - 1;
			}
			if ((Equipments[i] == null && unit.Items[index2].Id != 0) || (Equipments[i] != null && Equipments[i].Item != unit.Items[index2]))
			{
				if (Equipments[i] != null)
				{
					UnityEngine.Object.Destroy(Equipments[i].gameObject);
				}
				Equipments[i] = null;
				if (unit.Items[index2].Id != 0)
				{
					int index = i;
					asyncQueued++;
					ItemController.Instantiate(unit.Items[index2], unit.RaceId, unit.WarbandId, unit.Id, (UnitSlotId)index, delegate(ItemController ic)
					{
						asyncQueued--;
						Equipments[index] = ic;
						tempItemsRenderers.Clear();
						ic.GetComponentsInChildren(includeInactive: true, tempItemsRenderers);
						for (int j = 0; j < tempItemsRenderers.Count; j++)
						{
							tempItemsRenderers[j].enabled = false;
						}
						if (Equipments[index] != null)
						{
							Equipments[index].Unsheathe(BonesTr, index == 3 || index == 5);
						}
						if (asyncQueued == 0)
						{
							if (bodyPartsLoadCB != null)
							{
								bodyPartsLoadCB();
								bodyPartsLoadCB = null;
							}
							SwitchWeapons(unit.ActiveWeaponSlot);
						}
					});
				}
			}
		}
	}

	public virtual void EventDissolve()
	{
		Hide(hide: true);
	}

	public virtual void Hide(bool hide, bool force = false, UnityAction onDissolved = null)
	{
		visible = !hide;
		dissolver.Hide(hide, force, onDissolved);
	}

	public void InstantMove(Vector3 pos, Quaternion rot)
	{
		for (int i = 0; i < cloths.Count; i++)
		{
			if (cloths[i] != null)
			{
				cloths[i].enabled = false;
			}
		}
		base.transform.position = pos;
		base.transform.rotation = rot;
		for (int j = 0; j < cloths.Count; j++)
		{
			if (cloths[j] != null)
			{
				cloths[j].enabled = true;
			}
		}
	}
}
