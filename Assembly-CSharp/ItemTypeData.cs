using Mono.Data.Sqlite;

public class ItemTypeData : DataCore
{
	public ItemTypeId Id
	{
		get;
		private set;
	}

	public string Name
	{
		get;
		private set;
	}

	public ItemCategoryId ItemCategoryId
	{
		get;
		private set;
	}

	public bool IsTwoHanded
	{
		get;
		private set;
	}

	public bool IsRange
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = (ItemTypeId)reader.GetInt32(0);
		Name = reader.GetString(1);
		ItemCategoryId = (ItemCategoryId)reader.GetInt32(2);
		IsTwoHanded = (reader.GetInt32(3) != 0);
		IsRange = (reader.GetInt32(4) != 0);
	}
}
