using Mono.Data.Sqlite;

public class DestructibleData : DataCore
{
	public DestructibleId Id
	{
		get;
		private set;
	}

	public string Name
	{
		get;
		private set;
	}

	public int Wounds
	{
		get;
		private set;
	}

	public ZoneAoeId ZoneAoeId
	{
		get;
		private set;
	}

	public double ZoneAoeRadius
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = (DestructibleId)reader.GetInt32(0);
		Name = reader.GetString(1);
		Wounds = reader.GetInt32(2);
		ZoneAoeId = (ZoneAoeId)reader.GetInt32(3);
		ZoneAoeRadius = reader.GetDouble(4);
	}
}
