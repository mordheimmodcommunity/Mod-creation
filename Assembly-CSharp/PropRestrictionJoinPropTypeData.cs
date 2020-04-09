using Mono.Data.Sqlite;

public class PropRestrictionJoinPropTypeData : DataCore
{
	public PropRestrictionId PropRestrictionId
	{
		get;
		private set;
	}

	public PropTypeId PropTypeId
	{
		get;
		private set;
	}

	public int MaxProp
	{
		get;
		private set;
	}

	public int MaxPercentage
	{
		get;
		private set;
	}

	public int MinDistance
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		PropRestrictionId = (PropRestrictionId)reader.GetInt32(0);
		PropTypeId = (PropTypeId)reader.GetInt32(1);
		MaxProp = reader.GetInt32(2);
		MaxPercentage = reader.GetInt32(3);
		MinDistance = reader.GetInt32(4);
	}
}
