using Mono.Data.Sqlite;

public class BodyPartMaterialData : DataCore
{
	public BodyPartMaterialId Id
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
		Id = (BodyPartMaterialId)reader.GetInt32(0);
		Name = reader.GetString(1);
	}
}
