using Mono.Data.Sqlite;

public class MissionMapLayoutData : DataCore
{
	public MissionMapLayoutId Id
	{
		get;
		private set;
	}

	public string Name
	{
		get;
		private set;
	}

	public MissionMapId MissionMapId
	{
		get;
		private set;
	}

	public string CloudsName
	{
		get;
		private set;
	}

	public string LightsName
	{
		get;
		private set;
	}

	public string FxName
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = (MissionMapLayoutId)reader.GetInt32(0);
		Name = reader.GetString(1);
		MissionMapId = (MissionMapId)reader.GetInt32(2);
		CloudsName = reader.GetString(3);
		LightsName = reader.GetString(4);
		FxName = reader.GetString(5);
	}
}
