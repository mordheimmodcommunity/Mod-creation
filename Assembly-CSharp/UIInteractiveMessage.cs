using UnityEngine.UI;

public class UIInteractiveMessage : CanvasGroupDisabler
{
    public Text text;

    private void Awake()
    {
        base.gameObject.SetActive(value: false);
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.INTERACTION_POINTS_CHANGED, UpdateInteractivePoints);
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.LADDER_UNIT_CHANGED, UpdateInteractivePoints);
    }

    private void UpdateInteractivePoints()
    {
        UnitController currentUnit = PandoraSingleton<MissionManager>.Instance.GetCurrentUnit();
        if (currentUnit != null && currentUnit.IsPlayed())
        {
            bool flag = currentUnit.unit.Data.UnitSizeId == UnitSizeId.LARGE;
            string text = string.Empty;
            for (int i = 0; i < currentUnit.interactivePoints.Count; i++)
            {
                if (!currentUnit.interactivePoints[i].HasRequiredItem(currentUnit))
                {
                    text = PandoraSingleton<LocalizationManager>.Instance.GetStringById("na_interactive_required_item", Item.GetLocalizedName(currentUnit.interactivePoints[i].requestedItemId));
                    break;
                }
                if (currentUnit.interactivePoints[i] is ActionZone)
                {
                    ActionZone actionZone = (ActionZone)currentUnit.interactivePoints[i];
                    for (int j = 0; j < actionZone.destinations.Count; j++)
                    {
                        if (flag && !actionZone.destinations[j].destination.supportLargeUnit)
                        {
                            text = PandoraSingleton<LocalizationManager>.Instance.GetStringById("na_interactive_large");
                            break;
                        }
                        if (!actionZone.destinations[j].destination.PointsChecker.IsAvailable())
                        {
                            text = PandoraSingleton<LocalizationManager>.Instance.GetStringById("na_interactive_full");
                            break;
                        }
                    }
                    if (!string.IsNullOrEmpty(text))
                    {
                        break;
                    }
                }
                else if (currentUnit.interactivePoints[i] is SearchPoint)
                {
                    SearchPoint searchPoint = (SearchPoint)currentUnit.interactivePoints[i];
                    if (searchPoint.slots.Count > 0 && ((searchPoint.slots[0].restrictedItemId != 0 && !currentUnit.unit.HasItem(searchPoint.slots[0].restrictedItemId)) || (searchPoint.slots[0].restrictedItemTypeId != 0 && !currentUnit.unit.HasItem(searchPoint.slots[0].restrictedItemTypeId))) && currentUnit.unit.IsInventoryFull())
                    {
                        text = PandoraSingleton<LocalizationManager>.Instance.GetStringById("na_interactive_inventory_full");
                    }
                }
            }
            if (currentUnit.currentTeleporter != null && !currentUnit.currentTeleporter.IsActive())
            {
                text = PandoraSingleton<LocalizationManager>.Instance.GetStringById("na_teleport_full");
            }
            if (!string.IsNullOrEmpty(text))
            {
                Show(text);
            }
            else
            {
                Hide();
            }
        }
        else
        {
            Hide();
        }
    }

    public void Show(string message)
    {
        base.gameObject.SetActive(value: true);
        text.set_text(message);
    }

    public void Hide()
    {
        base.gameObject.SetActive(value: false);
    }
}
