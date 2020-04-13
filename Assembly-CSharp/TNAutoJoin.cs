using TNet;
using UnityEngine;

[RequireComponent(typeof(TNManager))]
public class TNAutoJoin : MonoBehaviour
{
    public static TNAutoJoin instance;

    public string serverAddress = "127.0.0.1";

    public int serverPort = 5127;

    public string firstLevel = "Example 1";

    public int channelID = 1;

    public string disconnectLevel;

    public bool allowUDP = true;

    public bool connectOnStart = true;

    public string successFunctionName;

    public string failureFunctionName;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        if (connectOnStart)
        {
            Connect();
        }
    }

    public void Connect()
    {
        Screen.sleepTimeout = -1;
        TNManager.Connect(serverAddress, serverPort);
    }

    private void OnNetworkConnect(bool result, string message)
    {
        if (result)
        {
            if (allowUDP)
            {
                TNManager.StartUDP(Random.Range(10000, 50000));
            }
            TNManager.JoinChannel(channelID, firstLevel);
        }
        else if (!string.IsNullOrEmpty(failureFunctionName))
        {
            UnityTools.Broadcast(failureFunctionName, message);
        }
        else
        {
            Debug.LogError(message);
        }
    }

    private void OnNetworkDisconnect()
    {
        if (!string.IsNullOrEmpty(disconnectLevel) && Application.loadedLevelName != disconnectLevel)
        {
            Application.LoadLevel(disconnectLevel);
        }
    }

    private void OnNetworkJoinChannel(bool result, string message)
    {
        if (result)
        {
            if (!string.IsNullOrEmpty(successFunctionName))
            {
                UnityTools.Broadcast(successFunctionName);
            }
            return;
        }
        if (!string.IsNullOrEmpty(failureFunctionName))
        {
            UnityTools.Broadcast(failureFunctionName, message);
        }
        else
        {
            Debug.LogError(message);
        }
        TNManager.Disconnect();
    }
}
