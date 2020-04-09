using Mono.Data.Sqlite;

public class WarbandDefaultUnitData : DataCore
{
	public int Id
	{
		get;
		private set;
	}

	public WarbandId WarbandId
	{
		get;
		private set;
	}

	public UnitId UnitId
	{
		get;
		private set;
	}

	public WarbandSlotTypeId WarbandSlotTypeId
	{
		get;
		private set;
	}

	public int Amount
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = reader.GetInt32(0);
		WarbandId = (WarbandId)reader.GetInt32(1);
		UnitId = (UnitId)reader.GetInt32(2);
		WarbandSlotTypeId = (WarbandSlotTypeId)reader.GetInt32(3);
		Amount = reader.GetInt32(4);
	}
}
