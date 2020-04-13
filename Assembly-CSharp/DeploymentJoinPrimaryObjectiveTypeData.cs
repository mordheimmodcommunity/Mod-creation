using Mono.Data.Sqlite;

public class DeploymentJoinPrimaryObjectiveTypeData : DataCore
{
    public DeploymentId DeploymentId
    {
        get;
        private set;
    }

    public PrimaryObjectiveTypeId PrimaryObjectiveTypeId
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        DeploymentId = (DeploymentId)reader.GetInt32(0);
        PrimaryObjectiveTypeId = (PrimaryObjectiveTypeId)reader.GetInt32(1);
    }
}
