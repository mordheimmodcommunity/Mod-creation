using Flowmap;
using UnityEngine;

[AddComponentMenu("Flowmaps/Fields/Heightmap")]
public class HeightmapField : FlowSimulationField
{
	public override FieldPass Pass => FieldPass.Heightmap;

	protected override Shader RenderShader => Shader.Find("Hidden/HeightmapFieldPreview");
}
