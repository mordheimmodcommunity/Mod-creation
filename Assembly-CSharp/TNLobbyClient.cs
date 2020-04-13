using TNet;
using UnityEngine;

public abstract class TNLobbyClient : MonoBehaviour
{
    public delegate void OnListChange();

    public static string errorString = string.Empty;

    public static ServerList knownServers = new ServerList();

    public static OnListChange onChange;

    public static bool isActive = false;

    public string remoteAddress;

    public int remotePort = 5129;

    protected virtual void OnDisable()
    {
        errorString = string.Empty;
        knownServers.Clear();
    }
}
