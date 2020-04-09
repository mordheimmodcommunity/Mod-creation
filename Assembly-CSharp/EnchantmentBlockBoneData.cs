using Mono.Data.Sqlite;

public class EnchantmentBlockBoneData : DataCore
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

	public BoneId BoneId
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = reader.GetInt32(0);
		EnchantmentId = (EnchantmentId)reader.GetInt32(1);
		BoneId = (BoneId)reader.GetInt32(2);
	}
}
