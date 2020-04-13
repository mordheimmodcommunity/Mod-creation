using Mono.Data.Sqlite;

public class DeploymentScenarioData : DataCore
{
    public DeploymentScenarioId Id
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
        Id = (DeploymentScenarioId)reader.GetInt32(0);
        Name = reader.GetString(1);
    }
}
