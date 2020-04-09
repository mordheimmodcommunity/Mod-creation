using Mono.Data.Sqlite;

public class WyrdstoneShipmentData : DataCore
{
	public WyrdstoneShipmentId Id
	{
		get;
		private set;
	}

	public string Name
	{
		get;
		private set;
	}

	public WarbandRankId WarbandRankId
	{
		get;
		private set;
	}

	public int MinWeight
	{
		get;
		private set;
	}

	public int MaxWeight
	{
		get;
		private set;
	}

	public int MinDays
	{
		get;
		private set;
	}

	public int MaxDays
	{
		get;
		private set;
	}

	public int NextMinDays
	{
		get;
		private set;
	}

	public int NextMaxDays
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = (WyrdstoneShipmentId)reader.GetInt32(0);
		Name = reader.GetString(1);
		WarbandRankId = (WarbandRankId)reader.GetInt32(2);
		MinWeight = reader.GetInt32(3);
		MaxWeight = reader.GetInt32(4);
		MinDays = reader.GetInt32(5);
		MaxDays = reader.GetInt32(6);
		NextMinDays = reader.GetInt32(7);
		NextMaxDays = reader.GetInt32(8);
	}
}
