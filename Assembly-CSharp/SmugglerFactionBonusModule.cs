using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SmugglerFactionBonusModule : UIModule
{
    public Sprite locked;

    public Sprite unlocked;

    public Text factionType;

    public List<UIIconDesc> bonus;

    public ScrollGroup scrollGroup;

    public void Setup(FactionMenuController faction)
    {
        factionType.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById(faction.Faction.Data.Desc + "_name"));
        for (int i = 0; i < bonus.Count; i++)
        {
            int num = i + 1;
            bonus[i].SetLocalized((num > faction.Faction.Save.rank) ? locked : unlocked, PandoraSingleton<LocalizationManager>.Instance.GetStringById("faction_rank_" + faction.Faction.Data.Id + "_" + num));
        }
        scrollGroup.scrollbar.set_value(1f);
    }

    private void Update()
    {
        if (PandoraSingleton<PandoraInput>.Instance.CurrentInputLayer == 0)
        {
            float axis = PandoraSingleton<PandoraInput>.Instance.GetAxis("cam_y");
            if (!Mathf.Approximately(axis, 0f) && Mathf.Abs(axis) > 0.8f)
            {
                scrollGroup.ForceScroll(axis < 0f, setSelected: false);
            }
        }
    }
}
