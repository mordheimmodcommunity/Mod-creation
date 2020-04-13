using UnityEngine;
using UnityEngine.UI;

public class FactionOverviewCard : MonoBehaviour
{
    private const string MODIFIER_FORMAT = "({0}%)";

    public Text factionType;

    public Text factionName;

    public Text daysLeft;

    public Text weightReqText;

    public Image factionIcon;

    public Text reputation;

    public Text progression;

    public Text reputationAdjustment;

    public Slider progressBar;

    public GameObject rewardsSection;

    public GameObject weightReqSection;

    public Text fragPrice;

    public Text fragPriceAdjust;

    public Text fragRep;

    public Text shardPrice;

    public Text shardPriceAdjust;

    public Text shardRep;

    public Text clusterPrice;

    public Text clusterPriceAdjust;

    public Text clusterRep;

    public FactionMenuController FactionCtrlr
    {
        get;
        set;
    }

    public void SetFaction(FactionMenuController faction)
    {
        FactionCtrlr = faction;
        factionName.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById(FactionCtrlr.Faction.Data.Desc + "_name"));
        if (FactionCtrlr.Faction.Primary)
        {
            factionIcon.set_sprite(Warband.GetIcon(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.Id));
            factionType.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_faction_primary"));
            if (FactionCtrlr.HasShipment)
            {
                rewardsSection.SetActive(value: false);
                weightReqSection.SetActive(value: true);
                daysLeft.set_text((FactionCtrlr.ShipmentDate - PandoraSingleton<HideoutManager>.Instance.Date.CurrentDate).ToString());
                weightReqText.set_text(FactionCtrlr.ShipmentWeight.ToString());
            }
            else
            {
                rewardsSection.SetActive(value: true);
                weightReqSection.SetActive(value: false);
            }
            ((Component)(object)daysLeft).gameObject.SetActive(FactionCtrlr.HasShipment);
        }
        else
        {
            Sprite sprite = PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("warband/" + FactionCtrlr.Faction.Data.Name, cached: true);
            if (sprite != null)
            {
                PandoraDebug.LogWarning("Cannot load faction icon : warband/" + FactionCtrlr.Faction.Data.Name);
            }
            factionIcon.set_sprite(sprite);
            factionType.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_faction_secondary"));
            rewardsSection.SetActive(value: true);
            weightReqSection.SetActive(value: false);
        }
        ((Behaviour)(object)progressBar).enabled = true;
        reputation.set_text(FactionCtrlr.Faction.Save.rank.ToString());
        if (FactionCtrlr.NextRankReputation == -1)
        {
            progression.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_max_rank"));
            progressBar.set_normalizedValue(1f);
            ((Component)(object)reputationAdjustment).gameObject.SetActive(value: false);
        }
        else
        {
            progression.set_text($"{FactionCtrlr.Faction.Reputation}/{FactionCtrlr.NextRankReputation}");
            progressBar.set_normalizedValue((float)FactionCtrlr.Faction.Reputation / (float)FactionCtrlr.NextRankReputation);
            bool flag = FactionCtrlr.NextRankReputationModifier != 0;
            if (flag)
            {
                reputationAdjustment.set_text(string.Format("({0}%)", Mathf.FloorToInt(100f / (float)FactionCtrlr.NextRankReputationModifier).ToString("+#;-#")));
            }
            ((Component)(object)reputationAdjustment).gameObject.SetActive(flag);
        }
        ((Behaviour)(object)progressBar).enabled = false;
        int num = FactionCtrlr.FragmentPrice;
        int num2 = FactionCtrlr.ShardPrice;
        int num3 = FactionCtrlr.ClusterPrice;
        bool flag2 = FactionCtrlr.PriceModifier != 0;
        if (flag2)
        {
            string text = string.Format("({0}%)", Mathf.FloorToInt(100f / (float)FactionCtrlr.PriceModifier).ToString("+#;-#"));
            fragPriceAdjust.set_text(text);
            shardPriceAdjust.set_text(text);
            clusterPriceAdjust.set_text(text);
            num += FactionCtrlr.FragmentPrice / FactionCtrlr.PriceModifier;
            num2 += FactionCtrlr.ShardPrice / FactionCtrlr.PriceModifier;
            num3 += FactionCtrlr.ClusterPrice / FactionCtrlr.PriceModifier;
        }
        fragPrice.set_text(num.ToString());
        shardPrice.set_text(num2.ToString());
        clusterPrice.set_text(num3.ToString());
        ((Component)(object)fragPriceAdjust).gameObject.SetActive(flag2);
        ((Component)(object)shardPriceAdjust).gameObject.SetActive(flag2);
        ((Component)(object)clusterPriceAdjust).gameObject.SetActive(flag2);
        fragRep.set_text(((float)FactionCtrlr.FragmentPrice * Constant.GetFloat(ConstantId.WYRDSTONE_WEIGHT)).ToString());
        shardRep.set_text(((float)FactionCtrlr.ShardPrice * Constant.GetFloat(ConstantId.WYRDSTONE_WEIGHT)).ToString());
        clusterRep.set_text(((float)FactionCtrlr.ClusterPrice * Constant.GetFloat(ConstantId.WYRDSTONE_WEIGHT)).ToString());
    }
}
