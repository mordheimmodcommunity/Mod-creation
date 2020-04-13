using UnityEngine;

public class NextUnitModule : UIModule
{
    public ButtonGroup previous;

    public ButtonGroup next;

    public Sprite prevSprite;

    public Sprite nextSprite;

    public void Setup()
    {
        previous.SetAction("switch_unit", "controls_action_prev_unit", 0, negative: true, prevSprite);
        previous.OnAction(NextUnit, mouseOnly: false);
        next.SetAction("switch_unit", "controls_action_next_unit", 0, negative: false, nextSprite);
        next.OnAction(PrevUnit, mouseOnly: false);
    }

    private void PrevUnit()
    {
        PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.NEXT_UNIT, v1: false);
    }

    private void NextUnit()
    {
        PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.NEXT_UNIT, v1: true);
    }
}
