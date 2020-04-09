using System;
using System.Collections.Generic;
using UnityEngine;

public class KGFGUIDataTable : KGFIControl
{
	private KGFDataTable itsDataTable;

	private Vector2 itsDataTableScrollViewPosition;

	private uint itsStartRow;

	private uint itsDisplayRowCount = 100u;

	private Dictionary<KGFDataColumn, uint> itsColumnWidth = new Dictionary<KGFDataColumn, uint>();

	private Dictionary<KGFDataColumn, bool> itsColumnVisible = new Dictionary<KGFDataColumn, bool>();

	private KGFDataRow itsClickedRow;

	private KGFDataRow itsCurrentSelected;

	private bool itsVisible = true;

	private static Texture2D itsTextureArrowUp;

	private static Texture2D itsTextureArrowDown;

	private KGFDataColumn itsSortColumn;

	private bool itsSortDirection;

	private Rect itsRectScrollView = default(Rect);

	private bool itsRepaint;

	public event EventHandler PreRenderRow;

	public event EventHandler PostRenderRow;

	public event EventHandler PreRenderColumn;

	public event EventHandler PostRenderColumn;

	public event Func<KGFDataRow, KGFDataColumn, uint, bool> PreCellContentHandler;

	public event EventHandler OnClickRow;

	public event EventHandler EventSettingsChanged;

	public KGFGUIDataTable(KGFDataTable theDataTable, params GUILayoutOption[] theLayout)
	{
		itsDataTable = theDataTable;
		foreach (KGFDataColumn column in itsDataTable.Columns)
		{
			itsColumnWidth.Add(column, 0u);
			itsColumnVisible.Add(column, value: true);
		}
	}

	private static void LoadTextures()
	{
		string str = "KGFCore/textures/";
		itsTextureArrowUp = (Texture2D)Resources.Load(str + "arrow_up", typeof(Texture2D));
		itsTextureArrowDown = (Texture2D)Resources.Load(str + "arrow_down", typeof(Texture2D));
	}

	public uint GetStartRow()
	{
		return itsStartRow;
	}

	public void SetStartRow(uint theStartRow)
	{
		itsStartRow = (uint)Math.Min(theStartRow, itsDataTable.Rows.Count);
	}

	public uint GetDisplayRowCount()
	{
		return itsDisplayRowCount;
	}

	public void SetDisplayRowCount(uint theDisplayRowCount)
	{
		itsDisplayRowCount = (uint)Math.Min(theDisplayRowCount, itsDataTable.Rows.Count - itsStartRow);
	}

	public void SetColumnVisible(int theColumIndex, bool theValue)
	{
		if (theColumIndex >= 0 && theColumIndex < itsDataTable.Columns.Count)
		{
			itsColumnVisible[itsDataTable.Columns[theColumIndex]] = theValue;
		}
	}

	public bool GetColumnVisible(int theColumIndex)
	{
		if (theColumIndex >= 0 && theColumIndex < itsDataTable.Columns.Count)
		{
			return itsColumnVisible[itsDataTable.Columns[theColumIndex]];
		}
		return false;
	}

	public void SetColumnWidth(int theColumIndex, uint theValue)
	{
		if (theColumIndex >= 0 && theColumIndex < itsDataTable.Columns.Count)
		{
			itsColumnWidth[itsDataTable.Columns[theColumIndex]] = theValue;
		}
	}

	public uint GetColumnWidth(int theColumIndex)
	{
		if (theColumIndex >= 0 && theColumIndex < itsDataTable.Columns.Count)
		{
			return itsColumnWidth[itsDataTable.Columns[theColumIndex]];
		}
		return 0u;
	}

	public KGFDataRow GetCurrentSelected()
	{
		return itsCurrentSelected;
	}

	public void SetCurrentSelected(KGFDataRow theDataRow)
	{
		if (itsDataTable.Rows.Contains(theDataRow))
		{
			itsCurrentSelected = theDataRow;
		}
	}

	private void RenderTableHeadings()
	{
		if (itsTextureArrowDown == null)
		{
			LoadTextures();
		}
		KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxDarkTop, GUILayout.ExpandWidth(expand: true), GUILayout.ExpandHeight(expand: false));
		foreach (KGFDataColumn column in itsDataTable.Columns)
		{
			if (itsColumnVisible[column])
			{
				GUILayoutOption[] options = (itsColumnWidth[column] == 0) ? new GUILayoutOption[1]
				{
					GUILayout.ExpandWidth(expand: true)
				} : new GUILayoutOption[1]
				{
					GUILayout.Width((float)(double)itsColumnWidth[column])
				};
				GUILayout.BeginHorizontal(options);
				KGFGUIUtility.Label(column.ColumnName, KGFGUIUtility.eStyleLabel.eLabelFitIntoBox);
				if (column == itsSortColumn)
				{
					if (itsSortDirection)
					{
						KGFGUIUtility.Label(string.Empty, itsTextureArrowDown, KGFGUIUtility.eStyleLabel.eLabelFitIntoBox, GUILayout.Width(14f));
					}
					else
					{
						KGFGUIUtility.Label(string.Empty, itsTextureArrowUp, KGFGUIUtility.eStyleLabel.eLabelFitIntoBox, GUILayout.Width(14f));
					}
				}
				GUILayout.EndHorizontal();
				if (Event.current.type == EventType.MouseUp && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
				{
					SortColumn(column);
				}
				KGFGUIUtility.Separator(KGFGUIUtility.eStyleSeparator.eSeparatorVerticalFitInBox);
			}
		}
		KGFGUIUtility.EndHorizontalBox();
	}

