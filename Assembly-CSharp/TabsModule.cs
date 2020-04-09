using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabsModule : UIModule
{
	public delegate bool IsAvailable(out string reason);

	public RectTransform anchor;

	public GameObject prefab;

	public Sprite lockIcon;

	private TitleModule titleModule;

	public GameObject tabIcons;

	public List<TabIcon> icons = new List<TabIcon>();

	public ButtonGroup nextTab;

	public ButtonGroup prevTab;

	private int currentIdx;

	public Action<TabIcon> onIconEnter;

	public Action<TabIcon> onIconExit;

	public override void Init()
	{
		base.Init();
	}

	public void Setup(TitleModule title)
	{
		titleModule = title;
		nextTab.SetAction("switch_tab", null);
		nextTab.OnAction(Next, mouseOnly: false);
		prevTab.SetAction("switch_tab", null, 0, negative: true);
		prevTab.OnAction(Prev, mouseOnly: false);
	}

	public virtual TabIcon AddTabIcon(HideoutManager.State state, int index, string spriteName = null, string loc = null, IsAvailable isAvailable = null)
	{
		if (string.IsNullOrEmpty(spriteName))
		{
			spriteName = "hideout_nav/" + state.ToLowerString();
		}
		if (isAvailable == null)
		{
			isAvailable = DefaultIsAvailable;
		}
		TabIcon tabIcon = icons[index];
		tabIcon.Init();
		PandoraSingleton<AssetBundleLoader>.Instance.LoadResourceAsync<Sprite>(spriteName, delegate(UnityEngine.Object go)
		{
			if (tabIcon != null && (UnityEngine.Object)(object)tabIcon.icon != null)
			{
				tabIcon.icon.set_sprite((Sprite)go);
			}
		}, cached: true);
		tabIcon.state = state;
		tabIcon.IsAvailableDelegate = isAvailable;
		tabIcon.titleText = (loc ?? ("hideout_" + state.ToLowerString()));
		tabIcon.exclamationMark.SetActive(value: false);
		tabIcon.impossibleMark.SetActive(value: false);
		((Selectable)tabIcon.toggle.toggle).set_interactable(false);
		tabIcon.toggle.onAction.AddListener(delegate
		{
			SetCurrentTabs(index);
		});
		tabIcon.toggle.onPointerEnter.AddListener(delegate
		{
			OnTabIconEnter(tabIcon);
		});
		tabIcon.toggle.onPointerExit.AddListener(delegate
		{
			OnTabIconExit(tabIcon);
		});
		return tabIcon;
	}

	public void OnTabIconEnter(TabIcon tabIcon)
	{
		for (int i = 0; i < icons.Count; i++)
		{
			icons[i].textContent.gameObject.SetActive(value: false);
		}
		tabIcon.textContent.gameObject.SetActive(value: true);
		((Component)(object)tabIcon.tabTitle).gameObject.SetActive(value: true);
		tabIcon.tabTitle.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById(tabIcon.titleText));
		if (PandoraSingleton<HideoutManager>.Instance.StateMachine.GetActiveStateId() != 0 && !tabIcon.available)
		{
			if (!((Component)(object)tabIcon.tabReasonTitle).gameObject.activeSelf)
			{
				((Component)(object)tabIcon.tabReasonTitle).gameObject.SetActive(value: true);
			}
			string stringById = PandoraSingleton<LocalizationManager>.Instance.GetStringById(tabIcon.reason);
			if (tabIcon.tabReasonTitle.get_text() != stringById)
			{
				tabIcon.tabReasonTitle.set_text(stringById);
			}
		}
		else if (((Component)(object)tabIcon.tabReasonTitle).gameObject.activeSelf)
		{
			((Component)(object)tabIcon.tabReasonTitle).gameObject.SetActive(value: false);
		}
		if (onIconEnter != null)
		{
			onIconEnter(tabIcon);
		}
	}

	public void OnTabIconExit(TabIcon tabIcon)
	{
		if (tabIcon.textContent.gameObject.activeSelf)
		{
			tabIcon.textContent.gameObject.SetActive(value: false);
		}
		if (onIconExit != null)
		{
			onIconExit(tabIcon);
		}
	}

	public bool DefaultIsAvailable(out string reason)
	{
		reason = string.Empty;
		return true;
	}

	public virtual void Refresh()
	{
		for (int i = 0; i < icons.Count; i++)
		{
			ActivateIcon(icons[i], icons[i].IsAvailableDelegate(out string reason), reason);
		}
	}

	private void ActivateIcon(TabIcon tabIcon, bool active, string reason)
	{
		tabIcon.available = active;
		tabIcon.canvasGroup.alpha = ((!active) ? 0.75f : 1f);
		tabIcon.toggle.highlightOnOver = active;
		if (((Selectable)tabIcon.toggle.toggle).get_interactable() != active)
		{
			((Selectable)tabIcon.toggle.toggle).set_interactable(active);
		}
		if (tabIcon.impossibleMark.activeSelf)
		{
			tabIcon.impossibleMark.SetActive(value: false);
		}
		if (active)
		{
			if (tabIcon.icon.get_overrideSprite() != null)
			{
				tabIcon.icon.set_overrideSprite((Sprite)null);
			}
			tabIcon.reason = string.Empty;
		}
		else
		{
			if (tabIcon.icon.get_overrideSprite() != lockIcon)
			{
				tabIcon.icon.set_overrideSprite(lockIcon);
			}
			tabIcon.reason = reason;
		}
	}

	public void SetCurrentTab(HideoutManager.State state)
	{
		for (int i = 0; i < icons.Count; i++)
		{
			if (icons[i].state == state)
			{
				currentIdx = i;
				break;
			}
		}
		Refresh();
		if (!tabIcons.activeSelf)
		{
			tabIcons.SetActive(value: true);
		}
		if (titleModule != null)
		{
			titleModule.Set(icons[currentIdx].titleText, showBg: false);
		}
		icons[currentIdx].toggle.toggle.set_isOn(true);
	}

	public void SetExclamation(params int[] marks)
	{
		for (int i = 0; i < icons.Count; i++)
		{
			if (icons[i].exclamationMark.activeSelf)
			{
				icons[i].exclamationMark.SetActive(value: false);
			}
		}
		for (int j = 0; j < marks.Length; j++)
		{
			if (!icons[marks[j]].exclamationMark.activeSelf)
			{
				icons[marks[j]].exclamationMark.SetActive(value: true);
			}
		}
	}

	protected virtual void SetCurrentTabs(int index)
	{
		if (IsTabAvailable(index))
		{
			currentIdx = index;
			BaseHideoutUnitState baseHideoutUnitState = PandoraSingleton<HideoutManager>.Instance.StateMachine.GetActiveState() as BaseHideoutUnitState;
			if (baseHideoutUnitState != null)
			{
				baseHideoutUnitState.CheckChangesAndChangeState(icons[currentIdx].state);
				return;
			}
			int state = (int)icons[currentIdx].state;
			if (PandoraSingleton<HideoutManager>.Instance.StateMachine.GetActiveStateId() != state)
			{
				PandoraSingleton<HideoutManager>.Instance.StateMachine.ChangeState(state);
			}
		}
		else
		{
			icons[currentIdx].toggle.toggle.set_isOn(true);
		}
	}

	protected bool IsTabAvailable(int index)
	{
		return icons[index].available;
	}

	public bool IsTabAvailable(HideoutManager.State iconState)
	{
		for (int i = 0; i < icons.Count; i++)
		{
			if (icons[i].state == iconState)
			{
				return IsTabAvailable(i);
			}
		}
		return false;
	}

	public void Prev()
	{
		if (tabIcons.activeSelf)
		{
			int num = currentIdx;
			do
			{
				num--;
				num = ((num < icons.Count) ? num : 0);
				num = ((num >= 0) ? num : (icons.Count - 1));
			}
			while (!IsTabAvailable(num));
			SetCurrentTabs(num);
		}
	}

	public void Next()
	{
		if (tabIcons.activeSelf)
		{
			int num = currentIdx;
			do
			{
				num++;
				num = ((num < icons.Count) ? num : 0);
				num = ((num >= 0) ? num : (icons.Count - 1));
			}
			while (!IsTabAvailable(num));
			SetCurrentTabs(num);
		}
	}
}
