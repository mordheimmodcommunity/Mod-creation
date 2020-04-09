using Mono.Data.Sqlite;

public class AssetBundleData : DataCore
{
	public AssetBundleId Id
	{
		get;
		private set;
	}

	public string Name
	{
		get;
		private set;
	}

	public string Mask
	{
		get;
		private set;
	}

	public string Format
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = (AssetBundleId)reader.GetInt32(0);
		Name = reader.GetString(1);
		Mask = reader.GetString(2);
		Format = reader.GetString(3);
	}
}
