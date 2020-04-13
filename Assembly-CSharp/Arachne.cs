using System.Collections.Generic;
using UnityEngine;

public class Arachne : MonoBehaviour
{
    public List<BoneId> bones;

    public void Init()
    {
        List<ClothSphereColliderPair> list = new List<ClothSphereColliderPair>();
        List<CapsuleCollider> list2 = new List<CapsuleCollider>();
        Cloth component = GetComponent<Cloth>();
        UnitMenuController component2 = base.transform.parent.gameObject.GetComponent<UnitMenuController>();
        for (int i = 0; i < bones.Count; i++)
        {
            Collider[] components = component2.BonesTr[bones[i]].GetComponents<Collider>();
            component2.BonesTr[bones[i]].gameObject.layer = LayerMask.NameToLayer("cloth");
            for (int j = 0; j < components.Length; j++)
            {
                if (components[j] is CapsuleCollider)
                {
                    list2.Add((CapsuleCollider)components[j]);
                }
                else if (components[j] is SphereCollider)
                {
                    list.Add(new ClothSphereColliderPair((SphereCollider)components[j]));
                }
            }
        }
        component.sphereColliders = list.ToArray();
        component.capsuleColliders = list2.ToArray();
        Object.Destroy(this);
    }
}
