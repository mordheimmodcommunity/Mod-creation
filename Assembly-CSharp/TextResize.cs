using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteInEditMode]
public class TextResize : UIBehaviour
{
    private RectTransform _transform;

    public ILayoutElement element;

    private bool isResize;

    public TextResize()
        : this()
    {
    }

    private void Start()
    {
        _transform = (base.transform as RectTransform);
    }

    public void AdjustSize()
    {
        _transform.sizeDelta = new Vector2(element.get_preferredWidth(), element.get_preferredHeight());
    }
}
