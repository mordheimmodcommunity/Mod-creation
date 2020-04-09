using UnityEngine;

namespace WellFired
{
	[USequencerEvent("Transform/Parent and reset Transform")]
	[USequencerEventHideDuration]
	[USequencerFriendlyName("Parent and reset Transform")]
	public class USParentAndResetObjectEvent : USEventBase
	{
		public Transform parent;

		public Transform child;

		private Transform previousParent;

		private Vector3 previousPosition;

		private Quaternion previousRotation;

		public USParentAndResetObjectEvent()
			: this()
		{
		}

		public override void FireEvent()
		{
			previousParent = child.parent;
			previousPosition = child.localPosition;
			previousRotation = child.localRotation;
			child.parent = parent;
			child.localPosition = Vector3.zero;
			child.localRotation = Quaternion.identity;
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
				child.parent = previousParent;
				child.localPosition = previousPosition;
				child.localRotation = previousRotation;
			}
		}
	}
}
