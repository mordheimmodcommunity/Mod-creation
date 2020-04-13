using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LobbyListModule : UIModule
{
    public ScrollGroup serverList;

    public UILobbyEntry serverItem;

    public ButtonGroup btnRefresh;

    public Text availableGames;

    private UnityAction<bool, int> onServerSelect;

    private UnityAction<int> onServerConfirm;

    public void Setup(UnityAction<bool, int> OnServerSelect, UnityAction<int> OnServerConfirm)
    {
        onServerSelect = OnServerSelect;
        onServerConfirm = OnServerConfirm;
        serverList.Setup(serverItem.gameObject, hideBarIfEmpty: true);
        btnRefresh.SetAction(string.Empty, "lobby_refresh");
        btnRefresh.OnAction(LookForGames, mouseOnly: false);
        btnRefresh.effects.toggle.set_isOn(false);
    }

    public void ClearServersList()
    {
        serverList.ClearList();
        btnRefresh.SetSelected();
    }

    public void FillServersList(List<Lobby> lobbies, List<SkirmishMap> skirmishMaps)
    {
        bool flag = false;
        for (int i = 0; i < lobbies.Count; i++)
        {
            if (lobbies[i].hostId != PandoraSingleton<Hephaestus>.Instance.GetUserId() && lobbies[i].privacy != 0)
            {
                UILobbyEntry uILobbyEntry = null;
                Toggle up = null;
                Selectable down = null;
                if (i == lobbies.Count - 1)
                {
                    down = (Selectable)(object)btnRefresh.effects.toggle;
                }
                int mapName = lobbies[i].mapName;
                string map = (lobbies[i].mapName <= 0) ? PandoraSingleton<LocalizationManager>.Instance.GetStringById("lobby_title_random") : PandoraSingleton<LocalizationManager>.Instance.GetStringById(skirmishMaps[mapName - 1].mapData.Name + "_name");
                uILobbyEntry = serverList.AddToList((Selectable)(object)up, down).GetComponent<UILobbyEntry>();
                int a = 0;
                List<ProcMissionRatingData> list = PandoraSingleton<DataFactory>.Instance.InitData<ProcMissionRatingData>();
                for (int j = 0; j < list.Count; j++)
                {
                    a = Mathf.Max(a, list[j].MaxValue);
                }
                uILobbyEntry.Set(lobbies[i].name, map, lobbies[i].ratingMin, lobbies[i].ratingMax, !lobbies[i].isExhibition);
                int index = i;
                ((UnityEvent<bool>)(object)uILobbyEntry.GetComponent<ToggleEffects>().toggle.onValueChanged).AddListener((UnityAction<bool>)delegate (bool isOn)
                {
                    onServerSelect(isOn, index);
                });
                uILobbyEntry.GetComponent<ToggleEffects>().onAction.AddListener(delegate
                {
                    if (PandoraSingleton<PandoraInput>.Instance.CurrentInputLayer != 1000)
                    {
                        onServerConfirm(index);
                    }
                });
            }
        }
    }

    public void LookForGames()
    {
        ClearServersList();
        PandoraSingleton<HideoutManager>.Instance.skirmishNodeGroup.nodes[3].DestroyContent();
        if (PandoraSingleton<Hephaestus>.Instance.IsOnline())
        {
            btnRefresh.SetDisabled();
            PandoraSingleton<Hephaestus>.Instance.SearchLobbies(OnSearchLobbiesCallback);
        }
    }

    private void OnSearchLobbiesCallback()
    {
        PandoraDebug.LogInfo("OnSearchLobbiesCallback - Lobby Count = " + PandoraSingleton<Hephaestus>.Instance.Lobbies.Count, "HEPHAESTUS");
        btnRefresh.SetDisabled(disabled: false);
        if (EventSystem.get_current().get_currentSelectedGameObject() == null)
        {
            btnRefresh.SetSelected(force: true);
        }
        FillServersList(PandoraSingleton<Hephaestus>.Instance.Lobbies, PandoraSingleton<SkirmishManager>.Instance.skirmishMaps);
    }
}
