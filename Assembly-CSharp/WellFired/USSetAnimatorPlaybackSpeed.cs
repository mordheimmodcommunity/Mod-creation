using UnityEngine;

namespace WellFired
{
	[USequencerEventHideDuration]
	[USequencerFriendlyName("Set Playback Speed")]
	[USequencerEvent("Animation (Mecanim)/Animator/Set Playback Speed")]
	public class USSetAnimatorPlaybackSpeed : USEventBase
	{
		public float playbackSpeed = 1f;

		private float prevPlaybackSpeed;

		public USSetAnimatorPlaybackSpeed()
			: this()
		{
		}

		public override void FireEvent()
		{
			Animator component = ((USEventBase)this).get_AffectedObject().GetComponent<Animator>();
			if (!component)
			{
				Debug.LogWarning("Affected Object has no Animator component, for uSequencer Event", (Object)(object)this);
				return;
			}
			prevPlaybackSpeed = component.speed;
			component.speed = playbackSpeed;
		}

		public override void ProcessEvent(float runningTime)
		{
		}

		public override void StopEvent()
		{
			UndoEvent();
		}

		public override void UndoEvent()
		{
			Animator component = ((USEventBase)this).get_AffectedObject().GetComponent<Animator>();
			if ((bool)component)
			{
				component.speed = prevPlaybackSpeed;
			}
		}
	}
}
