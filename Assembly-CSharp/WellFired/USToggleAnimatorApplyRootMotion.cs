using UnityEngine;

namespace WellFired
{
    [USequencerEventHideDuration]
    [USequencerEvent("Animation (Mecanim)/Animator/Toggle Apply Root Motion")]
    [USequencerFriendlyName("Toggle Apply Root Motion")]
    public class USToggleAnimatorApplyRootMotion : USEventBase
    {
        public bool applyRootMotion = true;

        private bool prevApplyRootMotion;

        public USToggleAnimatorApplyRootMotion()
            : this()
        {
        }

        public override void FireEvent()
        {
            Animator component = ((USEventBase)this).get_AffectedObject().GetComponent<Animator>();
            if (!component)
            {
                Debug.LogWarning("Affected Object has no Animator component, for uSequencer Event", (Object)(object)this);
                return;
            }
            prevApplyRootMotion = component.applyRootMotion;
            component.applyRootMotion = applyRootMotion;
        }

        public override void ProcessEvent(float runningTime)
        {
        }

        public override void StopEvent()
        {
            UndoEvent();
        }

        public override void UndoEvent()
        {
            Animator component = ((USEventBase)this).get_AffectedObject().GetComponent<Animator>();
            if ((bool)component)
            {
                component.applyRootMotion = prevApplyRootMotion;
            }
        }
    }
}
