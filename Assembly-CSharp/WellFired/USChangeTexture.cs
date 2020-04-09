using UnityEngine;

namespace WellFired
{
	[USequencerEventHideDuration]
	[USequencerFriendlyName("Change Texture")]
	[USequencerEvent("Render/Change Objects Texture")]
	public class USChangeTexture : USEventBase
	{
		public Texture newTexture;

		private Texture previousTexture;

		public USChangeTexture()
			: this()
		{
		}

		public override void FireEvent()
		{
			if ((bool)((USEventBase)this).get_AffectedObject())
			{
				if (!newTexture)
				{
					Debug.LogWarning("you've not given a texture to the USChangeTexture Event", (Object)(object)this);
				}
				else if (!Application.isPlaying && Application.isEditor)
				{
					previousTexture = ((USEventBase)this).get_AffectedObject().GetComponent<Renderer>().sharedMaterial.mainTexture;
					((USEventBase)this).get_AffectedObject().GetComponent<Renderer>().sharedMaterial.mainTexture = newTexture;
				}
				else
				{
					previousTexture = ((USEventBase)this).get_AffectedObject().GetComponent<Renderer>().material.mainTexture;
					((USEventBase)this).get_AffectedObject().GetComponent<Renderer>().material.mainTexture = newTexture;
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
			if ((bool)((USEventBase)this).get_AffectedObject() && (bool)previousTexture)
			{
				if (!Application.isPlaying && Application.isEditor)
				{
					((USEventBase)this).get_AffectedObject().GetComponent<Renderer>().sharedMaterial.mainTexture = previousTexture;
				}
				else
				{
					((USEventBase)this).get_AffectedObject().GetComponent<Renderer>().material.mainTexture = previousTexture;
				}
				previousTexture = null;
			}
		}
	}
}
