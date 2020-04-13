using System;
using UnityEngine;

public class Pandora : MonoBehaviour
{
    public static bool allSystemsInitialized;

    public static Pandora box;

    public GameObject kgf_debug_prefab;

    public GameObject kgf_console_prefab;

    public GameObject kgf_debug_gui_prefab;

    private static uint GUID;

    public static bool useLowSettings;

    public static bool fullLog;

    private void Awake()
    {
        string[] commandLineArgs = Environment.GetCommandLineArgs();
        for (int i = 0; i < commandLineArgs.Length; i++)
        {
            switch (commandLineArgs[i])
            {
                case "-lowSettings":
                    useLowSettings = true;
                    break;
                case "-fullLog":
                    fullLog = true;
                    break;
            }
        }
        Profiler.maxNumberOfSamplesPerFrame = -1;
        if (box == null)
        {
            box = this;
            UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
            allSystemsInitialized = true;
        }
        else
        {
            UnityEngine.Object.DestroyImmediate(base.gameObject);
        }
    }

    private void Start()
    {
    }

    public uint GetNextGUID()
    {
        return GUID++;
    }
}
