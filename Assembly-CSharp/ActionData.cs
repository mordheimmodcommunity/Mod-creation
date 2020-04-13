using UnityEngine;

public class ActionData
{
    public string name;

    public bool mastery;

    public Sprite icon;

    public string actionOutcome;

    public void SetAction(string actionName, Sprite actionIcon)
    {
        Reset();
        name = actionName;
        icon = actionIcon;
    }

    public void SetAction(ActionStatus actionStatus)
    {
        SetAction(name = actionStatus.LocalizedName, actionStatus.GetIcon());
        mastery = actionStatus.IsMastery;
    }

    public void SetActionOutcome(bool success)
    {
        actionOutcome = PandoraSingleton<LocalizationManager>.Instance.GetStringById((!success) ? "com_failure" : "com_success");
    }

    public void SetActionOutcome(string outcome)
    {
        actionOutcome = PandoraSingleton<LocalizationManager>.Instance.GetStringById(outcome);
    }

    public void Reset()
    {
        name = string.Empty;
        icon = null;
        actionOutcome = null;
        mastery = false;
    }
}
