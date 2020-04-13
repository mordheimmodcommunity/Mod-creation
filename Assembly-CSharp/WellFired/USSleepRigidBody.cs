using UnityEngine;

namespace WellFired
{
    [USequencerEvent("Physics/Sleep Rigid Body")]
    [USequencerFriendlyName("Sleep Rigid Body")]
    [USequencerEventHideDuration]
    public class USSleepRigidBody : USEventBase
    {
        public USSleepRigidBody()
            : this()
        {
        }

        public override void FireEvent()
        {
            Rigidbody component = ((USEventBase)this).get_AffectedObject().GetComponent<Rigidbody>();
            if (!component)
            {
                Debug.Log("Attempting to Nullify a force on an object, but it has no rigid body from USSleepRigidBody::FireEvent");
            }
            else
            {
                component.Sleep();
            }
        }

        public override void ProcessEvent(float deltaTime)
        {
        }

        public override void StopEvent()
        {
            UndoEvent();
        }

        public override void UndoEvent()
        {
            if ((bool)((USEventBase)this).get_AffectedObject())
            {
                Rigidbody component = ((USEventBase)this).get_AffectedObject().GetComponent<Rigidbody>();
                if ((bool)component)
                {
                    component.WakeUp();
                }
            }
        }
    }
}
