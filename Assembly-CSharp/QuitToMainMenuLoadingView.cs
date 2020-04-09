using UnityEngine;
using UnityEngine.UI;

public class QuitToMainMenuLoadingView : LoadingView
{
	public Text descriptionText;

	private void Awake()
	{
		((Behaviour)(object)descriptionText).enabled = false;
	}
}
