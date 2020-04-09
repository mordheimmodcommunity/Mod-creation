using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PanFlute : MonoBehaviour
{
	public Pan.Type fluteType;

	private void Awake()
	{
		PandoraSingleton<Pan>.Instance.AddSource(this);
		Object.Destroy(this);
	}
}
