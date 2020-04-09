using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

public class KGFConsole : KGFModule
{
	[Serializable]
	public class KGFDataConsole
	{
		public Texture2D itsIconModule;

		public Texture2D itsIconExecute;

		public Texture2D itsIconError;

		public Texture2D itsIconFatal;

		public Texture2D itsIconUnknown;

		public Texture2D itsIconSuccessful;

		public Texture2D itsIconHelp;

		public Texture2D itsIconDropDown;

		public KeyCode itsModifierKeyFocus = KeyCode.LeftControl;

		public KeyCode itsSchortcutKeyFocus = KeyCode.LeftShift;

		public KeyCode itsHideKeyModifier;

		public KeyCode itsHideKey = KeyCode.F2;

		public KeyCode itsExpandKeyModifier = KeyCode.LeftAlt;

		public KeyCode itsExpandKey = KeyCode.F2;

		public bool itsVisible = true;

		public uint itsVisibleSuggestions = 5u;
	}

	private class KGFMethodInfo
	{
		private MethodInfo itsMethodInfo;

		private List<Type> itsTypes = new List<Type>();

		public KGFMethodInfo(MethodInfo theMethodInfo, params Type[] theParameterTypes)
		{
			itsMethodInfo = theMethodInfo;
			foreach (Type item in theParameterTypes)
			{
				itsTypes.Add(item);
			}
		}

		public string GetName()
		{
			return itsMethodInfo.Name;
		}

		public uint GetParameterCount()
		{
			return (uint)itsTypes.Count;
		}

		public IEnumerable<Type> GetParameterTypes()
		{
			return itsTypes;
		}

		public Type GetParameterTypeAt(int thePosition)
		{
			if (thePosition >= 0 && thePosition < itsTypes.Count)
			{
				return itsTypes[thePosition];
			}
			Debug.LogError("the index of the parameter was out of range");
			return null;
		}

		public MethodInfo GetMethodInfo()
		{
			return itsMethodInfo;
		}
	}

	private class KGFCommandCode : IComparable<KGFCommandCode>
	{
		public string itsCommandCode;

		public string itsDescription;

		public string itsCategory;

		private uint itsExecutionCount;

		private object itsObject;

		private string itsNameOfMethod;

		private List<KGFMethodInfo> itsMethodList;

		public KGFCommandCode(string theCommandCode, string theDescription, string theCategory, object theObject, string theNameOfMethod)
		{
			itsCommandCode = theCommandCode;
			itsDescription = theDescription;
			itsCategory = theCategory;
			itsExecutionCount = 0u;
			itsObject = theObject;
			itsNameOfMethod = theNameOfMethod;
			GetMehods();
			if (itsMethodList.Count == 0)
			{
				Debug.LogError("no method with the name " + theNameOfMethod + " was found for command " + theCommandCode);
			}
		}

		public KGFCommandCode(string theCommandCode, string theDescription, string theCategory, object theObject, string theNameOfMethod, uint theExecutionCount)
		{
			itsCommandCode = theCommandCode;
			itsDescription = theDescription;
			itsCategory = theCategory;
			itsExecutionCount = theExecutionCount;
			itsObject = theObject;
			itsNameOfMethod = theNameOfMethod;
			GetMehods();
			if (itsMethodList.Count == 0)
			{
				Debug.LogError("no method with the name " + theNameOfMethod + " was found for command " + theCommandCode);
			}
		}

		public int CompareTo(KGFCommandCode other)
		{
			return itsExecutionCount.CompareTo(other.itsExecutionCount);
		}

		public uint GetExecutionCount()
		{
			return itsExecutionCount;
		}

		public bool Execute(string theParameters)
		{
			string[] array = (theParameters == null) ? new string[0] : theParameters.Split(new char[1]
			{
				' '
			});
			object[] theParametersObject;
			MethodInfo methodInfo = FindMatchingMethod(array, out theParametersObject);
			if ((object)methodInfo != null)
			{
				if (array.Length == 0)
				{
					methodInfo.Invoke(itsObject, null);
				}
				else
				{
					methodInfo.Invoke(itsObject, theParametersObject);
				}
				itsExecutionCount++;
				return true;
			}
			return false;
		}

		public IEnumerable<KGFMethodInfo> GetMehods()
		{
			if (itsMethodList == null)
			{
				itsMethodList = new List<KGFMethodInfo>();
				MethodInfo[] methods = itsObject.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (MethodInfo methodInfo in methods)
				{
					if (!methodInfo.Name.Trim().Equals(itsNameOfMethod.Trim()))
					{
						continue;
					}
					ParameterInfo[] parameters = methodInfo.GetParameters();
					if (methodInfo.GetParameters().Length > 0)
					{
						Type[] array = new Type[methodInfo.GetParameters().Length];
						ParameterInfo[] array2 = parameters;
						foreach (ParameterInfo parameterInfo in array2)
						{
							array[parameterInfo.Position] = parameterInfo.ParameterType;
						}
						itsMethodList.Add(new KGFMethodInfo(methodInfo, array));
					}
					else
					{
						itsMethodList.Add(new KGFMethodInfo(methodInfo));
					}
				}
			}
			return itsMethodList;
		}

		private MethodInfo FindMatchingMethod(string[] theParameters, out object[] theParametersObject)
		{
			foreach (KGFMethodInfo itsMethod in itsMethodList)
			{
				if (itsMethod.GetParameterCount() == (uint)theParameters.Length)
				{
					if (theParameters.Length == 0)
					{
						theParametersObject = new object[0];
						return itsMethod.GetMethodInfo();
					}
					bool flag = true;
					theParametersObject = new object[itsMethod.GetParameterCount()];
					for (int i = 0; i < itsMethod.GetParameterCount(); i++)
					{
						Type parameterTypeAt = itsMethod.GetParameterTypeAt(i);
						if (parameterTypeAt.IsValueType)
						{
							if ((object)parameterTypeAt == typeof(short))
							{
								if (!short.TryParse(theParameters[i], out short result))
								{
									flag = false;
									break;
								}
								theParametersObject[i] = result;
							}
							else if ((object)parameterTypeAt == typeof(int))
							{
								if (!int.TryParse(theParameters[i], out int result2))
								{
									flag = false;
									break;
								}
								theParametersObject[i] = result2;
							}
							else if ((object)parameterTypeAt == typeof(long))
							{
								if (!long.TryParse(theParameters[i], out long result3))
								{
									flag = false;
									break;
								}
								theParametersObject[i] = result3;
							}
							else if ((object)parameterTypeAt == typeof(float))
							{
								if (!float.TryParse(theParameters[i], out float result4))
								{
									flag = false;
									break;
								}
								theParametersObject[i] = result4;
							}
							else if ((object)parameterTypeAt == typeof(double))
							{
								if (!double.TryParse(theParameters[i], out double result5))
								{
									flag = false;
									break;
								}
								theParametersObject[i] = result5;
							}
							else if ((object)parameterTypeAt == typeof(decimal))
							{
								if (!decimal.TryParse(theParameters[i], out decimal result6))
								{
									flag = false;
									break;
								}
								theParametersObject[i] = result6;
							}
							else if ((object)parameterTypeAt == typeof(char))
							{
								if (!char.TryParse(theParameters[i], out char result7))
								{
									flag = false;
									break;
								}
								theParametersObject[i] = result7;
							}
							else
							{
								if ((object)parameterTypeAt != typeof(bool))
								{
									flag = false;
									break;
								}
								if (!bool.TryParse(theParameters[i], out bool result8))
								{
									flag = false;
									break;
								}
								theParametersObject[i] = result8;
							}
						}
						else if ((object)parameterTypeAt == typeof(string))
						{
							theParametersObject[i] = theParameters[i];
						}
					}
					if (flag)
					{
						return itsMethod.GetMethodInfo();
					}
				}
			}
			theParametersObject = new object[0];
			return null;
		}
	}

