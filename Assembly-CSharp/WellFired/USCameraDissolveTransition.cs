using UnityEngine;
using WellFired.Shared;

namespace WellFired
{
    [ExecuteInEditMode]
    [USequencerEvent("Camera/Transition/Dissolve")]
    [USequencerFriendlyName("Dissolve Transition")]
    public class USCameraDissolveTransition : USEventBase
    {
        private BaseTransition transition;

        [SerializeField]
        private Camera sourceCamera;

        [SerializeField]
        private Camera destinationCamera;

        public USCameraDissolveTransition()
            : this()
        {
        }

        private void OnGUI()
        {
            if (!(sourceCamera == null) && !(destinationCamera == null) && transition != null)
            {
                transition.ProcessTransitionFromOnGUI();
            }
        }

        public override void FireEvent()
        {
            //IL_000c: Unknown result type (might be due to invalid IL or missing references)
            //IL_0016: Expected O, but got Unknown
            if (transition == null)
            {
                transition = (BaseTransition)(object)new BaseTransition();
            }
            if (sourceCamera == null || destinationCamera == null || transition == null)
            {
                Debug.LogError("Can't continue this transition with null cameras.");
            }
            else
            {
                transition.InitializeTransition(sourceCamera, destinationCamera, (TypeOfTransition)1);
            }
        }

        public override void ProcessEvent(float deltaTime)
        {
            if (!(sourceCamera == null) && !(destinationCamera == null) && transition != null)
            {
                transition.ProcessEventFromNoneOnGUI(deltaTime, ((USEventBase)this).get_Duration());
            }
        }

        public override void EndEvent()
        {
            if (!(sourceCamera == null) && !(destinationCamera == null) && transition != null)
            {
                transition.TransitionComplete();
            }
        }

        public override void StopEvent()
        {
            if (!(sourceCamera == null) && !(destinationCamera == null) && transition != null)
            {
                UndoEvent();
            }
        }

        public override void UndoEvent()
        {
            if (!(sourceCamera == null) && !(destinationCamera == null) && transition != null)
            {
                transition.RevertTransition();
            }
        }
    }
}
