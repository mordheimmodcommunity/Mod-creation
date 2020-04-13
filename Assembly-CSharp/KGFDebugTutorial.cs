using UnityEngine;

public class KGFDebugTutorial : MonoBehaviour
{
    private void Start()
    {
        KGFDebug.LogDebug("Start() method of enemy: BossLevelOne was called", "Load.methods");
        KGFDebug.LogInfo("Currently 25 enemies in the Scene", "Enemy.Count");
        KGFDebug.LogWarning("Gravitation inconsistency: Make sure gravitation is -9.81.", "Physics.forces");
        KGFDebug.LogError("Cannot open file: d:\\tmp\\mytestdata. Make sure such a file exist and check read write permissions of the directory.", "IO.filereads", this);
        KGFDebug.LogFatal("The referenced module is missing. Make sure to keep the module running if its in use.", "Module.Status");
        Debug.LogError("This error demonstrates KGFDebugs ability to capture unity3d Debug.Log messages.");
    }
}