	private void SortColumn(KGFDataColumn theColumn)
	{
		if (itsSortColumn != theColumn)
		{
			SetSortingColumn(theColumn);
			itsSortDirection = false;
			itsDataTable.Rows.Sort(RowComparison);
		}
		else
		{
			itsSortDirection = !itsSortDirection;
			itsDataTable.Rows.Reverse();
		}
		itsRepaint = true;
	}

	private int RowComparison(KGFDataRow theRow1, KGFDataRow theRow2)
	{
		if (itsSortColumn != null)
		{
			return theRow1[itsSortColumn].Value.ToString().CompareTo(theRow2[itsSortColumn].Value.ToString());
		}
		return 0;
	}

	private void RenderTableRows()
	{
		itsDataTableScrollViewPosition = KGFGUIUtility.BeginScrollView(itsDataTableScrollViewPosition, false, true, GUILayout.ExpandHeight(expand: true));
		if (itsDataTable.Rows.Count > 0)
		{
			GUILayout.BeginVertical();
			Color color = GUI.color;
			for (int i = (int)itsStartRow; i < itsStartRow + itsDisplayRowCount && i < itsDataTable.Rows.Count; i++)
			{
				KGFDataRow kGFDataRow = itsDataTable.Rows[i];
				if (this.PreRenderRow != null)
				{
					this.PreRenderRow(kGFDataRow, EventArgs.Empty);
				}
				if (kGFDataRow == itsCurrentSelected)
				{
					KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxDarkTopInteractive, GUILayout.ExpandWidth(expand: true));
				}
				else
				{
					KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxMiddleVerticalInteractive, GUILayout.ExpandWidth(expand: true));
				}
				foreach (KGFDataColumn column in itsDataTable.Columns)
				{
					if (itsColumnVisible[column])
					{
						if (this.PreRenderColumn != null)
						{
							this.PreRenderColumn(column, EventArgs.Empty);
						}
						bool flag = false;
						if (this.PreCellContentHandler != null)
						{
							flag = this.PreCellContentHandler(kGFDataRow, column, itsColumnWidth[column]);
						}
						if (!flag)
						{
							int num = 85;
							string text = kGFDataRow[column].ToString().Substring(0, Math.Min(num, kGFDataRow[column].ToString().Length));
							if (text.Length == num)
							{
								text += "...";
							}
							if (itsColumnWidth[column] != 0)
							{
								KGFGUIUtility.Label(text, KGFGUIUtility.eStyleLabel.eLabelFitIntoBox, GUILayout.Width((float)(double)itsColumnWidth[column]));
							}
							else
							{
								KGFGUIUtility.Label(text, KGFGUIUtility.eStyleLabel.eLabelFitIntoBox, GUILayout.ExpandWidth(expand: true));
							}
						}
						KGFGUIUtility.Separator(KGFGUIUtility.eStyleSeparator.eSeparatorVerticalFitInBox);
						if (this.PostRenderColumn != null)
						{
							this.PostRenderColumn(column, EventArgs.Empty);
						}
					}
				}
				KGFGUIUtility.EndHorizontalBox();
				if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown && Event.current.button == 0)
				{
					itsClickedRow = kGFDataRow;
					itsRepaint = true;
				}
				if (this.OnClickRow != null && itsClickedRow != null && Event.current.type == EventType.Layout)
				{
					if (itsCurrentSelected != itsClickedRow)
					{
						itsCurrentSelected = itsClickedRow;
					}
					else
					{
						itsCurrentSelected = null;
					}
					this.OnClickRow(itsClickedRow, EventArgs.Empty);
					itsClickedRow = null;
				}
				if (this.PostRenderRow != null)
				{
					this.PostRenderRow(kGFDataRow, EventArgs.Empty);
				}
			}
			GUI.color = color;
			GUILayout.FlexibleSpace();
			GUILayout.EndVertical();
		}
		else
		{
			GUILayout.Label("no items found");
			GUILayout.FlexibleSpace();
		}
		GUILayout.EndScrollView();
		itsRectScrollView = GUILayoutUtility.GetLastRect();
	}

	public Rect GetLastRectScrollview()
	{
		return itsRectScrollView;
	}

	public bool GetRepaintWish()
	{
		bool result = itsRepaint;
		itsRepaint = false;
		return result;
	}

	public void SetSortingColumn(string theColumnName)
	{
		foreach (KGFDataColumn column in itsDataTable.Columns)
		{
			if (column.ColumnName == theColumnName)
			{
				itsSortColumn = column;
				itsRepaint = true;
				break;
			}
		}
	}

	public void SetSortingColumn(KGFDataColumn theColumn)
	{
		itsSortColumn = theColumn;
		itsRepaint = true;
		if (this.EventSettingsChanged != null)
		{
			this.EventSettingsChanged(this, null);
		}
	}

	public KGFDataColumn GetSortingColumn()
	{
		return itsSortColumn;
	}

	public string SaveSettings()
	{
		return string.Format("SortBy:" + ((itsSortColumn == null) ? string.Empty : itsSortColumn.ColumnName));
	}

	public void LoadSettings(string theSettingsString)
	{
		string[] array = theSettingsString.Split(new char[1]
		{
			':'
		});
		if (array.Length == 2 && array[0] == "SortBy")
		{
			if (array[1].Trim() == string.Empty)
			{
				SetSortingColumn((KGFDataColumn)null);
			}
			else
			{
				SetSortingColumn(array[1]);
			}
		}
	}

	public void Render()
	{
		if (itsVisible)
		{
			GUILayout.BeginVertical();
			RenderTableHeadings();
			RenderTableRows();
			GUILayout.EndVertical();
		}
	}

	public string GetName()
	{
		return "KGFGUIDataTable";
	}

	public bool IsVisible()
	{
		return itsVisible;
	}
}
