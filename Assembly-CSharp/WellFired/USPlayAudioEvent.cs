using UnityEngine;

namespace WellFired
{
	[USequencerEvent("Audio/Play Audio")]
	[USequencerFriendlyName("Play Audio")]
	[USequencerEventHideDuration]
	public class USPlayAudioEvent : USEventBase
	{
		public AudioClip audioClip;

		public bool loop;

		private bool wasPlaying;

		public USPlayAudioEvent()
			: this()
		{
		}

		public void Update()
		{
			if (!loop && (bool)audioClip)
			{
				((USEventBase)this).set_Duration(audioClip.length);
			}
			else
			{
				((USEventBase)this).set_Duration(-1f);
			}
		}

		public override void FireEvent()
		{
			AudioSource audioSource = ((USEventBase)this).get_AffectedObject().GetComponent<AudioSource>();
			if (!audioSource)
			{
				audioSource = ((USEventBase)this).get_AffectedObject().AddComponent<AudioSource>();
				audioSource.playOnAwake = false;
			}
			if (audioSource.clip != audioClip)
			{
				audioSource.clip = audioClip;
			}
			audioSource.time = 0f;
			audioSource.loop = loop;
			if (((USEventBase)this).get_Sequence().get_IsPlaying())
			{
				audioSource.Play();
			}
		}

		public override void ProcessEvent(float deltaTime)
		{
			AudioSource audioSource = ((USEventBase)this).get_AffectedObject().GetComponent<AudioSource>();
			if (!audioSource)
			{
				audioSource = ((USEventBase)this).get_AffectedObject().AddComponent<AudioSource>();
				audioSource.playOnAwake = false;
			}
			if (audioSource.clip != audioClip)
			{
				audioSource.clip = audioClip;
			}
			if (!audioSource.isPlaying)
			{
				audioSource.time = deltaTime;
				if (((USEventBase)this).get_Sequence().get_IsPlaying() && !audioSource.isPlaying)
				{
					audioSource.Play();
				}
			}
		}

		public override void ManuallySetTime(float deltaTime)
		{
			AudioSource component = ((USEventBase)this).get_AffectedObject().GetComponent<AudioSource>();
			if ((bool)component)
			{
				component.time = deltaTime;
			}
		}

		public override void ResumeEvent()
		{
			AudioSource component = ((USEventBase)this).get_AffectedObject().GetComponent<AudioSource>();
			if ((bool)component)
			{
				component.time = ((USEventBase)this).get_Sequence().get_RunningTime() - ((USEventBase)this).get_FireTime();
				if (wasPlaying)
				{
					component.Play();
				}
			}
		}

		public override void PauseEvent()
		{
			AudioSource component = ((USEventBase)this).get_AffectedObject().GetComponent<AudioSource>();
			wasPlaying = false;
			if ((bool)component && component.isPlaying)
			{
				wasPlaying = true;
			}
			if ((bool)component)
			{
				component.Pause();
			}
		}

		public override void StopEvent()
		{
			UndoEvent();
		}

		public override void EndEvent()
		{
			UndoEvent();
		}

		public override void UndoEvent()
		{
			if ((bool)((USEventBase)this).get_AffectedObject())
			{
				AudioSource component = ((USEventBase)this).get_AffectedObject().GetComponent<AudioSource>();
				if ((bool)component)
				{
					component.Stop();
				}
			}
		}
	}
}
