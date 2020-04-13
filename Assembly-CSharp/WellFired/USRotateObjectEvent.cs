using UnityEngine;

namespace WellFired
{
    [USequencerFriendlyName("Rotate")]
    [USequencerEvent("Transform/Rotate Object")]
    public class USRotateObjectEvent : USEventBase
    {
        public float rotateSpeedPerSecond = 90f;

        public Vector3 rotationAxis = Vector3.up;

        private Quaternion sourceOrientation = Quaternion.identity;

        private Quaternion previousRotation = Quaternion.identity;

        public USRotateObjectEvent()
            : this()
        {
        }

        public override void FireEvent()
        {
            previousRotation = ((USEventBase)this).get_AffectedObject().transform.rotation;
            sourceOrientation = ((USEventBase)this).get_AffectedObject().transform.rotation;
        }

        public override void ProcessEvent(float deltaTime)
        {
            ((USEventBase)this).get_AffectedObject().transform.rotation = sourceOrientation;
            ((USEventBase)this).get_AffectedObject().transform.Rotate(rotationAxis, rotateSpeedPerSecond * deltaTime);
        }

        public override void StopEvent()
        {
            UndoEvent();
        }

        public override void UndoEvent()
        {
            if ((bool)((USEventBase)this).get_AffectedObject())
            {
                ((USEventBase)this).get_AffectedObject().transform.rotation = previousRotation;
            }
        }

        public void Update()
        {
            if (((USEventBase)this).get_Duration() < 0f)
            {
                ((USEventBase)this).set_Duration(4f);
            }
        }
    }
}
