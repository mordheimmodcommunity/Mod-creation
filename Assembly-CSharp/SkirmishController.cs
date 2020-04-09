using UnityEngine;

public class SkirmishController : MonoBehaviour
{
	private bool quitMenu;

	private void Update()
	{
		if (!quitMenu)
		{
			if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("start"))
			{
				PandoraDebug.LogDebug("Main Menu Start Game Pressed", "FLOW", this);
				SceneLauncher.Instance.LaunchScene(SceneLoadingId.LAUNCH_MULTI);
			}
			else if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("quit"))
			{
				PandoraDebug.LogDebug("Main Menu Quit Game Pressed", "FLOW", this);
				PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.MENU_QUIT_GAME);
				quitMenu = true;
			}
		}
		else if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("quit") || PandoraSingleton<PandoraInput>.Instance.GetKeyUp("cancel"))
		{
			PandoraDebug.LogDebug("Main Menu Quit Game CANCEL", "FLOW", this);
			PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.MENU_QUIT_ACTION, v1: false);
			quitMenu = false;
		}
		else if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("start"))
		{
			PandoraDebug.LogDebug("Main Menu Quit Game ACCEPT", "FLOW", this);
			PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.MENU_QUIT_ACTION, v1: true);
		}
	}
}
