using UnityEngine;

namespace WellFired
{
	[USequencerEvent("Signal/Send Message (String)")]
	[USequencerEventHideDuration]
	[USequencerFriendlyName("Send Message (String)")]
	public class USSendMessageStringEvent : USEventBase
	{
		public GameObject receiver;

		public string action = "OnSignal";

		[SerializeField]
		private string valueToSend;

		public USSendMessageStringEvent()
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
