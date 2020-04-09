using Mono.Data.Sqlite;

public class ItemCategoryData : DataCore
{
	public ItemCategoryId Id
	{
		get;
		private set;
	}

	public string Name
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = (ItemCategoryId)reader.GetInt32(0);
		Name = reader.GetString(1);
	}
}
