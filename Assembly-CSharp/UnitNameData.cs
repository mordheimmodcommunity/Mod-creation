using Mono.Data.Sqlite;

public class UnitNameData : DataCore
{
	public int Id
	{
		get;
		private set;
	}

	public string TheName
	{
		get;
		private set;
	}

	public bool Surname
	{
		get;
		private set;
	}

	public WarbandId WarbandId
	{
		get;
		private set;
	}

	public UnitId UnitId
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = reader.GetInt32(0);
		TheName = reader.GetString(1);
		Surname = (reader.GetInt32(2) != 0);
		WarbandId = (WarbandId)reader.GetInt32(3);
		UnitId = (UnitId)reader.GetInt32(4);
	}
}
