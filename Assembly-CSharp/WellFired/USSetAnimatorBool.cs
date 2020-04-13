using UnityEngine;

namespace WellFired
{
    [USequencerEventHideDuration]
    [USequencerFriendlyName("Set Mecanim Bool")]
    [USequencerEvent("Animation (Mecanim)/Animator/Set Value/Bool")]
    internal class USSetAnimatorBool : USEventBase
    {
        public string valueName = string.Empty;

        public bool Value = true;

        private bool prevValue;

        private int hash;

        public USSetAnimatorBool()
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
            prevValue = component.GetBool(hash);
            component.SetBool(hash, Value);
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
            prevValue = component.GetBool(hash);
            component.SetBool(hash, Value);
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
                component.SetBool(hash, prevValue);
            }
        }
    }
}
