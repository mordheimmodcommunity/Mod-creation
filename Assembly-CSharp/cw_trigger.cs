using UnityEngine;

public class cw_trigger : MonoBehaviour
{
    private void Start()
    {
    }

    private void Update()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && !base.transform.parent.GetComponent<Rigidbody>().isKinematic)
        {
            GameObject gameObject = base.transform.parent.gameObject;
            cw_behavior component = gameObject.GetComponent<cw_behavior>();
            component.timetoSelectAgain = 0f;
            component.FoundFoodTarget = false;
            component.FoundFreeTarget = false;
        }
    }
}
