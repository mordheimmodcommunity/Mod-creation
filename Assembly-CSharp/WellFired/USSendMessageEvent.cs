using UnityEngine;

namespace WellFired
{
	[USequencerFriendlyName("Send Message")]
	[USequencerEventHideDuration]
	[USequencerEvent("Signal/Send Message")]
	public class USSendMessageEvent : USEventBase
	{
		public GameObject receiver;

		public string action = "OnSignal";

		public USSendMessageEvent()
			: this()
		{
		}

		public override void FireEvent()
		{
			if (Application.isPlaying)
			{
				if ((bool)receiver)
				{
					receiver.SendMessage(action);
				}
				else
				{
					Debug.LogWarning($"No receiver of signal \"{action}\" on object {receiver.name} ({receiver.GetType().Name})", receiver);
				}
			}
		}

		public override void ProcessEvent(float deltaTime)
		{
		}
	}
}
