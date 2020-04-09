using UnityEngine;

namespace WellFired
{
	[USequencerEvent("Fullscreen/Print Text")]
	[USequencerFriendlyName("Print Text")]
	public class USPrintTextEvent : USEventBase
	{
		public UILayer uiLayer;

		public string textToPrint = string.Empty;

		public Rect position = new Rect(0f, 0f, Screen.width, Screen.height);

		public float printRatePerCharacter;

		private string priorText = string.Empty;

		private string currentText = string.Empty;

		private bool display;

		public USPrintTextEvent()
			: this()
		{
		}

		public override void FireEvent()
		{
			priorText = currentText;
			currentText = textToPrint;
			if (((USEventBase)this).get_Duration() > 0f)
			{
				currentText = string.Empty;
			}
			display = true;
		}

		public override void ProcessEvent(float deltaTime)
		{
			if (printRatePerCharacter <= 0f)
			{
				currentText = textToPrint;
			}
			else
			{
				int num = (int)(deltaTime / printRatePerCharacter);
				if (num < textToPrint.Length)
				{
					currentText = textToPrint.Substring(0, num);
				}
				else
				{
					currentText = textToPrint;
				}
			}
			display = true;
		}

		public override void StopEvent()
		{
			UndoEvent();
		}

		public override void UndoEvent()
		{
			currentText = priorText;
			display = false;
		}

		private void OnGUI()
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Expected I4, but got Unknown
			if (((USEventBase)this).get_Sequence().get_IsPlaying() && display)
			{
				int depth = GUI.depth;
				GUI.depth = (int)uiLayer;
				GUI.Label(position, currentText);
				GUI.depth = depth;
			}
		}
	}
}
