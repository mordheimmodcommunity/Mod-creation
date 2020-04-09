using UnityEngine;

namespace WellFired
{
	[USequencerEvent("Sequence/Pause uSequence")]
	[USequencerEventHideDuration]
	[USequencerFriendlyName("Pause uSequence")]
	public class USPauseSequenceEvent : USEventBase
	{
		public USSequencer sequence;

		public USPauseSequenceEvent()
			: this()
		{
		}

		public override void FireEvent()
		{
			if (!(Object)(object)sequence)
			{
				Debug.LogWarning("No sequence for USPauseSequenceEvent : " + base.name, (Object)(object)this);
			}
			if ((bool)(Object)(object)sequence)
			{
				sequence.Pause();
			}
		}

		public override void ProcessEvent(float deltaTime)
		{
		}
	}
}
