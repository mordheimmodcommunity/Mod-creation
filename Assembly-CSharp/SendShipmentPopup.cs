using UnityEngine.UI;

public class SendShipmentPopup : UIPopupModule
{
	public Text nbFrag;

	public Text nbShard;

	public Text nbCluster;

	public Text totalGold;

	public Text totalRep;

	public void Set(FactionDelivery request)
	{
		nbFrag.set_text(request.FragmentCount.ToString());
		nbShard.set_text(request.ShardCount.ToString());
		nbCluster.set_text(request.ClusterCount.ToString());
		totalGold.set_text(request.totalGold.get_text());
		totalRep.set_text(request.totalRep.get_text());
	}
}
