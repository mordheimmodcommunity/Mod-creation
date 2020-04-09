using UnityEngine;

namespace WellFired
{
	[USequencerEvent("Attach/Attach To Parent")]
	[USequencerFriendlyName("Attach Object To Parent")]
	[USequencerEventHideDuration]
	public class USAttachToParentEvent : USEventBase
	{
		public Transform parentObject;

		private Transform originalParent;

		public USAttachToParentEvent()
			: this()
		{
		}

		public override void FireEvent()
		{
			if (!parentObject)
			{
				Debug.Log("USAttachEvent has been asked to attach an object, but it hasn't been given a parent from USAttachEvent::FireEvent");
				return;
			}
			originalParent = ((USEventBase)this).get_AffectedObject().transform.parent;
			((USEventBase)this).get_AffectedObject().transform.parent = parentObject;
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
				((USEventBase)this).get_AffectedObject().transform.SetParent(originalParent);
			}
		}
	}
}
