using System.Collections.Generic;
using UnityEngine;

public class Profile
{
    public const int TUTO_MISSION_MAX_IDX = 4;

    private const int MAX_WAR_IDX_PROGRESS = 4;

    private PlayerRankData rankData;

    public ProfileSave ProfileSave
    {
        get;
        private set;
    }

    public PlayerRankData RankData
    {
        get
        {
            return rankData;
        }
        set
        {
            rankData = value;
            HasNextRank = (PandoraSingleton<DataFactory>.Instance.InitData<PlayerRankData>((int)(RankData.Id + 1)) != null);
        }
    }

    public bool HasNextRank
    {
        get;
        set;
    }

    public int Rank => ProfileSave.rankId;

    public int CurrentXp => ProfileSave.xp;

    public int NewGameBonusGold => RankData.NewGameGold;

    public Achievement[] Achievements
    {
        get;
        private set;
    }

    public bool[] TutorialCompletion => ProfileSave.completedTutorials;

    public AchievementCategory[] AchievementsCategories
    {
        get;
        private set;
    }

    public int LastPlayedCampaign
    {
        get
        {
            return ProfileSave.lastPlayedCampaign;
        }
        set
        {
            ProfileSave.lastPlayedCampaign = value;
        }
    }

    public Profile(ProfileSave save)
    {
        ProfileSave = save;
        RankData = PandoraSingleton<DataFactory>.Instance.InitData<PlayerRankData>(ProfileSave.rankId);
        InitAchievements();
    }

    public void UpdateHash(int campaign, int hash)
    {
        ProfileSave.warbandSaves[campaign] = hash;
    }

    public void ClearHash(int campaign)
    {
        ProfileSave.warbandSaves.Remove(campaign);
    }

    public void CheckXp()
    {
        if (ProfileSave.xpChecked && ProfileSave.xp >= RankData.XpNeeded && HasNextRank)
        {
            AddXp(0);
            PandoraSingleton<GameManager>.Instance.SaveProfile();
        }
        else
        {
            if (ProfileSave.xpChecked)
            {
                return;
            }
            ProfileSave.xpChecked = true;
            List<int> list = new List<int>();
            list.Add(0);
            list.Add(10);
            list.Add(25);
            list.Add(45);
            list.Add(70);
            list.Add(100);
            list.Add(135);
            list.Add(175);
            list.Add(220);
            list.Add(270);
            list.Add(320);
            list.Add(370);
            list.Add(420);
            list.Add(470);
            list.Add(520);
            List<int> list2 = list;
            int num = 0;
            List<PlayerRankData> list3 = PandoraSingleton<DataFactory>.Instance.InitData<PlayerRankData>();
            for (int i = 0; i < list3.Count; i++)
            {
                if ((int)list3[i].Id < Rank)
                {
                    num += list3[i].XpNeeded;
                }
            }
            int xp = Mathf.Max(list2[Rank] - num, 0);
            AddXp(xp);
            PandoraSingleton<GameManager>.Instance.SaveProfile();
        }
    }

