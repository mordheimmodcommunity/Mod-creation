using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HireUnitSelectionModule : UIModule
{
    public ScrollGroup scrollGroup;

    public GameObject prefabItem;

    public List<ToggleEffects> items;

    public override void Init()
    {
        base.Init();
        scrollGroup.Setup(prefabItem, hideBarIfEmpty: false);
    }

    public void Set(List<UnitMenuController> hireUnits, Action prev, Action next, Action<int> unitSelected, Action<int> doubleClick)
    {
        Clear();
        for (int i = 0; i < hireUnits.Count; i++)
        {
            GameObject gameObject = scrollGroup.AddToList(null, null);
            HireUnitDescription component = gameObject.GetComponent<HireUnitDescription>();
            component.Set(hireUnits[i].unit);
            ToggleEffects component2 = gameObject.GetComponent<ToggleEffects>();
            items.Add(component2);
            component2.onAction.RemoveAllListeners();
            int index = i;
            component2.onSelect.AddListener(delegate
            {
                unitSelected(index);
            });
            if (doubleClick != null)
            {
                component2.onDoubleClick.AddListener(delegate
                {
                    doubleClick(index);
                });
                component.btnBuy.onAction.AddListener(delegate
                {
                    doubleClick(index);
                });
            }
            else
            {
                component.btnBuy.transform.GetChild(0).gameObject.SetActive(value: false);
            }
        }
        SelectFirstUnit();
    }

    public void SelectFirstUnit()
    {
        if (base.gameObject.activeSelf)
        {
            StartCoroutine(RealignOnNextFrame());
        }
    }

    private IEnumerator RealignOnNextFrame()
    {
        yield return null;
        OnUnitSelected(0);
    }

    public void OnUnitSelected(int index)
    {
        items[index].SetOn();
        scrollGroup.RealignList(isOn: true, index, force: true);
    }

    public void Clear()
    {
        scrollGroup.ClearList();
        items.Clear();
    }

    internal void Set(List<UnitMenuController> hireUnits, Action Prev, Action Next, Action<int> UnitConfirmed, object p)
    {
        throw new NotImplementedException();
    }
}
