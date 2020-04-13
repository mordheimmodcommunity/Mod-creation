using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ActionButtonModule : UIModule
{
    public Text title;

    public Text desc;

    public ButtonGroup btnAction;

    public void Refresh(string title, string desc, string buttonTextTag, UnityAction onAction)
    {
        ((Component)(object)this.title).gameObject.SetActive(!string.IsNullOrEmpty(title));
        this.title.set_text(title);
        ((Component)(object)this.desc).gameObject.SetActive(!string.IsNullOrEmpty(desc));
        this.desc.set_text(desc);
        btnAction.SetAction(string.Empty, buttonTextTag);
        btnAction.OnAction(onAction, mouseOnly: false);
    }

    public void SetFocus()
    {
        btnAction.SetSelected(force: true);
    }
}
