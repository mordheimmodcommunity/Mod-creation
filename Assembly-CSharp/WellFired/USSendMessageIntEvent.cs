using UnityEngine;

namespace WellFired
{
	[USequencerFriendlyName("Send Message (Int)")]
	[USequencerEventHideDuration]
	[USequencerEvent("Signal/Send Message (Int)")]
	public class USSendMessageIntEvent : USEventBase
	{
		public GameObject receiver;

		public string action = "OnSignal";

		[SerializeField]
		private int valueToSend;

		public USSendMessageIntEvent()
			: this()
		{
		}

		public override void FireEvent()
		{
			if (Application.isPlaying)
			{
				if ((bool)receiver)
				{
					receiver.SendMessage(action, valueToSend);
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
