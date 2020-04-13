using Mono.Data.Sqlite;

public class DeploymentScenarioSlotData : DataCore
{
    public DeploymentScenarioSlotId Id
    {
        get;
        private set;
    }

    public string Name
    {
        get;
        private set;
    }

    public DeploymentScenarioId DeploymentScenarioId
    {
        get;
        private set;
    }

    public DeploymentId DeploymentId
    {
        get;
        private set;
    }

    public string Title
    {
        get;
        private set;
    }

    public string Setup
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = (DeploymentScenarioSlotId)reader.GetInt32(0);
        Name = reader.GetString(1);
        DeploymentScenarioId = (DeploymentScenarioId)reader.GetInt32(2);
        DeploymentId = (DeploymentId)reader.GetInt32(3);
        Title = reader.GetString(4);
        Setup = reader.GetString(5);
    }
}
