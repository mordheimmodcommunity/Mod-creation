using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

public class KGFDebug : KGFModule, KGFIValidator
{
	[Serializable]
	public class KGFDataDebug
	{
		public Texture2D itsIconModule;

		public KGFeDebugLevel itsMinimumStackTraceLevel = KGFeDebugLevel.eFatal;
	}

	public class KGFDebugLog
	{
		private KGFeDebugLevel itsLevel;

		private string itsCategory;

		private string itsMessage;

		private DateTime itsLogTime;

		private string itsStackTrace;

		private object itsObject;

		public KGFDebugLog(KGFeDebugLevel theLevel, string theCategory, string theMessage, string theStackTrace)
		{
			itsLevel = theLevel;
			itsCategory = theCategory;
			itsMessage = theMessage;
			itsLogTime = DateTime.Now;
			itsStackTrace = theStackTrace;
			itsObject = null;
		}

		public KGFDebugLog(KGFeDebugLevel theLevel, string theCategory, string theMessage, string theStackTrace, object theObject)
		{
			itsLevel = theLevel;
			itsCategory = theCategory;
			itsMessage = theMessage;
			itsLogTime = DateTime.Now;
			itsStackTrace = theStackTrace;
			itsObject = theObject;
		}

		public KGFeDebugLevel GetLevel()
		{
			return itsLevel;
		}

		public string GetCategory()
		{
			return itsCategory;
		}

		public string GetMessage()
		{
			return itsMessage;
		}

		public DateTime GetLogTime()
		{
			return itsLogTime;
		}

		public string GetStackTrace()
		{
			return itsStackTrace;
		}

		public object GetObject()
		{
			return itsObject;
		}
	}

	private static KGFDebug itsInstance = null;

	public KGFDataDebug itsDataModuleDebugger = new KGFDataDebug();

	private static List<KGFIDebug> itsRegisteredLogger = new List<KGFIDebug>();

	private static List<KGFDebugLog> itsCachedLogs = new List<KGFDebugLog>();

	private static bool itsAlreadyChecked = false;

	public KGFDebug()
		: base(new Version(1, 2, 0, 0), new Version(1, 2, 0, 0))
	{
	}

