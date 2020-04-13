public class QuitGamePopup : ConfirmationPopupView
{
    public override void Init()
    {
        base.Init();
    }

    private void Update()
    {
        if (isShow && PandoraSingleton<PandoraInput>.Instance.GetKeyUp("menu", 1))
        {
            Hide();
        }
    }
}
