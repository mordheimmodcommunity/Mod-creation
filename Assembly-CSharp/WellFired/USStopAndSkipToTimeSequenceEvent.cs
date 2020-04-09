using UnityEngine;

namespace WellFired
{
	[USequencerEventHideDuration]
	[USequencerEvent("Sequence/Stop And Skip")]
	[USequencerFriendlyName("Stop and Skip sequencer")]
	public class USStopAndSkipToTimeSequenceEvent : USEventBase
	{
		[SerializeField]
		private USSequencer sequence;

		[SerializeField]
		private float timeToSkipTo;

		public USStopAndSkipToTimeSequenceEvent()
			: this()
		{
		}

		public override void FireEvent()
		{
			if (!(Object)(object)sequence)
			{
				Debug.LogWarning("No sequence for USstopSequenceEvent : " + base.name, (Object)(object)this);
			}
			if ((bool)(Object)(object)sequence)
			{
				sequence.Stop();
				sequence.SkipTimelineTo(timeToSkipTo);
				sequence.UpdateSequencer(0f);
			}
		}

		public override void ProcessEvent(float deltaTime)
		{
		}
	}
}
