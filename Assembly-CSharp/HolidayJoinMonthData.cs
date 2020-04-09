using Mono.Data.Sqlite;

public class HolidayJoinMonthData : DataCore
{
	public HolidayId HolidayId
	{
		get;
		private set;
	}

	public MonthId MonthId
	{
		get;
		private set;
	}

	public int Day
	{
		get;
		private set;
	}

	public bool Intercalary
	{
		get;
		private set;
	}

	public MoonId MoonId
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		HolidayId = (HolidayId)reader.GetInt32(0);
		MonthId = (MonthId)reader.GetInt32(1);
		Day = reader.GetInt32(2);
		Intercalary = (reader.GetInt32(3) != 0);
		MoonId = (MoonId)reader.GetInt32(4);
	}
}
