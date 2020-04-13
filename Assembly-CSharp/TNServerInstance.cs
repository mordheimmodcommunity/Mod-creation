using System;
using System.Net;
using TNet;
using UnityEngine;

[AddComponentMenu("TNet/Network Server (internal)")]
public class TNServerInstance : MonoBehaviour
{
    public enum Type
    {
        Lan,
        Udp,
        Tcp
    }

    public enum State
    {
        Inactive,
        Starting,
        Active
    }

    private static TNServerInstance mInstance;

    private GameServer mGame = new GameServer();

    private LobbyServer mLobby;

    private UPnP mUp = new UPnP();

    private static TNServerInstance instance
    {
        get
        {
            if (mInstance == null)
            {
                GameObject gameObject = new GameObject("_Server");
                mInstance = gameObject.AddComponent<TNServerInstance>();
                UnityEngine.Object.DontDestroyOnLoad(gameObject);
            }
            return mInstance;
        }
    }

    public static bool isActive => mInstance != null && mInstance.mGame.isActive;

    public static bool isListening => mInstance != null && mInstance.mGame.isListening;

    public static int listeningPort => (mInstance != null) ? mInstance.mGame.tcpPort : 0;

    public static string serverName
    {
        get
        {
            return (!(mInstance != null)) ? null : mInstance.mGame.name;
        }
        set
        {
            if (instance != null)
            {
                mInstance.mGame.name = value;
            }
        }
    }

    public static int playerCount => (mInstance != null) ? mInstance.mGame.playerCount : 0;

    public static GameServer game => (!(mInstance != null)) ? null : mInstance.mGame;

    public static LobbyServer lobby => (!(mInstance != null)) ? null : mInstance.mLobby;

    public static bool Start(int tcpPort)
    {
        return instance.StartLocal(tcpPort, 0, null, 0, Type.Udp);
    }

    public static bool Start(int tcpPort, int udpPort)
    {
        return instance.StartLocal(tcpPort, udpPort, null, 0, Type.Udp);
    }

    public static bool Start(int tcpPort, int udpPort, string fileName)
    {
        return instance.StartLocal(tcpPort, udpPort, fileName, 0, Type.Udp);
    }

    [Obsolete("Use TNServerInstance.Start(tcpPort, udpPort, lobbyPort, fileName) instead")]
    public static bool Start(int tcpPort, int udpPort, string fileName, int lanBroadcastPort)
    {
        return instance.StartLocal(tcpPort, udpPort, fileName, lanBroadcastPort, Type.Udp);
    }

    public static bool Start(int tcpPort, int udpPort, int lobbyPort, string fileName)
    {
        return instance.StartLocal(tcpPort, udpPort, fileName, lobbyPort, Type.Udp);
    }

    public static bool Start(int tcpPort, int udpPort, int lobbyPort, string fileName, Type type)
    {
        return instance.StartLocal(tcpPort, udpPort, fileName, lobbyPort, type);
    }

    public static bool Start(int tcpPort, int udpPort, string fileName, Type type, IPEndPoint remoteLobby)
    {
        return instance.StartRemote(tcpPort, udpPort, fileName, remoteLobby, type);
    }

    private bool StartLocal(int tcpPort, int udpPort, string fileName, int lobbyPort, Type type)
    {
        if (mGame.isActive)
        {
            Disconnect();
        }
        if (lobbyPort > 0)
        {
            if (type == Type.Tcp)
            {
                mLobby = new TcpLobbyServer();
            }
            else
            {
                mLobby = new UdpLobbyServer();
            }
            if (!mLobby.Start(lobbyPort))
            {
                mLobby = null;
                return false;
            }
            if (type == Type.Tcp)
            {
                mUp.OpenTCP(lobbyPort);
            }
            else
            {
                mUp.OpenUDP(lobbyPort);
            }
            mGame.lobbyLink = new LobbyServerLink(mLobby);
        }
        if (mGame.Start(tcpPort, udpPort))
        {
            mUp.OpenTCP(tcpPort);
            mUp.OpenUDP(udpPort);
            if (!string.IsNullOrEmpty(fileName))
            {
                mGame.LoadFrom(fileName);
            }
            return true;
        }
        Disconnect();
        return false;
    }

    private bool StartRemote(int tcpPort, int udpPort, string fileName, IPEndPoint remoteLobby, Type type)
    {
        if (mGame.isActive)
        {
            Disconnect();
        }
        if (remoteLobby != null && remoteLobby.Port > 0)
        {
            switch (type)
            {
                case Type.Tcp:
                    mLobby = new TcpLobbyServer();
                    mGame.lobbyLink = new TcpLobbyServerLink(remoteLobby);
                    break;
                case Type.Udp:
                    mLobby = new UdpLobbyServer();
                    mGame.lobbyLink = new UdpLobbyServerLink(remoteLobby);
                    break;
                default:
                    Debug.LogWarning("The remote lobby server type must be either UDP or TCP, not LAN");
                    break;
            }
        }
        if (mGame.Start(tcpPort, udpPort))
        {
            mUp.OpenTCP(tcpPort);
            mUp.OpenUDP(udpPort);
            if (!string.IsNullOrEmpty(fileName))
            {
                mGame.LoadFrom(fileName);
            }
            return true;
        }
        Disconnect();
        return false;
    }

    public static void Stop()
    {
        if (mInstance != null)
        {
            mInstance.Disconnect();
        }
    }

    public static void Stop(string fileName)
    {
        if (mInstance != null && mInstance.mGame.isActive)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                mInstance.mGame.SaveTo(fileName);
            }
            Stop();
        }
    }

    public static void SaveTo(string fileName)
    {
        if (mInstance != null && mInstance.mGame.isActive && !string.IsNullOrEmpty(fileName))
        {
            mInstance.mGame.SaveTo(fileName);
        }
    }

    public static void MakePrivate()
    {
        if (mInstance != null)
        {
            mInstance.mGame.MakePrivate();
        }
    }

    private void Disconnect()
    {
        mGame.Stop();
        if (mLobby != null)
        {
            mLobby.Stop();
            mLobby = null;
        }
        mUp.Close();
    }

    private void OnDestroy()
    {
        Disconnect();
        mUp.WaitForThreads();
    }
}
