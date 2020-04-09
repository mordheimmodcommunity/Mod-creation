using Mono.Data.Sqlite;

public class WarbandRankJoinUnitTypeData : DataCore
{
	public WarbandRankId WarbandRankId
	{
		get;
		private set;
	}

	public UnitTypeId UnitTypeId
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		WarbandRankId = (WarbandRankId)reader.GetInt32(0);
		UnitTypeId = (UnitTypeId)reader.GetInt32(1);
	}
}
