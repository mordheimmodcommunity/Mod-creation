using Mono.Data.Sqlite;

public class WarbandSlotTypeData : DataCore
{
	public WarbandSlotTypeId Id
	{
		get;
		private set;
	}

	public string Name
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = (WarbandSlotTypeId)reader.GetInt32(0);
		Name = reader.GetString(1);
	}
}
