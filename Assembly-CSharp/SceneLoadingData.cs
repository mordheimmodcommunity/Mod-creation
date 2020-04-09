using Mono.Data.Sqlite;

public class SceneLoadingData : DataCore
{
	public SceneLoadingId Id
	{
		get;
		private set;
	}

	public string Name
	{
		get;
		private set;
	}

	public string LoadScene
	{
		get;
		private set;
	}

	public SceneLoadingTypeId SceneLoadingTypeId
	{
		get;
		private set;
	}

	public string NextScene
	{
		get;
		private set;
	}

	public string TransitionName
	{
		get;
		private set;
	}

	public double TransitionDuration
	{
		get;
		private set;
	}

	public bool WaitAction
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = (SceneLoadingId)reader.GetInt32(0);
		Name = reader.GetString(1);
		LoadScene = reader.GetString(2);
		SceneLoadingTypeId = (SceneLoadingTypeId)reader.GetInt32(3);
		NextScene = reader.GetString(4);
		TransitionName = reader.GetString(5);
		TransitionDuration = reader.GetDouble(6);
		WaitAction = (reader.GetInt32(7) != 0);
	}
}
