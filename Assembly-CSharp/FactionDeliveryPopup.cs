using UnityEngine.UI;

public class FactionDeliveryPopup : UIPopupModule
{
    public Text totalWeight;

    public Text totalGold;

    public Text totalReputation;

    public void Set(FactionDelivery delivery)
    {
        totalWeight.set_text(delivery.TotalWeight.ToString());
        totalGold.set_text(delivery.TotalGold.ToString());
        totalReputation.set_text(delivery.TotalWeight.ToString());
    }
}
