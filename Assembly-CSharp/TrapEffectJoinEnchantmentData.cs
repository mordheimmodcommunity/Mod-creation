using Mono.Data.Sqlite;

public class TrapEffectJoinEnchantmentData : DataCore
{
	public TrapEffectId TrapEffectId
	{
		get;
		private set;
	}

	public EnchantmentId EnchantmentId
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		TrapEffectId = (TrapEffectId)reader.GetInt32(0);
		EnchantmentId = (EnchantmentId)reader.GetInt32(1);
	}
}
