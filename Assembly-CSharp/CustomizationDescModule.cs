using UnityEngine;
using UnityEngine.UI;

public class CustomizationDescModule : UIModule
{
	public Image icon;

	public Text title;

	public Text description;

	public void Set(Sprite icon, string titleLoc, string textLoc)
	{
		this.icon.set_sprite(icon);
		title.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById(titleLoc));
		description.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById(textLoc));
	}
}
