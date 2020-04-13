using System.Collections.Generic;

public class DeploymentIdComparer : IEqualityComparer<DeploymentId>
{
    public static readonly DeploymentIdComparer Instance = new DeploymentIdComparer();

    public bool Equals(DeploymentId x, DeploymentId y)
    {
        return x == y;
    }

    public int GetHashCode(DeploymentId obj)
    {
        return (int)obj;
    }
}
