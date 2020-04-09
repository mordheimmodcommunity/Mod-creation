using Prometheus;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemController : MonoBehaviour
{
	public Transform projectileStartPoint;

	public GameObject shootFxPrefab;

	public Transform parryFxAnchor;

	private GameObject shootFx;

	public List<WeaponTrail> trails;

	private GameObject projectilePrefab;

	private Animation anim;

	public Projectile projectile;

	public Item Item
	{
		get;
		private set;
	}

	public int CurrentShots()
	{
		return Item.Save.shots;
	}

	public static void Instantiate(Item item, RaceId raceId, WarbandId warbandId, UnitId unitId, UnitSlotId slotId, Action<ItemController> callback)
	{
		if (item.Id != 0)
		{
			ItemAssetData assetData = item.GetAssetData(raceId, warbandId, unitId);
			PandoraSingleton<AssetBundleLoader>.Instance.LoadAssetAsync<GameObject>("Assets/prefabs/equipments/", AssetBundleId.EQUIPMENTS, assetData.Asset + ".prefab", delegate(UnityEngine.Object ec)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)ec);
				gameObject.SetLayerRecursively(LayerMask.NameToLayer("characters"));
				ItemController component = gameObject.GetComponent<ItemController>();
				component.SetItem(item, slotId, assetData.NoTrail);
				callback(component);
			});
		}
	}

	public void SetItem(Item item, UnitSlotId slotId, bool noTrail)
	{
		Item = item;
		trails = new List<WeaponTrail>(GetComponents<WeaponTrail>());
		anim = GetComponent<Animation>();
		for (int num = trails.Count - 1; num >= 0; num--)
		{
			if (noTrail)
			{
				UnityEngine.Object.Destroy(trails[num]);
				trails.RemoveAt(num);
			}
			else
			{
				trails[num].Emit(activate: false);
			}
		}
		if (Item.ProjectileId != 0)
		{
			PandoraSingleton<AssetBundleLoader>.Instance.LoadAssetAsync<GameObject>("Assets/prefabs/projectiles/", AssetBundleId.EQUIPMENTS, item.ProjectileData.Name + ".prefab", delegate(UnityEngine.Object p)
			{
				projectilePrefab = UnityEngine.Object.Instantiate((GameObject)p);
				projectilePrefab.SetActive(value: false);
				if (!PandoraSingleton<MissionStartData>.Exists() || !PandoraSingleton<MissionStartData>.Instance.isReload)
				{
					Reload();
				}
				else if (Item.Save.shots > 0)
				{
					AddProjectile();
					Play("reload");
				}
			});
		}
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].material.HasProperty("_Color"))
			{
				componentsInChildren[i].material.color = componentsInChildren[i].sharedMaterial.color;
			}
		}
		List<ItemSlotEnchantmentData> list = PandoraSingleton<DataFactory>.Instance.InitData<ItemSlotEnchantmentData>(new string[2]
		{
			"fk_item_id",
			"fk_unit_slot_id"
		}, new string[2]
		{
			((int)item.Id).ToConstantString(),
			((int)slotId).ToConstantString()
		});
		for (int j = 0; j < list.Count; j++)
		{
			if (!Item.HasEnchantment(list[j].EnchantmentId))
			{
				Item.Enchantments.Add(new Enchantment(list[j].EnchantmentId, null, null, orig: false, innate: true));
			}
		}
	}

	public void Play(string label)
	{
		if (anim != null && label != string.Empty && (bool)anim[label])
		{
			anim.Play(label);
		}
	}

	public void Unsheathe(Dictionary<BoneId, Transform> BonesTr, bool offhand = false)
	{
		BoneId boneId = Item.BoneId;
		if (offhand)
		{
			BoneData boneData = PandoraSingleton<DataFactory>.Instance.InitData<BoneData>((int)boneId);
			boneId = boneData.BoneIdMirror;
		}
		base.transform.SetParent(BonesTr[boneId]);
		base.transform.localPosition = Vector3.zero;
		base.transform.localRotation = Quaternion.identity;
		if (offhand && Item.StyleData.Id == AnimStyleId.CLAW)
		{
			base.transform.Rotate(Vector3.forward, 180f);
			base.transform.Translate(0.08f, 0f, 0f);
		}
		if (projectile != null)
		{
			projectile.gameObject.SetActive(value: true);
		}
	}

	public void Sheathe(Dictionary<BoneId, Transform> BonesTr, bool offhand = false, UnitId unitId = UnitId.NONE)
	{
		if (projectile != null)
		{
			projectile.gameObject.SetActive(value: false);
		}
		base.transform.SetParent(BonesTr[BoneId.RIG_SPINE3]);
		if (Item.TypeData.Id == ItemTypeId.SHIELD)
		{
			base.transform.localPosition = new Vector3(0.118f, -0.191f, 0.043f);
			base.transform.localRotation = Quaternion.Euler(11.4f, 16.7f, 0.8f);
			return;
		}
		switch (unitId)
		{
		case UnitId.OGRE_MERCENARY:
			if (offhand)
			{
				base.transform.localPosition = new Vector3(-0.511f, -0.361f, 0.15f);
				base.transform.localRotation = Quaternion.Euler(356.4f, 16.15f, 176.41f);
			}
			else
			{
				base.transform.localPosition = new Vector3(-0.4f, -0.37f, -0.17f);
				base.transform.localRotation = Quaternion.Euler(356.8f, 334.7f, 163.1f);
			}
			return;
		case UnitId.EXECUTIONER:
			if (offhand)
			{
				base.transform.localPosition = new Vector3(-0.439f, -0.258f, 0.157f);
				base.transform.localRotation = Quaternion.Euler(4.2f, 16.2f, 180.2f);
			}
			else
			{
				base.transform.localPosition = new Vector3(-0.341f, -0.205f, -0.171f);
				base.transform.localRotation = Quaternion.Euler(356.8f, 334.7f, 163.1f);
			}
			return;
		case UnitId.DREG:
			if (Item.StyleData.Id == AnimStyleId.SPEAR_NO_SHIELD || Item.StyleData.Id == AnimStyleId.SPEAR_SHIELD || Item.StyleData.Id == AnimStyleId.WARHAMMER)
			{
				base.transform.localPosition = new Vector3(0.235f, -0.179f, 0.074f);
				base.transform.localRotation = Quaternion.Euler(9.2f, 338.5f, 192.12f);
			}
			else if (offhand && (Item.StyleData.Id == AnimStyleId.SPEAR_SHIELD || Item.StyleData.Id == AnimStyleId.ONE_HAND_SHIELD))
			{
				base.transform.localPosition = new Vector3(-0.339f, -0.181f, 0.143f);
				base.transform.localRotation = Quaternion.Euler(11.4f, 16.2f, 180.2f);
			}
			else if (offhand)
			{
				base.transform.localPosition = new Vector3(-0.328f, -0.289f, 0.117f);
				base.transform.localRotation = Quaternion.Euler(8.875f, 32.28f, 191.5f);
			}
			else
			{
				base.transform.localPosition = new Vector3(-0.281f, -0.298f, -0.15f);
				base.transform.localRotation = Quaternion.Euler(9.2f, 338.5f, 192.12f);
			}
			return;
		case UnitId.GHOUL:
			if (Item.StyleData.Id == AnimStyleId.SPEAR_NO_SHIELD || Item.StyleData.Id == AnimStyleId.WARHAMMER)
			{
				base.transform.localPosition = new Vector3(0.211f, -0.148f, 0.07f);
				base.transform.localRotation = Quaternion.Euler(11.25f, 334.75f, 208.8f);
			}
			else if (offhand)
			{
				base.transform.localPosition = new Vector3(-0.15f, -0.297f, 0.101f);
				base.transform.localRotation = Quaternion.Euler(9.93f, 44.92f, 195.475f);
			}
			else
			{
				base.transform.localPosition = new Vector3(-0.241f, -0.287f, -0.146f);
				base.transform.localRotation = Quaternion.Euler(9.2f, 338.5f, 186.43f);
			}
			return;
		case UnitId.CRYPT_HORROR:
			if (Item.StyleData.Id == AnimStyleId.SPEAR_NO_SHIELD || Item.StyleData.Id == AnimStyleId.WARHAMMER)
			{
				base.transform.localPosition = new Vector3(0.335f, -0.357f, 0.082f);
				base.transform.localRotation = Quaternion.Euler(11.25f, 334.75f, 208.8f);
			}
			else if (offhand)
			{
				base.transform.localPosition = new Vector3(-0.173f, -0.684f, 0.223f);
				base.transform.localRotation = Quaternion.Euler(4.75f, 58.387f, 202.28f);
			}
			else
			{
				base.transform.localPosition = new Vector3(-0.222f, -0.707f, -0.257f);
				base.transform.localRotation = Quaternion.Euler(11.25f, 334.75f, 208.8f);
			}
			return;
		}
		switch (Item.StyleData.Id)
		{
		case AnimStyleId.ONE_HAND_NO_SHIELD:
		case AnimStyleId.ONE_HAND_SHIELD:
		case AnimStyleId.DUAL_WIELD:
			if (offhand)
			{
				base.transform.localPosition = new Vector3(-0.339f, -0.181f, 0.143f);
				base.transform.localRotation = Quaternion.Euler(11.4f, 16.2f, 180.2f);
			}
			else
			{
				base.transform.localPosition = new Vector3(-0.337f, -0.123f, -0.155f);
				base.transform.localRotation = Quaternion.Euler(9.2f, 338.5f, 173.2f);
			}
			break;
		case AnimStyleId.DUAL_PISTOL:
			if (offhand)
			{
				base.transform.localPosition = new Vector3(-0.264f, -0.162f, 0.243f);
				base.transform.localRotation = Quaternion.Euler(2.4f, 83.4f, 176.9f);
			}
			else
			{
				base.transform.localPosition = new Vector3(-0.312f, -0.132f, -0.167f);
				base.transform.localRotation = Quaternion.Euler(6.1f, 111.5f, 7.3f);
			}
			break;
		case AnimStyleId.CLAW:
			if (offhand)
			{
				base.transform.localPosition = new Vector3(-0.145f, -0.192f, 0.146f);
				base.transform.localRotation = Quaternion.Euler(357.2f, 119.1f, 181.4f);
			}
			else
			{
				base.transform.localPosition = new Vector3(-0.226f, -0.161f, -0.081f);
				base.transform.localRotation = Quaternion.Euler(8f, 56.7f, 195f);
			}
			break;
		case AnimStyleId.HALBERD:
			base.transform.localPosition = new Vector3(-0.337f, -0.155f, -0.16f);
			base.transform.localRotation = Quaternion.Euler(9.2f, 338.5f, 173.2f);
			break;
		case AnimStyleId.BOW:
			base.transform.localPosition = new Vector3(0.1061865f, -0.1313297f, -0.01045499f);
			base.transform.localRotation = Quaternion.Euler(-7.007904f, 169.1543f, 167.5401f);
			break;
		case AnimStyleId.SPEAR_NO_SHIELD:
		case AnimStyleId.SPEAR_SHIELD:
			if (Item.owner.RaceId == RaceId.SKAVEN)
			{
				base.transform.localPosition = new Vector3(-0.653f, -0.142f, -0.156f);
				base.transform.localRotation = Quaternion.Euler(9.2f, 338.5f, 173.2f);
			}
			else
			{
				base.transform.localPosition = new Vector3(0.25f, -0.213f, 0.074f);
				base.transform.localRotation = Quaternion.Euler(9.2f, 338.5f, 173.2f);
			}
			break;
		case AnimStyleId.WARHAMMER:
			base.transform.localPosition = new Vector3(0.25f, -0.213f, 0.074f);
			base.transform.localRotation = Quaternion.Euler(9.2f, 338.5f, 173.2f);
			break;
		case AnimStyleId.CROSSBOW:
			base.transform.localPosition = new Vector3(-0.094f, -0.136f, -0.127f);
			base.transform.localRotation = Quaternion.Euler(9.2f, 338.5f, 173.2f);
			break;
		case AnimStyleId.RIFLE:
			base.transform.localPosition = new Vector3(-0.036184f, -0.15823f, -0.0025011f);
			base.transform.localRotation = Quaternion.Euler(9.2f, 24.91f, 173.2f);
			break;
		case AnimStyleId.TWO_HANDED:
			base.transform.SetParent(BonesTr[BoneId.RIG_SPINE3]);
			base.transform.localPosition = new Vector3(-0.34426f, -0.19861f, -0.19507f);
			base.transform.localRotation = Quaternion.Euler(-3.2f, -25.3f, -174.3f);
			break;
		}
	}

	public void Reload()
	{
		AddProjectile();
		Play("reload");
		Item.Save.shots = Item.Shots;
	}

	public GameObject GetProjectile()
	{
		if (projectile == null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(projectilePrefab);
			gameObject.SetActive(value: true);
			projectile = gameObject.GetComponent<Projectile>();
		}
		return projectile.gameObject;
	}

	public void AttachProjectile()
	{
		GameObject gameObject = projectile.gameObject;
		MeshFilter componentInChildren = gameObject.GetComponentInChildren<MeshFilter>();
		gameObject.transform.SetParent(projectileStartPoint);
		Transform transform = gameObject.transform;
		float[] array = new float[3];
		Vector3 extents = componentInChildren.mesh.bounds.extents;
		array[0] = extents.x;
		Vector3 extents2 = componentInChildren.mesh.bounds.extents;
		array[1] = extents2.y;
		Vector3 extents3 = componentInChildren.mesh.bounds.extents;
		array[2] = extents3.z;
		transform.localPosition = new Vector3(0f, 0f, Mathf.Max(array) * 2f);
		gameObject.transform.localRotation = Quaternion.identity;
	}

	private void AddProjectile()
	{
		GetProjectile();
		AttachProjectile();
	}

	public void Aim()
	{
		Play("aim");
	}

	public void Shoot(UnitController shooter, List<Vector3> targets, List<MonoBehaviour> defenders, List<bool> noCollisions, List<Transform> parts, bool isSecondary = false)
	{
		Item.Save.shots--;
		Play("shooting");
		for (int i = 0; i < defenders.Count; i++)
		{
			AddProjectile();
			projectile.Launch(targets[i], shooter, defenders[i], noCollisions[i], parts[i], isSecondary);
			projectile = null;
		}
		if (shootFxPrefab != null)
		{
			if (shootFx != null)
			{
				UnityEngine.Object.Destroy(shootFx);
			}
			PandoraSingleton<Prometheus.Prometheus>.Instance.SpawnFx(shootFxPrefab.name, shooter, null, delegate(GameObject fx)
			{
				shootFx = fx;
				if (shootFx != null)
				{
					shootFx.transform.SetParent(projectileStartPoint);
					shootFx.transform.localPosition = Vector3.zero;
					shootFx.transform.localRotation = Quaternion.identity;
				}
			});
		}
	}
}
