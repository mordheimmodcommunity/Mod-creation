using Prometheus;
using System.Collections.Generic;
using UnityEngine;

public class SkinnedMeshCombiner : MonoBehaviour
{
	public const string NORMAL_NAME = "_BumpMap";

	public const string SPEC_NAME = "_SpecTex";

	public const string OLD_SPEC_NAME = "_SpecMap";

	public const string EMI_NAME = "_Illum";

	private int maxAtlasSize = 2048;

	public List<LODConfig> lodConfigs;

	public List<SkinnedMeshRenderer> smRenderers = new List<SkinnedMeshRenderer>();

	private Rect[] packingResult;

	private Material combinedMat;

	private Shader oldShader;

	private Texture2D diffuseAtlas;

	private Texture2D normalAtlas;

	private Texture2D specAtlas;

	private Texture2D emiAtlas;

	private Vector3 oldPos;

	private Quaternion oldRot;

	private float mergeTime;

	private void Awake()
	{
	}

	public void MergeNoAtlas()
	{
		PandoraDebug.LogInfo("Merging " + base.name + " !", "GRAPHICS", this);
		PrePass();
		AttachAttachers();
		List<List<CombineInstance>> list = new List<List<CombineInstance>>();
		List<List<Material>> list2 = new List<List<Material>>();
		for (int i = 0; i < lodConfigs.Count; i++)
		{
			list.Add(new List<CombineInstance>());
			list2.Add(new List<Material>());
		}
		if (smRenderers.Count > 0)
		{
			for (int j = 0; j < smRenderers.Count; j++)
			{
				if (smRenderers[j].name.Contains("_LOD"))
				{
					SkinnedMeshRenderer skinnedMeshRenderer = smRenderers[j];
					skinnedMeshRenderer.transform.rotation = Quaternion.identity;
					skinnedMeshRenderer.transform.position = Vector3.zero;
					int index = (int)char.GetNumericValue(skinnedMeshRenderer.name[skinnedMeshRenderer.name.Length - 1]);
					list2[index].Add(skinnedMeshRenderer.sharedMaterial);
					CombineInstance item = default(CombineInstance);
					item.mesh = skinnedMeshRenderer.sharedMesh;
					item.transform = skinnedMeshRenderer.transform.localToWorldMatrix;
					list[index].Add(item);
				}
				else
				{
					PandoraDebug.LogError(smRenderers[j].name + " is not conform", "MERGE", this);
				}
			}
			LOD[] array = new LOD[lodConfigs.Count];
			for (int k = 0; k < lodConfigs.Count; k++)
			{
				Mesh mesh = new Mesh();
				mesh.CombineMeshes(list[k].ToArray(), mergeSubMeshes: false);
				mesh.RecalculateBounds();
				GameObject gameObject = new GameObject(base.gameObject.name + "_LOD" + k);
				gameObject.transform.SetParent(base.transform);
				gameObject.SetLayerRecursively(LayerMask.NameToLayer("characters"));
				SkinnedMeshRenderer skinnedMeshRenderer2 = gameObject.AddComponent<SkinnedMeshRenderer>();
				skinnedMeshRenderer2.sharedMesh = mesh;
				skinnedMeshRenderer2.sharedMaterials = list2[k].ToArray();
				array[k] = new LOD(lodConfigs[k].screenRelativeTransitionHeight, new Renderer[1]
				{
					skinnedMeshRenderer2
				});
			}
			InitLODGroup(array);
		}
		FinalPass();
	}

	private void InitLODGroup(LOD[] lods)
	{
		LODGroup lODGroup = GetComponent<LODGroup>();
		if (lODGroup == null)
		{
			lODGroup = base.gameObject.AddComponent<LODGroup>();
		}
		lODGroup.fadeMode = LODFadeMode.CrossFade;
		lODGroup.animateCrossFading = true;
		lODGroup.SetLODs(lods);
		lODGroup.RecalculateBounds();
	}

