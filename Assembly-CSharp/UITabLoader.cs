using System.Collections.Generic;
using UnityEngine;

public class UITabLoader : MonoBehaviour
{
	public RectTransform anchor;

	public List<string> modules = new List<string>();

	private List<GameObject> modulesGO = new List<GameObject>();

	public void Load(Dictionary<ModuleId, UIModule> tab)
	{
		for (int i = 0; i < modules.Count; i++)
		{
			modulesGO.Add(null);
			PandoraSingleton<HideoutManager>.Instance.tabsLoading++;
			int index = i;
			PandoraSingleton<AssetBundleLoader>.Instance.LoadAssetAsync<GameObject>("Assets/prefabs/gui/hideout/", AssetBundleId.SCENE_PREFABS, modules[i] + ".prefab", delegate(Object go)
			{
				PandoraSingleton<HideoutManager>.Instance.tabsLoading--;
				GameObject original = (GameObject)go;
				original = Object.Instantiate(original);
				modulesGO[index] = original;
				UIModule component = original.GetComponent<UIModule>();
				tab.Add(component.moduleId, component);
			});
		}
	}

	public void ParentModules()
	{
		for (int i = 0; i < modulesGO.Count; i++)
		{
			modulesGO[i].transform.SetParent(anchor, worldPositionStays: false);
		}
		Object.Destroy(this);
	}
}
