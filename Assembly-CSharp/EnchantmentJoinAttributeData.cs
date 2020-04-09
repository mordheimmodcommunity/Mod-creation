using Mono.Data.Sqlite;

public class EnchantmentJoinAttributeData : DataCore
{
	public EnchantmentId EnchantmentId
	{
		get;
		private set;
	}

	public AttributeId AttributeId
	{
		get;
		private set;
	}

	public int Modifier
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		EnchantmentId = (EnchantmentId)reader.GetInt32(0);
		AttributeId = (AttributeId)reader.GetInt32(1);
		Modifier = reader.GetInt32(2);
	}
}
