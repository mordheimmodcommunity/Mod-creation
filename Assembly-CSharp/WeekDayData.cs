using Mono.Data.Sqlite;

public class WeekDayData : DataCore
{
	public WeekDayId Id
	{
		get;
		private set;
	}

	public string Name
	{
		get;
		private set;
	}

	public bool RefreshMarket
	{
		get;
		private set;
	}

	public bool RefreshOutsiders
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = (WeekDayId)reader.GetInt32(0);
		Name = reader.GetString(1);
		RefreshMarket = (reader.GetInt32(2) != 0);
		RefreshOutsiders = (reader.GetInt32(3) != 0);
	}
}
