using UnityEngine;
using UnityEngine.UI;

public class LobbyCreateModule : UIModule
{
	public ButtonGroup createExhibitionGame;

	public ButtonGroup createContestGame;

	public Text contestUnavailableText;

	public void Setup()
	{
		createExhibitionGame.SetAction(string.Empty, "menu_skirmish_create_game");
		createExhibitionGame.OnAction(CreateExhibitionGame, mouseOnly: false);
		createContestGame.SetAction(string.Empty, "menu_skirmish_create_game");
		createContestGame.OnAction(CreateContestGame, mouseOnly: false);
		((Component)(object)contestUnavailableText).gameObject.SetActive(value: false);
		createContestGame.SetDisabled();
		createExhibitionGame.SetDisabled();
	}

	private void CreateExhibitionGame()
	{
		CreateExhibitionGamePopup();
	}

	public void CreateExhibitionGamePopup(bool silent = false)
	{
		if (PandoraSingleton<PandoraInput>.Instance.CurrentInputLayer != 1000)
		{
			PandoraSingleton<SkirmishManager>.Instance.OnCreateGame(exhibition: true, OnCancelCreateExhibitionGame, silent);
		}
	}

	private void CreateContestGame()
	{
		CreateContestGamePopup();
	}

	public void CreateContestGamePopup(bool silent = false)
	{
		if (PandoraSingleton<PandoraInput>.Instance.CurrentInputLayer != 1000)
		{
			PandoraSingleton<SkirmishManager>.Instance.OnCreateGame(exhibition: false, OnCancelCreateContestGame, silent);
		}
	}

	public void OnCancelCreateExhibitionGame()
	{
		createExhibitionGame.SetSelected(force: true);
	}

	public void OnCancelCreateContestGame()
	{
		createContestGame.SetSelected(force: true);
	}

	public void LockContest(string reasonTag)
	{
		contestUnavailableText.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById(reasonTag));
		((Component)(object)contestUnavailableText).gameObject.SetActive(value: true);
		createContestGame.SetDisabled();
	}
}