    public List<PlayerRankData> AddXp(int xp)
    {
        List<PlayerRankData> list = new List<PlayerRankData>();
        ProfileSave.xp += xp;
        PlayerRankData playerRankData = PandoraSingleton<DataFactory>.Instance.InitData<PlayerRankData>(ProfileSave.rankId);
        bool flag = false;
        while (ProfileSave.xp >= playerRankData.XpNeeded && HasNextRank)
        {
            ProfileSave.xp -= playerRankData.XpNeeded;
            PlayerRankData playerRankData2 = PandoraSingleton<DataFactory>.Instance.InitData<PlayerRankData>(ProfileSave.rankId + 1);
            if (playerRankData2 != null)
            {
                list.Add(playerRankData2);
                playerRankData = playerRankData2;
                ProfileSave.rankId++;
                RankData = playerRankData2;
                PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.PROFILE_RANK_UP, ProfileSave.rankId);
                flag = true;
            }
        }
        if (flag)
        {
            PandoraSingleton<Pan>.Instance.Narrate("new_veteran_rank");
        }
        else
        {
            PandoraSingleton<Pan>.Instance.Narrate("veteran_task_completed");
        }
        return list;
    }

    public void AddToStat(WarbandAttributeId statId, int increment)
    {
        if (ProfileSave != null)
        {
            ProfileSave.stats[(int)statId] += increment;
            CheckAchievement(statId, ProfileSave.stats[(int)statId]);
        }
    }

    public void SetStat(WarbandAttributeId statId, int value)
    {
        if (ProfileSave != null)
        {
            ProfileSave.stats[(int)statId] = value;
            CheckAchievement(statId, ProfileSave.stats[(int)statId]);
        }
    }

    private void InitAchievements()
    {
        Achievements = new Achievement[227];
        AchievementsCategories = new AchievementCategory[36];
        List<AchievementData> list = PandoraSingleton<DataFactory>.Instance.InitData<AchievementData>();
        for (int i = 0; i < list.Count; i++)
        {
            Achievement achievement = Achievement.Create(list[i]);
            if (achievement == null)
            {
                continue;
            }
            Achievements[(int)list[i].Id] = achievement;
            achievement.Completed = ProfileSave.unlockedAchievements[(int)list[i].Id];
            if (achievement.Data.PlayerProgression)
            {
                int achievementCategoryId = (int)achievement.Data.AchievementCategoryId;
                if (AchievementsCategories[achievementCategoryId] == null)
                {
                    AchievementsCategories[achievementCategoryId] = new AchievementCategory(achievement.Data.AchievementCategoryId);
                }
                AchievementsCategories[achievementCategoryId].achievements.Add(achievement);
            }
        }
    }

    public bool IsAchievementUnlocked(AchievementId achievementId)
    {
        return Achievements[(int)achievementId] != null && Achievements[(int)achievementId].Completed;
    }

    public void CheckAchievement(Warband warband, WarbandAttributeId statId, int value)
    {
        for (int i = 0; i < Achievements.Length; i++)
        {
            if (Achievements[i] != null && Achievements[i].CanCheck() && Achievements[i].Target == AchievementTargetId.WARBAND && Achievements[i].CheckWarband(warband, statId, value))
            {
                Unlock(Achievements[i].Id);
            }
        }
    }

    public void CheckAchievement(WarbandAttributeId statId, int value)
    {
        for (int i = 0; i < Achievements.Length; i++)
        {
            if (Achievements[i] != null && Achievements[i].CanCheck() && Achievements[i].Target == AchievementTargetId.PROFILE && Achievements[i].CheckProfile(statId, value))
            {
                Unlock(Achievements[i].Id);
            }
        }
    }

    public void CheckAchievement(Unit unit, AttributeId statId = AttributeId.NONE, int value = 0)
    {
        for (int i = 0; i < Achievements.Length; i++)
        {
            if (Achievements[i] != null && Achievements[i].CanCheck() && Achievements[i].Target == AchievementTargetId.UNIT && Achievements[i].CheckUnit(unit, statId, value))
            {
                Unlock(Achievements[i].Id);
            }
        }
    }

    public void CheckAchievement(CampaignMissionId missionId)
    {
        for (int i = 0; i < Achievements.Length; i++)
        {
            if (Achievements[i] != null && Achievements[i].CanCheck() && Achievements[i].Target == AchievementTargetId.PROFILE && Achievements[i].CheckFinishMission(missionId))
            {
                Unlock(Achievements[i].Id);
            }
        }
    }

    public void CheckEquipAchievement(Unit unit, UnitSlotId slotId)
    {
        for (int i = 0; i < Achievements.Length; i++)
        {
            if (Achievements[i] != null && Achievements[i].CanCheck() && Achievements[i].Target == AchievementTargetId.UNIT && Achievements[i].CheckEquipUnit(unit, slotId))
            {
                Unlock(Achievements[i].Id);
            }
        }
    }

    private void Unlock(AchievementId achievementId)
    {
        if (!ProfileSave.unlockedAchievements[(int)achievementId])
        {
            PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.TASK_COMPLETED, achievementId);
            ProfileSave.unlockedAchievements[(int)achievementId] = true;
            AddXp(Achievements[(int)achievementId].Xp);
            Achievements[(int)achievementId].Completed = true;
            PandoraSingleton<GameManager>.Instance.SaveProfile();
        }
    }

    public void CompleteTutorial(int idx)
    {
        if (!ProfileSave.completedTutorials[idx])
        {
            ProfileSave.completedTutorials[idx] = true;
        }
        int num = 0;
        for (int i = 0; i < 4; i++)
        {
            if (ProfileSave.completedTutorials[i])
            {
                num++;
            }
        }
        if (num >= 2)
        {
            PandoraSingleton<Hephaestus>.Instance.UnlockAchievement(Hephaestus.TrophyId.TUTO_1);
        }
        if (num >= 4)
        {
            PandoraSingleton<Hephaestus>.Instance.UnlockAchievement(Hephaestus.TrophyId.TUTO_2);
        }
    }

    public bool HasCompletedTutorials()
    {
        for (int i = 0; i < 4; i++)
        {
            if (ProfileSave.completedTutorials[i])
            {
                return true;
            }
        }
        return false;
    }

    public string GetNextRankDescription()
    {
        if (!HasNextRank)
        {
            return string.Empty;
        }
        return PandoraSingleton<LocalizationManager>.Instance.GetStringById("player_" + ((PlayerRankId)(Rank + 1)).ToLowerString() + "_perks_desc");
    }

    public string GetCurrentRankDescription()
    {
        return PandoraSingleton<LocalizationManager>.Instance.GetStringById("player_" + ((PlayerRankId)Rank).ToLowerString() + "_desc");
    }

    public void UpdateGameProgress(WarbandId warId, int progress)
    {
        int num = 0;
        switch (warId)
        {
            default:
                return;
            case WarbandId.SKAVENS:
                num = 1;
                break;
            case WarbandId.SISTERS_OF_SIGMAR:
                num = 2;
                break;
            case WarbandId.POSSESSED:
                num = 3;
                break;
            case WarbandId.WITCH_HUNTERS:
                num = 4;
                break;
            case WarbandId.UNDEAD:
                num = 5;
                break;
            case WarbandId.HUMAN_MERCENARIES:
                break;
        }
        ProfileSave.warProgress[num] = Mathf.Max(ProfileSave.warProgress[num], progress);
    }

    public int GetGameProgress()
    {
        int num = 0;
        for (int i = 0; i < 4; i++)
        {
            num += ProfileSave.warProgress[i];
        }
        return (int)((float)num * 100f / 4f / (float)Constant.GetInt(ConstantId.CAMPAIGN_LAST_MISSION));
    }
}
