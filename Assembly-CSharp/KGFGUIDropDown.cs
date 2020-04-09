using System;
using System.Collections.Generic;
using UnityEngine;

public class KGFGUIDropDown : KGFIControl
{
	public enum eDropDirection
	{
		eAuto,
		eDown,
		eUp
	}

	private List<string> itsEntrys = new List<string>();

	private GUILayoutOption[] itsLayoutOptions;

	private string itsCurrentSelected = string.Empty;

	private bool itsVisible = true;

	public Vector2 itsScrollPosition = Vector2.zero;

	public Rect itsLastRect;

	public static KGFGUIDropDown itsOpenInstance;

	public uint itsWidth;

	public uint itsHeight;

	private uint itsMaxVisibleItems = 1u;

	public eDropDirection itsDirection;

	public string itsTitle = string.Empty;

	public Texture2D itsIcon;

	public bool itsHover;

	public static bool itsCorrectedOffset;

	public event EventHandler SelectedValueChanged;

	public KGFGUIDropDown(IEnumerable<string> theEntrys, uint theWidth, uint theMaxVisibleItems, eDropDirection theDirection, params GUILayoutOption[] theLayout)
	{
		if (theEntrys != null)
		{
			foreach (string theEntry in theEntrys)
			{
				itsEntrys.Add(theEntry);
			}
			itsWidth = theWidth;
			itsMaxVisibleItems = theMaxVisibleItems;
			itsDirection = theDirection;
			if (itsEntrys.Count > 0)
			{
				itsCurrentSelected = itsEntrys[0];
			}
		}
		else
		{
			Debug.LogError("the list of entrys was null");
		}
	}

	public void SetEntrys(IEnumerable<string> theEntrys)
	{
		itsEntrys.Clear();
		foreach (string theEntry in theEntrys)
		{
			itsEntrys.Add(theEntry);
		}
		if (itsEntrys.Count > 0)
		{
			itsCurrentSelected = itsEntrys[0];
		}
	}

	public IEnumerable<string> GetEntrys()
	{
		return itsEntrys;
	}

	public string SelectedItem()
	{
		return itsCurrentSelected;
	}

	public void SetSelectedItem(string theValue)
	{
		if (itsEntrys.Contains(theValue))
		{
			itsCurrentSelected = theValue;
			if (this.SelectedValueChanged != null)
			{
				this.SelectedValueChanged(theValue, EventArgs.Empty);
			}
		}
	}

	public void Render()
	{
		if (itsEntrys.Count <= itsMaxVisibleItems)
		{
			itsHeight = (uint)(itsEntrys.Count * (int)(uint)KGFGUIUtility.GetSkinHeight());
		}
		else
		{
			itsHeight = itsMaxVisibleItems * (uint)KGFGUIUtility.GetSkinHeight();
		}
		if (!itsVisible)
		{
			return;
		}
		GUILayout.BeginHorizontal(GUILayout.Width((float)(double)itsWidth));
		KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxLeft);
		if (itsTitle != string.Empty)
		{
			KGFGUIUtility.Label(itsTitle, KGFGUIUtility.eStyleLabel.eLabelFitIntoBox, GUILayout.ExpandWidth(expand: true));
		}
		else
		{
			KGFGUIUtility.Label(itsCurrentSelected, KGFGUIUtility.eStyleLabel.eLabelFitIntoBox, GUILayout.ExpandWidth(expand: true));
		}
		KGFGUIUtility.EndHorizontalBox();
		if (itsIcon == null)
		{
			if (KGFGUIUtility.Button("v", KGFGUIUtility.eStyleButton.eButtonRight, GUILayout.ExpandWidth(expand: false)))
			{
				if (itsOpenInstance != this)
				{
					itsOpenInstance = this;
					itsCorrectedOffset = false;
				}
				else
				{
					itsOpenInstance = null;
					itsCorrectedOffset = false;
				}
			}
		}
		else if (KGFGUIUtility.Button(itsIcon, KGFGUIUtility.eStyleButton.eButtonRight, GUILayout.ExpandWidth(expand: false)))
		{
			if (itsOpenInstance != this)
			{
				itsOpenInstance = this;
				itsCorrectedOffset = false;
			}
			else
			{
				itsOpenInstance = null;
				itsCorrectedOffset = false;
			}
		}
		GUILayout.EndHorizontal();
		if (Event.current.type == EventType.Repaint)
		{
			itsLastRect = GUILayoutUtility.GetLastRect();
			return;
		}
		Vector3 mousePosition = Input.mousePosition;
		mousePosition.y = (float)Screen.height - mousePosition.y;
		if (itsLastRect.Contains(mousePosition))
		{
			itsHover = true;
		}
		else if (itsOpenInstance != this)
		{
			itsHover = false;
		}
	}

	public string GetName()
	{
		return "KGFGUIDropDown";
	}

	public bool IsVisible()
	{
		return itsVisible;
	}

	public bool GetHover()
	{
		return itsHover;
	}
}
