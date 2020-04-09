using UnityEngine;

namespace WellFired
{
	[USequencerEventHideDuration]
	[USequencerFriendlyName("Change Color")]
	[USequencerEvent("Render/Change Objects Color")]
	public class USChangeColor : USEventBase
	{
		public Color newColor;

		private Color previousColor;

		public USChangeColor()
			: this()
		{
		}

		public override void FireEvent()
		{
			if ((bool)((USEventBase)this).get_AffectedObject())
			{
				if (!Application.isPlaying && Application.isEditor)
				{
					previousColor = ((USEventBase)this).get_AffectedObject().GetComponent<Renderer>().sharedMaterial.color;
					((USEventBase)this).get_AffectedObject().GetComponent<Renderer>().sharedMaterial.color = newColor;
				}
				else
				{
					previousColor = ((USEventBase)this).get_AffectedObject().GetComponent<Renderer>().material.color;
					((USEventBase)this).get_AffectedObject().GetComponent<Renderer>().material.color = newColor;
				}
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
				if (!Application.isPlaying && Application.isEditor)
				{
					((USEventBase)this).get_AffectedObject().GetComponent<Renderer>().sharedMaterial.color = previousColor;
				}
				else
				{
					((USEventBase)this).get_AffectedObject().GetComponent<Renderer>().material.color = previousColor;
				}
			}
		}
	}
}
