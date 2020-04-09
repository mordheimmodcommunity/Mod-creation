using Steamworks;

public class Lobby
{
	public const string ID = "id";

	public const string VERSION = "version";

	public const string NAME = "name";

	public const string PRIVACY = "privacy";

	public const string MAP = "map";

	public const string WARBAND = "warband";

	public const string EXHIBITION = "exhibition";

	public const string RATING_MIN = "rating_min";

	public const string RATING_MAX = "rating_max";

	public const string PASSWORD = "password";

	public const string JOINABLE = "joinable";

	public ulong id;

	public ulong hostId;

	public string version;

	public string name;

	public Hephaestus.LobbyPrivacy privacy;

	public int mapName;

	public int warbandId;

	public int ratingMin;

	public int ratingMax = 5000;

	public bool isExhibition;

	public bool joinable = true;

	public void SetPrivacy(ELobbyType type)
	{
		switch (type)
		{
		case ELobbyType.k_ELobbyTypePublic:
			privacy = Hephaestus.LobbyPrivacy.PUBLIC;
			break;
		case ELobbyType.k_ELobbyTypeFriendsOnly:
			privacy = Hephaestus.LobbyPrivacy.FRIENDS;
			break;
		case ELobbyType.k_ELobbyTypePrivate:
			privacy = Hephaestus.LobbyPrivacy.OFFLINE;
			break;
		case ELobbyType.k_ELobbyTypeInvisible:
			privacy = Hephaestus.LobbyPrivacy.PRIVATE;
			break;
		}
	}
}
