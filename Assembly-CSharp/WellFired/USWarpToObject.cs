using UnityEngine;

namespace WellFired
{
	[USequencerFriendlyName("Warp To Object")]
	[USequencerEvent("Transform/Warp To Object")]
	[USequencerEventHideDuration]
	public class USWarpToObject : USEventBase
	{
		public GameObject objectToWarpTo;

		public bool useObjectRotation;

		private Transform previousTransform;

		public USWarpToObject()
			: this()
		{
		}

		public override void FireEvent()
		{
			if ((bool)objectToWarpTo)
			{
				((USEventBase)this).get_AffectedObject().transform.position = objectToWarpTo.transform.position;
				if (useObjectRotation)
				{
					((USEventBase)this).get_AffectedObject().transform.rotation = objectToWarpTo.transform.rotation;
				}
			}
			else
			{
				Debug.LogError(((USEventBase)this).get_AffectedObject().name + ": No Object attached to WarpToObjectSequencer Script");
			}
			previousTransform = ((USEventBase)this).get_AffectedObject().transform;
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
			if ((bool)previousTransform)
			{
				((USEventBase)this).get_AffectedObject().transform.position = previousTransform.position;
				((USEventBase)this).get_AffectedObject().transform.rotation = previousTransform.rotation;
			}
		}
	}
}
