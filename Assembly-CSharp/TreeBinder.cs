using RAIN.BehaviorTrees;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TreeBinder : PandoraSingleton<TreeBinder>
{
    public List<BTAssetBinding> BtBindings
    {
        get;
        private set;
    }

    private void Awake()
    {
        //IL_001d: Unknown result type (might be due to invalid IL or missing references)
        //IL_0023: Expected O, but got Unknown
        BTAsset[] array = Resources.LoadAll<BTAsset>("ai");
        BtBindings = new List<BTAssetBinding>();
        for (int i = 0; i < array.Length; i++)
        {
            BTAssetBinding val = (BTAssetBinding)(object)new BTAssetBinding();
            val.binding = ((Object)(object)array[i]).name;
            val.behaviorTree = array[i];
            BtBindings.Add(val);
        }
    }
}
