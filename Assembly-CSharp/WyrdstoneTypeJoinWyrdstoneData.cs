using Mono.Data.Sqlite;

public class WyrdstoneTypeJoinWyrdstoneData : DataCore
{
	public WyrdstoneTypeId WyrdstoneTypeId
	{
		get;
		private set;
	}

	public WyrdstoneId WyrdstoneId
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		WyrdstoneTypeId = (WyrdstoneTypeId)reader.GetInt32(0);
		WyrdstoneId = (WyrdstoneId)reader.GetInt32(1);
	}
}
