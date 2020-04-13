using System.Collections.Generic;

public class ProjectileIdComparer : IEqualityComparer<ProjectileId>
{
    public static readonly ProjectileIdComparer Instance = new ProjectileIdComparer();

    public bool Equals(ProjectileId x, ProjectileId y)
    {
        return x == y;
    }

    public int GetHashCode(ProjectileId obj)
    {
        return (int)obj;
    }
}
