using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KGFGUISelectionList
{
	private class ListItem
	{
		private string itsCachedString;

		private object itsItem;

		public bool itsSelected;

		public bool itsFiltered;

		public ListItem(object theItem)
		{
			itsItem = theItem;
			itsSelected = false;
			itsFiltered = false;
			UpdateCache(null);
		}

		public void UpdateCache(Func<object, string> theDisplayMethod)
		{
			if (theDisplayMethod != null)
			{
				itsCachedString = theDisplayMethod(itsItem);
			}
			else
			{
				itsCachedString = itsItem.ToString();
			}
		}

		public string GetString()
		{
			return itsCachedString;
		}

		public object GetItem()
		{
			return itsItem;
		}
	}

	private const string itsControlSearchName = "tagSearch";

	private const string itsTextSearch = "Search";

	private List<ListItem> itsData = new List<ListItem>();

	private string itsSearch = string.Empty;

	private IEnumerable itsListSource;

	private Vector2 itsScrollPosition = Vector2.zero;

	private Func<object, string> itsDisplayMethod;

	public event EventHandler EventItemChanged;

	public void SetValues(IEnumerable theList)
	{
		itsListSource = theList;
		UpdateList();
		UpdateItemFilter();
	}

	public bool GetIsSelected(object theItem)
	{
		foreach (ListItem itsDatum in itsData)
		{
			if (theItem == itsDatum.GetItem())
			{
				return itsDatum.itsSelected;
			}
		}
		return false;
	}

	public void SetDisplayMethod(Func<object, string> theDisplayMethod)
	{
		itsDisplayMethod = theDisplayMethod;
		UpdateItemFilter();
	}

	public void ClearDisplayMethod()
	{
		itsDisplayMethod = null;
		UpdateItemFilter();
	}

	private int ListItemComparer(ListItem theListItem1, ListItem theListItem2)
	{
		return theListItem1.GetString().CompareTo(theListItem2.GetString());
	}

	public void Render()
	{
		GUILayout.BeginVertical();
		KGFGUIUtility.BeginVerticalBox(KGFGUIUtility.eStyleBox.eBoxDarkTop);
		DrawButtons();
		KGFGUIUtility.EndVerticalBox();
		KGFGUIUtility.BeginVerticalBox(KGFGUIUtility.eStyleBox.eBoxMiddleVertical);
		DrawList();
		KGFGUIUtility.EndVerticalBox();
		KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxDarkMiddleVertical);
		KGFGUIUtility.Label(string.Empty, GUILayout.ExpandWidth(expand: true));
		KGFGUIUtility.EndHorizontalBox();
		KGFGUIUtility.BeginVerticalBox(KGFGUIUtility.eStyleBox.eBoxDarkBottom);
		DrawSearch();
		KGFGUIUtility.EndVerticalBox();
		GUILayout.EndVertical();
		if (GUI.GetNameOfFocusedControl().Equals("tagSearch") && itsSearch.Equals("Search"))
		{
			itsSearch = string.Empty;
		}
		if (!GUI.GetNameOfFocusedControl().Equals("tagSearch") && itsSearch.Equals(string.Empty))
		{
			itsSearch = "Search";
		}
	}

	public IEnumerable<object> GetSelected()
	{
		foreach (ListItem anItem in itsData)
		{
			if (anItem.itsSelected)
			{
				yield return anItem.GetItem();
			}
		}
	}

	public void SetSelected(IEnumerable<object> theList)
	{
		SetSelectedAll(theValue: false);
		foreach (object the in theList)
		{
			SetSelected(the, theSelectionState: true);
		}
	}

	public void SetSelected(object theItem, bool theSelectionState)
	{
		foreach (ListItem itsDatum in itsData)
		{
			if (theItem == itsDatum.GetItem())
			{
				itsDatum.itsSelected = theSelectionState;
				break;
			}
		}
	}

	public void SetSelected(string theItem, bool theSelectionState)
	{
		foreach (ListItem itsDatum in itsData)
		{
			if (theItem == itsDatum.GetItem().ToString())
			{
				itsDatum.itsSelected = theSelectionState;
				break;
			}
		}
	}

	private void UpdateList()
	{
		List<object> selected = new List<object>(GetSelected());
		itsData.Clear();
		foreach (object item in itsListSource)
		{
			itsData.Add(new ListItem(string.Empty + item));
		}
		itsData.Sort(ListItemComparer);
		SetSelected(selected);
	}

	public void UpdateItemFilter()
	{
		if (itsSearch.Trim() == string.Empty || itsSearch.Trim() == "Search")
		{
			foreach (ListItem itsDatum in itsData)
			{
				itsDatum.itsFiltered = false;
			}
		}
		else
		{
			foreach (ListItem itsDatum2 in itsData)
			{
				itsDatum2.UpdateCache(itsDisplayMethod);
				itsDatum2.itsFiltered = !itsDatum2.GetString().Trim().ToLower()
					.Contains(itsSearch.Trim().ToLower());
			}
		}
	}

	public void SetSelectedAll(bool theValue)
	{
		foreach (ListItem itsDatum in itsData)
		{
			itsDatum.itsSelected = theValue;
		}
		if (this.EventItemChanged != null)
		{
			this.EventItemChanged(this, null);
		}
	}

	private void DrawButtons()
	{
		GUILayout.BeginHorizontal();
		if (KGFGUIUtility.Button("All", KGFGUIUtility.eStyleButton.eButton))
		{
			SetSelectedAll(theValue: true);
		}
		if (KGFGUIUtility.Button("None", KGFGUIUtility.eStyleButton.eButton))
		{
			SetSelectedAll(theValue: false);
		}
		GUILayout.EndHorizontal();
	}

	private void DrawList()
	{
		itsScrollPosition = GUILayout.BeginScrollView(itsScrollPosition);
		KGFGUIUtility.BeginVerticalBox(KGFGUIUtility.eStyleBox.eBoxInvisible);
		foreach (ListItem itsDatum in itsData)
		{
			if (!itsDatum.itsFiltered)
			{
				bool flag = KGFGUIUtility.Toggle(itsDatum.itsSelected, itsDatum.GetString(), KGFGUIUtility.eStyleToggl.eTogglSuperCompact);
				if (flag != itsDatum.itsSelected)
				{
					itsDatum.itsSelected = flag;
					if (this.EventItemChanged != null)
					{
						this.EventItemChanged(this, null);
					}
				}
			}
		}
		KGFGUIUtility.EndVerticalBox();
		GUILayout.EndScrollView();
	}

	private void DrawSearch()
	{
		GUI.SetNextControlName("tagSearch");
		string a = KGFGUIUtility.TextField(itsSearch, KGFGUIUtility.eStyleTextField.eTextField);
		if (a != itsSearch)
		{
			itsSearch = a;
			UpdateItemFilter();
		}
	}
}