	public void AttachAttachers()
	{
		BodyPartAttacher[] componentsInChildren = GetComponentsInChildren<BodyPartAttacher>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			AttachGameObject(componentsInChildren[i].gameObject);
		}
	}

	public List<GameObject> AttachGameObject(GameObject go, bool noLOD = false)
	{
		List<GameObject> parts = new List<GameObject>();
		if (noLOD)
		{
			LODGroup component = go.GetComponent<LODGroup>();
			if (component != null)
			{
				Object.Destroy(component);
			}
		}
		SkinnedMeshRenderer[] componentsInChildren = go.GetComponentsInChildren<SkinnedMeshRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			bool flag = componentsInChildren[i].GetComponent<Cloth>() != null;
			if (!flag && noLOD && !componentsInChildren[i].name.Contains("LOD0"))
			{
				Object.DestroyImmediate(componentsInChildren[i].gameObject);
				continue;
			}
			AttachSkinMesh(componentsInChildren[i]);
			parts.Add(componentsInChildren[i].gameObject);
			if (!flag)
			{
				smRenderers.Add(componentsInChildren[i]);
				componentsInChildren[i].sharedMesh.RecalculateBounds();
			}
		}
		OlympusFireStarter component2 = go.GetComponent<OlympusFireStarter>();
		if (component2 != null)
		{
			PandoraSingleton<Prometheus.Prometheus>.Instance.SpawnFx(component2.fxName, GetComponent<UnitMenuController>(), null, delegate(GameObject fx)
			{
				parts.Add(fx);
			});
		}
		Object.Destroy(go);
		return parts;
	}

	private void AttachSkinMesh(SkinnedMeshRenderer smr)
	{
		smr.transform.SetParent(base.transform);
		smr.bones = null;
		Arachne component = smr.gameObject.GetComponent<Arachne>();
		if (component != null)
		{
			component.Init();
		}
	}

	private GameObject MergeLOD(int lodIdx)
	{
		List<CombineInstance> list = new List<CombineInstance>();
		List<Texture2D> list2 = new List<Texture2D>();
		List<Texture2D> list3 = new List<Texture2D>();
		List<Texture2D> list4 = new List<Texture2D>();
		List<Texture2D> list5 = new List<Texture2D>();
		List<Vector2> list6 = new List<Vector2>();
		List<int> list7 = new List<int>();
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			if (renderer.material.HasProperty("_Color"))
			{
				renderer.material.color = renderer.sharedMaterial.color;
			}
		}
		bool flag = false;
		for (int j = 0; j < smRenderers.Count; j++)
		{
			if (smRenderers[j].material.HasProperty("_Illum"))
			{
				flag = true;
				break;
			}
		}
		for (int k = 0; k < smRenderers.Count; k++)
		{
			SkinnedMeshRenderer skinnedMeshRenderer = smRenderers[k];
			if (!skinnedMeshRenderer.name.Contains("_LOD" + lodIdx))
			{
				continue;
			}
			skinnedMeshRenderer.transform.rotation = Quaternion.identity;
			skinnedMeshRenderer.transform.position = Vector3.zero;
			CombineInstance item = default(CombineInstance);
			item.mesh = skinnedMeshRenderer.sharedMesh;
			item.transform = skinnedMeshRenderer.transform.localToWorldMatrix;
			list.Add(item);
			for (int l = 0; l < skinnedMeshRenderer.sharedMesh.uv.Length; l++)
			{
				list6.Add(skinnedMeshRenderer.sharedMesh.uv[l]);
			}
			list7.Add(skinnedMeshRenderer.sharedMesh.uv.Length);
			if (lodIdx != 0)
			{
				continue;
			}
			int num = -1;
			Material material = skinnedMeshRenderer.GetComponent<Renderer>().material;
			Texture2D texture2D = material.mainTexture as Texture2D;
			int width = texture2D.width;
			int height = texture2D.height;
			texture2D.name = width + " " + height + " " + texture2D.name;
			num = list2.IndexOf(texture2D);
			list2.Add(texture2D);
			list3.Add(GetTexture(material, "_BumpMap", width, height));
			list4.Add(GetTexture(material, "_SpecTex", width, height));
			if (flag)
			{
				if (material.HasProperty("_Illum") && material.GetTexture("_Illum") != null)
				{
					list5.Add(GetTexture(material, "_Illum", width, height));
					continue;
				}
				if (num != -1)
				{
					list5.Add(list5[num]);
					continue;
				}
				Texture2D texture2D2 = new Texture2D(width, height, TextureFormat.Alpha8, mipmap: false);
				texture2D2.SetPixels32(new Color32[width * height]);
				texture2D2.Apply();
				texture2D2.name = texture2D.width + " " + texture2D.height + "fakeEmi ";
				list5.Add(texture2D2);
			}
		}
		if (lodIdx == 0)
		{
			diffuseAtlas = new Texture2D(1, 1, TextureFormat.ARGB32, mipmap: false);
			diffuseAtlas.filterMode = FilterMode.Trilinear;
			diffuseAtlas.anisoLevel = 4;
			packingResult = diffuseAtlas.PackTextures(list2.ToArray(), 0, maxAtlasSize, makeNoLongerReadable: false);
			diffuseAtlas.Compress(highQuality: true);
			diffuseAtlas.Apply(updateMipmaps: true, makeNoLongerReadable: true);
			normalAtlas = new Texture2D(1, 1, TextureFormat.RGB24, mipmap: false, linear: true);
			normalAtlas.filterMode = FilterMode.Trilinear;
			normalAtlas.anisoLevel = 4;
			normalAtlas.PackTextures(list3.ToArray(), 0, maxAtlasSize, makeNoLongerReadable: false);
			normalAtlas.Compress(highQuality: true);
			normalAtlas.Apply(updateMipmaps: true, makeNoLongerReadable: true);
			specAtlas = new Texture2D(1, 1, TextureFormat.ARGB32, mipmap: false, linear: true);
			specAtlas.filterMode = FilterMode.Trilinear;
			specAtlas.anisoLevel = 4;
			specAtlas.PackTextures(list4.ToArray(), 0, maxAtlasSize, makeNoLongerReadable: false);
			specAtlas.Compress(highQuality: true);
			specAtlas.Apply(updateMipmaps: true, makeNoLongerReadable: true);
			if (flag)
			{
				Texture2D texture2D3 = new Texture2D(1, 1, TextureFormat.Alpha8, mipmap: false, linear: true);
				texture2D3.PackTextures(list5.ToArray(), 0, maxAtlasSize, makeNoLongerReadable: false);
				emiAtlas = ScaleTexture(texture2D3, TextureFormat.Alpha8, texture2D3.width / 4, texture2D3.height / 4);
			}
		}
		Shader shader = (!flag) ? lodConfigs[lodIdx].shader : lodConfigs[lodIdx].shaderEmissive;
		if (oldShader == null || shader != oldShader)
		{
			combinedMat = new Material(shader);
			combinedMat.mainTexture = diffuseAtlas;
			if (combinedMat.HasProperty("_BumpMap"))
			{
				combinedMat.SetTexture("_BumpMap", normalAtlas);
			}
			if (combinedMat.HasProperty("_SpecTex"))
			{
				combinedMat.SetTexture("_SpecTex", specAtlas);
			}
			if (combinedMat.HasProperty("_Illum"))
			{
				combinedMat.SetTexture("_Illum", emiAtlas);
			}
			oldShader = shader;
		}
		int num2 = 0;
		int num3 = 0;
		for (int m = 0; m < list6.Count; m++)
		{
			if (m == num3 + list7[num2])
			{
				num3 += list7[num2];
				num2++;
			}
			float xMin = packingResult[num2].xMin;
			float xMax = packingResult[num2].xMax;
			Vector2 vector = list6[m];
			float x = Mathf.Lerp(xMin, xMax, vector.x % 1f);
			float yMin = packingResult[num2].yMin;
			float yMax = packingResult[num2].yMax;
			Vector2 vector2 = list6[m];
			float y = Mathf.Lerp(yMin, yMax, vector2.y % 1f);
			list6[m] = new Vector2(x, y);
		}
		GameObject gameObject = new GameObject(base.gameObject.name + "_LOD" + lodIdx);
		gameObject.transform.SetParent(base.transform);
		SkinnedMeshRenderer skinnedMeshRenderer2 = gameObject.AddComponent<SkinnedMeshRenderer>();
		Mesh mesh = new Mesh();
		mesh.CombineMeshes(list.ToArray());
		mesh.uv = list6.ToArray();
		skinnedMeshRenderer2.sharedMesh = mesh;
		skinnedMeshRenderer2.sharedMaterial = combinedMat;
		skinnedMeshRenderer2.updateWhenOffscreen = false;
		mesh.UploadMeshData(markNoLogerReadable: true);
		return gameObject;
	}

	private Texture2D GetTexture(Material mat, string name, int requiredWidth, int requiredHeight, bool overrideName = true)
	{
		Texture2D texture2D = mat.GetTexture(name) as Texture2D;
		if (overrideName)
		{
			texture2D.name = texture2D.width + " " + texture2D.height + " " + texture2D.name;
		}
		return texture2D;
	}

	public void CheckTextures(Material mat)
	{
		Texture2D texture2D = mat.mainTexture as Texture2D;
		int width = texture2D.width;
		int height = texture2D.height;
		GetTexture(mat, "_BumpMap", width, height, overrideName: false);
		GetTexture(mat, "_SpecTex", width, height, overrideName: false);
	}

	private void PrePass()
	{
		oldPos = base.transform.position;
		oldRot = base.transform.rotation;
		base.transform.position = Vector3.zero;
		base.transform.rotation = Quaternion.identity;
		mergeTime = Time.realtimeSinceStartup;
	}

	private void FinalPass()
	{
		for (int i = 0; i < smRenderers.Count; i++)
		{
			Object.Destroy(smRenderers[i].gameObject);
		}
		smRenderers.Clear();
		smRenderers = null;
		mergeTime = Time.realtimeSinceStartup - mergeTime;
		PandoraDebug.LogInfo("MergeTime " + base.name + " = " + mergeTime, "GRAPHICS", this);
		base.transform.position = oldPos;
		base.transform.rotation = oldRot;
		Animator component = GetComponent<Animator>();
		if ((bool)component)
		{
			component.Rebind();
		}
		Object.Destroy(this);
	}

	private void Weld(Mesh mesh, float threshold, List<BoneWeight> boneWeights = null)
	{
		Vector3[] vertices = mesh.vertices;
		List<int> list = new List<int>();
		List<Vector3> list2 = new List<Vector3>();
		List<Vector2> list3 = new List<Vector2>();
		int num = 0;
		Vector3[] array = vertices;
		foreach (Vector3 vector in array)
		{
			bool flag = false;
			foreach (Vector3 item in list2)
			{
				if (Vector3.Distance(item, vector) <= threshold)
				{
					flag = true;
					list.Add(num);
					break;
				}
			}
			if (!flag)
			{
				list2.Add(vector);
				list3.Add(mesh.uv[num]);
			}
			num++;
		}
		int[] triangles = mesh.triangles;
		for (int j = 0; j < triangles.Length; j++)
		{
			for (int k = 0; k < list2.Count; k++)
			{
				if (Vector3.Distance(list2[k], vertices[triangles[j]]) <= threshold)
				{
					triangles[j] = k;
					break;
				}
			}
		}
		if (boneWeights != null)
		{
			for (int num2 = list.Count - 1; num2 >= 0; num2--)
			{
				boneWeights.RemoveAt(list[num2]);
			}
		}
		mesh.triangles = triangles;
		mesh.vertices = list2.ToArray();
		mesh.uv = list3.ToArray();
	}

	private Texture2D ScaleTexture(Texture2D source, TextureFormat format, int targetWidth, int targetHeight)
	{
		Texture2D texture2D = new Texture2D(targetWidth, targetHeight, format, mipmap: true);
		Color[] pixels = texture2D.GetPixels(0);
		float num = 1f / (float)targetWidth;
		float num2 = 1f / (float)targetHeight;
		for (int i = 0; i < pixels.Length; i++)
		{
			pixels[i] = source.GetPixelBilinear(num * ((float)i % (float)targetWidth), num2 * Mathf.Floor(i / targetWidth));
		}
		texture2D.SetPixels(pixels, 0);
		texture2D.Apply(updateMipmaps: true, makeNoLongerReadable: true);
		return texture2D;
	}
}
