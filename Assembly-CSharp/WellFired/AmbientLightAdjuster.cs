using UnityEngine;

namespace WellFired
{
	[ExecuteInEditMode]
	public class AmbientLightAdjuster : MonoBehaviour
	{
		public Color ambientLightColor = Color.red;

		private void Update()
		{
			RenderSettings.ambientLight = ambientLightColor;
		}
	}
}
