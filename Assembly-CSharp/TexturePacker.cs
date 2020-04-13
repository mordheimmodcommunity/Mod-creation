using System.Collections.Generic;
using UnityEngine;

public class TexturePacker : MonoBehaviour
{
    private const string NORMAL_NAME = "_BumpMap";

    private const string SPEC_NAME = "_Spec_Gloss_Reflec_Masks";

    private void Start()
    {
    }

    public void Merge()
    {
        MeshRenderer component = base.gameObject.GetComponent<MeshRenderer>();
        List<Texture2D> list = new List<Texture2D>();
        List<Texture2D> list2 = new List<Texture2D>();
        List<Texture2D> list3 = new List<Texture2D>();
        Material[] materials = component.materials;
        Material[] array = materials;
        foreach (Material material in array)
        {
            list.Add(material.mainTexture as Texture2D);
            list2.Add(material.GetTexture("_BumpMap") as Texture2D);
            list3.Add(material.GetTexture("_Spec_Gloss_Reflec_Masks") as Texture2D);
        }
        Texture2D texture2D = new Texture2D(1, 1);
        Texture2D texture2D2 = new Texture2D(1, 1);
        Texture2D texture2D3 = new Texture2D(1, 1);
        Rect[] array2 = texture2D.PackTextures(list.ToArray(), 0);
        texture2D2.PackTextures(list2.ToArray(), 0);
        texture2D3.PackTextures(list3.ToArray(), 0);
        MeshFilter component2 = base.gameObject.GetComponent<MeshFilter>();
        Vector2[] uv = component2.mesh.uv;
        Vector2[] array3 = new Vector2[uv.Length];
        int num = 0;
        for (int j = 0; j < array3.Length; j++)
        {
            array3[j].x = Mathf.Lerp(array2[num].xMin, array2[num].xMax, uv[j].x);
            array3[j].y = Mathf.Lerp(array2[num].yMin, array2[num].yMax, uv[j].y);
        }
        Material material2 = new Material(Shader.Find("HardSurface/Hardsurface Free/Opaque Specular"));
        material2.mainTexture = texture2D;
        material2.SetTexture("_BumpMap", texture2D2);
        material2.SetTexture("_Spec_Gloss_Reflec_Masks", texture2D3);
        component.materials = new Material[1]
        {
            material2
        };
    }
}
