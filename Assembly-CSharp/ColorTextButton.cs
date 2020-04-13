using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ColorTextButton : MonoBehaviour, IPointerDownHandler, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
    public Graphic targetGraphic;

    public ColorBlock color = MordheimColorBlock.defaultColorBlock;

    void IDeselectHandler.OnDeselect(BaseEventData eventData)
    {
        Color(((ColorBlock)(ref color)).get_normalColor());
    }

    private void OnEnable()
    {
        Color(((ColorBlock)(ref color)).get_normalColor());
    }

    private void OnDisable()
    {
        Color(((ColorBlock)(ref color)).get_disabledColor());
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Color(((ColorBlock)(ref color)).get_pressedColor());
    }

    public void OnSelect(BaseEventData eventData)
    {
        Color(((ColorBlock)(ref color)).get_highlightedColor());
    }

    private void Color(Color newColor)
    {
        if (Application.isPlaying)
        {
            targetGraphic.CrossFadeColor(newColor, ((ColorBlock)(ref color)).get_fadeDuration(), true, true);
        }
    }
}
