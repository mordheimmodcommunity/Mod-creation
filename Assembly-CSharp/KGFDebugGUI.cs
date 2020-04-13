using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;

public class KGFDebugGUI : KGFModule, KGFIDebug
{
    [Serializable]
    public class KGFDataModuleGUILogger
    {
        public KGFeDebugLevel itsMinimumLogLevel;

        public KGFeItemsPerPage itsLogsPerPage = KGFeItemsPerPage.e25;

        public KGFeDebugLevel itsMinimumExpandLogLevel = KGFeDebugLevel.eOff;

        public Color itsColorDebug = Color.white;

        public Color itsColorInfo = Color.grey;

        public Color itsColorWarning = Color.yellow;

        public Color itsColorError = Color.red;

        public Color itsColorFatal = Color.magenta;

        public Texture2D itsIconDebug;

        public Texture2D itsIconInfo;

        public Texture2D itsIconWarning;

        public Texture2D itsIconError;

        public Texture2D itsIconFatal;

        public Texture2D itsIconHelp;

        public Texture2D itsIconLeft;

        public Texture2D itsIconRight;

        public float itsFPSUpdateInterval = 0.5f;

        public KeyCode itsHideKeyModifier;

        public KeyCode itsHideKey = KeyCode.F1;

        public KeyCode itsExpandKeyModifier = KeyCode.LeftAlt;

        public KeyCode itsExpandKey = KeyCode.F1;

        public bool itsVisible = true;
    }

    private class KGFDebugCategory
    {
        private int itsCount;

        private string itsName;

        public bool itsSelectedState;

        public KGFDebugCategory(string theName)
        {
            itsName = theName;
            itsCount = 0;
        }

        public string GetName()
        {
            return itsName;
        }

        public void IncreaseCount()
        {
            itsCount++;
        }

        public void DecreaseCount()
        {
            itsCount--;
        }

        public int GetCount()
        {
            return itsCount;
        }
    }

    private static KGFDebugGUI itsInstance;

    private KGFDataTable itsLogTable = new KGFDataTable();

    private KGFGUIDataTable itsTableControl;

    private List<KGFDebug.KGFDebugLog> itsLogList = new List<KGFDebug.KGFDebugLog>();

    private Dictionary<string, KGFDebugCategory> itsLogCategories = new Dictionary<string, KGFDebugCategory>();

    private Dictionary<KGFeDebugLevel, bool> itsLogLevelFilter = new Dictionary<KGFeDebugLevel, bool>();

    private Vector2 itsCategoryScrollViewPosition = Vector2.zero;

    private string itsSearchFilterMessage = "Search";

    private string itsSearchFilterCategory = "Search";

    private float itsCurrentHeight = Screen.height;

    private bool itsOpen;

    private bool itsLiveSearchChanged;

    private float itsLastChangeTime;

    private uint itsCurrentPage;

    private KGFDataRow itsCurrentSelectedRow;

    private Dictionary<KGFeDebugLevel, uint> itsLogCategoryCount = new Dictionary<KGFeDebugLevel, uint>();

    public KGFDataModuleGUILogger itsDataModuleGUILogger = new KGFDataModuleGUILogger();

    private Rect itsOpenWindow;

    private Rect itsMinimizedWindow;

    private float itsAccumulatedFrames;

    private int itsFramesInInterval;

    private float itsTimeLeft;

    private float itsCurrentFPS;

    private bool itsHover;

    private bool itsFocus;

    private bool itsLeaveFocus;

    public KGFDebugGUI()
        : base(new Version(1, 0, 0, 1), new Version(1, 1, 0, 0))
    {
    }

    private void OnEnable()
    {
        Application.RegisterLogCallback(HandleLog);
    }

    private void OnDisable()
    {
        Application.RegisterLogCallback(null);
    }

    private void HandleLog(string theLogString, string theStackTrace, LogType theLogType)
    {
        KGFeDebugLevel theLevel = KGFeDebugLevel.eInfo;
        switch (theLogType)
        {
            case LogType.Assert:
                theLevel = KGFeDebugLevel.eFatal;
                break;
            case LogType.Error:
                theLevel = KGFeDebugLevel.eError;
                break;
            case LogType.Exception:
                theLevel = KGFeDebugLevel.eError;
                break;
            case LogType.Log:
                theLevel = KGFeDebugLevel.eInfo;
                break;
            case LogType.Warning:
                theLevel = KGFeDebugLevel.eWarning;
                break;
        }
        Log(theLevel, "CONSOLE", theLogString, theStackTrace);
    }

    protected override void KGFAwake()
    {
        if (itsInstance == null)
        {
            Init();
            KGFDebug.AddLogger(this);
        }
        else if (itsInstance != this)
        {
            UnityEngine.Object.Destroy(itsInstance.gameObject);
            UnityEngine.Debug.Log("multiple instances of KGFGUILogger are not allowed");
        }
    }

    public void Start()
    {
        itsTimeLeft = itsDataModuleGUILogger.itsFPSUpdateInterval;
    }

