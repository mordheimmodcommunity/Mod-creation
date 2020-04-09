using Mono.Data.Sqlite;

public class WarbandEnchantmentWyrdstoneDensityModifierData : DataCore
{
	public int Id
	{
		get;
		private set;
	}

	public WarbandEnchantmentId WarbandEnchantmentId
	{
		get;
		private set;
	}

	public ProcMissionRatingId ProcMissionRatingId
	{
		get;
		private set;
	}

	public WyrdstoneDensityId WyrdstoneDensityId
	{
		get;
		private set;
	}

	public int Modifier
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = reader.GetInt32(0);
		WarbandEnchantmentId = (WarbandEnchantmentId)reader.GetInt32(1);
		ProcMissionRatingId = (ProcMissionRatingId)reader.GetInt32(2);
		WyrdstoneDensityId = (WyrdstoneDensityId)reader.GetInt32(3);
		Modifier = reader.GetInt32(4);
	}
}
