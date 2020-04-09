using UnityEngine;

namespace WellFired
{
	[USequencerEventHideDuration]
	[USequencerEvent("Time/Time Scale")]
	[USequencerFriendlyName("Time Scale")]
	public class USTimeScaleEvent : USEventBase
	{
		public AnimationCurve scaleCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.1f, 0.2f), new Keyframe(2f, 1f));

		private float currentCurveSampleTime;

		private float prevTimeScale = 1f;

		public USTimeScaleEvent()
			: this()
		{
		}

		public override void FireEvent()
		{
			prevTimeScale = Time.timeScale;
		}

		public override void ProcessEvent(float deltaTime)
		{
			currentCurveSampleTime = deltaTime;
			Time.timeScale = Mathf.Max(0f, scaleCurve.Evaluate(currentCurveSampleTime));
		}

		public override void EndEvent()
		{
			float time = scaleCurve.keys[scaleCurve.length - 1].time;
			Time.timeScale = Mathf.Max(0f, scaleCurve.Evaluate(time));
		}

		public override void StopEvent()
		{
			UndoEvent();
		}

		public override void UndoEvent()
		{
			currentCurveSampleTime = 0f;
			Time.timeScale = prevTimeScale;
		}
	}
}
