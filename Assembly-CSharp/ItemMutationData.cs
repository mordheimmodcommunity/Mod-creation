using Mono.Data.Sqlite;

public class ItemMutationData : DataCore
{
	public int Id
	{
		get;
		private set;
	}

	public ItemId ItemId
	{
		get;
		private set;
	}

	public MutationId MutationId
	{
		get;
		private set;
	}

	public int DamageMin
	{
		get;
		private set;
	}

	public int DamageMax
	{
		get;
		private set;
	}

	public ItemSpeedId ItemSpeedId
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = reader.GetInt32(0);
		ItemId = (ItemId)reader.GetInt32(1);
		MutationId = (MutationId)reader.GetInt32(2);
		DamageMin = reader.GetInt32(3);
		DamageMax = reader.GetInt32(4);
		ItemSpeedId = (ItemSpeedId)reader.GetInt32(5);
	}
}
