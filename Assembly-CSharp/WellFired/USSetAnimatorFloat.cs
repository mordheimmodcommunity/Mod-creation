using UnityEngine;

namespace WellFired
{
    [USequencerEventHideDuration]
    [USequencerFriendlyName("Set Mecanim Float")]
    [USequencerEvent("Animation (Mecanim)/Animator/Set Value/Float")]
    public class USSetAnimatorFloat : USEventBase
    {
        public string valueName = string.Empty;

        public float Value;

        private float prevValue;

        private int hash;

        public USSetAnimatorFloat()
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
            if (valueName.Length == 0)
            {
                Debug.LogWarning("Invalid name passed to the uSequencer Event Set Float", (Object)(object)this);
                return;
            }
            hash = Animator.StringToHash(valueName);
            prevValue = component.GetFloat(hash);
            component.SetFloat(hash, Value);
        }

        public override void ProcessEvent(float runningTime)
        {
            Animator component = ((USEventBase)this).get_AffectedObject().GetComponent<Animator>();
            if (!component)
            {
                Debug.LogWarning("Affected Object has no Animator component, for uSequencer Event", (Object)(object)this);
                return;
            }
            if (valueName.Length == 0)
            {
                Debug.LogWarning("Invalid name passed to the uSequencer Event Set Float", (Object)(object)this);
                return;
            }
            hash = Animator.StringToHash(valueName);
            prevValue = component.GetFloat(hash);
            component.SetFloat(hash, Value);
        }

        public override void StopEvent()
        {
            UndoEvent();
        }

        public override void UndoEvent()
        {
            Animator component = ((USEventBase)this).get_AffectedObject().GetComponent<Animator>();
            if ((bool)component && valueName.Length != 0)
            {
                component.SetFloat(hash, prevValue);
            }
        }
    }
}
