using UnityEngine;
using UnityEngine.UI;

public class UIIconDesc : MonoBehaviour
{
    public Image icon;

    public Text desc;

    public void Set(Sprite image, string descKey)
    {
        SetLocalized(image, PandoraSingleton<LocalizationManager>.Instance.GetStringById(descKey));
    }

    public void SetLocalized(Sprite image, string desc)
    {
        base.gameObject.SetActive(value: true);
        icon.set_sprite(image);
        this.desc.set_text(desc);
    }
}
