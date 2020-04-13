using UnityEngine;

namespace WellFired
{
    [USequencerFriendlyName("Set Culling Mask")]
    [ExecuteInEditMode]
    [USequencerEventHideDuration]
    [USequencerEvent("Camera/Set Culling Mask")]
    public class USCameraSetCullingMask : USEventBase
    {
        [SerializeField]
        private LayerMask newLayerMask;

        private int prevLayerMask;

        private Camera cameraToAffect;

        public USCameraSetCullingMask()
            : this()
        {
        }

        public override void FireEvent()
        {
            if (((USEventBase)this).get_AffectedObject() != null)
            {
                cameraToAffect = ((USEventBase)this).get_AffectedObject().GetComponent<Camera>();
            }
            if ((bool)cameraToAffect)
            {
                prevLayerMask = cameraToAffect.cullingMask;
                cameraToAffect.cullingMask = newLayerMask;
            }
        }

        public override void ProcessEvent(float deltaTime)
        {
        }

        public override void EndEvent()
        {
        }

        public override void StopEvent()
        {
            UndoEvent();
        }

        public override void UndoEvent()
        {
            if ((bool)cameraToAffect)
            {
                cameraToAffect.cullingMask = prevLayerMask;
            }
        }
    }
}
