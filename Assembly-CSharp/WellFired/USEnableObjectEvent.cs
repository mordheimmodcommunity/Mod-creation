namespace WellFired
{
    [USequencerEventHideDuration]
    [USequencerEvent("Object/Toggle Object")]
    [USequencerFriendlyName("Toggle Object")]
    public class USEnableObjectEvent : USEventBase
    {
        public bool enable;

        private bool prevEnable;

        public USEnableObjectEvent()
            : this()
        {
        }

        public override void FireEvent()
        {
            prevEnable = ((USEventBase)this).get_AffectedObject().activeSelf;
            ((USEventBase)this).get_AffectedObject().SetActive(enable);
        }

        public override void ProcessEvent(float deltaTime)
        {
        }

        public override void StopEvent()
        {
            UndoEvent();
        }

        public override void UndoEvent()
        {
            if ((bool)((USEventBase)this).get_AffectedObject())
            {
                ((USEventBase)this).get_AffectedObject().SetActive(prevEnable);
            }
        }
    }
}
