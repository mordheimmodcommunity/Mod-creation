using UnityEngine;

public class Beacon : MonoBehaviour
{
	public GameObject visual;

	public int idx;

	public void SetActive(bool active)
	{
		visual.SetActive(active);
	}
}
