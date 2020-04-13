using UnityEngine;

namespace WellFired
{
    [RequireComponent(typeof(Collider))]
    public class SequenceTrigger : MonoBehaviour
    {
        public bool isPlayerTrigger;

        public bool isMainCameraTrigger;

        public GameObject triggerObject;

        public USSequencer sequenceToPlay;

        private void OnTriggerEnter(Collider other)
        {
            if (!(Object)(object)sequenceToPlay)
            {
                Debug.LogWarning("You have triggered a sequence in your scene, however, you didn't assign it a Sequence To Play", base.gameObject);
            }
            else if (!sequenceToPlay.get_IsPlaying())
            {
                if (other.CompareTag("MainCamera") && isMainCameraTrigger)
                {
                    sequenceToPlay.Play();
                }
                else if (other.CompareTag("Player") && isPlayerTrigger)
                {
                    sequenceToPlay.Play();
                }
                else if (other.gameObject == triggerObject)
                {
                    sequenceToPlay.Play();
                }
            }
        }
    }
}
