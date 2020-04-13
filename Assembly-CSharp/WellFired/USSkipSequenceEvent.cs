using UnityEngine;

namespace WellFired
{
    [USequencerEventHideDuration]
    [USequencerFriendlyName("Skip uSequence")]
    [USequencerEvent("Sequence/Skip uSequence")]
    public class USSkipSequenceEvent : USEventBase
    {
        public USSequencer sequence;

        public bool skipToEnd = true;

        public float skipToTime = -1f;

        public USSkipSequenceEvent()
            : this()
        {
        }

        public override void FireEvent()
        {
            if (!(Object)(object)sequence)
            {
                Debug.LogWarning("No sequence for USSkipSequenceEvent : " + base.name, (Object)(object)this);
            }
            else if (!skipToEnd && skipToTime < 0f && skipToTime > sequence.get_Duration())
            {
                Debug.LogWarning("You haven't set the properties correctly on the Sequence for this USSkipSequenceEvent, either the skipToTime is invalid, or you haven't flagged it to skip to the end", (Object)(object)this);
            }
            else if (skipToEnd)
            {
                sequence.SkipTimelineTo(sequence.get_Duration());
            }
            else
            {
                sequence.SkipTimelineTo(skipToTime);
            }
        }

        public override void ProcessEvent(float deltaTime)
        {
        }
    }
}
