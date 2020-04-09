using UnityEngine;

public class xray_vision : MonoBehaviour
{
	public GameObject xray;

	private void Start()
	{
		if (xray == null && GameObject.Find("xray") != null)
		{
			xray = GameObject.Find("xray");
		}
	}

	private void OnTriggerEnter(Collider col)
	{
		if (col.gameObject == xray)
		{
			Vector3 v = GetComponent<Collider>().ClosestPointOnBounds(xray.transform.position);
			GetComponent<Renderer>().material.SetVector("_ObjPos", v);
			MonoBehaviour.print("xray vision has collided");
			GetComponent<Renderer>().material.SetFloat("_Radius", 5f);
		}
	}

	private void OnTriggerStay(Collider col)
	{
		if (col.gameObject == xray)
		{
			Vector3 v = GetComponent<Collider>().ClosestPointOnBounds(xray.transform.position);
			GetComponent<Renderer>().material.SetVector("_ObjPos", v);
			GetComponent<Renderer>().material.SetFloat("_Radius", 5f);
		}
	}

	private void OnTriggerExit(Collider col)
	{
		if (col.gameObject == xray)
		{
			GetComponent<Renderer>().material.SetFloat("_Radius", 0.1f);
			MonoBehaviour.print("xray vision has exited");
		}
	}
}
