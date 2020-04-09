using Mono.Data.Sqlite;

public class ProcWarbandRankData : DataCore
{
	public ProcWarbandRankId Id
	{
		get;
		private set;
	}

	public string Name
	{
		get;
		private set;
	}

	public int Rating
	{
		get;
		private set;
	}

	public int WarbandRank
	{
		get;
		private set;
	}

	public WarbandRankId WarbandRankId
	{
		get;
		private set;
	}

	public int MinUnit
	{
		get;
		private set;
	}

	public int MaxUnit
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = (ProcWarbandRankId)reader.GetInt32(0);
		Name = reader.GetString(1);
		Rating = reader.GetInt32(2);
		WarbandRank = reader.GetInt32(3);
		WarbandRankId = (WarbandRankId)reader.GetInt32(4);
		MinUnit = reader.GetInt32(5);
		MaxUnit = reader.GetInt32(6);
	}
}
