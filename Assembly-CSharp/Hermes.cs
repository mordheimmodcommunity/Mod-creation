using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Hermes : PandoraSingleton<Hermes>, IMyrtilus
{
    public enum SendTarget
    {
        NONE,
        HOST,
        ALL,
        OTHERS
    }

    private enum CommandList
    {
        ERROR,
        LOAD_LEVEL,
        CONNECTION,
        SET_PLAYER_INDEX,
        KEEP_ALIVE,
        CHAT,
        COUNT
    }

    public delegate void OnConnectedCallback();

    private const float DEFAULT_TIMEOUT = 20f;

    private const float SEND_WAIT = 5f;

    private int levelPrefix;

    public OnConnectedCallback connectedCallback;

    private bool doNotDisconnect;

    private List<IMyrtilus> myrtilii = new List<IMyrtilus>();

    private List<KeyValuePair<ulong, float>> connections = new List<KeyValuePair<ulong, float>>();

    private uint guid = 1u;

    private float timeout = 20f;

    private float lastSent;

    private float timer;

    public int PlayerIndex
    {
        get;
        private set;
    }

    public bool DoNotDisconnectMode
    {
        get
        {
            return doNotDisconnect;
        }
        set
        {
            if (!doNotDisconnect && value)
            {
                for (int i = 0; i < connections.Count; i++)
                {
                    KeyValuePair<ulong, float> keyValuePair = connections[i];
                    connections[i] = new KeyValuePair<ulong, float>(keyValuePair.Key, Time.realtimeSinceStartup);
                }
            }
            doNotDisconnect = value;
        }
    }

    public uint uid
    {
        get;
        set;
    }

    public uint owner
    {
        get;
        set;
    }

    public void ResetGUID()
    {
        guid = 1u;
    }

    public uint GetNextGUID()
    {
        return guid++;
    }

    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        PandoraSingleton<Hephaestus>.Instance.SetDataReceivedCallback(OnDataReceivedCallback);
        StopConnections();
        connections = new List<KeyValuePair<ulong, float>>();
        myrtilii = new List<IMyrtilus>();
        RegisterToHermes();
        uid = 4294967294u;
    }

    private void OnDestroy()
    {
        StopConnections();
    }

    private void LateUpdate()
    {
        timer += Time.deltaTime;
        if (!(timer >= 1f))
        {
            return;
        }
        timer = 0f;
        if (connections.Count > 0 && DoNotDisconnectMode)
        {
            Send(false, SendTarget.OTHERS, uid, 4u);
        }
        else
        {
            if (connections.Count <= 0 || !(Time.realtimeSinceStartup - lastSent > 5f))
            {
                return;
            }
            if (!DoNotDisconnectMode)
            {
                for (int i = 0; i < connections.Count; i++)
                {
                    float num = Time.realtimeSinceStartup - connections[i].Value;
                    if (num > 20f)
                    {
                        PandoraDebug.LogDebug("SendKeepAlive... Haven't recieved anything in a while", "HERMES", this);
                        ConnectionError(connections[i].Key, string.Empty);
                        return;
                    }
                }
            }
            Send(false, SendTarget.OTHERS, uid, 4u);
        }
    }

    public void SetTimeout(float time)
    {
        timeout = time;
    }

    public void StartHosting()
    {
        PlayerIndex = 0;
    }

    public bool IsConnected()
    {
        return connections.Count > 0;
    }

    public bool IsHost()
    {
        return PlayerIndex == 0;
    }

    public void StopConnections(bool resetPlayerIndex = true)
    {
        for (int i = 0; i < connections.Count; i++)
        {
            PandoraSingleton<Hephaestus>.instance.DisconnectFromUser(connections[i].Key);
        }
        if (PandoraSingleton<MissionManager>.Exists())
        {
            PandoraSingleton<Hephaestus>.instance.ResetNetwork();
        }
        connections = new List<KeyValuePair<ulong, float>>();
        if (resetPlayerIndex)
        {
            PlayerIndex = 0;
        }
    }

    public void ResetPlayerIndex()
    {
        PlayerIndex = 0;
    }

    public void RegisterMyrtilus(IMyrtilus myrtilus, bool needUID = true)
    {
        if (needUID)
        {
            myrtilus.uid = GetNextGUID();
        }
        PandoraDebug.LogDebug("RegisterMyrtilus ID = " + myrtilus.uid, "HERMES", this);
        myrtilii.Add(myrtilus);
    }

    public void RemoveMyrtilus(IMyrtilus myrtilus)
    {
        myrtilii.Remove(myrtilus);
    }

    public void OnDataReceivedCallback(ulong from, byte[] data)
    {
        uint num = 0u;
        uint num2 = 0u;
        for (int i = 0; i < connections.Count; i++)
        {
            if (connections[i].Key == from)
            {
                connections[i] = new KeyValuePair<ulong, float>(from, Time.realtimeSinceStartup);
            }
        }
        using (MemoryStream input = new MemoryStream(data))
        {
            using (BinaryReader binaryReader = new BinaryReader(input))
            {
                num = binaryReader.ReadUInt32();
                num2 = binaryReader.ReadUInt32();
                int num3 = 0;
                while (true)
                {
                    if (num3 >= myrtilii.Count)
                    {
                        return;
                    }
                    if (myrtilii[num3].uid == num)
                    {
                        break;
                    }
                    num3++;
                }
                myrtilii[num3].Receive(from, num2, binaryReader.ReadArray());
            }
        }
    }

    public void NewConnection(ulong userId)
    {
        PandoraDebug.LogDebug("NewConnection this user has connected to me = " + userId, "HERMES", this);
        connections.Add(new KeyValuePair<ulong, float>(userId, Time.realtimeSinceStartup));
        PandoraDebug.LogInfo("Send PlayerIndex ID " + connections.Count, "HERMES", this);
        Send(true, SendTarget.OTHERS, uid, 3u, connections.Count);
    }

    public void RemoveConnection(ulong userId)
    {
        PandoraDebug.LogDebug("RemoveConnection user has left me = " + userId, "HERMES", this);
        int i;
        for (i = 0; i < connections.Count && connections[i].Key != userId; i++)
        {
        }
        if (i < connections.Count)
        {
            connections.RemoveAt(i);
        }
        PandoraSingleton<Hephaestus>.instance.DisconnectFromUser(userId);
    }

    public void ConnectionError(ulong id, string error)
    {
        PandoraDebug.LogDebug("ConnectionError " + id + " error: " + error, "HERMES", this);
        StopConnections(resetPlayerIndex: false);
        PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.HERMES_CONNECTION_LOST);
    }

    public void SendLoadLevel(string nextSceneName, SceneLoadingTypeId loadingType, float transitionDuration, bool waitForAction = false, bool waitForPlayers = false, bool force = false)
    {
        PandoraDebug.LogInfo("SendLoadLevel  " + nextSceneName + "duration = " + transitionDuration + " waitforAction = " + waitForAction + " waitForPlayers = " + waitForPlayers, "HERMES", this);
        Send(true, SendTarget.ALL, uid, 1u, nextSceneName, (int)loadingType, transitionDuration, waitForAction, waitForPlayers, force);
    }

    private void LoadLevelRPC(string nextSceneName, int loadingType, float transitionDuration, bool waitForAction, bool waitForPlayers, bool force)
    {
        PandoraDebug.LogInfo("LoadLevelRPC  " + nextSceneName + "duration = " + transitionDuration + " waitforAction = " + waitForAction + " waitforPlayers = " + waitForPlayers, "HERMES", this);
        PandoraSingleton<TransitionManager>.Instance.LoadNextScene(nextSceneName, (SceneLoadingTypeId)loadingType, transitionDuration, waitForAction, waitForPlayers, force);
    }

    public void SetPlayerIndex(ulong host, int index)
    {
        PandoraDebug.LogInfo("CommandList.SET_PLAYER_INDEX index = " + index, "HERMES", this);
        connections.Add(new KeyValuePair<ulong, float>(host, Time.realtimeSinceStartup));
        PlayerIndex = index;
        if (connectedCallback != null)
        {
            connectedCallback();
            connectedCallback = null;
        }
    }

    private void KeepAliveRPC(ulong user)
    {
    }

    public void SendChat(string message)
    {
        if (PandoraSingleton<Hephaestus>.Instance.IsPrivilegeRestricted(Hephaestus.RestrictionId.CHAT))
        {
            PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.HERMES_CHAT, PandoraSingleton<Hephaestus>.Instance.GetUserId(), PandoraSingleton<LocalizationManager>.Instance.GetStringById("chat_restricted"), v3: true);
        }
        else
        {
            Send(true, SendTarget.ALL, uid, 5u, message);
        }
    }

    private void ChatRPC(ulong user, string message)
    {
        if (!PandoraSingleton<Hephaestus>.Instance.IsPrivilegeRestricted(Hephaestus.RestrictionId.CHAT))
        {
            if (user == 0L)
            {
                PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.HERMES_CHAT, user, message, v3: false);
            }
            else
            {
                PandoraSingleton<Hephaestus>.Instance.CanReceiveMessages(delegate (bool allowed)
                {
                    if (allowed)
                    {
                        PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.HERMES_CHAT, user, message, v3: false);
                    }
                });
            }
        }
    }

    private void OnNetworkError(string message)
    {
        PandoraDebug.LogInfo("Network Error = " + message, "HERMES", this);
    }

    private void OnNetworkConnect(bool success, string message)
    {
        if (!success)
        {
            PandoraDebug.LogWarning("This CLIENT has connected to a server success = " + success + " Message = " + message, "HERMES", this);
        }
        else
        {
            PandoraDebug.LogInfo("This CLIENT has connected to a server success = " + success + " Message = " + message, "HERMES", this);
        }
    }

    private void OnNetworkDisconnect()
    {
        PandoraDebug.LogInfo("This CLIENT has Disconnected from the server.", "HERMES", this);
    }

    private void OnNetworkJoinChannel(bool success, string message)
    {
        PandoraDebug.LogInfo("This CLIENT has connected to a server channel success = " + success + " ms = " + message, "HERMES", this);
        if (connectedCallback != null)
        {
            connectedCallback();
            connectedCallback = null;
        }
    }

    private void OnNetworkLeaveChannel()
    {
        PandoraDebug.LogInfo("This CLIENT has disconnected from the channel", "HERMES", this);
    }

    public void RegisterToHermes()
    {
        RegisterMyrtilus(this);
    }

    public void RemoveFromHermes()
    {
        RemoveMyrtilus(this);
    }

    public void Send(bool reliable, SendTarget target, uint id, uint command, params object[] parms)
    {
        byte[] data = null;
        using (MemoryStream memoryStream = new MemoryStream())
        {
            using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
            {
                binaryWriter.Write(id);
                binaryWriter.Write(command);
                binaryWriter.WriteArray(parms);
                data = memoryStream.ToArray();
            }
        }
        switch (target)
        {
            case SendTarget.ALL:
                {
                    lastSent = Time.realtimeSinceStartup;
                    for (int j = 0; j < connections.Count; j++)
                    {
                        PandoraSingleton<Hephaestus>.Instance.SendData(reliable, connections[j].Key, data);
                    }
                    OnDataReceivedCallback(0uL, data);
                    break;
                }
            case SendTarget.HOST:
                if (IsHost())
                {
                    OnDataReceivedCallback(0uL, data);
                }
                else
                {
                    PandoraSingleton<Hephaestus>.Instance.SendData(reliable, connections[0].Key, data);
                }
                break;
            case SendTarget.OTHERS:
                {
                    lastSent = Time.realtimeSinceStartup;
                    for (int i = 0; i < connections.Count; i++)
                    {
                        PandoraSingleton<Hephaestus>.Instance.SendData(reliable, connections[i].Key, data);
                    }
                    break;
                }
        }
    }

    public void Receive(ulong from, uint command, object[] parms)
    {
        switch (command)
        {
            case 2u:
                break;
            case 1u:
                {
                    string nextSceneName = (string)parms[0];
                    int loadingType = (int)parms[1];
                    float transitionDuration = (float)parms[2];
                    bool waitForAction = (bool)parms[3];
                    bool waitForPlayers = (bool)parms[4];
                    bool force = (bool)parms[5];
                    LoadLevelRPC(nextSceneName, loadingType, transitionDuration, waitForAction, waitForPlayers, force);
                    break;
                }
            case 3u:
                {
                    int index = (int)parms[0];
                    SetPlayerIndex(from, index);
                    break;
                }
            case 4u:
                KeepAliveRPC(from);
                break;
            case 5u:
                {
                    string message = (string)parms[0];
                    ChatRPC(from, message);
                    break;
                }
        }
    }
}
