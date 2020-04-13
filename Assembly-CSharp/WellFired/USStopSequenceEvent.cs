using UnityEngine;

namespace WellFired
{
    [USequencerEventHideDuration]
    [USequencerFriendlyName("stop uSequence")]
    [USequencerEvent("Sequence/Stop uSequence")]
    public class USStopSequenceEvent : USEventBase
    {
        public USSequencer sequence;

        public USStopSequenceEvent()
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
            }
        }

        public override void ProcessEvent(float deltaTime)
        {
        }
    }
}
