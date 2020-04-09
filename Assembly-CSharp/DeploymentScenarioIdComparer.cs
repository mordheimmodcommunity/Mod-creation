using System.Collections.Generic;

public class DeploymentScenarioIdComparer : IEqualityComparer<DeploymentScenarioId>
{
	public static readonly DeploymentScenarioIdComparer Instance = new DeploymentScenarioIdComparer();

	public bool Equals(DeploymentScenarioId x, DeploymentScenarioId y)
	{
		return x == y;
	}

	public int GetHashCode(DeploymentScenarioId obj)
	{
		return (int)obj;
	}
}
