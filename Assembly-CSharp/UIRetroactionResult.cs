using UnityEngine;
using UnityEngine.UI;

public class UIRetroactionResult : MonoBehaviour
{
	public Image actionIcon;

	public Text resultName;

	public RectTransform offset;

	private void Awake()
	{
		resultName.set_text(string.Empty);
	}

	public void Set(UnitController unitCtrlr)
	{
		if (!string.IsNullOrEmpty(unitCtrlr.currentActionData.actionOutcome))
		{
			base.gameObject.SetActive(value: true);
			((Behaviour)(object)actionIcon).enabled = false;
			resultName.set_text(unitCtrlr.currentActionData.actionOutcome);
		}
		else
		{
			resultName.set_text(string.Empty);
		}
	}

	public void Set(string effect, bool isBuff)
	{
		if ((Object)(object)actionIcon != null)
		{
			((Behaviour)(object)actionIcon).enabled = true;
			((Graphic)actionIcon).get_rectTransform().localScale = new Vector3(1f, (!isBuff) ? (-1f) : 1f, 1f);
			((Graphic)actionIcon).set_color((!isBuff) ? Constant.GetColor(ConstantId.COLOR_RED) : Constant.GetColor(ConstantId.COLOR_GREEN));
		}
		resultName.set_text(effect);
	}

	public void Set(string effect)
	{
		resultName.set_text(effect);
	}

	public void Show()
	{
		if (!string.IsNullOrEmpty(resultName.get_text()))
		{
			base.gameObject.SetActive(value: true);
		}
		if ((Object)(object)actionIcon != null)
		{
			((Behaviour)(object)actionIcon).enabled = true;
		}
	}

	public void Hide()
	{
		resultName.set_text(string.Empty);
		if ((Object)(object)actionIcon != null)
		{
			((Behaviour)(object)actionIcon).enabled = false;
		}
		base.gameObject.SetActive(value: false);
	}
}
