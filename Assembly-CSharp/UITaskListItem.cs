using UnityEngine;
using UnityEngine.UI;

public class UITaskListItem : MonoBehaviour
{
    public Text groupName;

    public Image isDoneToggle;

    public Text doneText;

    public Image strikeBar;

    public void Set(AchievementCategory category)
    {
        groupName.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("task_title_" + category.category));
        int nbDone = category.NbDone;
        ((Behaviour)(object)isDoneToggle).enabled = (nbDone == category.Count);
        ((Behaviour)(object)strikeBar).enabled = ((Behaviour)(object)isDoneToggle).enabled;
        doneText.set_text($"{nbDone}/{category.Count}");
    }
}
