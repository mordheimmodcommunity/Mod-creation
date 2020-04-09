using Mono.Data.Sqlite;

public class EffectTypeData : DataCore
{
	public EffectTypeId Id
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
		Id = (EffectTypeId)reader.GetInt32(0);
		Name = reader.GetString(1);
	}
}
