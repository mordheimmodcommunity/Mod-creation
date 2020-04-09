using UnityEngine;

namespace WellFired
{
	[USequencerFriendlyName("Toggle Component")]
	[USequencerEvent("Object/Toggle Component")]
	[USequencerEventHideDuration]
	public class USEnableComponentEvent : USEventBase
	{
		public bool enableComponent;

		private bool prevEnable;

		[HideInInspector]
		[SerializeField]
		private string componentName;

		public string ComponentName
		{
			get
			{
				return componentName;
			}
			set
			{
				componentName = value;
			}
		}

		public USEnableComponentEvent()
			: this()
		{
		}

		public override void FireEvent()
		{
			Behaviour behaviour = ((USEventBase)this).get_AffectedObject().GetComponent(ComponentName) as Behaviour;
			if ((bool)behaviour)
			{
				prevEnable = behaviour.enabled;
				behaviour.enabled = enableComponent;
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
				Behaviour behaviour = ((USEventBase)this).get_AffectedObject().GetComponent(ComponentName) as Behaviour;
				if ((bool)behaviour)
				{
					behaviour.enabled = prevEnable;
				}
			}
		}
	}
}
