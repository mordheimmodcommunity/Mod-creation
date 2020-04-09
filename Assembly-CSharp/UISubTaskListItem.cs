using UnityEngine;
using UnityEngine.UI;

public class UISubTaskListItem : MonoBehaviour
{
	public Text taskName;

	public Text xpGained;

	public Toggle isDoneToggle;

	public void Set(Achievement achievement)
	{
		taskName.set_text(achievement.LocDesc);
		xpGained.set_text(achievement.Xp.ToConstantString());
		isDoneToggle.set_isOn(achievement.Completed);
	}
}
