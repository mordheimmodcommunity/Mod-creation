using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SoundButton : MonoBehaviour, IPointerClickHandler, ISelectHandler, IEventSystemHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        UISound.Instance.OnClick();
    }

    public void OnSelect(BaseEventData eventData)
    {
        UISound.Instance.OnSelect();
    }
}
