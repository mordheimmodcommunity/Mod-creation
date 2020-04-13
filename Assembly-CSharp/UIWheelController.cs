using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIWheelController : UIUnitControllerChanged
{
    public enum Category
    {
        NONE = -1,
        BASE_ACTION,
        ACTIVE_SKILL,
        PASSIVE_SKILL,
        SPELLS,
        STANCES,
        INVENTORY,
        COUNT
    }

    private const PandoraInput.InputLayer layer = PandoraInput.InputLayer.WHEEL;

    private static readonly Color NotSelectedColor = new Color(0.5f, 0.5f, 0.5f);

    private static readonly Color SelectedColor = new Color(1f, 1f, 1f);

    public GameObject outerWheelBackGround;

    public GameObject outerWheel;

    public List<UIWheelIcon> innerWheelIcons;

    public List<UIWheelIcon> outerWheelIcons;

    private bool isShow;

    public Sprite availableSprite;

    public Sprite notAvailableSprite;

    private int currentCategory;

    private int currentAction;

    private bool selectingCategory = true;

    private List<List<WheelAction>> categories;

    public UIWheelDescription descriptionGroup;

    private ActionStatus oldCurrentAction;

    private bool activateAction;

    private List<ActionStatus> oldActions;

    private void Awake()
    {
        base.gameObject.SetActive(value: false);
        for (int i = 0; i < innerWheelIcons.Count; i++)
        {
            int catNo = i;
            innerWheelIcons[i].effects.onSelect.AddListener(delegate
            {
                PreviewCategory(catNo);
            });
            innerWheelIcons[i].effects.onUnselect.AddListener(delegate
            {
                UnHighlightCategory(catNo);
            });
            innerWheelIcons[i].effects.onAction.AddListener(delegate
            {
                SelectCategory(catNo);
                OnInputAction(canActivateAction: true);
            });
        }
        for (int j = 0; j < outerWheelIcons.Count; j++)
        {
            int actionId = j;
            outerWheelIcons[j].effects.onSelect.AddListener(delegate
            {
                PreviewAction(actionId);
            });
            outerWheelIcons[j].effects.onUnselect.AddListener(delegate
            {
                UnHighlightAction(actionId);
            });
            outerWheelIcons[j].effects.onAction.AddListener(delegate
            {
                SetCurrentAction(actionId);
                OnInputAction(canActivateAction: true);
            });
        }
    }

    public void Show()
    {
        if (isShow)
        {
            return;
        }
        activateAction = false;
        oldCurrentAction = base.CurrentUnitController.CurrentAction;
        if (base.CurrentUnitController.IsCurrentState(UnitController.State.MOVE))
        {
            oldActions = ((Moving)base.CurrentUnitController.StateMachine.GetActiveState()).actions;
        }
        else
        {
            oldActions = base.CurrentUnitController.availableActionStatus;
        }
        PandoraSingleton<PandoraInput>.Instance.SetCurrentState(PandoraInput.States.MISSION, showMouse: true);
        PandoraSingleton<PandoraInput>.Instance.PushInputLayer(PandoraInput.InputLayer.WHEEL);
        if (categories == null)
        {
            categories = new List<List<WheelAction>>(6);
            for (int i = 0; i < 6; i++)
            {
                categories.Add(new List<WheelAction>());
            }
        }
        else
        {
            for (int j = 0; j < categories.Count; j++)
            {
                categories[j].Clear();
            }
        }
        FillCategories();
        selectingCategory = false;
        isShow = true;
        base.gameObject.SetActive(value: true);
        base.UpdateUnit = false;
        OpenCategory(base.CurrentUnitController.CurrentAction);
    }

    private void FillCategories()
    {
        List<ActionStatus> list = new List<ActionStatus>();
        foreach (ActionStatus item in base.CurrentUnitController.actionStatus)
        {
            Category category = Category.NONE;
            switch (item.SkillId)
            {
                case SkillId.BASE_ATTACK:
                case SkillId.BASE_CHARGE:
                    if (base.CurrentUnitController.HasClose())
                    {
                        category = Category.BASE_ACTION;
                    }
                    break;
                case SkillId.BASE_SHOOT:
                case SkillId.BASE_RELOAD:
                case SkillId.BASE_AIM:
                    if (base.CurrentUnitController.HasRange())
                    {
                        category = Category.BASE_ACTION;
                    }
                    break;
                case SkillId.BASE_PERCEPTION:
                case SkillId.BASE_SWITCH_WEAPONS:
                case SkillId.BASE_DISENGAGE:
                case SkillId.BASE_FLEE:
                    category = Category.BASE_ACTION;
                    break;
                case SkillId.BASE_DELAY:
                case SkillId.BASE_STANCE_OVERWATCH:
                case SkillId.BASE_STANCE_AMBUSH:
                case SkillId.BASE_STANCE_PARRY:
                case SkillId.BASE_STANCE_DODGE:
                    category = Category.STANCES;
                    break;
                default:
                    {
                        UnitActionId actionId = item.ActionId;
                        if (actionId == UnitActionId.CONSUMABLE)
                        {
                            list.Add(item);
                        }
                        break;
                    }
                case SkillId.BASE_CLIMB:
                case SkillId.BASE_LEAP:
                case SkillId.BASE_JUMPDOWN:
                    continue;
            }
            if (category != Category.NONE)
            {
                categories[(int)category].Add(new WheelAction(item, category));
            }
        }
        for (int i = 0; i < base.CurrentUnitController.unit.PassiveSkills.Count; i++)
        {
            categories[2].Add(new WheelAction(new ActionStatus(base.CurrentUnitController.unit.PassiveSkills[i], base.CurrentUnitController), Category.PASSIVE_SKILL));
        }
        for (int j = 0; j < base.CurrentUnitController.unit.ActiveSkills.Count; j++)
        {
            categories[1].Add(new WheelAction(base.CurrentUnitController.GetAction(base.CurrentUnitController.unit.ActiveSkills[j].Id), Category.ACTIVE_SKILL));
        }
        for (int k = 0; k < base.CurrentUnitController.unit.Spells.Count; k++)
        {
            categories[3].Add(new WheelAction(base.CurrentUnitController.GetAction(base.CurrentUnitController.unit.Spells[k].Id), Category.SPELLS));
        }
        int count = base.CurrentUnitController.unit.Items.Count;
        for (int l = 6; l < count; l++)
        {
            bool flag = false;
            for (int m = 0; m < list.Count; m++)
            {
                if (list[m].LinkedItem.Id == base.CurrentUnitController.unit.Items[l].Id && list[m].LinkedItem.QualityData.Id == base.CurrentUnitController.unit.Items[l].QualityData.Id)
                {
                    flag = true;
                    categories[5].Add(new WheelAction(list[m], base.CurrentUnitController.unit.Items[l], Category.INVENTORY));
                }
            }
            if (!flag)
            {
                categories[5].Add(new WheelAction(base.CurrentUnitController.unit.Items[l]));
            }
        }
        for (int n = 0; n < categories.Count; n++)
        {
            ((Selectable)innerWheelIcons[n].toggle).get_image().set_sprite((categories[n].Count == 0) ? notAvailableSprite : availableSprite);
        }
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if (isShow)
        {
            PandoraSingleton<PandoraInput>.Instance.PopInputLayer(PandoraInput.InputLayer.WHEEL);
            PandoraSingleton<PandoraInput>.Instance.SetCurrentState(PandoraInput.States.MISSION, showMouse: false);
            isShow = false;
            PandoraSingleton<PandoraInput>.Instance.SetActive(active: true);
            if (!activateAction)
            {
                base.CurrentUnitController.SetCurrentAction(oldCurrentAction.SkillId);
                PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.CURRENT_UNIT_ACTION_CHANGED, base.CurrentUnitController, oldCurrentAction, oldActions);
            }
        }
    }

    private void OpenCategory(ActionStatus currentAction)
    {
        for (int i = 0; i < categories.Count; i++)
        {
            int num = categories[i].FindIndex((WheelAction x) => x.action == currentAction);
            if (num != -1)
            {
                SelectCategory(i);
                OpenCategory();
                SetCurrentAction(num);
                return;
            }
        }
        SelectCategory(0);
        OpenCategory();
        SetCurrentAction(0);
    }

    private void PreviewCategory(int categoryId)
    {
        ((Graphic)innerWheelIcons[currentCategory].icon).set_color(NotSelectedColor);
        ((Graphic)innerWheelIcons[categoryId].icon).set_color(SelectedColor);
        descriptionGroup.SetCurrentCategory((Category)categoryId);
    }

    private void UnHighlightCategory(int categoryId)
    {
        ((Graphic)innerWheelIcons[categoryId].icon).set_color(NotSelectedColor);
    }

    private void SelectCategory(int category)
    {
        UnHighlightCategory(currentCategory);
        innerWheelIcons[currentCategory].toggle.set_isOn(false);
        selectingCategory = true;
        if (category < 0)
        {
            category = categories.Count - 1;
        }
        else if (category >= categories.Count)
        {
            category = 0;
        }
        PreviewCategory(category);
        currentCategory = category;
        innerWheelIcons[currentCategory].toggle.set_isOn(true);
        if (categories[currentCategory].Count > 0)
        {
            for (int i = 0; i < outerWheelIcons.Count; i++)
            {
                if (i < categories[currentCategory].Count)
                {
                    WheelAction wheelAction = categories[currentCategory][i];
                    outerWheelIcons[i].gameObject.SetActive(value: true);
                    outerWheelIcons[i].icon.set_sprite(wheelAction.GetIcon());
                    ((Behaviour)(object)outerWheelIcons[i].mastery).enabled = (wheelAction.action != null && wheelAction.action.IsMastery);
                    SetActionIconColor(i, selected: false);
                }
                else
                {
                    outerWheelIcons[i].gameObject.SetActive(value: false);
                }
            }
        }
        else
        {
            for (int j = 0; j < outerWheelIcons.Count; j++)
            {
                outerWheelIcons[j].gameObject.SetActive(value: false);
            }
        }
        descriptionGroup.SetCurrentCategory((Category)currentCategory);
    }

    private void OpenCategory()
    {
        if (base.CurrentUnitController != null && categories[currentCategory].Count > 0)
        {
            SetCurrentAction(0);
            outerWheelIcons[0].SetSelected();
        }
    }

    private void SetActionIconColor(int index, bool selected)
    {
        if (index >= 0 && index < categories[currentCategory].Count)
        {
            if (selected)
            {
                ((Behaviour)(object)((Selectable)outerWheelIcons[index].toggle).get_image()).enabled = !categories[currentCategory][index].Available;
                ((Graphic)outerWheelIcons[index].icon).set_color(SelectedColor);
            }
            else
            {
                ((Behaviour)(object)((Selectable)outerWheelIcons[index].toggle).get_image()).enabled = !categories[currentCategory][index].Available;
                ((Graphic)outerWheelIcons[index].icon).set_color(NotSelectedColor);
            }
        }
        else
        {
            ((Behaviour)(object)((Selectable)outerWheelIcons[index].toggle).get_image()).enabled = false;
            ((Graphic)outerWheelIcons[index].icon).set_color(NotSelectedColor);
        }
    }

    private void PreviewAction(int actionIndex)
    {
        if (categories[currentCategory].Count != 0)
        {
            descriptionGroup.SetCurrentCategory((Category)currentCategory);
            UnHighlightAction(currentAction);
            SetActionIconColor(actionIndex, selected: true);
            if (currentCategory >= 0 && currentCategory < categories.Count && actionIndex >= 0 && actionIndex < categories[currentCategory].Count)
            {
                descriptionGroup.SetCurrentAction(categories[currentCategory][actionIndex]);
            }
        }
    }

    private void UnHighlightAction(int actionIndex)
    {
        SetActionIconColor(actionIndex, selected: false);
    }

    private void SetCurrentAction(int index)
    {
        if (categories[currentCategory].Count != 0)
        {
            outerWheelIcons[currentAction].toggle.set_isOn(false);
            selectingCategory = false;
            UnHighlightAction(currentAction);
            if (index < 0)
            {
                index = categories[currentCategory].Count - 1;
            }
            else if (index >= categories[currentCategory].Count)
            {
                index = 0;
            }
            PreviewAction(index);
            outerWheelIcons[index].toggle.set_isOn(true);
            currentAction = index;
            if (categories[currentCategory][index].Available)
            {
                base.CurrentUnitController.SetCurrentAction(categories[currentCategory][index].action.SkillId);
                PandoraSingleton<NoticeManager>.Instance.SendNotice<UnitController, ActionStatus, List<ActionStatus>>(Notices.CURRENT_UNIT_ACTION_CHANGED, base.CurrentUnitController, categories[currentCategory][index].action, null);
            }
            else
            {
                base.CurrentUnitController.SetCurrentAction(SkillId.NONE);
                PandoraSingleton<NoticeManager>.Instance.SendNotice<UnitController, List<ActionStatus>>(Notices.CURRENT_UNIT_ACTION_CHANGED, base.CurrentUnitController, null);
            }
        }
    }

    private void OnInputCancel(bool canQuit)
    {
        if (!selectingCategory)
        {
            for (int i = 0; i < outerWheelIcons.Count; i++)
            {
                outerWheelIcons[i].toggle.set_isOn(false);
            }
            selectingCategory = true;
            base.CurrentUnitController.SetCurrentAction(SkillId.NONE);
            PandoraSingleton<NoticeManager>.Instance.SendNotice<UnitController, List<ActionStatus>>(Notices.CURRENT_UNIT_ACTION_CHANGED, base.CurrentUnitController, null);
            SelectCategory(currentCategory);
        }
        else if (canQuit)
        {
            base.gameObject.SetActive(value: false);
        }
    }

    private void OnInputAction(bool canActivateAction)
    {
        if (selectingCategory)
        {
            if (categories[currentCategory].Count > 0)
            {
                OpenCategory();
            }
        }
        else if (canActivateAction && categories[currentCategory][currentAction].Available)
        {
            activateAction = true;
            PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.CURRENT_UNIT_ACTION_CHANGED, base.CurrentUnitController, categories[currentCategory][currentAction].action, oldActions);
            categories[currentCategory][currentAction].action.Select();
            base.gameObject.SetActive(value: false);
        }
    }

    public void Update()
    {
        if (!isShow)
        {
            return;
        }
        if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("menu", 5) || PandoraSingleton<PandoraInput>.Instance.GetKeyUp("wheel", 5))
        {
            base.gameObject.SetActive(value: false);
        }
        else if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("navig_confirm", 5))
        {
            OnInputAction(!selectingCategory);
        }
        else if (PandoraSingleton<PandoraInput>.Instance.GetNegKeyUp("h", 5) || PandoraSingleton<PandoraInput>.Instance.GetNegKeyUp("navig_x", 5))
        {
            OnInputAction(canActivateAction: false);
        }
        else if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("cancel", 5))
        {
            OnInputCancel(canQuit: true);
        }
        else if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("h", 5) || PandoraSingleton<PandoraInput>.Instance.GetKeyUp("navig_x", 5))
        {
            OnInputCancel(canQuit: false);
        }
        else if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("v", 5) || PandoraSingleton<PandoraInput>.Instance.GetKeyUp("navig_y", 5))
        {
            if (selectingCategory)
            {
                int num = currentCategory - 1;
                if (num < 0)
                {
                    num = categories.Count - 1;
                }
                innerWheelIcons[num].SetSelected();
                SelectCategory(num);
            }
            else if (categories[currentCategory].Count > 0)
            {
                int num2 = currentAction - 1;
                if (num2 < 0)
                {
                    num2 = categories[currentCategory].Count - 1;
                }
                outerWheelIcons[num2].SetSelected();
                SetCurrentAction(num2);
            }
        }
        else
        {
            if (!PandoraSingleton<PandoraInput>.Instance.GetNegKeyUp("v", 5) && !PandoraSingleton<PandoraInput>.Instance.GetNegKeyUp("navig_y", 5))
            {
                return;
            }
            if (selectingCategory)
            {
                int num3 = currentCategory + 1;
                if (num3 >= categories.Count)
                {
                    num3 = 0;
                }
                innerWheelIcons[num3].SetSelected();
                SelectCategory(num3);
            }
            else if (categories[currentCategory].Count > 0)
            {
                int num4 = currentAction + 1;
                if (num4 >= categories[currentCategory].Count)
                {
                    num4 = 0;
                }
                outerWheelIcons[num4].SetSelected();
                SetCurrentAction(num4);
            }
        }
    }

    protected override void OnUnitChanged()
    {
        base.gameObject.SetActive(value: false);
    }
}
