using Mono.Data.Sqlite;

public class UnitWoundData : DataCore
{
	public UnitWoundId Id
	{
		get;
		private set;
	}

	public string Name
	{
		get;
		private set;
	}

	public int BaseWound
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = (UnitWoundId)reader.GetInt32(0);
		Name = reader.GetString(1);
		BaseWound = reader.GetInt32(2);
	}
}
