using Mono.Data.Sqlite;

public class AttributeData : DataCore
{
	public AttributeId Id
	{
		get;
		private set;
	}

	public string Name
	{
		get;
		private set;
	}

	public int BaseRoll
	{
		get;
		private set;
	}

	public AttributeTypeId AttributeTypeId
	{
		get;
		private set;
	}

	public int Rating
	{
		get;
		private set;
	}

	public bool Persistent
	{
		get;
		private set;
	}

	public bool IsPercent
	{
		get;
		private set;
	}

	public AttributeId AttributeIdMax
	{
		get;
		private set;
	}

	public AttributeId AttributeIdModifier
	{
		get;
		private set;
	}

	public bool IsBaseRoll
	{
		get;
		private set;
	}

	public bool CheckAchievement
	{
		get;
		private set;
	}

	public bool Save
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = (AttributeId)reader.GetInt32(0);
		Name = reader.GetString(1);
		BaseRoll = reader.GetInt32(2);
		AttributeTypeId = (AttributeTypeId)reader.GetInt32(3);
		Rating = reader.GetInt32(4);
		Persistent = (reader.GetInt32(5) != 0);
		IsPercent = (reader.GetInt32(6) != 0);
		AttributeIdMax = (AttributeId)reader.GetInt32(7);
		AttributeIdModifier = (AttributeId)reader.GetInt32(8);
		IsBaseRoll = (reader.GetInt32(9) != 0);
		CheckAchievement = (reader.GetInt32(10) != 0);
		Save = (reader.GetInt32(11) != 0);
	}
}
