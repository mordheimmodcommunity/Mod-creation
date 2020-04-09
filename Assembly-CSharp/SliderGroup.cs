using System.Globalization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SliderGroup : MonoBehaviour
{
	public delegate void OnValueChanged(int id, float val);

	[HideInInspector]
	public Slider slider;

	public Text field;

	public int valMult = 100;

	public OnValueChanged onValueChanged;

	public int id;

	private void Awake()
	{
		slider = GetComponentInChildren<Slider>();
		((UnityEvent<float>)(object)slider.get_onValueChanged()).AddListener((UnityAction<float>)UpdateText);
	}

	private void UpdateText(float value)
	{
		int value2 = (int)(value * (float)valMult);
		field.set_text(value2.ToConstantString());
		if (onValueChanged != null)
		{
			onValueChanged(id, slider.get_value());
		}
	}

	private void UpdateText(string text)
	{
		slider.set_value(float.Parse(text, NumberFormatInfo.InvariantInfo) / (float)valMult);
		if (onValueChanged != null)
		{
			onValueChanged(id, slider.get_value());
		}
	}

	public void SetValue(float v)
	{
		UpdateText(v);
		int value = (int)(v * (float)valMult);
		UpdateText(value.ToConstantString());
	}

	public int GetValue()
	{
		return (int)slider.get_value();
	}
}
