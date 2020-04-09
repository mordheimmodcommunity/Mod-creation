using Mono.Data.Sqlite;

public class UnitRankProgressionData : DataCore
{
	public int Id
	{
		get;
		private set;
	}

	public int Rank
	{
		get;
		private set;
	}

	public UnitTypeId UnitTypeId
	{
		get;
		private set;
	}

	public int Xp
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = reader.GetInt32(0);
		Rank = reader.GetInt32(1);
		UnitTypeId = (UnitTypeId)reader.GetInt32(2);
		Xp = reader.GetInt32(3);
	}
}
