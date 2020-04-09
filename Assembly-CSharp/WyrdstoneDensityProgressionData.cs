using Mono.Data.Sqlite;

public class WyrdstoneDensityProgressionData : DataCore
{
	public WyrdstoneDensityId WyrdstoneDensityId
	{
		get;
		private set;
	}

	public int WarbandRank
	{
		get;
		private set;
	}

	public int Fragment
	{
		get;
		private set;
	}

	public int Shard
	{
		get;
		private set;
	}

	public int Cluster
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		WyrdstoneDensityId = (WyrdstoneDensityId)reader.GetInt32(0);
		WarbandRank = reader.GetInt32(1);
		Fragment = reader.GetInt32(2);
		Shard = reader.GetInt32(3);
		Cluster = reader.GetInt32(4);
	}
}
