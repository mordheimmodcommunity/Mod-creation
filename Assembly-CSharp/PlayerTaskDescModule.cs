using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTaskDescModule : UIModule
{
	private const string PROGRESS = "{0}/{1}";

	public Text taskName;

	public Text taskDesc;

	public Text progress;

	public Text taskListSubtitle;

	public ListGroup list;

	public GameObject prefab;

	public List<UISubTaskListItem> subTasks;

	public void Set(AchievementCategory cat)
	{
		base.gameObject.SetActive(value: true);
		taskName.set_text(cat.LocName);
		taskDesc.set_text(cat.LocDesc);
		taskListSubtitle.set_text(cat.LocDescShort);
		int count = cat.achievements.Count;
		int num = 0;
		for (int i = 0; i < subTasks.Count; i++)
		{
			if (i < cat.achievements.Count)
			{
				subTasks[i].gameObject.SetActive(value: true);
				num += (cat.achievements[i].Completed ? 1 : 0);
				subTasks[i].Set(cat.achievements[i]);
			}
			else
			{
				subTasks[i].gameObject.SetActive(value: false);
			}
		}
		progress.set_text($"{num}/{count}");
	}
}
