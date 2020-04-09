using UnityEngine;

public class MainMenuView : MonoBehaviour
{
	public RectTransform mainView;

	public RectTransform gameModesView;

	public RectTransform creditsView;

	private RectTransform _current;

	private void Awake()
	{
		mainView.gameObject.SetActive(value: false);
		gameModesView.gameObject.SetActive(value: false);
		creditsView.gameObject.SetActive(value: false);
		OnMain();
	}

	public void LoadCampaign()
	{
	}

	public void OnGameModes()
	{
		Show(gameModesView);
	}

	public void OnMain()
	{
		Show(mainView);
	}

	public void OnCredits()
	{
		Show(creditsView);
	}

	private void Show(RectTransform toShow)
	{
		if (_current != null)
		{
			_current.gameObject.SetActive(value: false);
		}
		_current = toShow;
		_current.gameObject.SetActive(value: true);
		_current.localPosition = Vector3.zero;
	}
}
