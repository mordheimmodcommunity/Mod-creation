using UnityEngine;

[ExecuteInEditMode]
public class fx_deadfish_splash : MonoBehaviour
{
    public GameObject fishSplash;

    private void OnParticleCollision(GameObject other)
    {
        if (!(fishSplash == null))
        {
            GameObject gameObject = Object.Instantiate(fishSplash);
            gameObject.transform.SetParent(base.transform);
            gameObject.transform.localPosition = Vector3.zero;
        }
    }
}
