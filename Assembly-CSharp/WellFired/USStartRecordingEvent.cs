using UnityEngine;

namespace WellFired
{
    [USequencerEvent("Recording/Start Recording")]
    [USequencerEventHideDuration]
    [USequencerFriendlyName("Start Recording")]
    public class USStartRecordingEvent : USEventBase
    {
        public USStartRecordingEvent()
            : this()
        {
        }

        public override void FireEvent()
        {
            if (!Application.isPlaying)
            {
                Debug.Log("Recording events only work when in play mode");
            }
            else
            {
                USRuntimeUtility.StartRecordingSequence(((USEventBase)this).get_Sequence(), USRecordRuntimePreferences.get_CapturePath(), USRecord.GetFramerate(), USRecord.GetUpscaleAmount());
            }
        }

        public override void ProcessEvent(float deltaTime)
        {
        }
    }
}