	private class KGFCcommandCategory
	{
		private int itsCount;

		private string itsName;

		public bool itsSelectedState;

		public KGFCcommandCategory(string theName)
		{
			itsName = theName.Trim();
			itsCount = 0;
			itsSelectedState = true;
		}

		public string GetName()
		{
			return itsName;
		}

		public void IncreaseCount()
		{
			itsCount++;
		}

		public int GetCount()
		{
			return itsCount;
		}
	}

	private class KGFFavouriteCommand : IComparable<KGFFavouriteCommand>
	{
		private KGFCommandCode itsCommandCode;

		private string itsParameters;

		private uint itsExecutionCount;

		public KGFFavouriteCommand(KGFCommandCode theCommandCode, string theParameter)
		{
			itsCommandCode = theCommandCode;
			itsParameters = theParameter;
			itsExecutionCount = 0u;
		}

		public KGFFavouriteCommand(KGFCommandCode theCommandCode, string theParameter, uint theCount)
		{
			itsCommandCode = theCommandCode;
			itsParameters = theParameter.Trim();
			itsExecutionCount = theCount;
		}

		public uint GetExecutionCount()
		{
			return itsExecutionCount;
		}

		public string GetCommandCode()
		{
			return itsCommandCode.itsCommandCode;
		}

		public string GetParameters()
		{
			return itsParameters;
		}

		public int CompareTo(KGFFavouriteCommand other)
		{
			return itsExecutionCount.CompareTo(other.itsExecutionCount);
		}

		public static KGFFavouriteCommand operator ++(KGFFavouriteCommand theItem)
		{
			theItem.itsExecutionCount++;
			return theItem;
		}
	}

	private class KGFSuggestionsControl
	{
		private Rect itsRect;

		private uint itsMaxItems;

		private int itsCurrentSelected;

		private List<string> itsItems = new List<string>();

		public KGFSuggestionsControl(uint theMaxItems)
		{
			itsMaxItems = theMaxItems;
		}

		public void SetWidth(float theWidth)
		{
			itsRect.width = theWidth;
		}

		public void SetPosition(float theXValue, float theYValue)
		{
			itsRect.x = theXValue;
			itsRect.y = theYValue - (float)(double)(GetNumberOfEntriesToDisplay() + 1) * KGFGUIUtility.GetSkinHeight();
		}

		private uint GetNumberOfEntriesToDisplay()
		{
			uint count = (uint)itsItems.Count;
			if (count > itsMaxItems)
			{
				count = itsMaxItems;
			}
			return count;
		}

		public void Render()
		{
			if (itsItems.Count <= 0)
			{
				return;
			}
			itsRect.height = (float)(double)GetNumberOfEntriesToDisplay() * KGFGUIUtility.GetSkinHeight();
			GUILayout.BeginArea(itsRect);
			KGFGUIUtility.BeginVerticalBox(KGFGUIUtility.eStyleBox.eBox, GUILayout.ExpandWidth(expand: true), GUILayout.ExpandHeight(expand: true));
			GUILayout.FlexibleSpace();
			Color color = GUI.color;
			for (int i = 0; i < itsItems.Count; i++)
			{
				if (itsCurrentSelected == i)
				{
					GUI.color = color;
				}
				KGFGUIUtility.Label(itsItems[i], KGFGUIUtility.eStyleLabel.eLabelFitIntoBox);
				GUI.color = Color.gray;
			}
			GUI.color = color;
			KGFGUIUtility.EndVerticalBox();
			GUILayout.EndArea();
		}

		public void SelectionUp()
		{
			if (itsItems.Count > 0)
			{
				itsCurrentSelected--;
				if (itsCurrentSelected < 0)
				{
					itsCurrentSelected += itsItems.Count;
				}
			}
		}

		public void SelectionDown()
		{
			itsCurrentSelected++;
			itsCurrentSelected %= itsItems.Count;
		}

		public int GetCount()
		{
			return itsItems.Count;
		}

		public uint GetMaxCount()
		{
			return itsMaxItems;
		}

		public string GetSelected()
		{
			if (itsCurrentSelected >= itsItems.Count)
			{
				return string.Empty;
			}
			return itsItems[itsCurrentSelected];
		}

		public void SetItems(IEnumerable<string> theItems)
		{
			itsItems.Clear();
			itsCurrentSelected = 0;
			itsItems.AddRange(theItems);
		}

		public void ClearItems()
		{
			itsItems.Clear();
			itsCurrentSelected = 0;
		}
	}

	private enum eExecutionState
	{
		eNone,
		eSuccessful,
		eNotFound,
		eError
	}

	private static KGFConsole itsInstance;

	private bool itsIsMobile;

	private bool itsOpen;

	private float itsCurrentHeight = Screen.height;

	private Vector2 itsCategoryScrollViewPosition = Vector2.zero;

	private string itsSearchFilterMessage = "Search";

	private string itsSearchFilterCategory = "Search";

	private bool itsLiveSearchChanged;

	private float itsLastChangeTime;

	private KGFDataRow itsCurrentSelectedRow;

	private Dictionary<string, KGFCcommandCategory> itsCommandCategories = new Dictionary<string, KGFCcommandCategory>();

	private Dictionary<string, KGFCommandCode> itsCommandDictionary = new Dictionary<string, KGFCommandCode>();

	private List<string> itsLastFiveCommandCodes = new List<string>();

	private Dictionary<string, KGFFavouriteCommand> itsFavouriteCommandCodes = new Dictionary<string, KGFFavouriteCommand>();

	private List<KGFFavouriteCommand> itsFavourite = new List<KGFFavouriteCommand>();

	private Dictionary<Type, string> itsTypeMapping;

	private KGFDataTable itsCommandTable;

	private string itsCommand = string.Empty;

