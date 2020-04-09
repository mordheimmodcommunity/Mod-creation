using UnityEngine;

public class FogSetter : MonoBehaviour
{
	public float y;

	private void Start()
	{
		GameObject gameObject = GameObject.Find("fx_fog_04");
		if ((bool)gameObject)
		{
			Vector3 position = gameObject.transform.position;
			position.y = y;
			gameObject.transform.position = position;
		}
		Object.Destroy(this);
	}
}
