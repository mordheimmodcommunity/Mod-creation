using System;
using UnityEngine;
using UnityEngine.UI;

public class ShopConfirmPopup : ConfirmationPopupView
{
    public SelectorGroup qtySelector;

    public Text totalValue;

    private int unitValue;

    private bool checkFunds;

    public void Show(string title, string desc, int availableQty, int unitValue, bool checkFunds, Action<bool, int> callback)
    {
        this.unitValue = unitValue;
        this.checkFunds = checkFunds;
        ((Graphic)totalValue).set_color(Color.white);
        confirmButton.SetDisabled(disabled: false);
        qtySelector.selections.Clear();
        for (int i = 1; i <= availableQty; i++)
        {
            qtySelector.selections.Add(i.ToString());
        }
        qtySelector.onValueChanged = delegate
        {
            UpdateTotal();
        };
        qtySelector.SetCurrentSel(0);
        UpdateTotal();
        base.ShowLocalized(title, desc, delegate (bool confirm)
        {
            callback(confirm, qtySelector.CurSel + 1);
        });
    }

    private void UpdateTotal()
    {
        int num = qtySelector.CurSel + 1;
        int num2 = num * unitValue;
        totalValue.set_text(num2.ToString());
        if (checkFunds)
        {
            if (PandoraSingleton<HideoutManager>.Instance.WarbandChest.GetGold() >= num2)
            {
                ((Graphic)totalValue).set_color(Color.white);
                confirmButton.SetDisabled(disabled: false);
            }
            else
            {
                ((Graphic)totalValue).set_color(Color.red);
                confirmButton.SetDisabled();
            }
        }
    }
}
