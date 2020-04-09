using Mono.Data.Sqlite;

public class BoneData : DataCore
{
	public BoneId Id
	{
		get;
		private set;
	}

	public string Name
	{
		get;
		private set;
	}

	public BoneId BoneIdMirror
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
		Id = (BoneId)reader.GetInt32(0);
		Name = reader.GetString(1);
		BoneIdMirror = (BoneId)reader.GetInt32(2);
		IsRange = (reader.GetInt32(3) != 0);
	}
}
