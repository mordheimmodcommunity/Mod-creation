using System.Collections.Generic;

public class DeploymentScenarioSlotIdComparer : IEqualityComparer<DeploymentScenarioSlotId>
{
	public static readonly DeploymentScenarioSlotIdComparer Instance = new DeploymentScenarioSlotIdComparer();

	public bool Equals(DeploymentScenarioSlotId x, DeploymentScenarioSlotId y)
	{
		return x == y;
	}

	public int GetHashCode(DeploymentScenarioSlotId obj)
	{
		return (int)obj;
	}
}
