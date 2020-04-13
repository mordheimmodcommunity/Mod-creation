using UnityEngine;

namespace Prometheus
{
    public class Translate : MonoBehaviour
    {
        public Vector3 transPerSec = new Vector3(0f, 0.1f, 0f);

        public bool smooth = true;

        private void Update()
        {
            Vector3 vector = transPerSec * ((!smooth) ? Time.deltaTime : Time.smoothDeltaTime);
            base.transform.localPosition += vector;
        }
    }
}
