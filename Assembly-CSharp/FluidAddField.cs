using Flowmap;
using UnityEngine;

[AddComponentMenu("Flowmaps/Fields/Add fluid")]
public class FluidAddField : FlowSimulationField
{
	public override FieldPass Pass => FieldPass.AddFluid;

	protected override Shader RenderShader => Shader.Find("Hidden/AddFluidPreview");
}
