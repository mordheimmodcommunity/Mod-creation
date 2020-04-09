using Mono.Data.Sqlite;

public class EnchantmentCostModifierData : DataCore
{
	public int Id
	{
		get;
		private set;
	}

	public EnchantmentId EnchantmentId
	{
		get;
		private set;
	}

	public UnitActionId UnitActionId
	{
		get;
		private set;
	}

	public SkillId SkillId
	{
		get;
		private set;
	}

	public SpellTypeId SpellTypeId
	{
		get;
		private set;
	}

	public int StrategyPoints
	{
		get;
		private set;
	}

	public int OffensePoints
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = reader.GetInt32(0);
		EnchantmentId = (EnchantmentId)reader.GetInt32(1);
		UnitActionId = (UnitActionId)reader.GetInt32(2);
		SkillId = (SkillId)reader.GetInt32(3);
		SpellTypeId = (SpellTypeId)reader.GetInt32(4);
		StrategyPoints = reader.GetInt32(5);
		OffensePoints = reader.GetInt32(6);
	}
}
