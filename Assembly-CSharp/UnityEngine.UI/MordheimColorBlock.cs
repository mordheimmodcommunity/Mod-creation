using System;

namespace UnityEngine.UI
{
	[Serializable]
	public struct MordheimColorBlock
	{
		public static ColorBlock defaultColorBlock
		{
			get
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				//IL_0094: Unknown result type (might be due to invalid IL or missing references)
				ColorBlock result = default(ColorBlock);
				((ColorBlock)(ref result)).set_normalColor(new Color(0.6f, 0.6f, 0.6f, 1f));
				((ColorBlock)(ref result)).set_highlightedColor(new Color(0.35f, 1f, 0.58f, 1f));
				((ColorBlock)(ref result)).set_pressedColor(new Color(0.35f, 1f, 0.58f, 1f));
				((ColorBlock)(ref result)).set_disabledColor(new Color(0.25f, 0.25f, 0.25f, 0.5f));
				((ColorBlock)(ref result)).set_fadeDuration(0.1f);
				return result;
			}
		}
	}
}
