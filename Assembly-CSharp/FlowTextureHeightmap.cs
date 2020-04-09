using Flowmap;
using UnityEngine;

[AddComponentMenu("Flowmaps/Heightmap/Texture")]
public class FlowTextureHeightmap : FlowHeightmap
{
	[SerializeField]
	private Texture2D heightmap;

	public bool isRaw;

	private Texture2D rawPreview;

	public override Texture HeightmapTexture
	{
		get
		{
			return heightmap;
		}
		set
		{
			heightmap = (value as Texture2D);
		}
	}

	public override Texture PreviewHeightmapTexture
	{
		get
		{
			if (isRaw)
			{
				if (rawPreview == null)
				{
					GenerateRawPreview();
				}
				return rawPreview;
			}
			return HeightmapTexture;
		}
	}

	public void GenerateRawPreview()
	{
		if ((bool)rawPreview)
		{
			Object.DestroyImmediate(rawPreview);
		}
		if ((bool)heightmap)
		{
			rawPreview = TextureUtilities.GetRawPreviewTexture(heightmap);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)rawPreview)
		{
			Object.DestroyImmediate(rawPreview);
		}
	}
}
