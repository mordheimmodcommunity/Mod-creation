using UnityEngine;

namespace WellFired
{
    [USequencerEventHideDuration]
    [USequencerFriendlyName("Play uSequence")]
    [USequencerEvent("Sequence/Play uSequence")]
    public class USPlaySequenceEvent : USEventBase
    {
        public USSequencer sequence;

        public bool restartSequencer;

        public USPlaySequenceEvent()
            : this()
        {
        }

        public override void FireEvent()
        {
            if (!(Object)(object)sequence)
            {
                Debug.LogWarning("No sequence for USPlaySequenceEvent : " + base.name, (Object)(object)this);
                return;
            }
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Sequence playback controls are not supported in the editor, but will work in game, just fine.");
                return;
            }
            if (!restartSequencer)
            {
                sequence.Play();
                return;
            }
            sequence.set_RunningTime(0f);
            sequence.Play();
        }

        public override void ProcessEvent(float deltaTime)
        {
        }
    }
}
