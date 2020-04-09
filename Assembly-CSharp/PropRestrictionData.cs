using Mono.Data.Sqlite;

public class PropRestrictionData : DataCore
{
	public PropRestrictionId Id
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
		Id = (PropRestrictionId)reader.GetInt32(0);
		Name = reader.GetString(1);
	}
}
