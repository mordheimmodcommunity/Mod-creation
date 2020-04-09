using UnityEngine;

public class LoadingFinisher : MonoBehaviour
{
	private void Start()
	{
		PandoraSingleton<TransitionManager>.Instance.SetGameLoadingDone();
	}
}
