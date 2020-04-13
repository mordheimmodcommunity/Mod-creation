using System.Collections.Generic;
using UnityEngine;

public class MeshBatcher : MonoBehaviour
{
    public List<float> lodValues = new List<float>
    {
        0.4f,
        0.3f,
        0.02f
    };

    private List<LODMeshGroup> meshGroups;

    public bool batchOnAwake;

    private void Start()
    {
        if (batchOnAwake)
        {
            Batch();
        }
    }

    public void Batch()
    {
        TryBatch();
    }

    public void TryBatch()
    {
        meshGroups = new List<LODMeshGroup>();
        for (int i = 0; i < lodValues.Count; i++)
        {
            meshGroups.Add(new LODMeshGroup());
        }
        MeshBatcherBlocker[] componentsInChildren = GetComponentsInChildren<MeshBatcherBlocker>();
        List<GameObject> list = new List<GameObject>();
        for (int j = 0; j < componentsInChildren.Length; j++)
        {
            list.Add(componentsInChildren[j].gameObject);
        }
        ParseTransform(list, base.transform);
        int count = lodValues.Count;
        LOD[] array = new LOD[count];
        for (int k = 0; k < meshGroups.Count; k++)
        {
            LODMeshGroup lODMeshGroup = meshGroups[k];
            List<Renderer> list2 = new List<Renderer>();
            foreach (Material key in lODMeshGroup.materialInstances.Keys)
            {
                GameObject gameObject = new GameObject();
                gameObject.name = key.name + "_LOD" + k;
                gameObject.transform.SetParent(base.transform);
                Mesh mesh = new Mesh();
                mesh.CombineMeshes(lODMeshGroup.materialInstances[key].ToArray());
                mesh.Optimize();
                MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = mesh;
                MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
                meshRenderer.sharedMaterial = key;
                gameObject.SetLayerRecursively(base.gameObject.layer);
                list2.Add(gameObject.GetComponent<Renderer>());
                mesh.UploadMeshData(markNoLogerReadable: true);
                gameObject.isStatic = true;
            }
            array[k] = new LOD(lodValues[k], list2.ToArray());
        }
        LODGroup lODGroup = base.gameObject.GetComponent<LODGroup>();
        if (lODGroup == null)
        {
            lODGroup = base.gameObject.AddComponent<LODGroup>();
        }
        lODGroup.SetLODs(array);
        lODGroup.RecalculateBounds();
        lODGroup.fadeMode = LODFadeMode.CrossFade;
        lODGroup.animateCrossFading = true;
        Object.Destroy(this);
    }

    private void ParseTransform(List<GameObject> lockedObjects, Transform trans)
    {
        List<Transform> list = new List<Transform>();
        for (int i = 0; i < trans.childCount; i++)
        {
            Transform child = trans.GetChild(i);
            list.Add(child);
        }
        list.Sort(new TransformComparer());
        for (int j = 0; j < list.Count; j++)
        {
            Transform transform = list[j];
            if (lockedObjects.IndexOf(transform.gameObject) != -1)
            {
                continue;
            }
            if (transform.name.Contains("LOD"))
            {
                int num = (int)char.GetNumericValue(transform.name[transform.name.Length - 1]);
                MeshFilter component = transform.GetComponent<MeshFilter>();
                Material material = null;
                Mesh mesh = null;
                if (num > 0 && (component == null || component.sharedMesh == null))
                {
                    string value = transform.name.Substring(0, transform.name.LastIndexOf("_"));
                    foreach (KeyValuePair<Material, List<CombineInstance>> materialInstance in meshGroups[num - 1].materialInstances)
                    {
                        for (int k = 0; k < materialInstance.Value.Count; k++)
                        {
                            if (materialInstance.Value[k].mesh.name.Contains(value))
                            {
                                material = materialInstance.Key;
                                mesh = materialInstance.Value[k].mesh;
                            }
                        }
                    }
                }
                if (component != null && component.sharedMesh != null)
                {
                    material = component.GetComponent<Renderer>().sharedMaterial;
                    mesh = component.sharedMesh;
                }
                if (mesh == null)
                {
                    PandoraDebug.LogWarning("Transform : " + transform.name + " contains no mesh", "MESH_BATCHER", this);
                    continue;
                }
                if (material == null)
                {
                    PandoraDebug.LogWarning("Transform : " + transform.name + " contains no material", "MESH_BATCHER", this);
                    continue;
                }
                if (!meshGroups[num].materialInstances.ContainsKey(material))
                {
                    meshGroups[num].materialInstances[material] = new List<CombineInstance>();
                }
                CombineInstance item = default(CombineInstance);
                item.mesh = mesh;
                item.transform = transform.localToWorldMatrix;
                meshGroups[num].materialInstances[material].Add(item);
                if ((bool)transform.gameObject.GetComponent<Collider>() && component != null)
                {
                    MeshRenderer component2 = component.gameObject.GetComponent<MeshRenderer>();
                    if (component2 != null)
                    {
                        Object.DestroyImmediate(component2);
                    }
                    Object.DestroyImmediate(component);
                }
                else
                {
                    Object.DestroyImmediate(transform.gameObject);
                }
            }
            else if (transform.childCount > 0)
            {
                ParseTransform(lockedObjects, transform);
            }
        }
    }
}