    public void Update()
    {
        itsTimeLeft -= Time.deltaTime;
        itsAccumulatedFrames += Time.timeScale / Time.deltaTime;
        itsFramesInInterval++;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            itsLeaveFocus = true;
        }
        if (itsDataModuleGUILogger.itsHideKey != 0 && !Input.GetKey(itsDataModuleGUILogger.itsExpandKeyModifier) && ((Input.GetKey(itsDataModuleGUILogger.itsHideKeyModifier) && Input.GetKey(itsDataModuleGUILogger.itsHideKeyModifier) && Input.GetKeyDown(itsDataModuleGUILogger.itsHideKey)) || (itsDataModuleGUILogger.itsHideKeyModifier == KeyCode.None && Input.GetKeyDown(itsDataModuleGUILogger.itsHideKey))))
        {
            itsDataModuleGUILogger.itsVisible = !itsDataModuleGUILogger.itsVisible;
        }
        if (itsDataModuleGUILogger.itsExpandKey != 0 && !Input.GetKey(itsDataModuleGUILogger.itsHideKeyModifier) && ((Input.GetKey(itsDataModuleGUILogger.itsExpandKeyModifier) && Input.GetKeyDown(itsDataModuleGUILogger.itsExpandKey)) || (itsDataModuleGUILogger.itsExpandKeyModifier == KeyCode.None && Input.GetKeyDown(itsDataModuleGUILogger.itsExpandKey))))
        {
            itsOpen = !itsOpen;
            if (itsOpen)
            {
                itsDataModuleGUILogger.itsVisible = true;
            }
        }
    }

    private static KGFDebugGUI GetInstance()
    {
        return itsInstance;
    }

    public static bool GetFocused()
    {
        if (itsInstance != null && itsInstance.itsDataModuleGUILogger.itsVisible && (itsInstance.itsHover || itsInstance.itsFocus))
        {
            return true;
        }
        return false;
    }

    public static bool GetHover()
    {
        if (itsInstance != null && itsInstance.itsDataModuleGUILogger.itsVisible && itsInstance.itsHover)
        {
            return true;
        }
        return false;
    }

    public static void Render()
    {
        KGFGUIUtility.SetSkinIndex(0);
        if (itsInstance != null)
        {
            if (!itsInstance.itsDataModuleGUILogger.itsVisible)
            {
                return;
            }
            KGFModule.RenderHelpWindow();
            if (itsInstance.itsOpen)
            {
                itsInstance.itsOpenWindow.x = 0f;
                itsInstance.itsOpenWindow.y = 0f;
                itsInstance.itsOpenWindow.width = Screen.width;
                itsInstance.itsOpenWindow.height = itsInstance.itsCurrentHeight;
                if (itsInstance.itsCurrentHeight < KGFGUIUtility.GetSkinHeight() * 11f)
                {
                    itsInstance.itsCurrentHeight = KGFGUIUtility.GetSkinHeight() * 11f;
                }
                else if (itsInstance.itsCurrentHeight > (float)Screen.height / 3f * 2f)
                {
                    itsInstance.itsCurrentHeight = (float)Screen.height / 3f * 2f;
                }
                GUILayout.BeginArea(itsInstance.itsOpenWindow);
                KGFGUIUtility.BeginVerticalBox(KGFGUIUtility.eStyleBox.eBoxDecorated);
                Texture2D theIcon = null;
                if (KGFDebug.GetInstance() != null)
                {
                    theIcon = KGFDebug.GetInstance().GetIcon();
                }
                KGFGUIUtility.BeginWindowHeader("KGFDebugger", theIcon);
                itsInstance.DrawSummary();
                GUILayout.FlexibleSpace();
                float num = itsInstance.itsCurrentHeight;
                itsInstance.itsCurrentHeight = KGFGUIUtility.HorizontalSlider(itsInstance.itsCurrentHeight, (float)Screen.height / 3f * 2f, KGFGUIUtility.GetSkinHeight() * 11f, GUILayout.Width(75f));
                if (num != itsInstance.itsCurrentHeight)
                {
                    itsInstance.SaveSizeToPlayerPrefs();
                }
                if (KGFGUIUtility.Button(itsInstance.itsDataModuleGUILogger.itsIconHelp, KGFGUIUtility.eStyleButton.eButton))
                {
                    KGFModule.OpenHelpWindow(itsInstance);
                }
                itsInstance.itsOpen = KGFGUIUtility.Toggle(itsInstance.itsOpen, string.Empty, KGFGUIUtility.eStyleToggl.eTogglSwitch, GUILayout.Width(KGFGUIUtility.GetSkinHeight()));
                KGFGUIUtility.EndWindowHeader(theCloseButton: false);
                GUILayout.Space(5f);
                GUILayout.BeginHorizontal();
                KGFGUIUtility.BeginVerticalBox(KGFGUIUtility.eStyleBox.eBoxDecorated, GUILayout.Width((float)Screen.width * 0.2f));
                itsInstance.DrawCategoryColumn();
                KGFGUIUtility.EndVerticalBox();
                KGFGUIUtility.BeginVerticalBox(KGFGUIUtility.eStyleBox.eBoxDecorated);
                itsInstance.DrawContentColumn();
                KGFGUIUtility.EndVerticalBox();
                GUILayout.EndHorizontal();
                KGFGUIUtility.EndVerticalBox();
                GUILayout.EndArea();
            }
            else
            {
                itsInstance.DrawMinimizedWindow();
            }
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.y = (float)Screen.height - mousePosition.y;
            if (itsInstance.itsOpen)
            {
                if (itsInstance.itsOpenWindow.Contains(mousePosition))
                {
                    if (!itsInstance.itsHover)
                    {
                        itsInstance.itsHover = true;
                    }
                }
                else if (itsInstance.itsHover && !itsInstance.itsOpenWindow.Contains(mousePosition))
                {
                    itsInstance.itsHover = false;
                }
            }
            else if (!itsInstance.itsOpen)
            {
                if (itsInstance.itsMinimizedWindow.Contains(mousePosition))
                {
                    if (!itsInstance.itsHover)
                    {
                        itsInstance.itsHover = true;
                    }
                }
                else if (itsInstance.itsHover && !itsInstance.itsMinimizedWindow.Contains(mousePosition))
                {
                    itsInstance.itsHover = false;
                }
            }
            GUI.SetNextControlName(string.Empty);
            if (itsInstance.itsLeaveFocus && itsInstance.itsFocus)
            {
                UnityEngine.Debug.Log("unfocus KGFDebugGUI");
                GUI.FocusControl(string.Empty);
                itsInstance.itsLeaveFocus = false;
            }
            else
            {
                itsInstance.itsLeaveFocus = false;
            }
        }
        KGFGUIUtility.SetSkinIndex(1);
    }

    public static void Expand()
    {
        if (itsInstance != null)
        {
            itsInstance.itsOpen = true;
        }
    }

    public static bool GetExpanded()
    {
        if (itsInstance != null)
        {
            return itsInstance.itsOpen;
        }
        return false;
    }

    public static void Minimize()
    {
        if (itsInstance != null)
        {
            itsInstance.itsOpen = false;
        }
    }

    public void OnGUI()
    {
        Render();
    }

    private void Init()
    {
        if (!(itsInstance != null))
        {
            itsInstance = this;
            itsInstance.itsLogTable.Columns.Add(new KGFDataColumn("Lvl", typeof(string)));
            itsInstance.itsLogTable.Columns.Add(new KGFDataColumn("Time", typeof(string)));
            itsInstance.itsLogTable.Columns.Add(new KGFDataColumn("Category", typeof(string)));
            itsInstance.itsLogTable.Columns.Add(new KGFDataColumn("Message", typeof(string)));
            itsInstance.itsLogTable.Columns.Add(new KGFDataColumn("StackTrace", typeof(string)));
            itsInstance.itsLogTable.Columns.Add(new KGFDataColumn("Object", typeof(object)));
            itsTableControl = new KGFGUIDataTable(itsLogTable, GUILayout.ExpandHeight(expand: true));
            itsTableControl.OnClickRow += OnLogTableRowIsClicked;
            itsTableControl.PostRenderRow += PostLogTableRowHook;
            itsTableControl.PreCellContentHandler += PreCellContentHook;
            itsTableControl.SetColumnWidth(0, 40u);
            itsTableControl.SetColumnWidth(1, 90u);
            itsTableControl.SetColumnWidth(2, 150u);
            itsTableControl.SetColumnWidth(3, 0u);
            itsTableControl.SetColumnVisible(4, theValue: false);
            itsTableControl.SetColumnVisible(5, theValue: false);
            itsLogLevelFilter.Clear();
            itsLogCategoryCount.Clear();
            foreach (int value in Enum.GetValues(typeof(KGFeDebugLevel)))
            {
                itsLogLevelFilter.Add((KGFeDebugLevel)value, value: true);
                itsLogCategoryCount.Add((KGFeDebugLevel)value, 0u);
            }
            itsDataModuleGUILogger.itsLogsPerPage = (KGFeItemsPerPage)(int)Enum.Parse(typeof(KGFeItemsPerPage), PlayerPrefs.GetInt("KGF.KGFModuleDebugger.itsLogsPerPage", 25).ToString());
            LoadCategoryFilterFromPlayerPrefs();
            itsOpenWindow = new Rect(0f, 0f, Screen.width, itsCurrentHeight);
            itsMinimizedWindow = new Rect(0f, 0f, Screen.width, 100f);
            itsInstance.LoadSizeFromPlayerPrefs();
            if (itsCurrentHeight == 0f)
            {
                itsCurrentHeight = (float)Screen.height * 0.5f;
            }
        }
    }

    private void DrawMouseCursor()
    {
        if (KGFGUIUtility.GetStyleCursor() != null)
        {
            Vector3 mousePosition = Input.mousePosition;
            Rect position = new Rect(mousePosition.x, (float)Screen.height - mousePosition.y, KGFGUIUtility.GetStyleCursor().fixedWidth, KGFGUIUtility.GetStyleCursor().fixedHeight);
            GUI.Label(position, string.Empty, KGFGUIUtility.GetStyleCursor());
        }
    }

    private void SaveSizeToPlayerPrefs()
    {
        PlayerPrefs.SetFloat("KGFDebugGUI.WindowSize", itsCurrentHeight);
    }

    private void LoadSizeFromPlayerPrefs()
    {
        itsCurrentHeight = PlayerPrefs.GetFloat("KGFDebugGUI.WindowSize", 0f);
    }

    public void Log(KGFDebug.KGFDebugLog aLog)
    {
        Log(aLog.GetLevel(), aLog.GetCategory(), aLog.GetMessage(), aLog.GetStackTrace(), aLog.GetObject() as MonoBehaviour);
    }

    public void Log(KGFeDebugLevel theLevel, string theCategory, string theMessage)
    {
        Log(theLevel, theCategory, theMessage, string.Empty, null);
    }

    public void Log(KGFeDebugLevel theLevel, string theCategory, string theMessage, string theStackTrace)
    {
        Log(theLevel, theCategory, theMessage, theStackTrace, null);
    }

    public void Log(KGFeDebugLevel theLevel, string theCategory, string theMessage, string theStackTrace, MonoBehaviour theObject)
    {
        Init();
        Dictionary<KGFeDebugLevel, uint> dictionary;
        Dictionary<KGFeDebugLevel, uint> dictionary2 = dictionary = itsLogCategoryCount;
        KGFeDebugLevel key;
        KGFeDebugLevel key2 = key = theLevel;
        uint num = dictionary[key];
        dictionary2[key2] = num + 1;
        KGFDebug.KGFDebugLog kGFDebugLog = new KGFDebug.KGFDebugLog(theLevel, theCategory, theMessage, theStackTrace, theObject);
        itsLogList.Add(kGFDebugLog);
        if (!itsLogCategories.ContainsKey(theCategory))
        {
            itsLogCategories[theCategory] = new KGFDebugCategory(theCategory);
            itsLogCategories[theCategory].itsSelectedState = true;
        }
        itsLogCategories[theCategory].IncreaseCount();
        if (FilterDebugLog(kGFDebugLog))
        {
            KGFDataRow kGFDataRow = itsLogTable.NewRow();
            kGFDataRow[0].Value = kGFDebugLog.GetLevel().ToString();
            kGFDataRow[1].Value = kGFDebugLog.GetLogTime().ToString("HH:mm:ss.fff");
            kGFDataRow[2].Value = kGFDebugLog.GetCategory();
            kGFDataRow[3].Value = kGFDebugLog.GetMessage();
            kGFDataRow[4].Value = kGFDebugLog.GetStackTrace();
            kGFDataRow[5].Value = kGFDebugLog.GetObject();
            itsLogTable.Rows.Add(kGFDataRow);
        }
        if (kGFDebugLog.GetLevel() >= itsDataModuleGUILogger.itsMinimumExpandLogLevel)
        {
            itsDataModuleGUILogger.itsVisible = true;
            itsOpen = true;
        }
    }

    public void SetMinimumLogLevel(KGFeDebugLevel theLevel)
    {
        itsDataModuleGUILogger.itsMinimumLogLevel = theLevel;
    }

    public KGFeDebugLevel GetMinimumLogLevel()
    {
        return itsDataModuleGUILogger.itsMinimumLogLevel;
    }

    private IEnumerable<KGFDebugCategory> GetAllCategories()
    {
        return itsLogCategories.Values;
    }

    private bool FilterDebugLog(KGFDebug.KGFDebugLog theLogItem)
    {
        if (!itsLogLevelFilter[theLogItem.GetLevel()])
        {
            return false;
        }
        if (!itsSearchFilterMessage.Equals("Search") && !itsSearchFilterMessage.Equals(string.Empty) && !theLogItem.GetMessage().Trim().ToLower()
            .Contains(itsSearchFilterMessage.Trim().ToLower()))
        {
            return false;
        }
        foreach (KGFDebugCategory value in itsLogCategories.Values)
        {
            if (value.itsSelectedState && value.GetName().ToLower().Contains(theLogItem.GetCategory().ToLower()))
            {
                return true;
            }
        }
        return false;
    }

    private IEnumerable<KGFDebug.KGFDebugLog> GetFilteredLogList()
    {
        foreach (KGFDebug.KGFDebugLog aDebugLog in itsLogList)
        {
            if (FilterDebugLog(aDebugLog))
            {
                yield return aDebugLog;
            }
        }
    }

    private void UpdateLogList()
    {
        itsLogTable.Rows.Clear();
        foreach (KGFDebug.KGFDebugLog filteredLog in GetFilteredLogList())
        {
            KGFDataRow kGFDataRow = itsLogTable.NewRow();
            kGFDataRow[0].Value = filteredLog.GetLevel().ToString();
            kGFDataRow[1].Value = filteredLog.GetLogTime().ToString("HH:mm:ss.fff");
            kGFDataRow[2].Value = filteredLog.GetCategory();
            kGFDataRow[3].Value = filteredLog.GetMessage();
            kGFDataRow[4].Value = filteredLog.GetStackTrace();
            kGFDataRow[5].Value = filteredLog.GetObject();
            itsLogTable.Rows.Add(kGFDataRow);
        }
        UpdateLogListPageNumber();
    }

    private void UpdateLogListPageNumber()
    {
        if (itsLogTable.Rows.Count <= (int)itsDataModuleGUILogger.itsLogsPerPage)
        {
            itsCurrentPage = 0u;
        }
        else
        {
            itsCurrentPage = (uint)(Mathf.CeilToInt((float)itsLogTable.Rows.Count / (float)itsDataModuleGUILogger.itsLogsPerPage) - 1);
        }
    }

    private void DrawMinimizedWindow()
    {
        float height = KGFGUIUtility.GetSkinHeight() + (float)KGFGUIUtility.GetStyleButton(KGFGUIUtility.eStyleButton.eButton).margin.vertical + (float)KGFGUIUtility.GetStyleBox(KGFGUIUtility.eStyleBox.eBoxDecorated).padding.vertical;
        itsMinimizedWindow.x = 0f;
        itsMinimizedWindow.y = 0f;
        itsMinimizedWindow.width = Screen.width;
        itsMinimizedWindow.height = height;
        GUILayout.BeginArea(itsMinimizedWindow);
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        KGFGUIUtility.BeginVerticalBox(KGFGUIUtility.eStyleBox.eBoxDecorated);
        Texture2D theIcon = null;
        if (KGFDebug.GetInstance() != null)
        {
            theIcon = KGFDebug.GetInstance().GetIcon();
        }
        KGFGUIUtility.BeginWindowHeader("KGFDebugger", theIcon);
        DrawSummary();
        GUILayout.FlexibleSpace();
        if (KGFGUIUtility.Button(itsDataModuleGUILogger.itsIconHelp, KGFGUIUtility.eStyleButton.eButton))
        {
            KGFModule.OpenHelpWindow(itsInstance);
        }
        itsOpen = KGFGUIUtility.Toggle(itsOpen, string.Empty, KGFGUIUtility.eStyleToggl.eTogglSwitch, GUILayout.Width(KGFGUIUtility.GetSkinHeight()));
        KGFGUIUtility.EndWindowHeader(theCloseButton: false);
        KGFGUIUtility.EndVerticalBox();
        GUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    private void DrawSummary()
    {
        float pixels = 10f;
        GUILayout.Space(10f);
        KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxInvisible);
        KGFGUIUtility.Label(string.Empty, itsDataModuleGUILogger.itsIconDebug, KGFGUIUtility.eStyleLabel.eLabel);
        KGFGUIUtility.Label($"{itsLogCategoryCount[KGFeDebugLevel.eDebug]}", KGFGUIUtility.eStyleLabel.eLabel);
        GUILayout.Space(pixels);
        KGFGUIUtility.Label(string.Empty, itsDataModuleGUILogger.itsIconInfo, KGFGUIUtility.eStyleLabel.eLabel);
        KGFGUIUtility.Label($"{itsLogCategoryCount[KGFeDebugLevel.eInfo]}", KGFGUIUtility.eStyleLabel.eLabel);
        GUILayout.Space(pixels);
        KGFGUIUtility.Label(string.Empty, itsDataModuleGUILogger.itsIconWarning, KGFGUIUtility.eStyleLabel.eLabel);
        KGFGUIUtility.Label($"{itsLogCategoryCount[KGFeDebugLevel.eWarning]}", KGFGUIUtility.eStyleLabel.eLabel);
        GUILayout.Space(pixels);
        KGFGUIUtility.Label(string.Empty, itsDataModuleGUILogger.itsIconError, KGFGUIUtility.eStyleLabel.eLabel);
        KGFGUIUtility.Label($"{itsLogCategoryCount[KGFeDebugLevel.eError]}", KGFGUIUtility.eStyleLabel.eLabel);
        GUILayout.Space(pixels);
        KGFGUIUtility.Label(string.Empty, itsDataModuleGUILogger.itsIconFatal, KGFGUIUtility.eStyleLabel.eLabel);
        KGFGUIUtility.Label($"{itsLogCategoryCount[KGFeDebugLevel.eFatal]}", KGFGUIUtility.eStyleLabel.eLabel);
        GUILayout.Space(pixels);
        if (itsTimeLeft < 0f)
        {
            itsCurrentFPS = itsAccumulatedFrames / (float)itsFramesInInterval;
            itsTimeLeft = itsDataModuleGUILogger.itsFPSUpdateInterval;
            itsAccumulatedFrames = 0f;
            itsFramesInInterval = 0;
        }
        KGFGUIUtility.Label($"FPS: {itsCurrentFPS:F2}", KGFGUIUtility.eStyleLabel.eLabel);
        KGFGUIUtility.EndHorizontalBox();
    }

    private void DrawContentColumn()
    {
        KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxDarkTop);
        KGFGUIUtility.BeginVerticalPadding();
        DrawFilterButtons();
        KGFGUIUtility.EndVerticalPadding();
        KGFGUIUtility.EndHorizontalBox();
        KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxMiddleVertical);
        itsTableControl.SetStartRow((uint)((long)itsCurrentPage * (long)itsDataModuleGUILogger.itsLogsPerPage));
        itsTableControl.SetDisplayRowCount((uint)itsDataModuleGUILogger.itsLogsPerPage);
        KGFGUIUtility.BeginVerticalPadding();
        itsTableControl.Render();
        KGFGUIUtility.EndVerticalPadding();
        KGFGUIUtility.EndHorizontalBox();
        KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxDarkBottom);
        DrawContentFilterBar();
        KGFGUIUtility.EndHorizontalBox();
    }

    private void DrawCategoryColumn()
    {
        bool flag = false;
        KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxDarkTop);
        KGFGUIUtility.BeginVerticalPadding();
        if (KGFGUIUtility.Button("All", KGFGUIUtility.eStyleButton.eButtonLeft, GUILayout.ExpandWidth(expand: true)))
        {
            foreach (KGFDebugCategory allCategory in GetAllCategories())
            {
                allCategory.itsSelectedState = true;
                flag = true;
            }
        }
        if (KGFGUIUtility.Button("None", KGFGUIUtility.eStyleButton.eButtonRight, GUILayout.ExpandWidth(expand: true)))
        {
            foreach (KGFDebugCategory allCategory2 in GetAllCategories())
            {
                allCategory2.itsSelectedState = false;
                flag = true;
            }
        }
        KGFGUIUtility.EndVerticalPadding();
        KGFGUIUtility.EndHorizontalBox();
        KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxMiddleVertical);
        itsCategoryScrollViewPosition = KGFGUIUtility.BeginScrollView(itsCategoryScrollViewPosition, false, true, GUILayout.ExpandWidth(expand: true));
        int num = 0;
        foreach (KGFDebugCategory allCategory3 in GetAllCategories())
        {
            if (itsSearchFilterCategory.Trim().Equals("Search") || allCategory3.GetName().ToLower().Contains(itsSearchFilterCategory.ToLower()))
            {
                num++;
                bool flag2 = KGFGUIUtility.Toggle(allCategory3.itsSelectedState, $"{allCategory3.GetName()} ({allCategory3.GetCount().ToString()})", KGFGUIUtility.eStyleToggl.eTogglSuperCompact);
                if (allCategory3.itsSelectedState != flag2)
                {
                    allCategory3.itsSelectedState = flag2;
                    flag = true;
                }
            }
        }
        if (num == 0)
        {
            KGFGUIUtility.Label("no items found");
        }
        GUILayout.EndScrollView();
        KGFGUIUtility.EndHorizontalBox();
        string text = itsSearchFilterCategory;
        KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxDarkBottom);
        GUI.SetNextControlName("categorySearch");
        itsSearchFilterCategory = KGFGUIUtility.TextField(itsSearchFilterCategory, KGFGUIUtility.eStyleTextField.eTextField, GUILayout.ExpandWidth(expand: true));
        KGFGUIUtility.EndHorizontalBox();
        if (GUI.GetNameOfFocusedControl().Equals("categorySearch"))
        {
            itsFocus = true;
            if (itsSearchFilterCategory.Equals("Search"))
            {
                itsSearchFilterCategory = string.Empty;
            }
        }
        if (!GUI.GetNameOfFocusedControl().Equals("categorySearch"))
        {
            if (!GUI.GetNameOfFocusedControl().Equals("messageSearch"))
            {
                itsFocus = false;
            }
            if (itsSearchFilterCategory.Equals(string.Empty))
            {
                itsSearchFilterCategory = "Search";
            }
        }
        if (!text.Equals(itsSearchFilterCategory))
        {
            flag = true;
        }
        if (flag)
        {
            UpdateLogList();
        }
    }

    private void DrawFilterButtons()
    {
        if (KGFGUIUtility.Button("All", KGFGUIUtility.eStyleButton.eButtonLeft, GUILayout.Width(70f)))
        {
            foreach (int value in Enum.GetValues(typeof(KGFeDebugLevel)))
            {
                itsLogLevelFilter[(KGFeDebugLevel)value] = true;
                UpdateLogList();
            }
            SaveCategoryFilterToPlayerPrefs();
        }
        if (KGFGUIUtility.Button("None", KGFGUIUtility.eStyleButton.eButtonRight, GUILayout.Width(70f)))
        {
            foreach (int value2 in Enum.GetValues(typeof(KGFeDebugLevel)))
            {
                itsLogLevelFilter[(KGFeDebugLevel)value2] = false;
                UpdateLogList();
            }
            SaveCategoryFilterToPlayerPrefs();
        }
        foreach (int value3 in Enum.GetValues(typeof(KGFeDebugLevel)))
        {
            if (value3 != 6 && value3 != 0)
            {
                bool flag = KGFGUIUtility.Toggle(itsLogLevelFilter[(KGFeDebugLevel)value3], ((KGFeDebugLevel)value3).ToString().Substring(1, ((KGFeDebugLevel)value3).ToString().Length - 1), KGFGUIUtility.eStyleToggl.eTogglSuperCompact);
                GUILayout.Space(10f);
                if (flag != itsLogLevelFilter[(KGFeDebugLevel)value3])
                {
                    itsLogLevelFilter[(KGFeDebugLevel)value3] = flag;
                    UpdateLogList();
                    SaveCategoryFilterToPlayerPrefs();
                }
            }
        }
        GUILayout.FlexibleSpace();
    }

    private void DrawContentFilterBar()
    {
        string a = itsSearchFilterMessage;
        bool flag = false;
        GUI.SetNextControlName("messageSearch");
        itsSearchFilterMessage = KGFGUIUtility.TextField(itsSearchFilterMessage, KGFGUIUtility.eStyleTextField.eTextField, GUILayout.Width((float)Screen.width * 0.2f));
        GUILayout.FlexibleSpace();
        if (GUI.GetNameOfFocusedControl().Equals("messageSearch"))
        {
            itsFocus = true;
            if (itsSearchFilterMessage.Equals("Search"))
            {
                itsSearchFilterMessage = string.Empty;
                flag = true;
            }
        }
        if (!GUI.GetNameOfFocusedControl().Equals("messageSearch"))
        {
            if (!GUI.GetNameOfFocusedControl().Equals("categorySearch"))
            {
                itsFocus = false;
            }
            if (itsSearchFilterMessage.Equals(string.Empty))
            {
                itsSearchFilterMessage = "Search";
                flag = true;
            }
        }
        if (a != itsSearchFilterMessage && !flag)
        {
            itsLiveSearchChanged = true;
            itsLastChangeTime = Time.time;
        }
        if (itsLiveSearchChanged && Time.time - itsLastChangeTime > 1f && Event.current.type != EventType.Layout)
        {
            itsLiveSearchChanged = false;
            UpdateLogList();
        }
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical();
        KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxInvisible);
        if (KGFGUIUtility.Button(itsDataModuleGUILogger.itsIconLeft, KGFGUIUtility.eStyleButton.eButtonLeft))
        {
            switch (itsDataModuleGUILogger.itsLogsPerPage)
            {
                case KGFeItemsPerPage.e25:
                    itsDataModuleGUILogger.itsLogsPerPage = KGFeItemsPerPage.e10;
                    break;
                case KGFeItemsPerPage.e50:
                    itsDataModuleGUILogger.itsLogsPerPage = KGFeItemsPerPage.e25;
                    break;
                case KGFeItemsPerPage.e100:
                    itsDataModuleGUILogger.itsLogsPerPage = KGFeItemsPerPage.e50;
                    break;
                case KGFeItemsPerPage.e250:
                    itsDataModuleGUILogger.itsLogsPerPage = KGFeItemsPerPage.e100;
                    break;
                case KGFeItemsPerPage.e500:
                    itsDataModuleGUILogger.itsLogsPerPage = KGFeItemsPerPage.e250;
                    break;
            }
            PlayerPrefs.SetInt("KGF.KGFModuleDebugger.itsLogsPerPage", (int)itsDataModuleGUILogger.itsLogsPerPage);
            UpdateLogListPageNumber();
        }
        KGFGUIUtility.BeginVerticalBox(KGFGUIUtility.eStyleBox.eBoxMiddleHorizontal);
        string theText = itsDataModuleGUILogger.itsLogsPerPage.ToString().Substring(1) + " entries per page";
        KGFGUIUtility.Label(theText, KGFGUIUtility.eStyleLabel.eLabelFitIntoBox);
        KGFGUIUtility.EndVerticalBox();
        if (KGFGUIUtility.Button(itsDataModuleGUILogger.itsIconRight, KGFGUIUtility.eStyleButton.eButtonRight))
        {
            switch (itsDataModuleGUILogger.itsLogsPerPage)
            {
                case KGFeItemsPerPage.e10:
                    itsDataModuleGUILogger.itsLogsPerPage = KGFeItemsPerPage.e25;
                    break;
                case KGFeItemsPerPage.e25:
                    itsDataModuleGUILogger.itsLogsPerPage = KGFeItemsPerPage.e50;
                    break;
                case KGFeItemsPerPage.e50:
                    itsDataModuleGUILogger.itsLogsPerPage = KGFeItemsPerPage.e100;
                    break;
                case KGFeItemsPerPage.e100:
                    itsDataModuleGUILogger.itsLogsPerPage = KGFeItemsPerPage.e250;
                    break;
                case KGFeItemsPerPage.e250:
                    itsDataModuleGUILogger.itsLogsPerPage = KGFeItemsPerPage.e500;
                    break;
            }
            PlayerPrefs.SetInt("KGF.KGFModuleDebugger.itsLogsPerPage", (int)itsDataModuleGUILogger.itsLogsPerPage);
            UpdateLogListPageNumber();
        }
        GUILayout.Space(10f);
        if (KGFGUIUtility.Button(itsDataModuleGUILogger.itsIconLeft, KGFGUIUtility.eStyleButton.eButtonLeft) && itsCurrentPage != 0)
        {
            itsCurrentPage--;
        }
        KGFGUIUtility.BeginVerticalBox(KGFGUIUtility.eStyleBox.eBoxMiddleHorizontal);
        int val = (int)Math.Ceiling((float)itsLogTable.Rows.Count / (float)itsDataModuleGUILogger.itsLogsPerPage);
        string theText2 = $"page {itsCurrentPage + 1}/{Math.Max(val, 1)}";
        KGFGUIUtility.Label(theText2, KGFGUIUtility.eStyleLabel.eLabelFitIntoBox);
        KGFGUIUtility.EndVerticalBox();
        if (KGFGUIUtility.Button(itsDataModuleGUILogger.itsIconRight, KGFGUIUtility.eStyleButton.eButtonRight) && itsLogTable.Rows.Count > (long)(itsCurrentPage + 1) * (long)itsDataModuleGUILogger.itsLogsPerPage)
        {
            itsCurrentPage++;
        }
        if (KGFGUIUtility.Button("clear", KGFGUIUtility.eStyleButton.eButton))
        {
            ClearCurrentLogs();
        }
        KGFGUIUtility.EndHorizontalBox();
        GUILayout.EndVertical();
    }

    private void ClearCurrentLogs()
    {
        List<KGFDebug.KGFDebugLog> list = new List<KGFDebug.KGFDebugLog>();
        foreach (KGFDebug.KGFDebugLog filteredLog in GetFilteredLogList())
        {
            list.Add(filteredLog);
            if (itsLogCategories.ContainsKey(filteredLog.GetCategory()))
            {
                itsLogCategories[filteredLog.GetCategory()].DecreaseCount();
            }
            if (itsLogCategoryCount.ContainsKey(filteredLog.GetLevel()))
            {
                Dictionary<KGFeDebugLevel, uint> dictionary;
                Dictionary<KGFeDebugLevel, uint> dictionary2 = dictionary = itsLogCategoryCount;
                KGFeDebugLevel level;
                KGFeDebugLevel key = level = filteredLog.GetLevel();
                uint num = dictionary[level];
                dictionary2[key] = num - 1;
            }
        }
        foreach (KGFDebug.KGFDebugLog item in list)
        {
            itsLogList.Remove(item);
        }
        UpdateLogList();
    }

    private void PostLogTableRowHook(object theSender, EventArgs theArguments)
    {
        KGFDataRow kGFDataRow = theSender as KGFDataRow;
        Color backgroundColor = GUI.backgroundColor;
        if (kGFDataRow != null && !kGFDataRow.IsNull("Lvl"))
        {
            if (Enum.IsDefined(typeof(KGFeDebugLevel), kGFDataRow["Lvl"].ToString()))
            {
                GUI.backgroundColor = GetColorForLevel((KGFeDebugLevel)(int)Enum.Parse(typeof(KGFeDebugLevel), kGFDataRow["Lvl"].ToString()));
            }
            else
            {
                UnityEngine.Debug.Log("the color is not defined");
            }
        }
        if (kGFDataRow != null)
        {
            GUI.contentColor = Color.white;
            if (kGFDataRow == itsCurrentSelectedRow)
            {
                KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxDarkBottom, GUILayout.ExpandWidth(expand: true));
                GUILayout.BeginVertical();
                GUILayout.TextArea(string.Format("Object Path: {1}{0}{0}Time: {2}{0}{0}Category: {3}{0}{0}Message: {4}{0}{0}Stack Trace: {5}", Environment.NewLine, GetObjectPath(kGFDataRow[5].Value as MonoBehaviour), kGFDataRow[1].Value, kGFDataRow[2].Value, kGFDataRow[3].Value, kGFDataRow[4].Value), GUILayout.ExpandWidth(expand: true), GUILayout.ExpandHeight(expand: false));
                if (KGFGUIUtility.Button("copy to file", KGFGUIUtility.eStyleButton.eButton, GUILayout.ExpandWidth(expand: true)))
                {
                    string theFilePath = CreateTempFile(new Dictionary<string, string>
                    {
                        {
                            "Message",
                            kGFDataRow[3].Value.ToString()
                        },
                        {
                            "Time",
                            kGFDataRow[1].Value.ToString()
                        },
                        {
                            "StackTrace",
                            kGFDataRow[4].Value.ToString()
                        }
                    });
                    OpenFile(theFilePath);
                }
                GUILayout.EndVertical();
                KGFGUIUtility.EndHorizontalBox();
                GUILayout.Space(5f);
            }
        }
        GUI.backgroundColor = backgroundColor;
    }

    private bool PreCellContentHook(KGFDataRow theRow, KGFDataColumn theColumn, uint theWidth)
    {
        if (theColumn.ColumnName.Equals("Lvl"))
        {
            switch (theRow[theColumn.ColumnName].ToString())
            {
                case "eDebug":
                    KGFGUIUtility.Label(string.Empty, itsDataModuleGUILogger.itsIconDebug, KGFGUIUtility.eStyleLabel.eLabelFitIntoBox, GUILayout.Width((float)(double)theWidth));
                    return true;
                case "eInfo":
                    KGFGUIUtility.Label(string.Empty, itsDataModuleGUILogger.itsIconInfo, KGFGUIUtility.eStyleLabel.eLabelFitIntoBox, GUILayout.Width((float)(double)theWidth));
                    return true;
                case "eWarning":
                    KGFGUIUtility.Label(string.Empty, itsDataModuleGUILogger.itsIconWarning, KGFGUIUtility.eStyleLabel.eLabelFitIntoBox, GUILayout.Width((float)(double)theWidth));
                    return true;
                case "eError":
                    KGFGUIUtility.Label(string.Empty, itsDataModuleGUILogger.itsIconError, KGFGUIUtility.eStyleLabel.eLabelFitIntoBox, GUILayout.Width((float)(double)theWidth));
                    return true;
                case "eFatal":
                    KGFGUIUtility.Label(string.Empty, itsDataModuleGUILogger.itsIconFatal, KGFGUIUtility.eStyleLabel.eLabelFitIntoBox, GUILayout.Width((float)(double)theWidth));
                    return true;
                default:
                    return false;
            }
        }
        return false;
    }

    private void OnDebugGuiAdd(object theSender)
    {
        KGFICustomGUI kGFICustomGUI = (KGFICustomGUI)theSender;
        if (!itsLogCategories.ContainsKey(kGFICustomGUI.GetName()))
        {
            itsLogCategories[kGFICustomGUI.GetName()] = new KGFDebugCategory(kGFICustomGUI.GetName());
        }
    }

    private void OnLogTableRowIsClicked(object theSender, EventArgs theArguments)
    {
        KGFDataRow kGFDataRow = theSender as KGFDataRow;
        if (kGFDataRow != null)
        {
            if (kGFDataRow != itsCurrentSelectedRow)
            {
                itsCurrentSelectedRow = kGFDataRow;
            }
            else
            {
                itsCurrentSelectedRow = null;
            }
        }
    }

    private void SaveCategoryFilterToPlayerPrefs()
    {
        StringBuilder stringBuilder = new StringBuilder();
        foreach (int value in Enum.GetValues(typeof(KGFeDebugLevel)))
        {
            stringBuilder.Append($"{((KGFeDebugLevel)value).ToString()}.{itsLogLevelFilter[(KGFeDebugLevel)value].ToString()}:");
        }
        stringBuilder.Remove(stringBuilder.Length - 1, 1);
        PlayerPrefs.SetString("KGF.KGFModuleDebug", stringBuilder.ToString());
    }

    private void LoadCategoryFilterFromPlayerPrefs()
    {
        string @string = PlayerPrefs.GetString("KGF.KGFModuleDebug");
        string[] array = @string.Split(new char[1]
        {
            ':'
        });
        string[] array2 = array;
        foreach (string text in array2)
        {
            string[] array3 = text.Split(new char[1]
            {
                '.'
            });
            if (array3.Length == 2 && Enum.IsDefined(typeof(KGFeDebugLevel), array3[0]) && bool.TryParse(array3[1], out bool result))
            {
                itsLogLevelFilter[(KGFeDebugLevel)(int)Enum.Parse(typeof(KGFeDebugLevel), array3[0])] = result;
            }
        }
    }

    public Color GetColorForLevel(KGFeDebugLevel theLevel)
    {
        switch (theLevel)
        {
            case KGFeDebugLevel.eDebug:
                return itsDataModuleGUILogger.itsColorDebug;
            case KGFeDebugLevel.eInfo:
                return itsDataModuleGUILogger.itsColorInfo;
            case KGFeDebugLevel.eWarning:
                return itsDataModuleGUILogger.itsColorWarning;
            case KGFeDebugLevel.eError:
                return itsDataModuleGUILogger.itsColorError;
            case KGFeDebugLevel.eFatal:
                return itsDataModuleGUILogger.itsColorFatal;
            default:
                return Color.white;
        }
    }

    private string CreateTempFile(Dictionary<string, string> theContent)
    {
        string tempFileName = Path.GetTempFileName();
        UnityEngine.Debug.Log("temp file path: " + tempFileName);
        string text = Path.ChangeExtension(tempFileName, "txt");
        File.Move(tempFileName, text);
        using (StreamWriter streamWriter = new StreamWriter(text, append: true, Encoding.ASCII))
        {
            foreach (string key in theContent.Keys)
            {
                if (theContent[key] != null)
                {
                    streamWriter.WriteLine(key);
                    streamWriter.WriteLine(string.Empty.PadLeft(key.Length, '='));
                    string[] array = theContent[key].Split(Environment.NewLine.ToCharArray());
                    foreach (string value in array)
                    {
                        streamWriter.WriteLine(value);
                    }
                    streamWriter.WriteLine();
                }
            }
            return text;
        }
    }

    private void OpenFile(string theFilePath)
    {
        if (File.Exists(theFilePath))
        {
            Process process = new Process();
            process.StartInfo.FileName = theFilePath;
            process.Start();
        }
        else
        {
            UnityEngine.Debug.LogWarning("the file path was not found: " + theFilePath);
        }
    }

    private static string GetObjectPath(MonoBehaviour theObject)
    {
        if (theObject != null)
        {
            List<string> list = new List<string>();
            Transform transform = theObject.transform;
            do
            {
                list.Add(transform.name);
                transform = transform.parent;
            }
            while (transform != null);
            list.Reverse();
            return string.Join("/", list.ToArray());
        }
        return string.Empty;
    }

    public override string GetName()
    {
        return "KGFDebugGUI";
    }

    public override string GetDocumentationPath()
    {
        return "KGFDebugGUI_Manual.html";
    }

    public override string GetForumPath()
    {
        return "index.php?qa=kgfdebug";
    }

    public override Texture2D GetIcon()
    {
        return null;
    }

    public override KGFMessageList Validate()
    {
        KGFMessageList kGFMessageList = new KGFMessageList();
        if (itsDataModuleGUILogger.itsIconDebug == null)
        {
            kGFMessageList.AddWarning("the debug icon is missing");
        }
        if (itsDataModuleGUILogger.itsIconInfo == null)
        {
            kGFMessageList.AddWarning("the info icon is missing");
        }
        if (itsDataModuleGUILogger.itsIconWarning == null)
        {
            kGFMessageList.AddWarning("the warning icon is missing");
        }
        if (itsDataModuleGUILogger.itsIconError == null)
        {
            kGFMessageList.AddWarning("the error icon is missing");
        }
        if (itsDataModuleGUILogger.itsIconFatal == null)
        {
            kGFMessageList.AddWarning("the fatal error icon is missing");
        }
        if (itsDataModuleGUILogger.itsIconHelp == null)
        {
            kGFMessageList.AddWarning("the help icon is missing");
        }
        if (itsDataModuleGUILogger.itsIconLeft == null)
        {
            kGFMessageList.AddWarning("the left arrow icon is missing");
        }
        if (itsDataModuleGUILogger.itsIconRight == null)
        {
            kGFMessageList.AddWarning("the right arrow icon is missing");
        }
        if (itsDataModuleGUILogger.itsFPSUpdateInterval < 0f)
        {
            kGFMessageList.AddError("the FPS update interval\ufffdl must be greater than zero");
        }
        return kGFMessageList;
    }
}
