using Mono.Data.Sqlite;

public class DeploymentData : DataCore
{
	public DeploymentId Id
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
		Id = (DeploymentId)reader.GetInt32(0);
		Name = reader.GetString(1);
	}
}
