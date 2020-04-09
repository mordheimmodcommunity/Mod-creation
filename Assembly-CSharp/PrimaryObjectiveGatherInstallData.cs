using Mono.Data.Sqlite;

public class PrimaryObjectiveGatherInstallData : DataCore
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

	public bool CheckUnits
	{
		get;
		private set;
	}

	public bool CheckWagon
	{
		get;
		private set;
	}

	public string CheckSearch
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
		CheckUnits = (reader.GetInt32(4) != 0);
		CheckWagon = (reader.GetInt32(5) != 0);
		CheckSearch = reader.GetString(6);
	}
}
