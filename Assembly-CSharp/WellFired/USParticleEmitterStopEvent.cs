using UnityEngine;

namespace WellFired
{
	[USequencerEventHideDuration]
	[USequencerEvent("Particle System/Stop Emitter")]
	[USequencerFriendlyName("Stop Emitter (Legacy)")]
	public class USParticleEmitterStopEvent : USEventBase
	{
		public USParticleEmitterStopEvent()
			: this()
		{
		}

		public override void FireEvent()
		{
			ParticleSystem component = ((USEventBase)this).get_AffectedObject().GetComponent<ParticleSystem>();
			if (!component)
			{
				Debug.Log("Attempting to emit particles, but the object has no particleSystem USParticleEmitterStartEvent::FireEvent");
			}
			else
			{
				component.Stop();
			}
		}

		public override void ProcessEvent(float deltaTime)
		{
		}
	}
}
