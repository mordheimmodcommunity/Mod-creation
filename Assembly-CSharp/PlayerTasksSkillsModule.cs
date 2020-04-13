using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerTasksSkillsModule : UIModule
{
    public ListTabsModule tabs;

    public GameObject points;

    public Text unspentPoints;

    public ScrollGroup list;

    public GameObject taskPrefab;

    public GameObject skillPrefab;

    public Text tasksCompletion;

    private PlayerProgressTab currentTab;

    private bool isFocused;

    private UnityAction<PlayerProgressTab> onTabChanged;

    private UnityAction<AchievementCategory> onTaskSelected;

    private UnityAction<WarbandSkill> onSkillSelected;

    private UnityAction<WarbandSkill> onSkillConfirmed;

    private WarbandSkill currentSkill;

    private List<WarbandSkill> warbandSkills;

    private void Start()
    {
        List<WarbandSkillData> list = PandoraSingleton<DataFactory>.Instance.InitData<WarbandSkillData>("fk_warband_skill_type_id", 3.ToString());
        warbandSkills = new List<WarbandSkill>();
        for (int i = 0; i < list.Count; i++)
        {
            warbandSkills.Add(new WarbandSkill(list[i].Id));
        }
        warbandSkills.Sort((WarbandSkill x, WarbandSkill y) => x.LocalizedName.CompareTo(y.LocalizedName));
    }

    public void Set(UnityAction<PlayerProgressTab> tabChanged, UnityAction<AchievementCategory> taskSelected, UnityAction<WarbandSkill> skillSelected, UnityAction<WarbandSkill> skillConfirmed)
    {
        onTabChanged = tabChanged;
        onTaskSelected = taskSelected;
        onSkillSelected = skillSelected;
        onSkillConfirmed = skillConfirmed;
        tabs.gameObject.SetActive(value: true);
        tabs.Setup(OnTabChanged, (int)currentTab);
        int num = 0;
        int num2 = 0;
        AchievementCategory[] achievementsCategories = PandoraSingleton<GameManager>.Instance.Profile.AchievementsCategories;
        for (int i = 0; i < achievementsCategories.Length; i++)
        {
            if (achievementsCategories[i] != null)
            {
                num += achievementsCategories[i].Count;
                num2 += achievementsCategories[i].NbDone;
            }
        }
        tasksCompletion.set_text(num2.ToString() + "/" + num.ToString());
    }

    private void OnTabChanged(int tab)
    {
        SetTab((PlayerProgressTab)tab, callBack: true);
    }

    public void SetTab(PlayerProgressTab tab, bool callBack)
    {
        if (currentTab != tab || list.items.Count == 0)
        {
            currentTab = tab;
            switch (currentTab)
            {
                case PlayerProgressTab.TASKS:
                    tabs.tabs[0].SetOn();
                    SetTasksList();
                    break;
                case PlayerProgressTab.SKILLS:
                    tabs.tabs[1].SetOn();
                    SetSkillsList(clearOnly: true);
                    break;
            }
        }
        if (callBack && onTabChanged != null)
        {
            onTabChanged(currentTab);
        }
    }

    public void SetCurrentSkill(WarbandSkill skill)
    {
        currentSkill = skill;
    }

    private void SetTasksList()
    {
        points.SetActive(value: false);
        this.list.ClearList();
        this.list.Setup(taskPrefab, hideBarIfEmpty: true);
        List<AchievementCategory> list = PandoraSingleton<GameManager>.Instance.Profile.AchievementsCategories.Remove(new AchievementCategory[1]).Sorted((AchievementCategory x, AchievementCategory y) => x.LocName.CompareTo(y.LocName)).ToDynList();
        for (int i = 0; i < list.Count; i++)
        {
            GameObject gameObject = this.list.AddToList(null, null);
            UITaskListItem component = gameObject.GetComponent<UITaskListItem>();
            component.Set(list[i]);
            ToggleEffects component2 = gameObject.GetComponent<ToggleEffects>();
            component2.selectOnOver = true;
            AchievementCategory task = list[i];
            component2.onSelect.AddListener(delegate
            {
                onTaskSelected(task);
            });
        }
    }

    public void SetSkillsList(bool clearOnly = false)
    {
        RefreshAvailablePoints();
        list.ClearList();
        if (clearOnly || (currentSkill != null && currentSkill.IsMastery))
        {
            return;
        }
        list.Setup(skillPrefab, hideBarIfEmpty: true);
        int num = 0;
        while (true)
        {
            if (num >= warbandSkills.Count)
            {
                return;
            }
            if (currentSkill != null)
            {
                if (warbandSkills[num].Data.WarbandSkillIdPrerequisite == currentSkill.Id)
                {
                    break;
                }
            }
            else if (warbandSkills[num].Data.SkillQualityId == SkillQualityId.NORMAL_QUALITY && !PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.HasSkill(warbandSkills[num].Data.Id, includeMastery: true))
            {
                AddSkillToList(warbandSkills[num]);
            }
            num++;
        }
        AddSkillToList(warbandSkills[num]);
    }

    public void RefreshAvailablePoints()
    {
        int playerSkillsAvailablePoints = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetPlayerSkillsAvailablePoints();
        if (playerSkillsAvailablePoints > 0)
        {
            points.SetActive(value: true);
            unspentPoints.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("hideout_unspent_skill_point", playerSkillsAvailablePoints.ToString()));
        }
        else
        {
            points.SetActive(value: false);
        }
    }

    private void AddSkillToList(WarbandSkill data)
    {
        GameObject gameObject = list.AddToList(null, null);
        UISkillItem component = gameObject.GetComponent<UISkillItem>();
        component.Set(data);
        ToggleEffects component2 = gameObject.GetComponent<ToggleEffects>();
        component2.onSelect.AddListener(delegate
        {
            onSkillSelected(data);
        });
        component2.onAction.AddListener(delegate
        {
            onSkillConfirmed(data);
        });
    }

    public void SetFocus(bool focus)
    {
        isFocused = focus;
        if (focus && PandoraSingleton<PandoraInput>.Instance.CurrentInputLayer == 0)
        {
            list.items[0].SetSelected(force: true);
        }
    }

    private void Update()
    {
        if (isFocused)
        {
            if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("h") && tabs.currentTab != 0)
            {
                tabs.Next();
            }
            else if (PandoraSingleton<PandoraInput>.Instance.GetNegKeyUp("h"))
            {
                tabs.Prev();
            }
            float axis = PandoraSingleton<PandoraInput>.Instance.GetAxis("cam_y");
            if (axis != 0f)
            {
                list.ForceScroll(axis < 0f, setSelected: false);
            }
        }
    }
}
