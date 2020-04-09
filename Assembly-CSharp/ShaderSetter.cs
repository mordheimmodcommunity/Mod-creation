using UnityEngine;

public class ShaderSetter : MonoBehaviour
{
	public Color DiffColor;

	public Color SpecColor;

	public float SpecIntensity;

	public float Sharpness;

	public float FresnelStrength;

	public Color GlowColor;

	public float GlowStrength;

	public void ApplyShaderParams()
	{
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].transform.parent == null || componentsInChildren[i].transform.parent.gameObject.GetComponent<ItemController>() == null)
			{
				Material[] materials = componentsInChildren[i].materials;
				for (int j = 0; j < materials.Length; j++)
				{
					ApplyShaderParams(materials[j]);
				}
			}
		}
	}

	private void ApplyShaderParams(Material mat)
	{
		mat.SetColor("_Color", DiffColor);
		mat.SetColor("_SpecColor", SpecColor);
		mat.SetFloat("_SpecInt", SpecIntensity);
		mat.SetFloat("_Shininess", Sharpness);
		mat.SetFloat("_Fresnel", FresnelStrength);
		if (mat.HasProperty("_Illum"))
		{
			mat.SetColor("_GlowColor", GlowColor);
			mat.SetFloat("_GlowStrength", GlowStrength);
		}
	}
}
