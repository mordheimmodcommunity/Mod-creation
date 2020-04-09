using UnityEngine;
using UnityEngine.UI;

public class UIInventoryItemAttribute : MonoBehaviour
{
	public Image icon;

	public Text value;

	public Image improveIcon;

	public Text difference;

	public void Set(string attributeValue)
	{
		base.gameObject.SetActive(value: true);
		value.set_text(attributeValue);
		if ((Object)(object)improveIcon != null)
		{
			((Behaviour)(object)improveIcon).enabled = false;
		}
		if ((Object)(object)difference != null)
		{
			((Behaviour)(object)difference).enabled = false;
		}
	}
}
