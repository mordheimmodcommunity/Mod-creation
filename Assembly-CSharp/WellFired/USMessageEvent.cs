using UnityEngine;

namespace WellFired
{
    [USequencerEventHideDuration]
    [USequencerEvent("Debug/Log Message")]
    [USequencerFriendlyName("Debug Message")]
    public class USMessageEvent : USEventBase
    {
        public string message = "Default Message";

        public USMessageEvent()
            : this()
        {
        }

        public override void FireEvent()
        {
            Debug.Log(message);
        }

        public override void ProcessEvent(float deltaTime)
        {
        }
    }
}
