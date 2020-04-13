using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleOnOver : MonoBehaviour, IEventSystemHandler, IPointerEnterHandler
{
    private Toggle toggle;

    private void Awake()
    {
        toggle = GetComponent<Toggle>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        toggle.set_isOn(true);
    }
}
