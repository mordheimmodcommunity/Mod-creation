using Mono.Data.Sqlite;

public class ItemJoinBodyPartData : DataCore
{
	public ItemId ItemId
	{
		get;
		private set;
	}

	public BodyPartId BodyPartId
	{
		get;
		private set;
	}

	public bool Lock
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		ItemId = (ItemId)reader.GetInt32(0);
		BodyPartId = (BodyPartId)reader.GetInt32(1);
		Lock = (reader.GetInt32(2) != 0);
	}
}
