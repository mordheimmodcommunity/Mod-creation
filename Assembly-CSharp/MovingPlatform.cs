using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class MovingPlatform : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		UnitController component = other.GetComponent<UnitController>();
		if (component != null)
		{
			component.transform.parent = base.transform;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		UnitController component = other.GetComponent<UnitController>();
		if (component != null)
		{
			component.transform.parent = null;
		}
	}
}
