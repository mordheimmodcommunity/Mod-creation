using System.Collections.Generic;

public class DeploymentScenarioMapLayoutIdComparer : IEqualityComparer<DeploymentScenarioMapLayoutId>
{
    public static readonly DeploymentScenarioMapLayoutIdComparer Instance = new DeploymentScenarioMapLayoutIdComparer();

    public bool Equals(DeploymentScenarioMapLayoutId x, DeploymentScenarioMapLayoutId y)
    {
        return x == y;
    }

    public int GetHashCode(DeploymentScenarioMapLayoutId obj)
    {
        return (int)obj;
    }
}
