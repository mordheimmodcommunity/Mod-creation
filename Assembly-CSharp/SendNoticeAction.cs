using UnityEngine;

public class SendNoticeAction : MonoBehaviour
{
	public Notices noticeId;

	private void SendNotice<T>(Notices notice, T arg)
	{
		PandoraSingleton<NoticeManager>.Instance.SendNotice(noticeId, arg);
	}

	public void SendNotice()
	{
		PandoraSingleton<NoticeManager>.Instance.SendNotice(noticeId);
	}

	public void SendNoticeInt(int arg)
	{
		SendNotice(noticeId, arg);
	}

	public void SendNoticeFloat(float arg)
	{
		SendNotice(noticeId, arg);
	}

	public void SendNoticeString(string arg)
	{
		SendNotice(noticeId, arg);
	}

	public void SendNoticeGameObject(GameObject arg)
	{
		SendNotice(noticeId, arg);
	}

	public void SendNoticeTransform(Transform arg)
	{
		SendNotice(noticeId, arg);
	}

	public void SendNoticeRectTransform(RectTransform arg)
	{
		SendNotice(noticeId, arg);
	}

	public void SendNoticeMainMenuState(MainMenuController.State arg)
	{
		SendNotice(noticeId, arg);
	}
}
