using Mono.Data.Sqlite;

public class BodyPartUnitExcludedData : DataCore
{
	public int Id
	{
		get;
		private set;
	}

	public UnitId UnitId
	{
		get;
		private set;
	}

	public BodyPartId BodyPartId
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = reader.GetInt32(0);
		UnitId = (UnitId)reader.GetInt32(1);
		BodyPartId = (BodyPartId)reader.GetInt32(2);
	}
}