	protected override void KGFAwake()
	{
		base.KGFAwake();
		if (itsInstance == null)
		{
			itsInstance = this;
		}
		else if (itsInstance != this)
		{
			UnityEngine.Debug.Log("there is more than one KFGDebug instance in this scene. please ensure there is always exactly one instance in this scene");
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	protected void Start()
	{
		if (itsCachedLogs != null)
		{
			itsCachedLogs.Clear();
			itsCachedLogs = null;
		}
	}

	public static KGFDebug GetInstance()
	{
		return itsInstance;
	}

	public override string GetName()
	{
		return "KGFDebug";
	}

	public override string GetDocumentationPath()
	{
		return "KGFDebug_Manual.html";
	}

	public override string GetForumPath()
	{
		return "index.php?qa=kgfdebug";
	}

	public override Texture2D GetIcon()
	{
		CheckInstance();
		if (itsInstance != null)
		{
			return itsInstance.itsDataModuleDebugger.itsIconModule;
		}
		return null;
	}

	public static KGFIDebug GetLogger(string theName)
	{
		foreach (KGFIDebug item in itsRegisteredLogger)
		{
			if (item.GetName().Equals(theName))
			{
				return item;
			}
		}
		return null;
	}

	public static void AddLogger(KGFIDebug theLogger)
	{
		CheckInstance();
		if (!itsRegisteredLogger.Contains(theLogger))
		{
			itsRegisteredLogger.Add(theLogger);
			if (itsCachedLogs != null)
			{
				foreach (KGFDebugLog itsCachedLog in itsCachedLogs)
				{
					theLogger.Log(itsCachedLog);
				}
			}
		}
		else
		{
			UnityEngine.Debug.LogError("this logger is already registered.");
		}
	}

	public static void RemoveLogger(KGFIDebug theLogger)
	{
		CheckInstance();
		if (itsRegisteredLogger.Contains(theLogger))
		{
			itsRegisteredLogger.Remove(theLogger);
		}
		else
		{
			UnityEngine.Debug.LogError("the logger you tried to remove wasnt found.");
		}
	}

	private static void CheckInstance()
	{
		if (itsInstance == null)
		{
			UnityEngine.Object @object = UnityEngine.Object.FindObjectOfType(typeof(KGFDebug));
			if (@object != null)
			{
				itsInstance = (@object as KGFDebug);
			}
			else if (!itsAlreadyChecked)
			{
				UnityEngine.Debug.LogError("Kolmich Debug Module is not running. Make sure that there is an instance of the KGFDebug prefab in the current scene. Adding loggers in Awake() can cause problems, prefer to add loggers in Start().");
				itsAlreadyChecked = true;
			}
		}
	}

	public static void LogDebug(string theMessage)
	{
		Log(KGFeDebugLevel.eDebug, "uncategorized", theMessage);
	}

	public static void LogDebug(string theMessage, string theCategory)
	{
		Log(KGFeDebugLevel.eDebug, theCategory, theMessage);
	}

	public static void LogDebug(string theMessage, string theCategory, MonoBehaviour theObject)
	{
		Log(KGFeDebugLevel.eDebug, theCategory, theMessage, theObject);
	}

	public static void LogInfo(string theMessage)
	{
		Log(KGFeDebugLevel.eInfo, "uncategorized", theMessage);
	}

	public static void LogInfo(string theMessage, string theCategory)
	{
		Log(KGFeDebugLevel.eInfo, theCategory, theMessage);
	}

	public static void LogInfo(string theMessage, string theCategory, MonoBehaviour theObject)
	{
		Log(KGFeDebugLevel.eInfo, theCategory, theMessage, theObject);
	}

	public static void LogWarning(string theMessage)
	{
		Log(KGFeDebugLevel.eWarning, "uncategorized", theMessage);
	}

	public static void LogWarning(string theMessage, string theCategory)
	{
		Log(KGFeDebugLevel.eWarning, theCategory, theMessage);
	}

	public static void LogWarning(string theMessage, string theCategory, MonoBehaviour theObject)
	{
		Log(KGFeDebugLevel.eWarning, theCategory, theMessage, theObject);
	}

	public static void LogError(string theMessage)
	{
		Log(KGFeDebugLevel.eError, "uncategorized", theMessage);
	}

	public static void LogError(string theMessage, string theCategory)
	{
		Log(KGFeDebugLevel.eError, theCategory, theMessage);
	}

	public static void LogError(string theMessage, string theCategory, MonoBehaviour theObject)
	{
		Log(KGFeDebugLevel.eError, theCategory, theMessage, theObject);
	}

	public static void LogFatal(string theMessage)
	{
		Log(KGFeDebugLevel.eFatal, "uncategorized", theMessage);
	}

	public static void LogFatal(string theMessage, string theCategory)
	{
		Log(KGFeDebugLevel.eFatal, theCategory, theMessage);
	}

	public static void LogFatal(string theMessage, string theCategory, MonoBehaviour theObject)
	{
		Log(KGFeDebugLevel.eFatal, theCategory, theMessage, theObject);
	}

	private static void Log(KGFeDebugLevel theLevel, string theCategory, string theMessage)
	{
		Log(theLevel, theCategory, theMessage, null);
	}

	private static void Log(KGFeDebugLevel theLevel, string theCategory, string theMessage, MonoBehaviour theObject)
	{
		CheckInstance();
		if (!(itsInstance != null))
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (theLevel >= itsInstance.itsDataModuleDebugger.itsMinimumStackTraceLevel)
		{
			StackTrace stackTrace = new StackTrace(fNeedFileInfo: true);
			for (int i = 2; i < stackTrace.FrameCount; i++)
			{
				stringBuilder.Append(stackTrace.GetFrames()[i].ToString());
				stringBuilder.Append(Environment.NewLine);
			}
		}
		KGFDebugLog kGFDebugLog = new KGFDebugLog(theLevel, theCategory, theMessage, stringBuilder.ToString(), theObject);
		if (itsCachedLogs != null)
		{
			itsCachedLogs.Add(kGFDebugLog);
		}
		foreach (KGFIDebug item in itsRegisteredLogger)
		{
			if (item.GetMinimumLogLevel() <= theLevel)
			{
				item.Log(kGFDebugLog);
			}
		}
	}

	public override KGFMessageList Validate()
	{
		KGFMessageList kGFMessageList = new KGFMessageList();
		if (itsDataModuleDebugger.itsIconModule == null)
		{
			kGFMessageList.AddWarning("the module icon is missing");
		}
		return kGFMessageList;
	}
}
