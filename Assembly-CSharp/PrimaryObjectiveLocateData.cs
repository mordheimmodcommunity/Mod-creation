using Mono.Data.Sqlite;

public class PrimaryObjectiveLocateData : DataCore
{
	public int Id
	{
		get;
		private set;
	}

	public PrimaryObjectiveId PrimaryObjectiveId
	{
		get;
		private set;
	}

	public ItemId ItemId
	{
		get;
		private set;
	}

	public int ItemCount
	{
		get;
		private set;
	}

	public string Zone
	{
		get;
		private set;
	}

	public int ZoneCount
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = reader.GetInt32(0);
		PrimaryObjectiveId = (PrimaryObjectiveId)reader.GetInt32(1);
		ItemId = (ItemId)reader.GetInt32(2);
		ItemCount = reader.GetInt32(3);
		Zone = reader.GetString(4);
		ZoneCount = reader.GetInt32(5);
	}
}
