using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AvailableUnitsModule : UIModule
{
    public GameObject itemPrefab;

    public Text listDesc;

    public ScrollGroup scrollGroup;

    public override void Init()
    {
        base.Init();
        scrollGroup.Setup(itemPrefab, hideBarIfEmpty: true);
    }

    public void Set(List<UnitMenuController> units, bool canHire)
    {
        if (units.Count > 0)
        {
            listDesc.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById((!canHire) ? "hideout_unit_unavailable_desc" : "hideout_unit_available_desc"));
        }
        else
        {
            EventLogger logger = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.Logger;
            int currentDate = PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate;
            Tuple<int, EventLogger.LogEvent, int> tuple = logger.FindEventAfter(EventLogger.LogEvent.OUTSIDER_ROTATION, currentDate);
            if (tuple != null)
            {
                Date date = new Date(tuple.Item1);
                listDesc.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_outsider_unavailable_desc", date.ToLocalizedString()));
            }
            else
            {
                listDesc.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_outsider_unavailable_later_desc"));
            }
        }
        scrollGroup.ClearList();
        for (int i = 0; i < units.Count; i++)
        {
            GameObject gameObject = scrollGroup.AddToList(null, null);
            HireUnitDescription component = gameObject.GetComponent<HireUnitDescription>();
            component.Set(units[i].unit);
        }
    }

    private void Update()
    {
        float axis = PandoraSingleton<PandoraInput>.Instance.GetAxis("cam_y");
        if (axis != 0f)
        {
            scrollGroup.ForceScroll(axis < 0f, setSelected: false);
        }
    }
}
