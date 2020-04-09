using UnityEngine;
using UnityEngine.UI;

public class UIDescription : MonoBehaviour
{
	public Text title;

	public Text subtitle;

	public Text desc;

	public void Set(string titleKey, string descKey)
	{
		Set(titleKey, descKey, string.Empty);
	}

	public void Set(string titleKey, string descKey, string keySubtitle)
	{
		SetLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById(titleKey), PandoraSingleton<LocalizationManager>.Instance.GetStringById(descKey));
	}

	public void SetLocalized(string title, string desc)
	{
		SetLocalized(title, desc, string.Empty);
	}

	public void SetLocalized(string title, string desc, string subtitle)
	{
		base.gameObject.SetActive(value: true);
		this.title.set_text(title);
		this.desc.set_text(desc);
		if ((Object)(object)this.subtitle != null)
		{
			this.subtitle.set_text(subtitle);
		}
	}
}
