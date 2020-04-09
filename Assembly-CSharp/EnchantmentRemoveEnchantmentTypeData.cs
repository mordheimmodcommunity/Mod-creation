using Mono.Data.Sqlite;

public class EnchantmentRemoveEnchantmentTypeData : DataCore
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

	public EnchantmentTypeId EnchantmentTypeId
	{
		get;
		private set;
	}

	public int Count
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = reader.GetInt32(0);
		EnchantmentId = (EnchantmentId)reader.GetInt32(1);
		EnchantmentTypeId = (EnchantmentTypeId)reader.GetInt32(2);
		Count = reader.GetInt32(3);
	}
}
