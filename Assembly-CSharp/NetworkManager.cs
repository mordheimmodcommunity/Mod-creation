public class NetworkManager : IMyrtilus
{
    private enum CommandList
    {
        NONE,
        LOADING_DONE,
        READY_TO_START,
        TURN_READY,
        QUIT_MISSION,
        FORFEIT,
        COUNT
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

    public NetworkManager()
    {
        RegisterToHermes();
        uid = 4294967292u;
    }

    public void Remove()
    {
        RemoveFromHermes();
    }

    public void SendLoadingDone(int warbandIndex)
    {
        PandoraDebug.LogDebug("SendLoadingDone WarbandIndex = " + warbandIndex, "HERMES");
        Send(true, Hermes.SendTarget.ALL, uid, 1u, warbandIndex);
    }

    private void LoadingDoneRPC(int warbandIndex)
    {
        PandoraDebug.LogDebug("SendLoadingDoneRPC  warbandIndex = " + warbandIndex, "HERMES");
        PandoraSingleton<MissionManager>.Instance.SetLoadingDone();
    }

    public void SendReadyToStart()
    {
        PandoraDebug.LogDebug("SendReadyToStart", "HERMES");
        Send(true, Hermes.SendTarget.OTHERS, uid, 2u);
    }

    private void ReadyToStartRPC()
    {
        PandoraDebug.LogDebug("ReadyToStartRPC", "HERMES");
        PandoraSingleton<TransitionManager>.Instance.OnPlayersReady();
    }

    public void SendTurnReady()
    {
        PandoraDebug.LogDebug("SendTurnReady", "HERMES");
        Send(true, Hermes.SendTarget.ALL, uid, 3u);
    }

    private void TurnReadyRPC()
    {
        PandoraDebug.LogDebug("TurnReadyRPC", "HERMES");
        PandoraSingleton<MissionManager>.Instance.SetTurnReady();
    }

    public void SendForfeitMission(int warbandIdx)
    {
        PandoraDebug.LogDebug("SendForfeitMission", "HERMES");
        Send(true, Hermes.SendTarget.ALL, uid, 5u, warbandIdx);
    }

    private void ForfeitMissionRPC(int warbandIdx)
    {
        PandoraDebug.LogDebug("ForfeitMissionRPC", "HERMES");
        PandoraSingleton<MissionManager>.Instance.ForfeitMission(warbandIdx);
    }

    public void SendQuitMission()
    {
        PandoraDebug.LogDebug("SendQuitMission", "HERMES");
        Send(true, Hermes.SendTarget.ALL, uid, 4u);
    }

    private void QuitMissionRPC()
    {
        PandoraDebug.LogDebug("QuitMissionRPC", "HERMES");
        PandoraSingleton<MissionManager>.Instance.ForceQuitMission();
    }

    public void RegisterToHermes()
    {
        PandoraSingleton<Hermes>.Instance.RegisterMyrtilus(this);
    }

    public void RemoveFromHermes()
    {
        PandoraSingleton<Hermes>.Instance.RemoveMyrtilus(this);
    }

    public void Send(bool reliable, Hermes.SendTarget target, uint id, uint command, params object[] parms)
    {
        PandoraSingleton<Hermes>.Instance.Send(reliable, target, id, command, parms);
    }

    public void Receive(ulong from, uint command, object[] parms)
    {
        switch (command)
        {
            case 1u:
                {
                    int warbandIndex = (int)parms[0];
                    LoadingDoneRPC(warbandIndex);
                    break;
                }
            case 2u:
                ReadyToStartRPC();
                break;
            case 3u:
                TurnReadyRPC();
                break;
            case 4u:
                QuitMissionRPC();
                break;
            case 5u:
                {
                    int warbandIdx = (int)parms[0];
                    ForfeitMissionRPC(warbandIdx);
                    break;
                }
        }
    }
}
