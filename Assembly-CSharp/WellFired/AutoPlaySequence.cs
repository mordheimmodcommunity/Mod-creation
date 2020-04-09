using UnityEngine;

namespace WellFired
{
	public class AutoPlaySequence : MonoBehaviour
	{
		public USSequencer sequence;

		public float delay = 1f;

		private float currentTime;

		private bool hasPlayed;

		private void Start()
		{
			if (!(Object)(object)sequence)
			{
				Debug.LogError("You have added an AutoPlaySequence, however you haven't assigned it a sequence", base.gameObject);
			}
		}

		private void Update()
		{
			if (!hasPlayed)
			{
				currentTime += Time.deltaTime;
				if (currentTime >= delay && (bool)(Object)(object)sequence)
				{
					sequence.Play();
					hasPlayed = true;
				}
			}
		}
	}
}
