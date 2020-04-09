using Mono.Data.Sqlite;

public class WarbandAttributeData : DataCore
{
	public WarbandAttributeId Id
	{
		get;
		private set;
	}

	public string Name
	{
		get;
		private set;
	}

	public int BaseValue
	{
		get;
		private set;
	}

	public bool Persistent
	{
		get;
		private set;
	}

	public bool CheckAchievement
	{
		get;
		private set;
	}

	public AttributeId AttributeId
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = (WarbandAttributeId)reader.GetInt32(0);
		Name = reader.GetString(1);
		BaseValue = reader.GetInt32(2);
		Persistent = (reader.GetInt32(3) != 0);
		CheckAchievement = (reader.GetInt32(4) != 0);
		AttributeId = (AttributeId)reader.GetInt32(5);
	}
}
