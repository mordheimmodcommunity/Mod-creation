using UnityEngine.UI;

public class SendShipmentRequestPopup : UIPopupModule
{
	public Text weightRequirement;

	public Text goldReward;

	public Text totalWeight;

	public Text overweightGold;

	public Text overweightReputation;

	public void Set(FactionRequest request)
	{
		totalWeight.set_text(request.totalWeight.get_text());
		weightRequirement.set_text(request.weightRequirement.get_text());
		goldReward.set_text(request.goldReward.get_text());
		overweightGold.set_text(request.overweightGold.get_text());
		overweightReputation.set_text(request.overweightReputation.get_text());
	}
}
