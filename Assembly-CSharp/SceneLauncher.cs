public class SceneLauncher
{
	private static SceneLauncher instance;

	public static SceneLauncher Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new SceneLauncher();
			}
			return instance;
		}
	}

	public void LaunchScene(SceneLoadingId id, bool waitForPlayers = false, bool force = false)
	{
		SceneLoadingData sceneLoadingData = PandoraSingleton<DataFactory>.Instance.InitData<SceneLoadingData>((int)id);
		PandoraSingleton<Hermes>.Instance.SendLoadLevel(sceneLoadingData.NextScene, sceneLoadingData.SceneLoadingTypeId, (float)sceneLoadingData.TransitionDuration, sceneLoadingData.WaitAction, waitForPlayers, force);
	}
}
