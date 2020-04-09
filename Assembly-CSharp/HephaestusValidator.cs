using UnityEngine;

public class HephaestusValidator : MonoBehaviour
{
	private ulong lobbyId;

	private bool searching;

	private void OnGUI()
	{
		GUILayout.BeginArea(new Rect(25f, 25f, 300f, 700f));
		if (PandoraSingleton<Hephaestus>.Instance.Lobby != null)
		{
			GUILayout.Label("LobbyID = " + PandoraSingleton<Hephaestus>.Instance.Lobby.id);
			if (GUILayout.Button("Delete Lobby"))
			{
				PandoraSingleton<Hephaestus>.Instance.LeaveLobby();
			}
			if (GUILayout.Button("Invite Friends"))
			{
				PandoraSingleton<Hephaestus>.Instance.InviteFriends();
			}
		}
		else
		{
			GUILayout.Label("No Lobby Created");
			if (GUILayout.Button("Create Lobby"))
			{
				PandoraSingleton<Hephaestus>.Instance.CreateLobby("Testing12", Hephaestus.LobbyPrivacy.PUBLIC, OnLobbyEnteredCallback);
			}
			if (!searching && GUILayout.Button("Find Lobby"))
			{
				searching = true;
				PandoraSingleton<Hephaestus>.Instance.SearchLobbies(OnSearchLobbiesCallback);
			}
			if (!searching && PandoraSingleton<Hephaestus>.Instance.Lobbies.Count > 0)
			{
				for (int i = 0; i < PandoraSingleton<Hephaestus>.Instance.Lobbies.Count; i++)
				{
					if (GUILayout.Button(PandoraSingleton<Hephaestus>.Instance.Lobbies[i].name))
					{
					}
				}
			}
		}
		GUILayout.EndArea();
	}

	private void OnLobbyEnteredCallback(ulong lobbyId, bool success)
	{
	}

	private void OnSearchLobbiesCallback()
	{
		searching = false;
	}

	private void OnJoinLobbyCallback(bool success)
	{
	}

	private void OnLobbyUpdate()
	{
	}
}