	private eExecutionState itsCommandState;

	private KGFGUIDataTable itsTableControl;

	private KGFGUIDropDown itsDropDownLastFive;

	private KGFGUIDropDown itsDropDownFavouriteFive;

	private bool itsHover;

	private bool itsFocus;

	private bool itsUnfocus;

	private float itsLastExecution;

	private static bool itsAlreadyChecked;

	public KGFDataConsole itsDataModuleConsole = new KGFDataConsole();

	private Rect itsMinimizedWindow = new Rect(0f, Screen.height - 100, Screen.width, 100f);

	private Rect itsOpenWindow;

	private KGFSuggestionsControl itsSuggestions = new KGFSuggestionsControl(5u);

	private bool itsModifierKeyFocusPressed;

	private bool itsShortcutKeyFocusPressed;

	public KGFConsole()
		: base(new Version(1, 2, 0, 0), new Version(1, 2, 0, 0))
	{
	}

	protected override void KGFAwake()
	{
		if (itsInstance == null)
		{
			itsInstance = this;
			itsInstance.Init();
		}
		else if (itsInstance != this)
		{
			Debug.Log("there is more than one KFGDebug instance in this scene. please ensure there is always exactly one instance in this scene");
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	protected void OnGUI()
	{
		Render();
	}

	public static void Render()
	{
		KGFGUIUtility.SetSkinIndex(0);
		if (itsInstance != null)
		{
			if (!itsInstance.itsDataModuleConsole.itsVisible)
			{
				return;
			}
			if (itsInstance.itsUnfocus)
			{
				GUI.FocusControl(string.Empty);
				itsInstance.itsUnfocus = false;
			}
			KGFModule.RenderHelpWindow();
			if (itsInstance.itsOpen)
			{
				itsInstance.itsOpenWindow.x = 0f;
				itsInstance.itsOpenWindow.y = (float)Screen.height - itsInstance.itsCurrentHeight;
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
				KGFGUIUtility.BeginVerticalBox(KGFGUIUtility.eStyleBox.eBoxDecorated, GUILayout.ExpandWidth(expand: true));
				GUILayout.BeginHorizontal();
				KGFGUIUtility.BeginVerticalBox(KGFGUIUtility.eStyleBox.eBoxLeft, GUILayout.Width((float)Screen.width * 0.2f));
				KGFGUIUtility.BeginHorizontalPadding();
				itsInstance.DrawCategoryColumn();
				KGFGUIUtility.EndHorizontalPadding();
				KGFGUIUtility.EndVerticalBox();
				KGFGUIUtility.BeginVerticalBox(KGFGUIUtility.eStyleBox.eBoxRight);
				KGFGUIUtility.BeginHorizontalPadding();
				KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxDarkTop, GUILayout.ExpandWidth(expand: true));
				KGFGUIUtility.Label("click on entry to see details");
				KGFGUIUtility.EndHorizontalBox();
				KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxMiddleVertical, GUILayout.ExpandWidth(expand: true));
				KGFGUIUtility.BeginVerticalPadding();
				itsInstance.itsTableControl.Render();
				KGFGUIUtility.EndVerticalPadding();
				KGFGUIUtility.EndHorizontalBox();
				KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxDarkBottom, GUILayout.ExpandWidth(expand: true));
				itsInstance.DrawFilterSearchBar();
				KGFGUIUtility.EndHorizontalBox();
				KGFGUIUtility.EndHorizontalPadding();
				KGFGUIUtility.EndVerticalBox();
				GUILayout.EndHorizontal();
				GUILayout.Space(10f);
				KGFGUIUtility.BeginWindowHeader("KGFConsole", itsInstance.GetIcon());
				itsInstance.DrawFooterControls();
				GUILayout.FlexibleSpace();
				float num = itsInstance.itsCurrentHeight;
				if (itsInstance.itsIsMobile)
				{
					itsInstance.itsCurrentHeight = (float)Screen.height * 0.9f;
				}
				else
				{
					itsInstance.itsCurrentHeight = KGFGUIUtility.HorizontalSlider(itsInstance.itsCurrentHeight, (float)Screen.height / 3f * 2f, KGFGUIUtility.GetSkinHeight() * 11f, GUILayout.Width(75f));
					if (num != itsInstance.itsCurrentHeight)
					{
						itsInstance.SaveSizeToPlayerPrefs();
					}
				}
				if (!itsInstance.itsIsMobile && KGFGUIUtility.Button(itsInstance.itsDataModuleConsole.itsIconHelp, KGFGUIUtility.eStyleButton.eButton))
				{
					KGFModule.OpenHelpWindow(itsInstance);
				}
				itsInstance.itsOpen = KGFGUIUtility.Toggle(itsInstance.itsOpen, string.Empty, KGFGUIUtility.eStyleToggl.eTogglSwitch, GUILayout.Width(KGFGUIUtility.GetSkinHeight()));
				KGFGUIUtility.EndWindowHeader(theCloseButton: false);
				KGFGUIUtility.EndVerticalBox();
				GUILayout.EndArea();
			}
			else
			{
				itsInstance.DrawMinimizedWindow();
			}
			KGFGUIUtility.RenderDropDownList();
			itsInstance.itsSuggestions.Render();
			if (!itsInstance.itsIsMobile)
			{
				Vector3 mousePosition = Input.mousePosition;
				mousePosition.y = (float)Screen.height - mousePosition.y;
				if (itsInstance.itsOpen)
				{
					if (itsInstance.itsOpenWindow.Contains(mousePosition))
					{
						if (!itsInstance.itsHover)
						{
							itsInstance.itsHover = true;
							Cursor.visible = false;
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
			}
			if (GUI.GetNameOfFocusedControl() == "commandCommand" || GUI.GetNameOfFocusedControl().Equals("messageSearch") || GUI.GetNameOfFocusedControl().Equals("categorySearch"))
			{
				itsInstance.itsFocus = true;
			}
			else
			{
				itsInstance.itsFocus = false;
			}
			GUI.SetNextControlName(string.Empty);
			if ((itsInstance.itsModifierKeyFocusPressed || itsInstance.itsDataModuleConsole.itsModifierKeyFocus == KeyCode.None) && itsInstance.itsShortcutKeyFocusPressed)
			{
				itsInstance.itsModifierKeyFocusPressed = false;
				itsInstance.itsShortcutKeyFocusPressed = false;
				GUI.FocusControl("commandCommand");
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

	public static void Minimize()
	{
		if (itsInstance != null)
		{
			itsInstance.itsOpen = false;
		}
	}

	private void DrawMinimizedWindow()
	{
		float num = KGFGUIUtility.GetSkinHeight() + (float)KGFGUIUtility.GetStyleButton(KGFGUIUtility.eStyleButton.eButton).margin.vertical + (float)KGFGUIUtility.GetStyleBox(KGFGUIUtility.eStyleBox.eBoxDecorated).padding.vertical;
		itsMinimizedWindow.width = Screen.width;
		itsMinimizedWindow.y = (float)Screen.height - num;
		itsMinimizedWindow.height = num;
		GUILayout.BeginArea(itsMinimizedWindow);
		GUILayout.BeginVertical();
		GUILayout.BeginHorizontal();
		KGFGUIUtility.BeginVerticalBox(KGFGUIUtility.eStyleBox.eBoxDecorated);
		KGFGUIUtility.BeginWindowHeader("KGFConsole", GetIcon());
		DrawFooterControls();
		GUILayout.FlexibleSpace();
		if (!itsInstance.itsIsMobile && KGFGUIUtility.Button(itsDataModuleConsole.itsIconHelp, KGFGUIUtility.eStyleButton.eButton))
		{
			KGFModule.OpenHelpWindow(this);
		}
		itsOpen = KGFGUIUtility.Toggle(itsOpen, string.Empty, KGFGUIUtility.eStyleToggl.eTogglSwitch, GUILayout.Width(KGFGUIUtility.GetSkinHeight()));
		KGFGUIUtility.EndWindowHeader(theCloseButton: false);
		KGFGUIUtility.EndVerticalBox();
		GUILayout.EndHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.EndVertical();
		GUILayout.EndArea();
	}

	protected void Update()
	{
		if (itsDataModuleConsole.itsVisible)
		{
			Cursor.visible = true;
		}
		if (itsDataModuleConsole.itsHideKey != 0 && !Input.GetKey(itsDataModuleConsole.itsExpandKeyModifier) && ((Input.GetKey(itsDataModuleConsole.itsHideKeyModifier) && Input.GetKey(itsDataModuleConsole.itsHideKeyModifier) && Input.GetKeyDown(itsDataModuleConsole.itsHideKey)) || (itsDataModuleConsole.itsHideKeyModifier == KeyCode.None && Input.GetKeyDown(itsDataModuleConsole.itsHideKey))))
		{
			itsDataModuleConsole.itsVisible = !itsDataModuleConsole.itsVisible;
			if (!itsDataModuleConsole.itsVisible)
			{
				itsUnfocus = true;
			}
		}
		if (itsDataModuleConsole.itsExpandKey != 0 && !Input.GetKey(itsDataModuleConsole.itsHideKeyModifier) && ((Input.GetKey(itsDataModuleConsole.itsExpandKeyModifier) && Input.GetKeyDown(itsDataModuleConsole.itsExpandKey)) || (itsDataModuleConsole.itsExpandKeyModifier == KeyCode.None && Input.GetKeyDown(itsDataModuleConsole.itsExpandKey))))
		{
			itsOpen = !itsOpen;
			if (itsOpen)
			{
				itsDataModuleConsole.itsVisible = true;
			}
		}
		if (Input.GetKeyUp(itsDataModuleConsole.itsModifierKeyFocus))
		{
			itsModifierKeyFocusPressed = true;
		}
		if (Input.GetKeyUp(itsDataModuleConsole.itsSchortcutKeyFocus))
		{
			itsShortcutKeyFocusPressed = true;
		}
	}

	private void ReactoOnEnterEscape()
	{
		if (itsInstance == null || !(GUI.GetNameOfFocusedControl() == "commandCommand"))
		{
			return;
		}
		Event current = Event.current;
		if (current == null)
		{
			return;
		}
		itsShortcutKeyFocusPressed = false;
		itsModifierKeyFocusPressed = false;
		if (current.type == EventType.KeyDown && current.keyCode == KeyCode.Return)
		{
			if (GetFocused() && itsInstance.itsSuggestions.GetCount() > 0)
			{
				itsCommand = itsInstance.itsSuggestions.GetSelected();
				ExecuteCommand();
			}
			else
			{
				ExecuteCommand();
			}
		}
		if (current.type == EventType.KeyDown && current.keyCode == KeyCode.Escape)
		{
			itsCommand = string.Empty;
			GUI.FocusControl(string.Empty);
		}
		if (current.type == EventType.KeyDown && current.keyCode == KeyCode.UpArrow)
		{
			itsInstance.itsSuggestions.SelectionUp();
		}
		if (current.type == EventType.KeyDown && current.keyCode == KeyCode.DownArrow)
		{
			itsInstance.itsSuggestions.SelectionDown();
		}
	}

	private void DrawFooterControls()
	{
		KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxInvisible);
		GUI.SetNextControlName("commandCommand");
		string b = itsCommand;
		ReactoOnEnterEscape();
		itsCommand = KGFGUIUtility.TextField(itsCommand, KGFGUIUtility.eStyleTextField.eTextFieldLeft, GUILayout.Width((float)Screen.width * 0.125f));
		if (itsCommand != b)
		{
			if (itsCommand != string.Empty)
			{
				int num = 0;
				List<string> list = new List<string>();
				foreach (string key in itsCommandDictionary.Keys)
				{
					if (key.ToLower().StartsWith(itsCommand.ToLower()))
					{
						list.Add(key);
						num++;
					}
					if (num > itsInstance.itsSuggestions.GetMaxCount())
					{
						break;
					}
				}
				itsInstance.itsSuggestions.SetItems(list);
			}
			else
			{
				itsInstance.itsSuggestions.ClearItems();
			}
		}
		itsSuggestions.SetWidth((float)Screen.width * 0.125f);
		if (Event.current.type == EventType.Repaint)
		{
			Rect lastRect = GUILayoutUtility.GetLastRect();
			if (!itsInstance.itsOpen)
			{
				itsSuggestions.SetPosition(lastRect.x, (float)Screen.height - lastRect.y);
			}
			else
			{
				itsSuggestions.SetPosition(lastRect.x, (float)Screen.height - lastRect.y + itsInstance.itsCurrentHeight - lastRect.height - KGFGUIUtility.GetStyleTextField(KGFGUIUtility.eStyleTextField.eTextFieldLeft).fixedHeight - (float)KGFGUIUtility.GetStyleTextField(KGFGUIUtility.eStyleTextField.eTextFieldLeft).padding.top);
			}
		}
		itsDropDownLastFive.itsWidth = (uint)((float)Screen.width * 0.125f);
		itsDropDownFavouriteFive.itsWidth = (uint)((float)Screen.width * 0.125f);
		if (KGFGUIUtility.Button(itsDataModuleConsole.itsIconExecute, "execute", KGFGUIUtility.eStyleButton.eButtonRight))
		{
			ExecuteCommand();
		}
		if (Time.time > itsLastExecution + 0.5f)
		{
			itsLastExecution = 0f;
			itsCommandState = eExecutionState.eNone;
		}
		switch (itsCommandState)
		{
		case eExecutionState.eSuccessful:
			GUILayout.Space(8f);
			KGFGUIUtility.Label(string.Empty, itsDataModuleConsole.itsIconSuccessful, KGFGUIUtility.eStyleLabel.eLabel);
			break;
		case eExecutionState.eNotFound:
			GUILayout.Space(8f);
			KGFGUIUtility.Label(string.Empty, itsDataModuleConsole.itsIconFatal, KGFGUIUtility.eStyleLabel.eLabel);
			break;
		case eExecutionState.eError:
			GUILayout.Space(8f);
			KGFGUIUtility.Label(string.Empty, itsDataModuleConsole.itsIconError, KGFGUIUtility.eStyleLabel.eLabel);
			break;
		case eExecutionState.eNone:
			GUILayout.Space(8f);
			KGFGUIUtility.Label(string.Empty, itsDataModuleConsole.itsIconUnknown, KGFGUIUtility.eStyleLabel.eLabel);
			break;
		}
		GUILayout.Space(10f);
		itsInstance.itsDropDownLastFive.Render();
		if (Event.current.type == EventType.Repaint && !itsOpen)
		{
			itsInstance.itsDropDownLastFive.itsLastRect.x += itsMinimizedWindow.x;
			itsInstance.itsDropDownLastFive.itsLastRect.y += itsMinimizedWindow.y;
		}
		else if (Event.current.type == EventType.Repaint && itsOpen)
		{
			itsInstance.itsDropDownLastFive.itsLastRect.x += itsOpenWindow.x;
			itsInstance.itsDropDownLastFive.itsLastRect.y += itsOpenWindow.y;
		}
		else if (Event.current.type == EventType.Layout && KGFGUIDropDown.itsOpenInstance != null)
		{
			KGFGUIDropDown.itsCorrectedOffset = true;
		}
		GUILayout.Space(10f);
		itsInstance.itsDropDownFavouriteFive.Render();
		if (Event.current.type == EventType.Repaint && !itsOpen)
		{
			itsInstance.itsDropDownFavouriteFive.itsLastRect.x += itsMinimizedWindow.x;
			itsInstance.itsDropDownFavouriteFive.itsLastRect.y += itsMinimizedWindow.y;
		}
		else if (Event.current.type == EventType.Repaint && itsOpen)
		{
			itsInstance.itsDropDownFavouriteFive.itsLastRect.x += itsOpenWindow.x;
			itsInstance.itsDropDownFavouriteFive.itsLastRect.y += itsOpenWindow.y;
		}
		else if (Event.current.type == EventType.Layout && KGFGUIDropDown.itsOpenInstance != null)
		{
			KGFGUIDropDown.itsCorrectedOffset = true;
		}
		if (!itsInstance.itsIsMobile)
		{
			GUILayout.Space(10f);
		}
		KGFGUIUtility.EndHorizontalBox();
		GUILayout.FlexibleSpace();
	}

	private void DrawCategoryColumn()
	{
		bool flag = false;
		KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxDarkTop);
		KGFGUIUtility.BeginVerticalPadding();
		if (KGFGUIUtility.Button("All", KGFGUIUtility.eStyleButton.eButtonLeft, GUILayout.ExpandWidth(expand: true)))
		{
			foreach (KGFCcommandCategory allCategory in GetAllCategories())
			{
				allCategory.itsSelectedState = true;
				flag = true;
			}
		}
		if (KGFGUIUtility.Button("None", KGFGUIUtility.eStyleButton.eButtonRight, GUILayout.ExpandWidth(expand: true)))
		{
			foreach (KGFCcommandCategory allCategory2 in GetAllCategories())
			{
				allCategory2.itsSelectedState = false;
				flag = true;
			}
		}
		KGFGUIUtility.EndVerticalPadding();
		KGFGUIUtility.EndHorizontalBox();
		KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxMiddleVertical);
		itsCategoryScrollViewPosition = KGFGUIUtility.BeginScrollView(itsCategoryScrollViewPosition, false, false, GUILayout.ExpandWidth(expand: true));
		int num = 0;
		foreach (KGFCcommandCategory allCategory3 in GetAllCategories())
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
		if (num < 1)
		{
			KGFGUIUtility.Label("no items found");
		}
		KGFGUIUtility.EndScrollView();
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
			if (!GUI.GetNameOfFocusedControl().Equals("categorySearch"))
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
			UpdateCommandListsResult();
		}
	}

	private void DrawFilterSearchBar()
	{
		string a = itsSearchFilterMessage;
		bool flag = false;
		GUI.SetNextControlName("messageSearch");
		itsSearchFilterMessage = KGFGUIUtility.TextField(itsSearchFilterMessage, KGFGUIUtility.eStyleTextField.eTextField, GUILayout.Width(150f));
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
			if (!GUI.GetNameOfFocusedControl().Equals("messageSearch"))
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
			UpdateCommandListsResult();
		}
	}

