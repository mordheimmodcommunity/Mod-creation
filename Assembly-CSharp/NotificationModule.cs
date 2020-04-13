using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotificationModule : UIModule
{
    public GameObject notif;

    public Text title;

    public Text desc;

    public GameObject rank;

    public Text rankPoints;

    public GameObject task;

    public Text task1;

    public Text task2;

    public float duration = 5f;

    private List<UINotification> notifications;

    private float timer;

    private new void Awake()
    {
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.PROFILE_RANK_UP, RankUp);
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.TASK_COMPLETED, TaskCompleted);
        notifications = new List<UINotification>();
    }

    private void Update()
    {
        notif.SetActive(notifications.Count > 0);
        if (notifications.Count <= 0)
        {
            return;
        }
        if (!notifications[0].activated)
        {
            title.set_text(notifications[0].title);
            desc.set_text(notifications[0].desc);
            switch (notifications[0].type)
            {
                case NotificationType.RANK:
                    rank.SetActive(value: true);
                    rankPoints.set_text(notifications[0].text1);
                    task.SetActive(value: false);
                    break;
                case NotificationType.TASK:
                    rank.SetActive(value: false);
                    task.SetActive(value: true);
                    task1.set_text(notifications[0].text1);
                    task2.set_text(notifications[0].text2);
                    break;
            }
        }
        timer += Time.deltaTime;
        if (timer >= duration)
        {
            notifications.RemoveAt(0);
            timer = 0f;
        }
    }

    private void RankUp()
    {
        PlayerRankId playerRankId = (PlayerRankId)(int)PandoraSingleton<NoticeManager>.Instance.Parameters[0];
        int num = 0;
        PlayerRankData playerRankData = PandoraSingleton<DataFactory>.Instance.InitData<PlayerRankData>((int)playerRankId);
        WarbandSkill warbandSkill = new WarbandSkill(playerRankData.WarbandSkillId);
        for (int i = 0; i < warbandSkill.Enchantments.Count; i++)
        {
            for (int j = 0; j < warbandSkill.Enchantments[i].Attributes.Count; j++)
            {
                if (warbandSkill.Enchantments[i].Attributes[j].WarbandAttributeId == WarbandAttributeId.PLAYER_SKILL_POINTS)
                {
                    num += warbandSkill.Enchantments[i].Attributes[j].Modifier;
                }
            }
        }
        string stringById = PandoraSingleton<LocalizationManager>.Instance.GetStringById("player_rank_up_title");
        LocalizationManager instance = PandoraSingleton<LocalizationManager>.Instance;
        string[] array = new string[1];
        int num2 = (int)playerRankId;
        array[0] = num2.ToString();
        AddNotification(stringById, instance.GetStringById("player_rank_up_desc", array), PandoraSingleton<LocalizationManager>.Instance.GetStringById("player_rank_up_gained_points", num.ToString()), string.Empty);
    }

    private void TaskCompleted()
    {
        AchievementId id = (AchievementId)(int)PandoraSingleton<NoticeManager>.Instance.Parameters[0];
        AchievementData achievementData = PandoraSingleton<DataFactory>.Instance.InitData<AchievementData>((int)id);
        AddNotification(PandoraSingleton<LocalizationManager>.Instance.GetStringById("task_completed_title"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("task_completed_desc"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("task_title_" + achievementData.AchievementCategoryId.ToLowerString()), PandoraSingleton<LocalizationManager>.Instance.GetStringById("task_xp", achievementData.Xp.ToString()));
    }

    private void AddNotification(string title, string desc, string text1 = "", string text2 = "")
    {
        UINotification uINotification = new UINotification();
        uINotification.title = title;
        uINotification.desc = desc;
        uINotification.text1 = text1;
        uINotification.text2 = text2;
        notifications.Add(uINotification);
    }
}
