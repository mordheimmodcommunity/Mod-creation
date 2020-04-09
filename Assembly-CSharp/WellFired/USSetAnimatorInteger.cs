using UnityEngine;

namespace WellFired
{
	[USequencerFriendlyName("Set Mecanim Integer")]
	[USequencerEvent("Animation (Mecanim)/Animator/Set Value/Integer")]
	[USequencerEventHideDuration]
	public class USSetAnimatorInteger : USEventBase
	{
		public string valueName = string.Empty;

		public int Value;

		private int prevValue;

		private int hash;

		public USSetAnimatorInteger()
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
			prevValue = component.GetInteger(hash);
			component.SetInteger(hash, Value);
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
			prevValue = component.GetInteger(hash);
			component.SetInteger(hash, Value);
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
				component.SetInteger(hash, prevValue);
			}
		}
	}
}
