public class EndGameInjuryModule : UIModule
{
    public UIDescription[] injury;

    public void Setup(MissionEndUnitSave unit)
    {
        for (int i = 0; i < injury.Length; i++)
        {
            injury[i].gameObject.SetActive(value: false);
        }
        for (int j = 0; j < unit.injuries.Count; j++)
        {
            if (unit.injuries[j].Duration > 0)
            {
                injury[j].SetLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("injury_name_" + unit.injuries[j].Name), PandoraSingleton<LocalizationManager>.Instance.GetStringById("injury_desc_" + unit.injuries[j].Name), PandoraSingleton<LocalizationManager>.Instance.GetStringById("end_game_recovery_time_value", unit.injuries[j].Duration.ToString()));
            }
            else
            {
                injury[j].SetLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("injury_name_" + unit.injuries[j].Name), PandoraSingleton<LocalizationManager>.Instance.GetStringById("injury_desc_" + unit.injuries[j].Name));
            }
            showQueue.Enqueue(injury[j].gameObject);
        }
        StartShow(0.5f);
    }
}