	public override string GetName()
	{
		return "KGFConsole";
	}

	public override string GetDocumentationPath()
	{
		return "KGFConsole_Manual.html";
	}

	public override string GetForumPath()
	{
		return "index.php?qa=kgfconsole";
	}

	public override Texture2D GetIcon()
	{
		CheckInstance();
		if (itsInstance != null && itsInstance.itsDataModuleConsole != null)
		{
			return itsInstance.itsDataModuleConsole.itsIconModule;
		}
		return null;
	}

	private static KGFConsole GetInstance()
	{
		return itsInstance;
	}

	public static bool GetFocused()
	{
		if (itsInstance != null && itsInstance.itsDataModuleConsole.itsVisible && (itsInstance.itsHover || itsInstance.itsFocus))
		{
			return true;
		}
		return false;
	}

	public static bool GetHover()
	{
		if (itsInstance != null && itsInstance.itsDataModuleConsole.itsVisible && (itsInstance.itsHover || itsInstance.itsDropDownFavouriteFive.GetHover() || itsInstance.itsDropDownLastFive.GetHover()))
		{
			return true;
		}
		return false;
	}

	public static void AddCommand(string theCommandName, object theObject, string theMethodName)
	{
		AddCommand(theCommandName, string.Empty, "uncategorized", theObject, theMethodName);
	}

