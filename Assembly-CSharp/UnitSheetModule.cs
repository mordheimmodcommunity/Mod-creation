using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitSheetModule : UIModule
{
    private const string statusLoc = "unit_status_name_{0}";

    public HightlightAnimate highlight;

    public ToggleGroup toggleGroup;

    public Text rank;

    public Text unitName;

    public Text xpValue;

    public Slider xp;

    public Image icon;

    public Image iconStar;

    public Text type;

    public Text rating;

    public Text status;

    public ToggleEffects statusToggle;

    public ToggleEffects ratingToggle;

    public List<UIStat> stats;

    public List<RectTransform> advancements;

    private Unit unit;

    public override void Init()
    {
        base.Init();
        for (int i = 0; i < advancements.Count; i++)
        {
            advancements[i].gameObject.SetActive(value: false);
        }
    }

    public void Refresh(Selectable right, Unit unit, Action<AttributeId> onAttributeSelected = null, Action<string, string> showDescription = null, Action<AttributeId> onAttributeUnselected = null)
    {
        //IL_0054: Unknown result type (might be due to invalid IL or missing references)
        //IL_0059: Unknown result type (might be due to invalid IL or missing references)
        //IL_0078: Unknown result type (might be due to invalid IL or missing references)
        //IL_00ba: Unknown result type (might be due to invalid IL or missing references)
        //IL_00bf: Unknown result type (might be due to invalid IL or missing references)
        //IL_00d3: Unknown result type (might be due to invalid IL or missing references)
        this.unit = unit;
        Navigation navigation;
        for (int i = 0; i < stats.Count; i++)
        {
            if (stats[i].canGoRight)
            {
                navigation = ((Selectable)stats[i].statSelector.toggle).get_navigation();
                ((Navigation)(ref navigation)).set_selectOnRight(right);
                ((Selectable)stats[i].statSelector.toggle).set_navigation(navigation);
            }
            stats[i].Refresh(unit, showArrows: true, onAttributeSelected, null, onAttributeUnselected);
        }
        navigation = ((Selectable)statusToggle.toggle).get_navigation();
        ((Navigation)(ref navigation)).set_selectOnRight(right);
        ((Selectable)statusToggle.toggle).set_navigation(navigation);
        if (showDescription != null)
        {
            statusToggle.onSelect.AddListener(delegate
            {
                string str = unit.GetActiveStatus().ToLowerString();
                showDescription(PandoraSingleton<LocalizationManager>.Instance.GetStringById("unit_status_name_" + str), PandoraSingleton<LocalizationManager>.Instance.GetStringById("unit_status_desc_simple_" + str));
            });
            ratingToggle.onSelect.AddListener(delegate
            {
                string stringById = PandoraSingleton<LocalizationManager>.Instance.GetStringById("unit_type_" + unit.GetUnitTypeId().ToLowerString());
                int unitTypeRating = unit.GetUnitTypeRating();
                int rankRating = unit.GetRankRating();
                int equipmentRating = unit.GetEquipmentRating();
                int statsRating = unit.GetStatsRating();
                int skillsRating = unit.GetSkillsRating();
                int num = unit.GetInjuriesRating() + unit.GetMutationsRating();
                int num2 = unitTypeRating + rankRating + equipmentRating + statsRating + skillsRating + num;
                showDescription(PandoraSingleton<LocalizationManager>.Instance.GetStringById("unit_rating_title"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("unit_rating_desc", stringById, unitTypeRating.ToString(), unit.Rank.ToString(), rankRating.ToString(), equipmentRating.ToString(), statsRating.ToString(), skillsRating.ToString(), num.ToString(), num2.ToString()));
            });
        }
        RefreshAttributes(unit);
    }

    public void RefreshAttributes(Unit unit)
    {
        DOTween.Kill((object)xp, false);
        this.unit = unit;
        rank.set_text(unit.Rank.ToString());
        unitName.set_text(unit.Name);
        if (unit.Rank >= Constant.GetInt(ConstantId.MAX_UNIT_RANK))
        {
            xp.set_normalizedValue(1f);
            for (int i = 0; i < advancements.Count; i++)
            {
                advancements[i].gameObject.SetActive(value: false);
            }
            ((Behaviour)(object)xpValue).enabled = false;
        }
        else
        {
            UnitRankData unitRankData = PandoraSingleton<DataFactory>.Instance.InitData<UnitRankData>((int)unit.UnitSave.rankId);
            int advancementsCountForRank = GetAdvancementsCountForRank(unit.Rank);
            List<UnitRankProgressionData> list = PandoraSingleton<DataFactory>.Instance.InitData<UnitRankProgressionData>(new string[2]
            {
                "fk_unit_type_id",
                "rank"
            }, new string[2]
            {
                ((int)unit.GetUnitTypeId()).ToString(),
                unit.Rank.ToString()
            });
            if (base.isActiveAndEnabled)
            {
                StartCoroutine(RefreshAdvancementBars(advancementsCountForRank));
            }
            xp.set_maxValue((float)(advancementsCountForRank * list[0].Xp));
            xp.set_value((float)(unitRankData.Advancement * list[0].Xp + unit.Xp));
            xp.get_fillRect().gameObject.SetActive(xp.get_value() > 0f);
            ((Behaviour)(object)xpValue).enabled = true;
            xpValue.set_text(xp.get_value() + " / " + xp.get_maxValue());
        }
        icon.set_sprite(unit.GetIcon());
        switch (unit.GetUnitTypeId())
        {
            case UnitTypeId.LEADER:
                ((Behaviour)(object)iconStar).enabled = true;
                iconStar.set_sprite(PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("icn_leader", cached: true));
                break;
            case UnitTypeId.HERO_1:
            case UnitTypeId.HERO_2:
            case UnitTypeId.HERO_3:
                ((Behaviour)(object)iconStar).enabled = true;
                iconStar.set_sprite(PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("icn_heroes", cached: true));
                break;
            case UnitTypeId.IMPRESSIVE:
                ((Behaviour)(object)iconStar).enabled = true;
                iconStar.set_sprite(PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("icn_impressive", cached: true));
                break;
            default:
                ((Behaviour)(object)iconStar).enabled = false;
                break;
        }
        type.set_text(unit.LocalizedType);
        rating.set_text((!unit.Active) ? ("0 (" + unit.GetRating() + ")") : unit.GetRating().ToString());
        status.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById($"unit_status_name_{unit.GetActiveStatus().ToLowerString()}"));
        for (int j = 0; j < stats.Count; j++)
        {
            stats[j].RefreshAttribute(unit);
        }
    }

    private IEnumerator RefreshAdvancementBars(int rankProgressionCount)
    {
        while (((RectTransform)advancements[0].transform.parent).rect.width <= 1f)
        {
            yield return 0;
        }
        float offset = ((RectTransform)advancements[0].transform.parent).rect.width / (float)rankProgressionCount;
        for (int i = 0; i < advancements.Count; i++)
        {
            if (i < rankProgressionCount - 1)
            {
                advancements[i].gameObject.SetActive(value: true);
                Vector2 pos = advancements[i].anchoredPosition;
                pos.x = offset * (float)(i + 1);
                advancements[i].anchoredPosition = pos;
            }
            else
            {
                advancements[i].gameObject.SetActive(value: false);
            }
        }
    }

    private void OnEnable()
    {
        if ((UnityEngine.Object)(object)toggleGroup != null)
        {
            toggleGroup.SetAllTogglesOff();
        }
        highlight.Deactivate();
    }

    private int GetAdvancementsCountForRank(int rank)
    {
        List<UnitRankData> list = PandoraSingleton<DataFactory>.Instance.InitData<UnitRankData>("rank", rank.ToString());
        int num = 0;
        for (int i = 0; i < list.Count; i++)
        {
            if (PandoraSingleton<DataFactory>.Instance.InitData<UnitJoinUnitRankData>("fk_unit_id", unit.Id.ToIntString(), "fk_unit_rank_id", list[i].Id.ToIntString()).Count > 0)
            {
                num++;
            }
        }
        return num;
    }

    public void RemoveDisplayedXp(int xpRemoved)
    {
        ((Behaviour)(object)xpValue).enabled = true;
        UnitRankData unitRankData = PandoraSingleton<DataFactory>.Instance.InitData<UnitRankData>((int)unit.UnitSave.rankId);
        int advancementsCountForRank = GetAdvancementsCountForRank(unit.Rank);
        int num = (unit.Rank != Constant.GetInt(ConstantId.MAX_UNIT_RANK)) ? PandoraSingleton<DataFactory>.Instance.InitData<UnitRankProgressionData>(new string[2]
        {
            "fk_unit_type_id",
            "rank"
        }, new string[2]
        {
            ((int)unit.GetUnitTypeId()).ToString(),
            unit.Rank.ToString()
        })[0].Xp : 0;
        int num2 = unitRankData.Advancement * num + unit.Xp;
        int num3 = advancementsCountForRank * num;
        if (num2 < xpRemoved)
        {
            xpRemoved -= num2;
            int num4 = int.Parse(rank.get_text()) - 1;
            rank.set_text(num4.ToString());
            advancementsCountForRank = GetAdvancementsCountForRank(num4);
            num = ((num4 != Constant.GetInt(ConstantId.MAX_UNIT_RANK)) ? PandoraSingleton<DataFactory>.Instance.InitData<UnitRankProgressionData>(new string[2]
            {
                "fk_unit_type_id",
                "rank"
            }, new string[2]
            {
                ((int)unit.GetUnitTypeId()).ToString(),
                num4.ToString()
            })[0].Xp : 0);
            num3 = advancementsCountForRank * num;
            xp.set_value((float)(num3 - xpRemoved));
        }
        else
        {
            xp.set_value(xp.get_value() - (float)xpRemoved);
        }
        xp.set_maxValue((float)num3);
        xpValue.set_text(xp.get_value() + " / " + xp.get_maxValue());
    }

    public void AddXp(int xpAdded)
    {
        DOTween.Kill((object)xp, false);
        if (xpAdded != 0)
        {
            AddOneXp(xpAdded);
        }
    }

    private void AddOneXp(int total)
    {
        //IL_006f: Unknown result type (might be due to invalid IL or missing references)
        //IL_0079: Expected O, but got Unknown
        int num = (int)xp.get_value() + ((total > 0) ? 1 : (-1));
        total += ((total <= 0) ? 1 : (-1));
        TweenSettingsExtensions.OnComplete<Tweener>(ShortcutExtensions.DOValue(xp, (float)num, 0.1f, false), (TweenCallback)(object)(TweenCallback)delegate
        {
            if (xp.get_value() >= xp.get_maxValue())
            {
                DoLoopXp();
            }
            xpValue.set_text(xp.get_value() + " / " + xp.get_maxValue());
            if (total != 0)
            {
                AddOneXp(total);
            }
        });
    }

    private void DoLoopXp()
    {
        PandoraSingleton<Pan>.Instance.Narrate("unit_progressed" + PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(1, 6));
        int num = int.Parse(rank.get_text()) + 1;
        rank.set_text(num.ToString());
        if (num < Constant.GetInt(ConstantId.MAX_UNIT_RANK))
        {
            xp.set_value(0f);
            int advancementsCountForRank = GetAdvancementsCountForRank(num);
            int num2 = PandoraSingleton<DataFactory>.Instance.InitData<UnitRankProgressionData>(new string[2]
            {
                "fk_unit_type_id",
                "rank"
            }, new string[2]
            {
                ((int)unit.GetUnitTypeId()).ToString(),
                unit.Rank.ToString()
            })[0].Xp;
            if (base.isActiveAndEnabled)
            {
                StartCoroutine(RefreshAdvancementBars(advancementsCountForRank));
            }
            xp.set_maxValue((float)(advancementsCountForRank * num2));
        }
        else
        {
            for (int i = 0; i < advancements.Count; i++)
            {
                advancements[i].gameObject.SetActive(value: false);
            }
            ((Behaviour)(object)xpValue).enabled = false;
            xp.set_maxValue(1E+09f);
        }
    }
}
