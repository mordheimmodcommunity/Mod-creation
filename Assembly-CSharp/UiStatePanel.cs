using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UiStatePanel
{
	public UIMissionManager.State state;

	public List<MonoBehaviour> uiControllers;
}
