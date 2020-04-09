using UnityEngine;
using UnityEngine.UI;

public class UIInventoryMutation : MonoBehaviour
{
	public Image icon;

	public Text title;

	public Text desc;

	public void Set(Mutation mut)
	{
		icon.set_sprite(mut.GetIcon());
		if ((Object)(object)title != null)
		{
			title.set_text(mut.LocName);
		}
		if ((Object)(object)desc != null)
		{
			desc.set_text(mut.LocDesc);
		}
	}
}
