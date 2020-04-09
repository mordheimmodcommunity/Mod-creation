using UnityEngine;

namespace Prometheus
{
	[ExecuteInEditMode]
	public class AnimateCurve : MonoBehaviour
	{
		public bool useWorldSpace;

		public Vector3 rotPerSec = new Vector3(0f, 360f, 0f);

		private void Update()
		{
			Vector3 eulerAngles = rotPerSec * Time.deltaTime;
			base.transform.Rotate(eulerAngles, (!useWorldSpace) ? Space.Self : Space.World);
		}
	}
}
