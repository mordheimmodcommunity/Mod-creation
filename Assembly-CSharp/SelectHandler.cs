using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class SelectHandler : MonoBehaviour, ISelectHandler, IEventSystemHandler
{
    public UnityEvent selected;

    public void OnSelect(BaseEventData eventData)
    {
        selected.Invoke();
    }
}
