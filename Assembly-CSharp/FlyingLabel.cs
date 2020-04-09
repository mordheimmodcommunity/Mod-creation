using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

public class FlyingLabel : FlyingText
{
	private Text label;

	private void Awake()
	{
		label = GetComponent<Text>();
	}

	public override void Destroy()
	{
		base.Destroy();
		DOTween.Kill((object)label, false);
	}

	public void Play(Vector3 position, bool loc, string text, params string[] parameters)
	{
		Play(position, null, loc, text, parameters);
	}

	public void Play(Vector3 position, Transform anchor, bool loc, string text, params string[] parameters)
	{
		Play(position, anchor);
		label.set_text((!loc) ? string.Format(text, parameters) : PandoraSingleton<LocalizationManager>.Instance.GetStringById(text, parameters));
		((Graphic)label).set_color(startColor);
		if (duration > 0f)
		{
			TweenSettingsExtensions.SetDelay<TweenerCore<Color, Color, ColorOptions>>(DOTween.To((DOGetter<Color>)(() => ((Graphic)label).get_color()), (DOSetter<Color>)delegate(Color c)
			{
				((Graphic)label).set_color(c);
			}, endColor, duration / 2f), duration / 2f);
		}
	}
}
