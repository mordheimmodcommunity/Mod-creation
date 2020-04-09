using Mono.Data.Sqlite;

public class MissionMapData : DataCore
{
	public MissionMapId Id
	{
		get;
		private set;
	}

	public string Name
	{
		get;
		private set;
	}

	public DistrictId DistrictId
	{
		get;
		private set;
	}

	public int Idx
	{
		get;
		private set;
	}

	public int LoadingImageCount
	{
		get;
		private set;
	}

	public bool HasRecastHelper
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = (MissionMapId)reader.GetInt32(0);
		Name = reader.GetString(1);
		DistrictId = (DistrictId)reader.GetInt32(2);
		Idx = reader.GetInt32(3);
		LoadingImageCount = reader.GetInt32(4);
		HasRecastHelper = (reader.GetInt32(5) != 0);
	}
}
