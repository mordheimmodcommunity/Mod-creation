using Mono.Data.Sqlite;

public class BodyPartColorData : DataCore
{
	public int Id
	{
		get;
		private set;
	}

	public UnitId UnitId
	{
		get;
		private set;
	}

	public ColorPresetId ColorPresetId
	{
		get;
		private set;
	}

	public BodyPartId BodyPartId
	{
		get;
		private set;
	}

	public ItemTypeId ItemTypeId
	{
		get;
		private set;
	}

	public string Color
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = reader.GetInt32(0);
		UnitId = (UnitId)reader.GetInt32(1);
		ColorPresetId = (ColorPresetId)reader.GetInt32(2);
		BodyPartId = (BodyPartId)reader.GetInt32(3);
		ItemTypeId = (ItemTypeId)reader.GetInt32(4);
		Color = reader.GetString(5);
	}
}
