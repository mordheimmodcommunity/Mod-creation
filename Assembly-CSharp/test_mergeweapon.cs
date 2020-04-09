using UnityEngine;

public class test_mergeweapon : MonoBehaviour
{
	public GameObject DaWeapon;

	public bool isUsed;

	public GameObject DaWeaponOnceUsed;

	private void Start()
	{
	}

	public void AddEquipment()
	{
		isUsed = true;
		SkinnedMeshRenderer[] componentsInChildren = DaWeapon.GetComponentsInChildren<SkinnedMeshRenderer>();
		SkinnedMeshRenderer[] array = componentsInChildren;
		foreach (SkinnedMeshRenderer thisRenderer in array)
		{
			ProcessBonedObject(thisRenderer);
		}
		DaWeapon.SetActive(value: false);
	}

	public void RemoveEquipment()
	{
		isUsed = false;
		Object.Destroy(DaWeaponOnceUsed);
		DaWeapon.SetActive(value: true);
	}

	public void ProcessBonedObject(SkinnedMeshRenderer ThisRenderer)
	{
		DaWeaponOnceUsed = new GameObject(ThisRenderer.gameObject.name);
		DaWeaponOnceUsed.transform.parent = base.transform;
		SkinnedMeshRenderer skinnedMeshRenderer = DaWeaponOnceUsed.AddComponent(typeof(SkinnedMeshRenderer)) as SkinnedMeshRenderer;
		Transform[] array = new Transform[ThisRenderer.bones.Length];
		for (int i = 0; i < ThisRenderer.bones.Length; i++)
		{
			array[i] = FindChildByName(ThisRenderer.bones[i].name, base.transform);
		}
		skinnedMeshRenderer.bones = array;
		skinnedMeshRenderer.sharedMesh = ThisRenderer.sharedMesh;
		skinnedMeshRenderer.materials = ThisRenderer.materials;
	}

	public Transform FindChildByName(string ThisName, Transform ThisGObj)
	{
		if (ThisGObj.name == ThisName)
		{
			return ThisGObj.transform;
		}
		foreach (Transform item in ThisGObj)
		{
			Transform transform = FindChildByName(ThisName, item);
			if (transform != null)
			{
				return transform;
			}
		}
		return null;
	}
}
