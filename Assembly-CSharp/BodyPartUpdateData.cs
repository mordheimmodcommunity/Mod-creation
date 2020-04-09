using Mono.Data.Sqlite;

public class BodyPartUpdateData : DataCore
{
	public int Id
	{
		get;
		private set;
	}

	public BodyPartId BodyPartId
	{
		get;
		private set;
	}

	public BodyPartId BodyPartIdUpdated
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = reader.GetInt32(0);
		BodyPartId = (BodyPartId)reader.GetInt32(1);
		BodyPartIdUpdated = (BodyPartId)reader.GetInt32(2);
	}
}
