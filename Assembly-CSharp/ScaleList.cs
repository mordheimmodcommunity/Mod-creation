using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ScaleList
{
	public List<RectTransform> toScale;

	public ScaleBlock scale = ScaleBlock.defaultScaleBlock;
}
