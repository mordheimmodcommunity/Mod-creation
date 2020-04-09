using UnityEngine;

[ExecuteInEditMode]
public class SetCameraDepth : MonoBehaviour
{
	[SerializeField]
	private DepthTextureMode depthMode;

	private void Update()
	{
		GetComponent<Camera>().depthTextureMode = depthMode;
	}
}
