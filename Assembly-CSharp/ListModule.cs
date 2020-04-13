using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ListModule : UIModule
{
    public GameObject prefab;

    public ScrollGroup scrollGroup;

    private bool isFocus;

    private ListTabsModule listHeader;

    public void SetTabs(ListTabsModule tabs)
    {
        listHeader = tabs;
    }

    public void Set<T>(IList<T> items, Action<int, T, GameObject> itemAdded, Action<int, T> select, Action<int, T> confirm)
    {
        Clear();
        scrollGroup.Setup(prefab, hideBarIfEmpty: true);
        isFocus = true;
        for (int i = 0; i < items.Count; i++)
        {
            T item = items[i];
            GameObject gameObject = scrollGroup.AddToList(null, null);
            ToggleEffects component = gameObject.GetComponent<ToggleEffects>();
            int idx = items.Count;
            ((UnityEvent<bool>)(object)component.toggle.onValueChanged).AddListener((UnityAction<bool>)delegate (bool isOn)
            {
                if (isOn)
                {
                    select(idx, item);
                }
            });
            component.onAction.AddListener(delegate
            {
                confirm(idx, item);
            });
            itemAdded(idx, item, gameObject);
        }
        scrollGroup.RepositionScrollListOnNextFrame();
    }

    public void Clear()
    {
        scrollGroup.ClearList();
        isFocus = false;
    }

    private void OnDisable()
    {
        Clear();
    }

    private void Update()
    {
        if (isFocus)
        {
            if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("h"))
            {
                listHeader.Next();
            }
            else if (PandoraSingleton<PandoraInput>.Instance.GetNegKeyUp("h"))
            {
                listHeader.Prev();
            }
        }
    }

    public void Reset()
    {
        Clear();
        listHeader = null;
    }
}
