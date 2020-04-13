using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class KGFGUIObjectList
{
    public enum KGFeItemsPerPage
    {
        e10 = 10,
        e25 = 25,
        e50 = 50,
        e100 = 100,
        e250 = 250,
        e500 = 500
    }

    public class KGFGUIObjectListSelectEventArgs : EventArgs
    {
        private KGFITaggable itsItem;

        public KGFGUIObjectListSelectEventArgs(KGFITaggable theItem)
        {
            itsItem = theItem;
        }

        public KGFITaggable GetItem()
        {
            return itsItem;
        }
    }

    private class KGFObjectListColumnItem
    {
        public string itsHeader;

        public bool itsSearchable;

        public bool itsDisplay;

        public KGFGUIDropDown itsDropDown;

        public string itsFilterString = string.Empty;

        private MemberInfo itsMemberInfo;

        public KGFObjectListColumnItem(MemberInfo theMemberInfo)
        {
            itsMemberInfo = theMemberInfo;
        }

        public Type GetReturnType()
        {
            if (itsMemberInfo is FieldInfo)
            {
                return ((FieldInfo)itsMemberInfo).FieldType;
            }
            if (itsMemberInfo is PropertyInfo)
            {
                return ((PropertyInfo)itsMemberInfo).PropertyType;
            }
            return null;
        }

        public object GetReturnValue(object theInstance)
        {
            if (itsMemberInfo is FieldInfo)
            {
                return ((FieldInfo)itsMemberInfo).GetValue(theInstance);
            }
            if (itsMemberInfo is PropertyInfo)
            {
                return ((PropertyInfo)itsMemberInfo).GetValue(theInstance, null);
            }
            return null;
        }

        public bool GetIsFiltered(object theInstance)
        {
            if ((object)GetReturnType() == typeof(bool) || GetReturnType().IsEnum)
            {
                if (itsDropDown != null)
                {
                    if (itsDropDown.SelectedItem() == "<NONE>")
                    {
                        return false;
                    }
                    if (itsDropDown.SelectedItem() != theInstance.ToString())
                    {
                        return true;
                    }
                }
                return false;
            }
            if (!string.IsNullOrEmpty(itsFilterString) && !theInstance.ToString().ToLower().Contains(itsFilterString.ToLower()))
            {
                return true;
            }
            return false;
        }
    }

    private const string NONE_STRING = "<NONE>";

    private const string itsControlSearchName = "KGFGuiObjectList.FullTextSearch";

    private const string itsTextSearch = "Search";

    private const string UnTagged = "<untagged>";

    private List<KGFITaggable> itsListData;

    private Type itsItemType;

    private KGFDataTable itsData;

    private KGFGUIDataTable itsGuiData;

    private List<KGFObjectListColumnItem> itsListFieldCache;

    private bool itsDisplayFullTextSearch;

    private string itsFulltextSearch = string.Empty;

    private KGFGUISelectionList itsListViewCategories;

    private KGFDataRow itsCurrentSelectedRow;

    private KGFITaggable itsCurrentSelectedItem;

    private KGFeItemsPerPage itsItemsPerPage = KGFeItemsPerPage.e50;

    private bool itsIncludeAll = true;

    public int itsCurrentPage;

    private bool itsLoadingActive;

    private bool itsDisplayEntriesPerPage = true;

    private bool itsRepaintWish;

    private bool itsUpdateWish;

    private string[] itsBoolValues = new string[2]
    {
        "True",
        "False"
    };

    public event EventHandler EventSelect;

    public event EventHandler EventSettingsChanged;

    public event EventHandler EventNew;

    public event EventHandler EventDelete;

    public event EventHandler PreRenderRow
    {
        add
        {
            itsGuiData.PreRenderRow += value;
        }
        remove
        {
            itsGuiData.PreRenderRow -= value;
        }
    }

    public event Func<KGFDataRow, KGFDataColumn, uint, bool> PreCellHandler
    {
        add
        {
            itsGuiData.PreCellContentHandler += value;
        }
        remove
        {
            itsGuiData.PreCellContentHandler -= value;
        }
    }

    public event EventHandler PostRenderRow
    {
        add
        {
            itsGuiData.PostRenderRow += value;
        }
        remove
        {
            itsGuiData.PostRenderRow -= value;
        }
    }

    public KGFGUIObjectList(Type theType)
    {
        itsListData = new List<KGFITaggable>();
        itsItemType = theType;
        itsData = new KGFDataTable();
        itsListFieldCache = new List<KGFObjectListColumnItem>();
        CacheTypeMembers();
        itsGuiData = new KGFGUIDataTable(itsData);
        itsGuiData.OnClickRow += OnClickRow;
        itsGuiData.EventSettingsChanged += OnGuiDataSettingsChanged;
        itsGuiData.SetColumnVisible(0, theValue: false);
        for (int i = 0; i < itsListFieldCache.Count; i++)
        {
            itsGuiData.SetColumnVisible(i + 1, itsListFieldCache[i].itsDisplay);
        }
        itsListViewCategories = new KGFGUISelectionList();
        itsListViewCategories.EventItemChanged += OnCategoriesChanged;
    }

    private void OnGuiDataSettingsChanged(object theSender, EventArgs theArgs)
    {
        OnSettingsChanged();
    }

    public void SetFulltextFilter(string theFulltextSearch)
    {
        itsFulltextSearch = theFulltextSearch;
        UpdateList();
    }

    public void SetColumnWidthAll(uint theWidth)
    {
        for (int i = 1; i < itsListFieldCache.Count + 1; i++)
        {
            itsGuiData.SetColumnWidth(i, theWidth);
        }
    }

    public void SetColumnWidth(string theColumnHeader, uint theWidth)
    {
        int num = 0;
        while (true)
        {
            if (num < itsListFieldCache.Count)
            {
                if (itsListFieldCache[num].itsDisplay && itsListFieldCache[num].itsHeader == theColumnHeader)
                {
                    break;
                }
                num++;
                continue;
            }
            return;
        }
        itsGuiData.SetColumnWidth(num + 1, theWidth);
    }

    public void SetColumnVisible(string theColumnHeader, bool theVisible)
    {
        int num = 0;
        while (true)
        {
            if (num < itsListFieldCache.Count)
            {
                if (itsListFieldCache[num].itsDisplay && itsListFieldCache[num].itsHeader == theColumnHeader)
                {
                    break;
                }
                num++;
                continue;
            }
            return;
        }
        itsGuiData.SetColumnVisible(num + 1, theVisible);
    }

    public void SetList(IEnumerable theList)
    {
        List<KGFITaggable> list = new List<KGFITaggable>();
        foreach (object the in theList)
        {
            if (the is KGFITaggable)
            {
                list.Add((KGFITaggable)the);
            }
        }
        SetList(list);
    }

    public void SetList(IEnumerable<KGFITaggable> theList)
    {
        itsListData = new List<KGFITaggable>(theList);
        itsListViewCategories.SetValues(GetAllTags().Distinct());
        UpdateList();
    }

    public void AddMember(MemberInfo theMemberInfo, string theHeader)
    {
        AddMember(theMemberInfo, theHeader, theSearchable: false);
    }

    public void AddMember(MemberInfo theMemberInfo, string theHeader, bool theSearchable)
    {
        AddMember(theMemberInfo, theHeader, theSearchable, theDisplay: true);
    }

    public void AddMember(MemberInfo theMemberInfo, string theHeader, bool theSearchable, bool theDisplay)
    {
        KGFObjectListColumnItem kGFObjectListColumnItem = new KGFObjectListColumnItem(theMemberInfo);
        kGFObjectListColumnItem.itsHeader = theHeader;
        kGFObjectListColumnItem.itsSearchable = theSearchable;
        kGFObjectListColumnItem.itsDisplay = theDisplay;
        itsListFieldCache.Add(kGFObjectListColumnItem);
        itsData.Columns.Add(new KGFDataColumn(theHeader, kGFObjectListColumnItem.GetReturnType()));
        if (kGFObjectListColumnItem.itsSearchable)
        {
            itsDisplayFullTextSearch = true;
        }
    }

    public object GetCurrentSelected()
    {
        return itsCurrentSelectedItem;
    }

    public void ClearSelected()
    {
        itsCurrentSelectedItem = null;
    }

    public void SetSelected(KGFITaggable theObject)
    {
        itsCurrentSelectedItem = theObject;
        int num = 0;
        foreach (KGFDataRow row in itsData.Rows)
        {
            if (row[0].Value == theObject)
            {
                itsGuiData.SetCurrentSelected(row);
                itsCurrentPage = num / (int)itsItemsPerPage;
                break;
            }
            num++;
        }
    }

    public Rect GetLastRectScrollView()
    {
        return itsGuiData.GetLastRectScrollview();
    }

    public string SaveSettings()
    {
        List<string> list = new List<string>();
        foreach (KGFObjectListColumnItem item in itsListFieldCache)
        {
            if (item.itsDropDown != null)
            {
                list.Add(item.itsHeader + "=" + item.itsDropDown.SelectedItem());
            }
            else
            {
                list.Add(item.itsHeader + "=" + item.itsFilterString);
            }
        }
        string arg = (itsGuiData.GetSortingColumn() == null) ? string.Empty : itsGuiData.GetSortingColumn().ColumnName;
        List<string> list2 = new List<string>();
        foreach (object item2 in itsListViewCategories.GetSelected())
        {
            list2.Add(string.Empty + item2);
        }
        string arg2 = list2.JoinToString(",");
        return string.Format("Filter:{0};SortBy:{1};Tags:{2}", list.JoinToString(","), arg, arg2);
    }

    public void LoadSettings(string theSettingsString)
    {
        itsLoadingActive = true;
        string[] array = theSettingsString.Split(new char[1]
        {
            ';'
        });
        string[] array2 = array;
        foreach (string text in array2)
        {
            string[] array3 = text.Split(new char[1]
            {
                ':'
            });
            if (array3.Length != 2)
            {
                continue;
            }
            if (array3[0] == "Filter")
            {
                string[] array4 = array3[1].Split(new char[1]
                {
                    ','
                });
                foreach (string text2 in array4)
                {
                    string[] array5 = text2.Split(new char[1]
                    {
                        '='
                    });
                    if (array5.Length == 2)
                    {
                        SetFilterInternal(array5[0], array5[1]);
                    }
                }
            }
            if (array3[0] == "SortBy")
            {
                if (array3[1].Trim() == string.Empty)
                {
                    itsGuiData.SetSortingColumn((KGFDataColumn)null);
                }
                else
                {
                    itsGuiData.SetSortingColumn(array3[1]);
                }
            }
            if (array3[0] == "Tags")
            {
                itsListViewCategories.SetSelectedAll(theValue: false);
                string[] array6 = array3[1].Split(new char[1]
                {
                    ','
                });
                foreach (string theItem in array6)
                {
                    itsListViewCategories.SetSelected(theItem, theSelectionState: true);
                }
            }
        }
        itsRepaintWish = true;
        UpdateList();
        itsLoadingActive = false;
    }

    public void SetFilter(string theColumnName, string theFilter)
    {
        if (SetFilterInternal(theColumnName, theFilter))
        {
            OnSettingsChanged();
        }
    }

    public void ClearFilters()
    {
        foreach (KGFObjectListColumnItem item in itsListFieldCache)
        {
            item.itsFilterString = string.Empty;
            if (item.itsDropDown != null)
            {
                item.itsDropDown.SetSelectedItem("<NONE>");
            }
        }
        itsRepaintWish = true;
        OnSettingsChanged();
    }

    private bool SetFilterInternal(string theColumnName, string theFilter)
    {
        foreach (KGFObjectListColumnItem item in itsListFieldCache)
        {
            if (theColumnName == item.itsHeader)
            {
                item.itsFilterString = theFilter;
                itsRepaintWish = true;
                return true;
            }
        }
        return false;
    }

    public void Render()
    {
        if (itsUpdateWish)
        {
            UpdateList();
        }
        int num = (int)Math.Ceiling((float)itsData.Rows.Count / (float)itsItemsPerPage);
        if (itsCurrentPage >= num)
        {
            itsCurrentPage = 0;
        }
        itsRepaintWish = false;
        itsGuiData.SetDisplayRowCount((uint)itsItemsPerPage);
        KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxDecorated);
        GUILayout.BeginVertical(GUILayout.Width(180f));
        itsListViewCategories.Render();
        GUILayout.EndVertical();
        KGFGUIUtility.SpaceSmall();
        GUILayout.BeginVertical();
        itsGuiData.SetStartRow((uint)(itsCurrentPage * (uint)itsItemsPerPage));
        itsGuiData.Render();
        KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxMiddleVerticalInteractive);
        int num2 = 0;
        foreach (KGFObjectListColumnItem item in itsListFieldCache)
        {
            num2++;
            if (item.itsDisplay && itsGuiData.GetColumnVisible(num2))
            {
                if (item.itsSearchable && (item.GetReturnType().IsEnum || (object)item.GetReturnType() == typeof(bool) || (object)item.GetReturnType() == typeof(string)))
                {
                    GUILayout.BeginHorizontal(GUILayout.Width((float)(double)itsGuiData.GetColumnWidth(num2)));
                    KGFGUIUtility.BeginVerticalBox(KGFGUIUtility.eStyleBox.eBoxInvisible);
                    DrawFilterBox(item, itsGuiData.GetColumnWidth(num2) - 4);
                    KGFGUIUtility.EndVerticalBox();
                    GUILayout.EndHorizontal();
                    KGFGUIUtility.Separator(KGFGUIUtility.eStyleSeparator.eSeparatorVerticalFitInBox);
                }
                else
                {
                    GUILayout.BeginHorizontal(GUILayout.Width((float)(double)itsGuiData.GetColumnWidth(num2)));
                    GUILayout.Label(" ");
                    GUILayout.EndHorizontal();
                    KGFGUIUtility.Separator(KGFGUIUtility.eStyleSeparator.eSeparatorVerticalFitInBox);
                }
            }
        }
        GUILayout.FlexibleSpace();
        KGFGUIUtility.EndHorizontalBox();
        KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxDarkMiddleVertical);
        GUILayout.Label(string.Empty);
        GUILayout.FlexibleSpace();
        KGFGUIUtility.EndHorizontalBox();
        KGFGUIUtility.BeginVerticalBox(KGFGUIUtility.eStyleBox.eBoxDarkBottom);
        GUILayout.BeginHorizontal();
        if (!Application.isPlaying)
        {
            if (this.EventNew != null && KGFGUIUtility.Button("New", KGFGUIUtility.eStyleButton.eButton, GUILayout.Width(75f)))
            {
                this.EventNew(this, null);
            }
            if (this.EventDelete != null && KGFGUIUtility.Button("Delete", KGFGUIUtility.eStyleButton.eButton, GUILayout.Width(75f)))
            {
                this.EventDelete(this, null);
            }
            GUILayout.FlexibleSpace();
        }
        if (itsDisplayFullTextSearch)
        {
            GUI.SetNextControlName("KGFGuiObjectList.FullTextSearch");
            string a = KGFGUIUtility.TextField(itsFulltextSearch, KGFGUIUtility.eStyleTextField.eTextField, GUILayout.Width(200f));
            if (a != itsFulltextSearch)
            {
                itsFulltextSearch = a;
                UpdateList();
            }
        }
        KGFGUIUtility.Space();
        bool flag = KGFGUIUtility.Toggle(itsIncludeAll, "all Tags", KGFGUIUtility.eStyleToggl.eTogglSuperCompact, GUILayout.Width(70f));
        if (flag != itsIncludeAll)
        {
            itsIncludeAll = flag;
            UpdateList();
        }
        if (KGFGUIUtility.Button("clear filters", KGFGUIUtility.eStyleButton.eButton, GUILayout.Width(100f)))
        {
            itsFulltextSearch = string.Empty;
            ClearFilters();
            UpdateList();
        }
        GUILayout.FlexibleSpace();
        KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxInvisible);
        if (GetDisplayEntriesPerPage())
        {
            if (KGFGUIUtility.Button("<", KGFGUIUtility.eStyleButton.eButtonLeft, GUILayout.Width(25f)))
            {
                switch (itsItemsPerPage)
                {
                    case KGFeItemsPerPage.e25:
                        itsItemsPerPage = KGFeItemsPerPage.e10;
                        break;
                    case KGFeItemsPerPage.e50:
                        itsItemsPerPage = KGFeItemsPerPage.e25;
                        break;
                    case KGFeItemsPerPage.e100:
                        itsItemsPerPage = KGFeItemsPerPage.e50;
                        break;
                    case KGFeItemsPerPage.e250:
                        itsItemsPerPage = KGFeItemsPerPage.e100;
                        break;
                    case KGFeItemsPerPage.e500:
                        itsItemsPerPage = KGFeItemsPerPage.e250;
                        break;
                }
            }
            KGFGUIUtility.BeginVerticalBox(KGFGUIUtility.eStyleBox.eBoxMiddleHorizontal);
            string theText = itsItemsPerPage.ToString().Substring(1) + " entries per page";
            KGFGUIUtility.Label(theText, KGFGUIUtility.eStyleLabel.eLabelFitIntoBox);
            KGFGUIUtility.EndVerticalBox();
            if (KGFGUIUtility.Button(">", KGFGUIUtility.eStyleButton.eButtonRight, GUILayout.Width(25f)))
            {
                switch (itsItemsPerPage)
                {
                    case KGFeItemsPerPage.e10:
                        itsItemsPerPage = KGFeItemsPerPage.e25;
                        break;
                    case KGFeItemsPerPage.e25:
                        itsItemsPerPage = KGFeItemsPerPage.e50;
                        break;
                    case KGFeItemsPerPage.e50:
                        itsItemsPerPage = KGFeItemsPerPage.e100;
                        break;
                    case KGFeItemsPerPage.e100:
                        itsItemsPerPage = KGFeItemsPerPage.e250;
                        break;
                    case KGFeItemsPerPage.e250:
                        itsItemsPerPage = KGFeItemsPerPage.e500;
                        break;
                }
            }
        }
        GUILayout.Space(10f);
        if (KGFGUIUtility.Button("<", KGFGUIUtility.eStyleButton.eButtonLeft, GUILayout.Width(25f)) && itsCurrentPage > 0)
        {
            itsCurrentPage--;
        }
        KGFGUIUtility.BeginVerticalBox(KGFGUIUtility.eStyleBox.eBoxMiddleHorizontal);
        string theText2 = $"page {itsCurrentPage + 1}/{Math.Max(num, 1)}";
        KGFGUIUtility.Label(theText2, KGFGUIUtility.eStyleLabel.eLabelFitIntoBox);
        KGFGUIUtility.EndVerticalBox();
        if (KGFGUIUtility.Button(">", KGFGUIUtility.eStyleButton.eButtonRight, GUILayout.Width(25f)) && itsData.Rows.Count > (itsCurrentPage + 1) * (int)itsItemsPerPage)
        {
            itsCurrentPage++;
        }
        KGFGUIUtility.EndHorizontalBox();
        GUILayout.EndHorizontal();
        KGFGUIUtility.EndVerticalBox();
        GUILayout.EndVertical();
        KGFGUIUtility.EndHorizontalBox();
        if (GUI.GetNameOfFocusedControl().Equals("KGFGuiObjectList.FullTextSearch") && itsFulltextSearch.Equals("Search"))
        {
            itsFulltextSearch = string.Empty;
        }
        if (!GUI.GetNameOfFocusedControl().Equals("KGFGuiObjectList.FullTextSearch") && itsFulltextSearch.Equals(string.Empty))
        {
            itsFulltextSearch = "Search";
        }
    }

    public void SetDisplayEntriesPerPage(bool theDisplay)
    {
        itsDisplayEntriesPerPage = theDisplay;
    }

    public bool GetDisplayEntriesPerPage()
    {
        return itsDisplayEntriesPerPage;
    }

    public bool GetRepaint()
    {
        return itsGuiData.GetRepaintWish() || itsRepaintWish;
    }

    private void OnClickRow(object theSender, EventArgs theArgs)
    {
        KGFDataRow kGFDataRow = theSender as KGFDataRow;
        if (kGFDataRow != null)
        {
            itsCurrentSelectedItem = (KGFITaggable)kGFDataRow[0].Value;
            if (itsCurrentSelectedRow != kGFDataRow)
            {
                itsCurrentSelectedRow = kGFDataRow;
            }
            if (this.EventSelect != null)
            {
                this.EventSelect(this, new KGFGUIObjectListSelectEventArgs(itsCurrentSelectedItem));
            }
        }
    }

    private void OnCategoriesChanged(object theSender, EventArgs theArgs)
    {
        UpdateList();
        OnSettingsChanged();
    }

    private void OnSettingsChanged()
    {
        if (!itsLoadingActive && this.EventSettingsChanged != null)
        {
            this.EventSettingsChanged(this, null);
        }
    }

    private IEnumerable<string> GetAllTags()
    {
        foreach (KGFITaggable anItem in itsListData)
        {
            if (anItem.GetTags().Length == 0)
            {
                yield return "<untagged>";
            }
            string[] tags = anItem.GetTags();
            for (int i = 0; i < tags.Length; i++)
            {
                yield return tags[i];
            }
        }
    }

    private void CacheTypeMembers()
    {
        itsDisplayFullTextSearch = false;
        itsData.Rows.Clear();
        itsData.Columns.Clear();
        itsListFieldCache.Clear();
        itsData.Columns.Add(new KGFDataColumn("DATA", itsItemType));
        FieldInfo[] fields = itsItemType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (FieldInfo theMemberInfo in fields)
        {
            TryAddMember(theMemberInfo);
        }
        PropertyInfo[] properties = itsItemType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (PropertyInfo theMemberInfo2 in properties)
        {
            TryAddMember(theMemberInfo2);
        }
    }

    private void TryAddMember(MemberInfo theMemberInfo)
    {
        KGFObjectListItemDisplayAttribute[] array = theMemberInfo.GetCustomAttributes(typeof(KGFObjectListItemDisplayAttribute), inherit: true) as KGFObjectListItemDisplayAttribute[];
        if (array.Length == 1)
        {
            AddMember(theMemberInfo, array[0].Header, array[0].Searchable, array[0].Display);
        }
    }

    private bool FullTextFilter(KGFITaggable theItem)
    {
        if (itsFulltextSearch.Trim() == "Search")
        {
            return false;
        }
        string[] array = itsFulltextSearch.Trim().ToLower().Split(new char[1]
        {
            ' '
        });
        foreach (string text in array)
        {
            bool flag = false;
            string value = text;
            string text2 = null;
            string[] array2 = text.Split(new char[1]
            {
                '='
            });
            if (array2.Length == 2)
            {
                value = array2[1];
                text2 = array2[0];
            }
            foreach (KGFObjectListColumnItem item in itsListFieldCache)
            {
                if (text2 == null || !(item.itsHeader.ToLower() != text2.ToLower()))
                {
                    object returnValue = item.GetReturnValue(theItem);
                    if (item.itsSearchable)
                    {
                        if (returnValue is IEnumerable && !(returnValue is string))
                        {
                            foreach (object item2 in (IEnumerable)returnValue)
                            {
                                if (item2 != null && item2.ToString().Trim().ToLower()
                                    .Contains(value))
                                {
                                    flag = true;
                                }
                            }
                        }
                        else
                        {
                            string text3 = returnValue.ToString();
                            if (text3.Trim().ToLower().Contains(value))
                            {
                                flag = true;
                            }
                        }
                    }
                }
            }
            if (!flag)
            {
                return true;
            }
        }
        return false;
    }

    private bool PerItemFilter(KGFITaggable theItem)
    {
        foreach (KGFObjectListColumnItem item in itsListFieldCache)
        {
            object returnValue = item.GetReturnValue(theItem);
            if (item.GetIsFiltered(returnValue))
            {
                return true;
            }
        }
        return false;
    }

    private void UpdateList()
    {
        if (Event.current != null && Event.current.type != EventType.Layout)
        {
            itsUpdateWish = true;
            return;
        }
        itsUpdateWish = false;
        itsData.Rows.Clear();
        foreach (KGFITaggable itsListDatum in itsListData)
        {
            if (GetIsTagSelected(itsListDatum.GetTags()) && (string.IsNullOrEmpty(itsFulltextSearch) || !FullTextFilter(itsListDatum)) && !PerItemFilter(itsListDatum))
            {
                KGFDataRow kGFDataRow = itsData.NewRow();
                kGFDataRow[0].Value = itsListDatum;
                int num = 1;
                foreach (KGFObjectListColumnItem item in itsListFieldCache)
                {
                    object returnValue = item.GetReturnValue(itsListDatum);
                    kGFDataRow[num].Value = returnValue;
                    num++;
                }
                itsData.Rows.Add(kGFDataRow);
            }
        }
    }

    private bool GetIsTagSelected(string[] theTags)
    {
        List<object> list = new List<object>(itsListViewCategories.GetSelected());
        int count = list.Count;
        int num = 0;
        foreach (string item in itsListViewCategories.GetSelected())
        {
            if (theTags.Length == 0 && item == "<untagged>")
            {
                if (!itsIncludeAll)
                {
                    return true;
                }
                num++;
            }
            foreach (string a in theTags)
            {
                if (a == item)
                {
                    if (!itsIncludeAll)
                    {
                        return true;
                    }
                    num++;
                }
            }
        }
        if (num == count && itsIncludeAll)
        {
            return true;
        }
        return false;
    }

    private void OnDropDownValueChanged(object theSender, EventArgs theArgs)
    {
        UpdateList();
        OnSettingsChanged();
    }

    private void DrawFilterBox(KGFObjectListColumnItem theItem, uint theWidth)
    {
        if (theItem.GetReturnType().IsEnum || (object)theItem.GetReturnType() == typeof(bool))
        {
            if (theItem.itsDropDown == null)
            {
                if ((object)theItem.GetReturnType() == typeof(bool))
                {
                    theItem.itsDropDown = new KGFGUIDropDown(new List<string>(itsBoolValues).InsertItem("<NONE>", 0), theWidth, 5u, KGFGUIDropDown.eDropDirection.eUp);
                }
                else if (theItem.GetReturnType().IsEnum)
                {
                    theItem.itsDropDown = new KGFGUIDropDown(Enum.GetNames(theItem.GetReturnType()).InsertItem("<NONE>", 0), theWidth, 5u, KGFGUIDropDown.eDropDirection.eUp);
                }
                theItem.itsDropDown.itsTitle = string.Empty;
                theItem.itsDropDown.SetSelectedItem(theItem.itsFilterString);
                theItem.itsDropDown.SelectedValueChanged += OnDropDownValueChanged;
            }
            theItem.itsDropDown.Render();
        }
        else if ((object)theItem.GetReturnType() == typeof(string))
        {
            if (theItem.itsFilterString == null)
            {
                theItem.itsFilterString = string.Empty;
            }
            string text = KGFGUIUtility.TextField(theItem.itsFilterString, KGFGUIUtility.eStyleTextField.eTextField, GUILayout.Width((float)(double)theWidth));
            if (text != theItem.itsFilterString)
            {
                theItem.itsFilterString = text;
                UpdateList();
                OnSettingsChanged();
            }
        }
    }
}
