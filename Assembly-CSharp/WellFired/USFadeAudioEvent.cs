using UnityEngine;

namespace WellFired
{
    [USequencerEvent("Audio/Fade Audio")]
    [USequencerFriendlyName("Fade Audio")]
    [USequencerEventHideDuration]
    public class USFadeAudioEvent : USEventBase
    {
        private float previousVolume = 1f;

        public AnimationCurve fadeCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));

        public USFadeAudioEvent()
            : this()
        {
        }

        public void Update()
        {
            ((USEventBase)this).set_Duration((float)fadeCurve.length);
        }

        public override void FireEvent()
        {
            AudioSource component = ((USEventBase)this).get_AffectedObject().GetComponent<AudioSource>();
            if (!component)
            {
                Debug.LogWarning("Trying to fade audio on an object without an AudioSource");
            }
            else
            {
                previousVolume = component.volume;
            }
        }

        public override void ProcessEvent(float deltaTime)
        {
            AudioSource component = ((USEventBase)this).get_AffectedObject().GetComponent<AudioSource>();
            if (!component)
            {
                Debug.LogWarning("Trying to fade audio on an object without an AudioSource");
            }
            else
            {
                component.volume = fadeCurve.Evaluate(deltaTime);
            }
        }

        public override void StopEvent()
        {
            UndoEvent();
        }

        public override void UndoEvent()
        {
            AudioSource component = ((USEventBase)this).get_AffectedObject().GetComponent<AudioSource>();
            if (!component)
            {
                Debug.LogWarning("Trying to fade audio on an object without an AudioSource");
            }
            else
            {
                component.volume = previousVolume;
            }
        }
    }
}
