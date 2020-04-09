using System.Collections.Generic;

public class ZoneTriggerIdComparer : IEqualityComparer<ZoneTriggerId>
{
	public static readonly ZoneTriggerIdComparer Instance = new ZoneTriggerIdComparer();

	public bool Equals(ZoneTriggerId x, ZoneTriggerId y)
	{
		return x == y;
	}

	public int GetHashCode(ZoneTriggerId obj)
	{
		return (int)obj;
	}
}
