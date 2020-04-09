using UnityEngine;

namespace WellFired
{
	[USequencerFriendlyName("Set Ambient Light")]
	[USequencerEventHideDuration]
	[USequencerEvent("Light/Set Ambient Light")]
	public class USSetAmbientLightEvent : USEventBase
	{
		public Color lightColor = Color.red;

		private Color prevLightColor;

		public USSetAmbientLightEvent()
			: this()
		{
		}

		public override void FireEvent()
		{
			prevLightColor = RenderSettings.ambientLight;
			RenderSettings.ambientLight = lightColor;
		}

		public override void ProcessEvent(float deltaTime)
		{
		}

		public override void StopEvent()
		{
			UndoEvent();
		}

		public override void UndoEvent()
		{
			RenderSettings.ambientLight = prevLightColor;
		}
	}
}
