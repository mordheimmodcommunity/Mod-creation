using Mono.Data.Sqlite;

public class ShrineData : DataCore
{
	public ShrineId Id
	{
		get;
		private set;
	}

	public string Name
	{
		get;
		private set;
	}

	public AllegianceId AllegianceId
	{
		get;
		private set;
	}

	public WarbandId WarbandId
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = (ShrineId)reader.GetInt32(0);
		Name = reader.GetString(1);
		AllegianceId = (AllegianceId)reader.GetInt32(2);
		WarbandId = (WarbandId)reader.GetInt32(3);
	}
}