	public static void AddCommand(string theCommandName, string theCategory, object theObject, string theMethodName)
	{
		AddCommand(theCommandName, string.Empty, theCategory, theObject, theMethodName);
	}

	public static void AddCommand(string theCommandName, string theDescription, string theCategory, object theObject, string theMethodName)
	{
		CheckInstance();
		if (itsInstance == null)
		{
			return;
		}
		if (!itsInstance.itsCommandDictionary.ContainsKey(theCommandName))
		{
			KGFCommandCode value = new KGFCommandCode(theCommandName, theDescription, theCategory, theObject, theMethodName);
			itsInstance.itsCommandDictionary[theCommandName] = value;
			if (!itsInstance.itsCommandCategories.ContainsKey(theCategory))
			{
				itsInstance.itsCommandCategories.Add(theCategory, new KGFCcommandCategory(theCategory));
				itsInstance.itsCommandCategories[theCategory].IncreaseCount();
			}
			else
			{
				itsInstance.itsCommandCategories[theCategory].IncreaseCount();
			}
			itsInstance.LoadLastFiveFromPlayerPrefs();
			itsInstance.LoadFavouriteFiveFromPlayerPrefs();
			itsInstance.itsDropDownLastFive.SetEntrys(itsInstance.itsLastFiveCommandCodes);
			itsInstance.itsDropDownFavouriteFive.SetEntrys(itsInstance.itsFavouriteCommandCodes.Keys);
			itsInstance.UpdateCommandListsResult();
		}
		else
		{
			Debug.LogError($"command '{theCommandName}' is already registered in the console module.");
		}
	}

