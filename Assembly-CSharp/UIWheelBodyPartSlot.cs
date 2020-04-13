using UnityEngine;
using UnityEngine.UI;

public class UIWheelBodyPartSlot : UIWheelSlot
{
    public BodyPartId bodyPart;

    public Sprite bodyPartSprite;

    public Text idNum;

    protected void Start()
    {
        idNum = GetComponentsInChildren<Text>(includeInactive: true)[0];
        ((Component)(object)idNum).gameObject.SetActive(value: false);
    }

    public void SetLocked(bool locked)
    {
        if (locked)
        {
            icon.set_sprite(PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("item/none", cached: true));
            ((Selectable)slot.toggle).set_interactable(false);
        }
        else
        {
            icon.set_sprite(bodyPartSprite);
            ((Selectable)slot.toggle).set_interactable(true);
        }
    }

    public bool IsLocked()
    {
        return !((Selectable)slot.toggle).get_interactable();
    }
}
