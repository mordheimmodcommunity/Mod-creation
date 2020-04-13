using System;
using System.Collections.Generic;

public class ListTabsModule : UIModule
{
    public List<ToggleEffects> tabs;

    private Action<int> tabChangedCallback;

    public int currentTab;

    public void Setup(Action<int> tabChanged, int current)
    {
        tabChangedCallback = tabChanged;
        for (int i = 0; i < tabs.Count; i++)
        {
            int index = i;
            tabs[i].onAction.AddListener(delegate
            {
                OnTabChanged(index);
            });
        }
        OnTabChanged(current);
    }

    private void OnTabChanged(int index)
    {
        currentTab = index;
        tabChangedCallback(index);
    }

    public void Next()
    {
        currentTab = ((currentTab + 1 < tabs.Count) ? (currentTab + 1) : 0);
        if (!tabs[currentTab].isActiveAndEnabled)
        {
            Next();
            return;
        }
        tabs[currentTab].SetOn();
        tabChangedCallback(currentTab);
    }

    public void Prev()
    {
        currentTab = ((currentTab - 1 < 0) ? (tabs.Count - 1) : (currentTab - 1));
        if (!tabs[currentTab].isActiveAndEnabled)
        {
            Prev();
            return;
        }
        tabs[currentTab].SetOn();
        tabChangedCallback(currentTab);
    }
}