	public static void RemoveCommand(string theCommandString)
	{
		CheckInstance();
		if (!(itsInstance == null) && itsInstance.itsCommandDictionary.ContainsKey(theCommandString))
		{
			itsInstance.itsCommandDictionary.Remove(theCommandString);
			itsInstance.UpdateCommandListsResult();
		}
	}

	private static void CheckInstance()
	{
		if (itsInstance == null)
		{
			UnityEngine.Object @object = UnityEngine.Object.FindObjectOfType(typeof(KGFConsole));
			if (@object != null)
			{
				itsInstance = (@object as KGFConsole);
				itsInstance.Init();
			}
			else if (!itsAlreadyChecked)
			{
				Debug.LogError("KOLMICH Console Module is not running. Make sure that there is an instance of the KGFConsole prefab in the current scene.");
				itsAlreadyChecked = true;
			}
		}
	}

	private void Init()
	{
		if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
		{
			itsInstance.itsIsMobile = true;
			string skinPath = KGFGUIUtility.GetSkinPath();
			if (Screen.width >= 800)
			{
				KGFGUIUtility.SetSkinPathEditor(skinPath.Replace("_16", "_32"));
			}
			else
			{
				KGFGUIUtility.SetSkinPathEditor(skinPath.Replace("_32", "_16"));
			}
		}
		else
		{
			itsInstance.itsIsMobile = false;
		}
		List<string> theEntrys = new List<string>();
		itsInstance.itsDropDownLastFive = new KGFGUIDropDown(theEntrys, 120u, 5u, KGFGUIDropDown.eDropDirection.eUp);
		itsInstance.itsDropDownFavouriteFive = new KGFGUIDropDown(theEntrys, 120u, 5u, KGFGUIDropDown.eDropDirection.eUp);
		itsInstance.itsDropDownLastFive.itsTitle = "last 5";
		itsInstance.itsDropDownFavouriteFive.itsTitle = "favourite 5";
		itsInstance.itsDropDownLastFive.itsIcon = itsDataModuleConsole.itsIconDropDown;
		itsInstance.itsDropDownFavouriteFive.itsIcon = itsDataModuleConsole.itsIconDropDown;
		itsInstance.itsDropDownLastFive.SelectedValueChanged += SelectedComboBoxValueChanged;
		itsInstance.itsDropDownFavouriteFive.SelectedValueChanged += SelectedComboBoxValueChanged;
		itsInstance.itsCommandTable = new KGFDataTable();
		if (itsInstance.itsIsMobile)
		{
			itsInstance.itsCommandTable.Columns.Add(new KGFDataColumn("Cmd", typeof(string)));
			itsInstance.itsCommandTable.Columns.Add(new KGFDataColumn("Desc", typeof(string)));
			itsInstance.itsCommandTable.Columns.Add(new KGFDataColumn("Cat", typeof(string)));
			itsInstance.itsCommandTable.Columns.Add(new KGFDataColumn("Cnt", typeof(string)));
		}
		else
		{
			itsInstance.itsCommandTable.Columns.Add(new KGFDataColumn("Command", typeof(string)));
			itsInstance.itsCommandTable.Columns.Add(new KGFDataColumn("Description", typeof(string)));
			itsInstance.itsCommandTable.Columns.Add(new KGFDataColumn("Category", typeof(string)));
			itsInstance.itsCommandTable.Columns.Add(new KGFDataColumn("Count", typeof(string)));
		}
		itsTableControl = new KGFGUIDataTable(itsCommandTable, GUILayout.ExpandHeight(expand: true));
		itsTableControl.OnClickRow += OnCommandTableRowIsClicked;
		itsTableControl.PostRenderRow += PostCommandTableRow;
		itsTableControl.SetColumnWidth(0, (uint)((float)Screen.width / 10f));
		itsTableControl.SetColumnWidth(1, (uint)((float)Screen.width / 4f));
		itsTableControl.SetColumnWidth(2, (uint)((float)Screen.width / 7f));
		itsTableControl.SetColumnWidth(3, 20u);
		itsTypeMapping = new Dictionary<Type, string>();
		itsTypeMapping.Add(typeof(short), "short");
		itsTypeMapping.Add(typeof(int), "int");
		itsTypeMapping.Add(typeof(long), "long");
		itsTypeMapping.Add(typeof(float), "float");
		itsTypeMapping.Add(typeof(double), "double");
		itsTypeMapping.Add(typeof(decimal), "decimal");
		itsTypeMapping.Add(typeof(string), "string");
		itsTypeMapping.Add(typeof(char), "char");
		itsTypeMapping.Add(typeof(bool), "bool");
		KGFConsoleCommands.AddCommands();
		itsInstance.LoadSizeFromPlayerPrefs();
		if (itsCurrentHeight == 0f)
		{
			itsCurrentHeight = (float)Screen.height * 0.5f;
		}
	}

	private IEnumerable<KGFCcommandCategory> GetAllCategories()
	{
		return itsCommandCategories.Values;
	}

	private bool FilterCommandItem(KGFCommandCode theCommandCode)
	{
		if (!itsSearchFilterMessage.Equals("Search") && !itsSearchFilterMessage.Equals(string.Empty))
		{
			string value = itsSearchFilterMessage.Trim().ToLower();
			if (!theCommandCode.itsDescription.Trim().ToLower().Contains(value) && !theCommandCode.itsCommandCode.Trim().ToLower().Contains(value) && !theCommandCode.itsCategory.Trim().ToLower().Contains(value))
			{
				return false;
			}
		}
		foreach (KGFCcommandCategory value2 in itsCommandCategories.Values)
		{
			if (value2.itsSelectedState && value2.GetName().ToLower().Contains(theCommandCode.itsCategory.ToLower()))
			{
				return true;
			}
		}
		return false;
	}

