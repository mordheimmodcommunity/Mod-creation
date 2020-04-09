using UnityEngine.UI;

public class NewCampaignLoadingView : LoadingView
{
	public Text titleText;

	public Text descriptionText;

	public Image flag;

	public override void Show()
	{
		base.Show();
		WarbandSave currentSave = PandoraSingleton<GameManager>.Instance.currentSave;
		string str = ((WarbandId)currentSave.id).ToString();
		titleText.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("com_wb_type_" + str));
		descriptionText.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("main_camp_intro_" + str));
		flag.set_sprite(Warband.GetIcon((WarbandId)currentSave.id));
		LoadBackground("bg_warband_" + str);
		LoadDialog("main_camp_intro_" + str);
	}
}
