using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class ToggleOnSelect : MonoBehaviour, ISelectHandler, IEventSystemHandler
{
    public Toggle toggle;

    private void Awake()
    {
        if ((Object)(object)toggle == null)
        {
            toggle = GetComponent<Toggle>();
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        toggle.set_isOn(true);
    }
}
