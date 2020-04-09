using UnityEngine;
using UnityEngine.UI;

public class UIPropsInfoController : CanvasGroupDisabler
{
	public Image icon;

	public Text label;

	public void Set(Sprite icon, string label)
	{
		if (this.icon.get_sprite() != icon)
		{
			this.icon.set_sprite(icon);
		}
		if (this.label.get_text() != label)
		{
			this.label.set_text(label);
		}
	}
}
