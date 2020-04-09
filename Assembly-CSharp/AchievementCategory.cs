using System.Collections.Generic;

public class AchievementCategory
{
	public AchievementCategoryId category;

	public List<Achievement> achievements;

	private string locName;

	private string locDesc;

	private string locDescShort;

	public string LocName
	{
		get
		{
			if (locName == null)
			{
				locName = PandoraSingleton<LocalizationManager>.Instance.GetStringById("task_title_" + category.ToLowerString());
			}
			return locName;
		}
	}

	public string LocDesc
	{
		get
		{
			if (locDesc == null)
			{
				locDesc = PandoraSingleton<LocalizationManager>.Instance.GetStringById("task_desc_" + category.ToLowerString());
			}
			return locDesc;
		}
	}

	public string LocDescShort
	{
		get
		{
			if (locDescShort == null)
			{
				locDescShort = PandoraSingleton<LocalizationManager>.Instance.GetStringById("task_desc_" + category.ToLowerString() + "_short");
			}
			return locDescShort;
		}
	}

	public int NbDone
	{
		get
		{
			int num = 0;
			for (int i = 0; i < achievements.Count; i++)
			{
				if (achievements[i].Completed)
				{
					num++;
				}
			}
			return num;
		}
	}

	public int Count => achievements.Count;

	public AchievementCategory(AchievementCategoryId catId)
	{
		category = catId;
		achievements = new List<Achievement>();
	}
}
