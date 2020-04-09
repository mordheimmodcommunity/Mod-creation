using Flowmap;
using UnityEngine;

[AddComponentMenu("Flowmaps/Fields/Remove fluid")]
public class FluidRemoveField : FlowSimulationField
{
	public override FieldPass Pass => FieldPass.RemoveFluid;

	protected override Shader RenderShader => Shader.Find("Hidden/RemoveFluidPreview");
}
