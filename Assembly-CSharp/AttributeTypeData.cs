using Mono.Data.Sqlite;

public class AttributeTypeData : DataCore
{
	public AttributeTypeId Id
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
		Id = (AttributeTypeId)reader.GetInt32(0);
		Name = reader.GetString(1);
	}
}