	private IEnumerable<KGFCommandCode> GetFilteredCommandListRealtime()
	{
		foreach (KGFCommandCode aCommandCode in itsCommandDictionary.Values)
		{
			if (itsCommandCategories.ContainsKey(aCommandCode.itsCategory) && itsCommandCategories[aCommandCode.itsCategory].itsSelectedState && FilterCommandItem(aCommandCode))
			{
				yield return aCommandCode;
			}
		}
	}

	private void UpdateCommandListsResult()
	{
		itsCommandTable.Rows.Clear();
		foreach (KGFCommandCode item in GetFilteredCommandListRealtime())
		{
			KGFDataRow kGFDataRow = itsCommandTable.NewRow();
			kGFDataRow[0].Value = item.itsCommandCode;
			kGFDataRow[1].Value = item.itsDescription;
			kGFDataRow[2].Value = item.itsCategory;
			kGFDataRow[3].Value = item.GetExecutionCount().ToString();
			itsCommandTable.Rows.Add(kGFDataRow);
		}
	}

	private void ExecuteCommand()
	{
		itsCommand = CleanCommandString(itsCommand);
		string[] array = itsCommand.Split(new char[1]
		{
			' '
		});
		string text = array[0];
		string empty = string.Empty;
		empty = ((itsCommand.Split(new char[1]
		{
			' '
		}).Length <= 1) ? null : itsCommand.Substring(text.Length + 1, itsCommand.Length - (text.Length + 1)));
		if (itsCommandDictionary.ContainsKey(text))
		{
			KGFCommandCode kGFCommandCode = itsCommandDictionary[text];
			if (kGFCommandCode.Execute(empty))
			{
				PlayerPrefs.SetInt("KGF.KGFModuleConsole.aCommand." + kGFCommandCode + empty, (int)kGFCommandCode.GetExecutionCount());
				itsCommand = itsCommand.Trim();
				if (!itsLastFiveCommandCodes.Contains(itsCommand))
				{
					if (itsLastFiveCommandCodes.Count < 5)
					{
						itsLastFiveCommandCodes.Add(itsCommand);
					}
					else
					{
						itsLastFiveCommandCodes = itsLastFiveCommandCodes.GetRange(1, 4);
						itsLastFiveCommandCodes.Add(itsCommand);
					}
					SaveLastFiveToPlayerPrefs();
				}
				if (itsFavouriteCommandCodes.ContainsKey(itsCommand))
				{
					Dictionary<string, KGFFavouriteCommand> dictionary;
					Dictionary<string, KGFFavouriteCommand> dictionary2 = dictionary = itsFavouriteCommandCodes;
					string key;
					string key2 = key = itsCommand;
					KGFFavouriteCommand kGFFavouriteCommand = dictionary[key];
					KGFFavouriteCommand kGFFavouriteCommand2 = kGFFavouriteCommand;
					dictionary2[key2] = ++kGFFavouriteCommand2;
				}
				else
				{
					itsFavouriteCommandCodes.Add(itsCommand, new KGFFavouriteCommand(kGFCommandCode, empty));
					itsFavourite.Add(itsFavouriteCommandCodes[itsCommand]);
				}
				itsFavourite.Sort();
				itsFavourite.Reverse();
				SaveFavouriteFiveToPlayerPrefs();
				itsCommandState = eExecutionState.eSuccessful;
				itsCommand = string.Empty;
				itsInstance.itsSuggestions.ClearItems();
				itsInstance.itsDropDownLastFive.SetEntrys(itsLastFiveCommandCodes);
				List<string> list = new List<string>();
				foreach (KGFFavouriteCommand item in itsFavourite.GetRange(0, Math.Min(5, itsFavourite.Count)))
				{
					list.Add(item.GetCommandCode() + " " + item.GetParameters());
				}
				itsInstance.itsDropDownFavouriteFive.SetEntrys(list);
			}
			else
			{
				itsCommandState = eExecutionState.eError;
			}
		}
		else
		{
			itsCommandState = eExecutionState.eNotFound;
		}
		itsLastExecution = Time.time;
		UpdateCommandListsResult();
	}

