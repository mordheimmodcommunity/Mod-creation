using Mono.Data.Sqlite;

public class CampaignUnitJoinEnchantmentData : DataCore
{
	public CampaignUnitId CampaignUnitId
	{
		get;
		private set;
	}

	public EnchantmentId EnchantmentId
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		CampaignUnitId = (CampaignUnitId)reader.GetInt32(0);
		EnchantmentId = (EnchantmentId)reader.GetInt32(1);
	}
}
