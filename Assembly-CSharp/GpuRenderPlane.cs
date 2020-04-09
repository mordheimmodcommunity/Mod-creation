using UnityEngine;

[ExecuteInEditMode]
public class GpuRenderPlane : MonoBehaviour
{
	public FlowSimulationField field;

	private void Update()
	{
		if (field == null)
		{
			if (Application.isPlaying && (bool)base.gameObject)
			{
				Object.Destroy(base.gameObject);
			}
			else
			{
				Object.DestroyImmediate(base.gameObject);
			}
		}
		else if (field.RenderPlane != this)
		{
			if (Application.isPlaying && (bool)base.gameObject)
			{
				Object.Destroy(base.gameObject);
			}
			else
			{
				Object.DestroyImmediate(base.gameObject);
			}
		}
	}
}
