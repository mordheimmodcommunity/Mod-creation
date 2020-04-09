using Mono.Data.Sqlite;

public class InjuryJoinAttributeData : DataCore
{
	public InjuryId InjuryId
	{
		get;
		private set;
	}

	public AttributeId AttributeId
	{
		get;
		private set;
	}

	public int Modifier
	{
		get;
		private set;
	}

	public int Limit
	{
		get;
		private set;
	}

	public bool Retire
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		InjuryId = (InjuryId)reader.GetInt32(0);
		AttributeId = (AttributeId)reader.GetInt32(1);
		Modifier = reader.GetInt32(2);
		Limit = reader.GetInt32(3);
		Retire = (reader.GetInt32(4) != 0);
	}
}
