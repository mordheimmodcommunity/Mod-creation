using UnityEngine;
using UnityEngine.UI;

public class UILobbyEntry : MonoBehaviour
{
	public Text gameName;

	public Text mapName;

	public Text rating;

	public Image difficultyRating;

	public Image contestIcon;

	public Image exhibitionIcon;

	public void Set(string game, string map, int ratingMin, int ratingMax, bool isContest)
	{
		gameName.set_text(game);
		mapName.set_text(map);
		rating.set_text("[" + ratingMin + "," + ratingMax + "]");
		((Component)(object)difficultyRating).gameObject.SetActive(value: false);
		((Component)(object)exhibitionIcon).gameObject.SetActive(!isContest);
		((Component)(object)contestIcon).gameObject.SetActive(isContest);
	}
}
