using Mono.Data.Sqlite;

public class MarketRefillData : DataCore
{
	public int Id
	{
		get;
		private set;
	}

	public WarbandRankId WarbandRankId
	{
		get;
		private set;
	}

	public ItemCategoryId ItemCategoryId
	{
		get;
		private set;
	}

	public int QuantityMin
	{
		get;
		private set;
	}

	public int QuantityMax
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = reader.GetInt32(0);
		WarbandRankId = (WarbandRankId)reader.GetInt32(1);
		ItemCategoryId = (ItemCategoryId)reader.GetInt32(2);
		QuantityMin = reader.GetInt32(3);
		QuantityMax = reader.GetInt32(4);
	}
}
