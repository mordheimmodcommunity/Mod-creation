using Mono.Data.Sqlite;

public class CombatStyleData : DataCore
{
	public CombatStyleId Id
	{
		get;
		private set;
	}

	public string Name
	{
		get;
		private set;
	}

	public ItemTypeId ItemTypeIdMain
	{
		get;
		private set;
	}

	public UnitSlotId UnitSlotIdMain
	{
		get;
		private set;
	}

	public ItemTypeId ItemTypeIdOff
	{
		get;
		private set;
	}

	public UnitSlotId UnitSlotIdOff
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = (CombatStyleId)reader.GetInt32(0);
		Name = reader.GetString(1);
		ItemTypeIdMain = (ItemTypeId)reader.GetInt32(2);
		UnitSlotIdMain = (UnitSlotId)reader.GetInt32(3);
		ItemTypeIdOff = (ItemTypeId)reader.GetInt32(4);
		UnitSlotIdOff = (UnitSlotId)reader.GetInt32(5);
	}
}
