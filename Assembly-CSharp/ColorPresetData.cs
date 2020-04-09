using Mono.Data.Sqlite;

public class ColorPresetData : DataCore
{
	public ColorPresetId Id
	{
		get;
		private set;
	}

	public string Name
	{
		get;
		private set;
	}

	public WarbandId WarbandId
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = (ColorPresetId)reader.GetInt32(0);
		Name = reader.GetString(1);
		WarbandId = (WarbandId)reader.GetInt32(2);
	}
}
