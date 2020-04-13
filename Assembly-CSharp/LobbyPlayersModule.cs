using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayersModule : UIModule
{
    public SkirmishLobbyPlayer player1;

    public SkirmishLobbyPlayer player2;

    public SkirmishLobbyPlayer empty;

    public GameObject pressReadyMessage;

    public GameObject errorMessage;

    public Text errorMessageText;

    public void RefreshPlayers()
    {
        bool flag = PandoraSingleton<Hermes>.Instance.IsHost();
        pressReadyMessage.SetActive(!flag);
        if (PandoraSingleton<MissionStartData>.Instance.FightingWarbands.Count > 1)
        {
            player1.gameObject.SetActive(value: true);
            player1.SetPlayerInfo((!flag) ? 1 : 0);
            player2.gameObject.SetActive(value: true);
            player2.SetPlayerInfo(flag ? 1 : 0);
            empty.gameObject.SetActive(value: false);
        }
        else if (PandoraSingleton<MissionStartData>.Instance.FightingWarbands.Count == 1)
        {
            player1.gameObject.SetActive(value: true);
            player1.SetPlayerInfo(0);
            player2.gameObject.SetActive(value: false);
            empty.gameObject.SetActive(value: true);
        }
        else
        {
            player1.gameObject.SetActive(value: false);
            player2.gameObject.SetActive(value: false);
            empty.gameObject.SetActive(value: false);
        }
    }

    public void SetErrorMessage(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            errorMessage.SetActive(value: false);
            return;
        }
        errorMessage.SetActive(value: true);
        errorMessageText.set_text(text);
    }
}
