using UnityEngine;

namespace WellFired
{
	[USequencerEvent("Sequence/Set Playback Rate")]
	[USequencerEventHideDuration]
	[USequencerFriendlyName("Set uSequence Playback Rate")]
	public class USSetPlaybackRateEvent : USEventBase
	{
		public USSequencer sequence;

		public float playbackRate = 1f;

		private float prevPlaybackRate = 1f;

		public USSetPlaybackRateEvent()
			: this()
		{
		}

		public override void FireEvent()
		{
			if (!(Object)(object)sequence)
			{
				Debug.LogWarning("No sequence for USSetPlaybackRate : " + base.name, (Object)(object)this);
			}
			if ((bool)(Object)(object)sequence)
			{
				prevPlaybackRate = sequence.get_PlaybackRate();
				sequence.set_PlaybackRate(playbackRate);
			}
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
			if ((bool)(Object)(object)sequence)
			{
				sequence.set_PlaybackRate(prevPlaybackRate);
			}
		}
	}
}
