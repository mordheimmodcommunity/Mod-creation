using Mono.Data.Sqlite;

public class AssetBundleFolderData : DataCore
{
	public int Id
	{
		get;
		private set;
	}

	public AssetBundleId AssetBundleId
	{
		get;
		private set;
	}

	public string Folder
	{
		get;
		private set;
	}

	public AssetExtensionId AssetExtensionId
	{
		get;
		private set;
	}

	public bool IncludeSubFolders
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = reader.GetInt32(0);
		AssetBundleId = (AssetBundleId)reader.GetInt32(1);
		Folder = reader.GetString(2);
		AssetExtensionId = (AssetExtensionId)reader.GetInt32(3);
		IncludeSubFolders = (reader.GetInt32(4) != 0);
	}
}
