using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkirmishCreatePopup : ConfirmationPopupView
{
    public SelectorGroup lobbyPrivacy;

    public SelectorGroup ratingMin;

    public SelectorGroup ratingMax;

    public InputField lobbyName;

    protected override void Awake()
    {
        base.Awake();
        ((Component)(object)lobbyName).transform.parent.GetComponentInChildren<ToggleEffects>(includeInactive: true).onSelect.AddListener(OnLobbyNameSelected);
        ((Component)(object)lobbyName).transform.parent.GetComponentInChildren<ToggleEffects>(includeInactive: true).onAction.AddListener(OnLobbyNameSelected);
    }

    private void OnLobbyNameSelected()
    {
        if (PandoraSingleton<PandoraInput>.Instance.lastInputMode != PandoraInput.InputMode.JOYSTICK || !PandoraSingleton<Hephaestus>.Instance.ShowVirtualKeyboard(PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_skirmish_create_game"), lobbyName.get_text(), 40u, multiLine: false, OnVirtualKeyboard, lobbyPrivacy.selections.Count > 1))
        {
            ((MonoBehaviour)(object)lobbyName).SetSelected(force: true);
        }
    }

    private void OnVirtualKeyboard(bool success, string newstring)
    {
        if (success)
        {
            lobbyName.set_text(newstring);
        }
    }

    public void Show(string titleId, string textId, bool allowOffline, int rating, Action<bool> callback, bool hideButtons = false)
    {
        lobbyPrivacy.selections.Clear();
        if (PandoraSingleton<Hephaestus>.Instance.IsOnline())
        {
            for (int i = 0; i < 4; i++)
            {
                if (allowOffline || i != 3)
                {
                    string key = "lobby_privacy_" + ((Hephaestus.LobbyPrivacy)i).ToLowerString();
                    string stringById = PandoraSingleton<LocalizationManager>.Instance.GetStringById(key);
                    lobbyPrivacy.SetButtonsVisible(show: true);
                    lobbyPrivacy.selections.Add(stringById);
                }
            }
        }
        else
        {
            string key2 = "lobby_privacy_" + Hephaestus.LobbyPrivacy.OFFLINE.ToLowerString();
            string stringById2 = PandoraSingleton<LocalizationManager>.Instance.GetStringById(key2);
            lobbyPrivacy.selections.Add(stringById2);
            lobbyPrivacy.SetButtonsVisible(show: false);
        }
        int num = rating % 50;
        int num2 = rating - num;
        int num3 = rating + (50 - num);
        ratingMin.selections.Clear();
        ratingMax.selections.Clear();
        ratingMin.selections.Add("0");
        for (int j = 50; j < 5000; j += 50)
        {
            if (j <= num2)
            {
                ratingMin.selections.Add(j.ToConstantString());
            }
            else if (j >= num3)
            {
                ratingMax.selections.Add(j.ToConstantString());
            }
        }
        ratingMin.SetCurrentSel(Mathf.Max(0, ratingMin.selections.Count - 2));
        ratingMax.SetCurrentSel(1);
        base.Show(titleId, textId, callback, hideButtons);
        lobbyPrivacy.SetCurrentSel(0);
        ((Behaviour)(object)lobbyName).enabled = true;
        lobbyName.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_name_default", PandoraSingleton<Hephaestus>.Instance.GetUserName()));
        ((Component)(object)lobbyName).transform.parent.gameObject.GetComponent<ToggleEffects>().SetSelected(force: true);
    }

    public string GetRatingMin()
    {
        return ratingMin.selections[ratingMin.CurSel];
    }

    public string GetRatingMax()
    {
        return ratingMax.selections[ratingMax.CurSel];
    }

    public override void Confirm()
    {
        if (((Component)(object)lobbyName).transform.parent.gameObject != EventSystem.get_current().get_currentSelectedGameObject() && ((Component)(object)lobbyName).gameObject != EventSystem.get_current().get_currentSelectedGameObject())
        {
            if (lobbyName.get_text() == string.Empty)
            {
                lobbyName.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_name_default", PandoraSingleton<Hephaestus>.Instance.GetUserName()));
            }
            base.Confirm();
        }
        else
        {
            lobbyName.DeactivateInputField();
        }
    }
}
