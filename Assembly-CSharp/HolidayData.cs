using Mono.Data.Sqlite;

public class HolidayData : DataCore
{
	public HolidayId Id
	{
		get;
		private set;
	}

	public string Name
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = (HolidayId)reader.GetInt32(0);
		Name = reader.GetString(1);
	}
}
