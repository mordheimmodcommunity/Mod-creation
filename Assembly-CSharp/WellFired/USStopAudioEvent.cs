using UnityEngine;

namespace WellFired
{
    [USequencerFriendlyName("Stop Audio")]
    [USequencerEventHideDuration]
    [USequencerEvent("Audio/Stop Audio")]
    public class USStopAudioEvent : USEventBase
    {
        public USStopAudioEvent()
            : this()
        {
        }

        public override void FireEvent()
        {
            if (!((USEventBase)this).get_AffectedObject())
            {
                Debug.Log("USSequencer is trying to play an audio clip, but you didn't give it Audio To Play from USPlayAudioEvent::FireEvent");
                return;
            }
            AudioSource component = ((USEventBase)this).get_AffectedObject().GetComponent<AudioSource>();
            if (!component)
            {
                Debug.Log("USSequencer is trying to play an audio source, but the GameObject doesn't contain an AudioClip from USPlayAudioEvent::FireEvent");
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
