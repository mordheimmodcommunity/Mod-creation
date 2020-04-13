using UnityEngine;

public interface KGFIDebug
{
    string GetName();

    void Log(KGFDebug.KGFDebugLog theLog);

    void Log(KGFeDebugLevel theLevel, string theCategory, string theMessage);

    void Log(KGFeDebugLevel theLevel, string theCategory, string theMessage, string theStackTrace);

    void Log(KGFeDebugLevel theLevel, string theCategory, string theMessage, string theStackTrace, MonoBehaviour theObject);

    void SetMinimumLogLevel(KGFeDebugLevel theLevel);

    KGFeDebugLevel GetMinimumLogLevel();
}
