using System;

public class ContinuePopupView : ConfirmationPopupView
{
    public ButtonGroup abandonButton;

    protected Action<bool> btnCallback;

    public override void Init()
    {
        base.Init();
        if (abandonButton != null)
        {
            abandonButton.SetAction(string.Empty, "abandon", 1);
            abandonButton.OnAction(Abandon, mouseOnly: false);
        }
    }

    public void Show(string titleId, string textId, Action<bool> callback)
    {
        base.Show(titleId, textId, null);
        btnCallback = callback;
    }

    public override void Confirm()
    {
        base.Confirm();
        if (btnCallback != null)
        {
            btnCallback(obj: true);
        }
    }

    public void Abandon()
    {
        base.Confirm();
        if (btnCallback != null)
        {
            btnCallback(obj: false);
        }
    }
}
