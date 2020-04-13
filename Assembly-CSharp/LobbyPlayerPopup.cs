using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerPopup : PandoraSingleton<LobbyPlayerPopup>
{
    public Text playerName;

    public Text text;

    private bool isShow;

    private void Awake()
    {
        if (PandoraSingleton<LobbyPlayerPopup>.instance == null)
        {
            PandoraSingleton<LobbyPlayerPopup>.instance = this;
        }
    }

    private void Start()
    {
        if (!isShow)
        {
            base.gameObject.SetActive(value: false);
        }
    }

    public void Show(string player, string textId)
    {
        base.gameObject.SetActive(value: true);
        isShow = true;
        if (string.IsNullOrEmpty(player))
        {
            ((Behaviour)(object)playerName).enabled = false;
        }
        else
        {
            ((Behaviour)(object)playerName).enabled = true;
            playerName.set_text(player);
        }
        text.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById(textId));
    }

    private void OnDisable()
    {
        if (isShow)
        {
            isShow = false;
        }
    }
}
