using Mono.Data.Sqlite;

public class ShrineEnchantmentData : DataCore
{
	public int Id
	{
		get;
		private set;
	}

	public ShrineId ShrineId
	{
		get;
		private set;
	}

	public EnchantmentId EnchantmentId
	{
		get;
		private set;
	}

	public WarbandRankId WarbandRankId
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = reader.GetInt32(0);
		ShrineId = (ShrineId)reader.GetInt32(1);
		EnchantmentId = (EnchantmentId)reader.GetInt32(2);
		WarbandRankId = (WarbandRankId)reader.GetInt32(3);
	}
}