	private void SaveLastFiveToPlayerPrefs()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string itsLastFiveCommandCode in itsLastFiveCommandCodes)
		{
			stringBuilder.Append(itsLastFiveCommandCode + "|");
		}
		stringBuilder.Remove(stringBuilder.Length - 1, 1);
		PlayerPrefs.SetString("KGF.KGFModuleConsole.itsLastFiveCommandCodes", stringBuilder.ToString());
	}

	private void SaveFavouriteFiveToPlayerPrefs()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KGFFavouriteCommand item in itsFavourite)
		{
			stringBuilder.Append($"{item.GetCommandCode().Trim()}:{item.GetParameters()}:{item.GetExecutionCount()}|");
		}
		stringBuilder.Remove(stringBuilder.Length - 1, 1);
		PlayerPrefs.SetString("KGF.KGFModuleConsole.itsFavouriteCommandCodes", stringBuilder.ToString());
	}

	private void LoadLastFiveFromPlayerPrefs()
	{
		string @string = PlayerPrefs.GetString("KGF.KGFModuleConsole.itsLastFiveCommandCodes", string.Empty);
		if (!(@string != string.Empty))
		{
			return;
		}
		string[] array = @string.Split(new char[1]
		{
			'|'
		});
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (itsCommandDictionary.ContainsKey(text.Split(new char[1]
			{
				' '
			})[0]) && !itsLastFiveCommandCodes.Contains(text))
			{
				itsLastFiveCommandCodes.Add(text.Trim());
			}
		}
	}

	private void LoadFavouriteFiveFromPlayerPrefs()
	{
		string @string = PlayerPrefs.GetString("KGF.KGFModuleConsole.itsFavouriteCommandCodes", string.Empty);
		if (!(@string != string.Empty))
		{
			return;
		}
		string[] array = @string.Split(new char[1]
		{
			'|'
		});
		string[] array2 = array;
		foreach (string text in array2)
		{
			string[] array3 = text.Split(new char[1]
			{
				':'
			});
			for (int j = 0; j < array3.Length; j++)
			{
				array3[j] = array3[j].Trim();
			}
			if (!itsCommandDictionary.ContainsKey(array3[0]))
			{
				continue;
			}
			if (array3[1].Length == 0)
			{
				if (!itsFavouriteCommandCodes.ContainsKey(array3[0]))
				{
					itsFavouriteCommandCodes.Add(array3[0], new KGFFavouriteCommand(itsCommandDictionary[array3[0]], array3[1], uint.Parse(array3[2])));
					itsFavourite.Add(itsFavouriteCommandCodes[array3[0]]);
				}
				continue;
			}
			string key = $"{array3[0]} {array3[1]}";
			if (!itsFavouriteCommandCodes.ContainsKey(key))
			{
				itsFavouriteCommandCodes.Add(key, new KGFFavouriteCommand(itsCommandDictionary[array3[0]], array3[1], uint.Parse(array3[2])));
				itsFavourite.Add(itsFavouriteCommandCodes[key]);
			}
		}
		itsFavourite.Sort();
		itsFavourite = itsFavourite.GetRange(0, Math.Min(5, itsFavourite.Count));
	}

	private void SaveSizeToPlayerPrefs()
	{
		PlayerPrefs.SetFloat("KGFConsole.WindowSize", itsCurrentHeight);
	}

	private void LoadSizeFromPlayerPrefs()
	{
		itsCurrentHeight = PlayerPrefs.GetFloat("KGFConsole.WindowSize", 0f);
	}

	private string CleanCommandString(string theCommand)
	{
		while (theCommand.Contains("  "))
		{
			theCommand = theCommand.Replace("  ", " ");
		}
		return theCommand.Trim();
	}

	private void OnCommandTableRowIsClicked(object theSender, EventArgs theArguments)
	{
		KGFDataRow kGFDataRow = theSender as KGFDataRow;
		if (kGFDataRow != null && itsInstance.itsCommandDictionary.ContainsKey(kGFDataRow[0].ToString()))
		{
			if (itsCurrentSelectedRow != kGFDataRow)
			{
				itsCurrentSelectedRow = kGFDataRow;
			}
			else
			{
				itsCurrentSelectedRow = null;
			}
		}
	}

	private void PostCommandTableRow(object theSender, EventArgs theArguments)
	{
		KGFDataRow kGFDataRow = theSender as KGFDataRow;
		if (kGFDataRow != null)
		{
			GUI.contentColor = Color.white;
			if (kGFDataRow == itsCurrentSelectedRow && itsCommandDictionary.ContainsKey(kGFDataRow[0].ToString()))
			{
				KGFCommandCode kGFCommandCode = itsCommandDictionary[kGFDataRow[0].ToString()];
				KGFGUIUtility.BeginVerticalBox(KGFGUIUtility.eStyleBox.eBoxDarkBottom, GUILayout.ExpandWidth(expand: true));
				GUILayout.Space(KGFGUIUtility.GetStyleLabel(KGFGUIUtility.eStyleLabel.eLabel).fontSize);
				KGFGUIUtility.Label(string.Format("{0} {1}", "Description: ", kGFCommandCode.itsDescription), GUILayout.ExpandWidth(expand: true));
				GUILayout.Space(KGFGUIUtility.GetStyleLabel(KGFGUIUtility.eStyleLabel.eLabel).fontSize);
				KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxDarkBottom, GUILayout.ExpandWidth(expand: true));
				StringBuilder stringBuilder = new StringBuilder();
				foreach (KGFMethodInfo mehod in kGFCommandCode.GetMehods())
				{
					stringBuilder.Append(string.Format("{0} {1}", kGFCommandCode.itsCommandCode, " "));
					uint num = 0u;
					foreach (Type parameterType in mehod.GetParameterTypes())
					{
						num++;
						stringBuilder.Append($"<{itsTypeMapping[parameterType]}> ");
					}
					Texture2D theImage = null;
					if (num == 0)
					{
						theImage = itsDataModuleConsole.itsIconExecute;
					}
					if (KGFGUIUtility.Button(theImage, stringBuilder.ToString(), KGFGUIUtility.eStyleButton.eButton, GUILayout.ExpandWidth(expand: true)))
					{
						stringBuilder.Remove(0, stringBuilder.Length);
						stringBuilder.Append($"{kGFCommandCode.itsCommandCode} ");
						foreach (Type parameterType2 in mehod.GetParameterTypes())
						{
							if ((object)parameterType2 == typeof(short))
							{
								stringBuilder.Append($"{((short)0).ToString()} ");
							}
							else if ((object)parameterType2 == typeof(int))
							{
								stringBuilder.Append($"{0.ToString()} ");
							}
							else if ((object)parameterType2 == typeof(long))
							{
								stringBuilder.Append($"{0L.ToString()} ");
							}
							else if ((object)parameterType2 == typeof(float))
							{
								stringBuilder.Append($"{0f.ToString()} ");
							}
							else if ((object)parameterType2 == typeof(double))
							{
								stringBuilder.Append($"{0.0.ToString()} ");
							}
							else if ((object)parameterType2 == typeof(decimal))
							{
								stringBuilder.Append($"{0m.ToString()} ");
							}
							else if ((object)parameterType2 == typeof(string))
							{
								stringBuilder.Append("string ");
							}
							else if ((object)parameterType2 == typeof(char))
							{
								stringBuilder.Append($"{'\0'.ToString()} ");
							}
							else if ((object)parameterType2 == typeof(bool))
							{
								stringBuilder.Append($"{false.ToString()} ");
							}
						}
						itsCommand = CleanCommandString(stringBuilder.ToString());
						if (mehod.GetParameterCount() == 0)
						{
							ExecuteCommand();
						}
						else
						{
							GUI.FocusControl("commandCommand");
							itsFocus = true;
						}
					}
					stringBuilder.Remove(0, stringBuilder.Length);
				}
				KGFGUIUtility.EndHorizontalBox();
				KGFGUIUtility.EndVerticalBox();
			}
		}
	}

	private void SelectedComboBoxValueChanged(object theSender, EventArgs theArguments)
	{
		string text = theSender as string;
		if (text != null)
		{
			itsCommand = text;
			ExecuteCommand();
		}
	}

	public override KGFMessageList Validate()
	{
		KGFMessageList kGFMessageList = new KGFMessageList();
		if (itsDataModuleConsole.itsIconModule == null)
		{
			kGFMessageList.AddWarning("the module icon is missing");
		}
		if (itsDataModuleConsole.itsIconExecute == null)
		{
			kGFMessageList.AddWarning("the execution icon is missing");
		}
		if (itsDataModuleConsole.itsIconError == null)
		{
			kGFMessageList.AddWarning("the error icon is missing");
		}
		if (itsDataModuleConsole.itsIconFatal == null)
		{
			kGFMessageList.AddWarning("the fatal error icon is missing");
		}
		if (itsDataModuleConsole.itsIconUnknown == null)
		{
			kGFMessageList.AddWarning("the unknown icon is missing");
		}
		if (itsDataModuleConsole.itsIconSuccessful == null)
		{
			kGFMessageList.AddWarning("the successful icon is missing");
		}
		if (itsDataModuleConsole.itsIconHelp == null)
		{
			kGFMessageList.AddWarning("the help icon is missing");
		}
		if (itsDataModuleConsole.itsIconDropDown == null)
		{
			kGFMessageList.AddWarning("the drop down icon is missing");
		}
		if (itsDataModuleConsole.itsModifierKeyFocus == itsDataModuleConsole.itsSchortcutKeyFocus)
		{
			kGFMessageList.AddInfo("the modifier key is equal to the shortcut key");
		}
		return kGFMessageList;
	}
}
