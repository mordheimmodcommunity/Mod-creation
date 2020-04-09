using Mono.Data.Sqlite;

public class UnitTypeData : DataCore
{
	public UnitTypeId Id
	{
		get;
		private set;
	}

	public string Name
	{
		get;
		private set;
	}

	public int StartSp
	{
		get;
		private set;
	}

	public int MaxSp
	{
		get;
		private set;
	}

	public int StartOp
	{
		get;
		private set;
	}

	public int MaxOp
	{
		get;
		private set;
	}

	public int InitiativeBonus
	{
		get;
		private set;
	}

	public int Rating
	{
		get;
		private set;
	}

	public int MoralImpact
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = (UnitTypeId)reader.GetInt32(0);
		Name = reader.GetString(1);
		StartSp = reader.GetInt32(2);
		MaxSp = reader.GetInt32(3);
		StartOp = reader.GetInt32(4);
		MaxOp = reader.GetInt32(5);
		InitiativeBonus = reader.GetInt32(6);
		Rating = reader.GetInt32(7);
		MoralImpact = reader.GetInt32(8);
	}
}
