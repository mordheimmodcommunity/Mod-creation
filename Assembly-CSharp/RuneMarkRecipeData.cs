using Mono.Data.Sqlite;

public class RuneMarkRecipeData : DataCore
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

	public RuneMarkId RuneMarkId
	{
		get;
		private set;
	}

	public RuneMarkQualityId RuneMarkQualityId
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = reader.GetInt32(0);
		ItemId = (ItemId)reader.GetInt32(1);
		RuneMarkId = (RuneMarkId)reader.GetInt32(2);
		RuneMarkQualityId = (RuneMarkQualityId)reader.GetInt32(3);
	}
}
