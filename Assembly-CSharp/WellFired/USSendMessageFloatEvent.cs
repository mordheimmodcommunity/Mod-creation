using UnityEngine;

namespace WellFired
{
    [USequencerEvent("Signal/Send Message (Float)")]
    [USequencerFriendlyName("Send Message (Float)")]
    [USequencerEventHideDuration]
    public class USSendMessageFloatEvent : USEventBase
    {
        public GameObject receiver;

        public string action = "OnSignal";

        [SerializeField]
        private float valueToSend;

        public USSendMessageFloatEvent()
            : this()
        {
        }

        public override void FireEvent()
        {
            if (Application.isPlaying)
            {
                if ((bool)receiver)
                {
                    receiver.SendMessage(action, valueToSend);
                }
                else
                {
                    Debug.LogWarning($"No receiver of signal \"{action}\" on object {receiver.name} ({receiver.GetType().Name})", receiver);
                }
            }
        }

        public override void ProcessEvent(float deltaTime)
        {
        }
    }
}
