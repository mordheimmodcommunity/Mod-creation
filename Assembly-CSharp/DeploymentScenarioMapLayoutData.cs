using Mono.Data.Sqlite;

public class DeploymentScenarioMapLayoutData : DataCore
{
	public DeploymentScenarioMapLayoutId Id
	{
		get;
		private set;
	}

	public string Name
	{
		get;
		private set;
	}

	public bool Skirmish
	{
		get;
		private set;
	}

	public bool Procedural
	{
		get;
		private set;
	}

	public bool Ambush
	{
		get;
		private set;
	}

	public DeploymentScenarioId DeploymentScenarioId
	{
		get;
		private set;
	}

	public MissionMapId MissionMapId
	{
		get;
		private set;
	}

	public MissionMapLayoutId MissionMapLayoutId
	{
		get;
		private set;
	}

	public PropRestrictionId PropRestrictionIdProps
	{
		get;
		private set;
	}

	public PropRestrictionId PropRestrictionIdMadstuff
	{
		get;
		private set;
	}

	public PropRestrictionId PropRestrictionIdBarricade
	{
		get;
		private set;
	}

	public string PropsLayer
	{
		get;
		private set;
	}

	public string DeploymentLayer
	{
		get;
		private set;
	}

	public string TrapsLayer
	{
		get;
		private set;
	}

	public string SearchLayer
	{
		get;
		private set;
	}

	public string ExtraLightsFxLayer
	{
		get;
		private set;
	}

	public int TrapCount
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = (DeploymentScenarioMapLayoutId)reader.GetInt32(0);
		Name = reader.GetString(1);
		Skirmish = (reader.GetInt32(2) != 0);
		Procedural = (reader.GetInt32(3) != 0);
		Ambush = (reader.GetInt32(4) != 0);
		DeploymentScenarioId = (DeploymentScenarioId)reader.GetInt32(5);
		MissionMapId = (MissionMapId)reader.GetInt32(6);
		MissionMapLayoutId = (MissionMapLayoutId)reader.GetInt32(7);
		PropRestrictionIdProps = (PropRestrictionId)reader.GetInt32(8);
		PropRestrictionIdMadstuff = (PropRestrictionId)reader.GetInt32(9);
		PropRestrictionIdBarricade = (PropRestrictionId)reader.GetInt32(10);
		PropsLayer = reader.GetString(11);
		DeploymentLayer = reader.GetString(12);
		TrapsLayer = reader.GetString(13);
		SearchLayer = reader.GetString(14);
		ExtraLightsFxLayer = reader.GetString(15);
		TrapCount = reader.GetInt32(16);
	}
}
