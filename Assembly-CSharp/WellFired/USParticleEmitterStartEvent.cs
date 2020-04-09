using UnityEngine;

namespace WellFired
{
	[USequencerEvent("Particle System/Start Emitter")]
	[USequencerEventHideDuration]
	[USequencerFriendlyName("Start Emitter (Legacy)")]
	public class USParticleEmitterStartEvent : USEventBase
	{
		public USParticleEmitterStartEvent()
			: this()
		{
		}

		public void Update()
		{
			if ((bool)((USEventBase)this).get_AffectedObject())
			{
				ParticleSystem component = ((USEventBase)this).get_AffectedObject().GetComponent<ParticleSystem>();
				if ((bool)component)
				{
					((USEventBase)this).set_Duration(component.duration + component.startLifetime);
				}
			}
		}

		public override void FireEvent()
		{
			if ((bool)((USEventBase)this).get_AffectedObject())
			{
				ParticleSystem component = ((USEventBase)this).get_AffectedObject().GetComponent<ParticleSystem>();
				if (!component)
				{
					Debug.Log("Attempting to emit particles, but the object has no particleSystem USParticleEmitterStartEvent::FireEvent");
				}
				else if (Application.isPlaying)
				{
					component.Play();
				}
			}
		}

		public override void ProcessEvent(float deltaTime)
		{
			if (!Application.isPlaying)
			{
				ParticleSystem component = ((USEventBase)this).get_AffectedObject().GetComponent<ParticleSystem>();
				component.Simulate(deltaTime);
			}
		}

		public override void StopEvent()
		{
			UndoEvent();
		}

		public override void UndoEvent()
		{
			if ((bool)((USEventBase)this).get_AffectedObject())
			{
				ParticleSystem component = ((USEventBase)this).get_AffectedObject().GetComponent<ParticleSystem>();
				if ((bool)component)
				{
					component.Stop();
				}
			}
		}
	}
}
