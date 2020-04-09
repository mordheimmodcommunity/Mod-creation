using Mono.Data.Sqlite;

public class ItemAssetData : DataCore
{
	public int Id
	{
		get;
		private set;
	}

	public ItemId ItemId
	{
		get;
		private set;
	}

	public RaceId RaceId
	{
		get;
		private set;
	}

	public WarbandId WarbandId
	{
		get;
		private set;
	}

	public UnitId UnitId
	{
		get;
		private set;
	}

	public string Asset
	{
		get;
		private set;
	}

	public bool NoTrail
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = reader.GetInt32(0);
		ItemId = (ItemId)reader.GetInt32(1);
		RaceId = (RaceId)reader.GetInt32(2);
		WarbandId = (WarbandId)reader.GetInt32(3);
		UnitId = (UnitId)reader.GetInt32(4);
		Asset = reader.GetString(5);
		NoTrail = (reader.GetInt32(6) != 0);
	}
}
