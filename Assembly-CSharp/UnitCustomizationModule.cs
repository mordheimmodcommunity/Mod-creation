using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitCustomizationModule : UIModule
{
    [SerializeField]
    private ScrollGroup styleList;

    [SerializeField]
    private GameObject listItemTemplate;

    [SerializeField]
    private ToggleEffects[] tabs;

    [SerializeField]
    private Text titleField;

    public Action<int> onTabSelected;

    public bool IsFocused
    {
        get;
        private set;
    }

    public override void Init()
    {
        base.Init();
        styleList.Setup(listItemTemplate, hideBarIfEmpty: true);
        tabs[0].toggle.set_isOn(true);
        for (int i = 0; i < tabs.Length; i++)
        {
            int idx = i;
            tabs[i].onAction.AddListener(delegate
            {
                TabSelected(idx);
            });
        }
        Clear();
    }

    public void Refresh(List<string> styles, Action<int> onStyleSelected, string title)
    {
        titleField.set_text(title);
        styleList.ClearList();
        if (styles != null)
        {
            for (int i = 0; i < styles.Count; i++)
            {
                GameObject gameObject = styleList.AddToList(null, null);
                gameObject.SetActive(value: true);
                gameObject.GetComponentInChildren<Text>().set_text(styles[i]);
                int idx = i;
                gameObject.GetComponent<ToggleEffects>().onAction.AddListener(delegate
                {
                    SetFocused(focused: true);
                    onStyleSelected(idx);
                });
            }
        }
        styleList.RealignList(isOn: true, 0, force: true);
    }

    public void SetSelectedStyle(int idx)
    {
        StartCoroutine(DelaySetSelected(idx));
    }

    private IEnumerator DelaySetSelected(int idx)
    {
        yield return 0;
        styleList.items[idx].SetSelected(force: true);
        styleList.RealignList(isOn: true, idx, force: true);
    }

    public void Clear()
    {
        styleList.ClearList();
        SetTabsVisible(visible: false);
        ((Component)(object)titleField).gameObject.SetActive(value: false);
    }

    private void TabSelected(int tabIdx)
    {
        SetFocused(focused: true);
        if (onTabSelected != null)
        {
            onTabSelected(tabIdx);
        }
    }

    public void SetTabsVisible(bool visible = true)
    {
        ((Component)(object)titleField).gameObject.SetActive(!visible);
        for (int i = 0; i < tabs.Length; i++)
        {
            tabs[i].gameObject.SetActive(visible);
        }
    }

    public int GetSelectedTabIndex()
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            if (tabs[i].toggle.get_isOn())
            {
                return i;
            }
        }
        return -1;
    }

    public void SetFocused(bool focused)
    {
        IsFocused = focused;
    }

    private void Update()
    {
        if (IsFocused && tabs[0].isActiveAndEnabled && tabs[1].isActiveAndEnabled)
        {
            if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("h"))
            {
                tabs[1].SetOn();
                TabSelected(1);
            }
            else if (PandoraSingleton<PandoraInput>.Instance.GetNegKeyUp("h"))
            {
                tabs[0].SetOn();
                TabSelected(0);
            }
        }
    }
}
