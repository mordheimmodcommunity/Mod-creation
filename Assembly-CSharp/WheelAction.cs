using UnityEngine;

public class WheelAction
{
    public string name;

    public string description;

    public UIWheelController.Category category;

    public bool available;

    public Sprite icon;

    public ActionStatus action;

    public Item item;

    public bool Available => available;

    public WheelAction(ActionStatus action, Item item, UIWheelController.Category category)
        : this(action.LocalizedName, action.LocalizedDescription, category, action.GetIcon())
    {
        this.action = action;
        this.item = item;
        available = action.Available;
    }

    public WheelAction(ActionStatus action, UIWheelController.Category category)
        : this(action.LocalizedName, action.LocalizedDescription, category, action.GetIcon())
    {
        this.action = action;
        item = null;
        available = action.Available;
    }

    public WheelAction(Item item)
        : this(item.LocalizedName, item.GetLocalizedDescription(null), UIWheelController.Category.INVENTORY, item.GetIcon())
    {
        this.item = item;
        available = false;
    }

    private WheelAction(string name, string description, UIWheelController.Category category, Sprite icon)
    {
        this.name = name;
        this.description = description;
        this.category = category;
        this.icon = icon;
        available = true;
    }

    public Sprite GetIcon()
    {
        return icon;
    }
}
