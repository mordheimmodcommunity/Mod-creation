public class UIDeployControls : CanvasGroupDisabler
{
	public ImageGroup btnPrev;

	public ImageGroup btnNext;

	public ImageGroup btnDeploy;

	public void Start()
	{
		btnPrev.SetAction("h", string.Empty, 0, negative: true);
		btnNext.SetAction("h", string.Empty);
		btnDeploy.SetAction("action", string.Empty);
	}
}
