internal class UIMissionCounter : UIMissionTarget
{
    public UIMissionCounter(UIMissionManager uiMissionManager)
        : base(uiMissionManager)
    {
    }

    public override void Enter(int iFrom)
    {
        base.Enter(iFrom);
    }

    public override void Exit(int iTo)
    {
        base.Exit(iTo);
        base.UiMissionManager.leftSequenceMessage.OnDisable();
    }
}
