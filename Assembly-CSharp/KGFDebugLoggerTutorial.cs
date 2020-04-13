using System;
using UnityEngine;

public class KGFDebugLoggerTutorial : MonoBehaviour, KGFIDebug
{
    private KGFeDebugLevel itsMinimumLogLevel;

    public void Awake()
    {
        KGFDebug.AddLogger(this);
    }

    public string GetName()
    {
        return "KGFTutorialLogger";
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
        if (theObject != null)
        {
            Debug.Log(string.Format("{0} {1} {5} {2}{5}{3}{5}{4}", theLevel, theCategory, theMessage, theObject.name, theStackTrace, Environment.NewLine));
        }
        else
        {
            Debug.Log(string.Format("{0} {1} {4}{2}{4}{3}", theLevel, theCategory, theMessage, theStackTrace, Environment.NewLine));
        }
    }

    public void SetMinimumLogLevel(KGFeDebugLevel theLevel)
    {
        itsMinimumLogLevel = theLevel;
    }

    public KGFeDebugLevel GetMinimumLogLevel()
    {
        return itsMinimumLogLevel;
    }
}
