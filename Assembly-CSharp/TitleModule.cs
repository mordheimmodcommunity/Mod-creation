using UnityEngine;
using UnityEngine.UI;

public class TitleModule : UIModule
{
	public Text title;

	public Image bg;

	public void Set(string titleKey, bool showBg = true)
	{
		SetLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById(titleKey), showBg);
	}

	public void SetLocalized(string titleText, bool showBg = true)
	{
		if (((Behaviour)(object)bg).enabled != showBg)
		{
			((Behaviour)(object)bg).enabled = showBg;
		}
		if (title.get_text() != titleText)
		{
			title.set_text(titleText);
		}
	}
}
