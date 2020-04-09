using UnityEngine;

public class RackLoadoutNode : MonoBehaviour
{
	private GameObject loadout;

	public void SetLoadout(string warbandWagon)
	{
		if (!(loadout != null))
		{
			string str = warbandWagon.Replace("wagon_dis_00", "props_dis_00_weapon_rack_loadout");
			PandoraSingleton<AssetBundleLoader>.Instance.LoadAssetAsync<GameObject>("Assets/prefabs/environments/props/", AssetBundleId.PROPS, str + ".prefab", delegate(Object p)
			{
				loadout = (GameObject)Object.Instantiate(p);
				loadout.transform.SetParent(base.transform);
				loadout.transform.localPosition = Vector3.zero;
				loadout.transform.localRotation = Quaternion.identity;
			});
		}
	}
}
