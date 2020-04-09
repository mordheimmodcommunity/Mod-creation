using UnityEngine;

namespace WellFired
{
	[USequencerEventHideDuration]
	[USequencerEvent("Physics/Apply Force")]
	[USequencerFriendlyName("Apply Force")]
	public class USApplyForceEvent : USEventBase
	{
		public Vector3 direction = Vector3.up;

		public float strength = 1f;

		public ForceMode type = ForceMode.Impulse;

		private Transform previousTransform;

		public USApplyForceEvent()
			: this()
		{
		}

		public override void FireEvent()
		{
			Rigidbody component = ((USEventBase)this).get_AffectedObject().GetComponent<Rigidbody>();
			if (!component)
			{
				Debug.Log("Attempting to apply an impulse to an object, but it has no rigid body from USequencerApplyImpulseEvent::FireEvent");
				return;
			}
			component.AddForceAtPosition(direction * strength, base.transform.position, type);
			previousTransform = component.transform;
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
			if (!((USEventBase)this).get_AffectedObject())
			{
				return;
			}
			Rigidbody component = ((USEventBase)this).get_AffectedObject().GetComponent<Rigidbody>();
			if ((bool)component)
			{
				component.Sleep();
				if ((bool)previousTransform)
				{
					((USEventBase)this).get_AffectedObject().transform.position = previousTransform.position;
					((USEventBase)this).get_AffectedObject().transform.rotation = previousTransform.rotation;
				}
			}
		}
	}
}
