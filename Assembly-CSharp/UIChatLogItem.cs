using UnityEngine;
using UnityEngine.UI;

public class UIChatLogItem : MonoBehaviour
{
    private LayoutElement layoutElem;

    private Text text;

    public RectTransform RctTransform
    {
        get;
        private set;
    }

    public void Init(string content)
    {
        layoutElem = GetComponent<LayoutElement>();
        text = GetComponent<Text>();
        RctTransform = GetComponent<RectTransform>();
        text.set_text(content);
    }

    public void OnInsideMask()
    {
        if (!((Behaviour)(object)text).enabled)
        {
            ((Behaviour)(object)text).enabled = true;
        }
    }

    public void OnOutsideMask()
    {
        if (((Behaviour)(object)text).enabled)
        {
            Vector2 sizeDelta = RctTransform.sizeDelta;
            if (sizeDelta.y != 0f)
            {
                LayoutElement obj = layoutElem;
                Vector2 sizeDelta2 = RctTransform.sizeDelta;
                obj.set_preferredWidth(sizeDelta2.x);
                LayoutElement obj2 = layoutElem;
                Vector2 sizeDelta3 = RctTransform.sizeDelta;
                obj2.set_preferredHeight(sizeDelta3.y);
                ((Behaviour)(object)text).enabled = false;
            }
        }
    }
}
