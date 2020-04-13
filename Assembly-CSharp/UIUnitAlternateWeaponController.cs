public class UIUnitAlternateWeaponController : UIUnitControllerChanged
{
    public UIUnitAlternateWeaponGroup mainHand;

    public UIUnitAlternateWeaponGroup offHand;

    protected virtual void Awake()
    {
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.UNIT_WEAPON_CHANGED, OnWeaponSwapped);
    }

    private void OnWeaponSwapped()
    {
        UnitController y = PandoraSingleton<NoticeManager>.Instance.Parameters[0] as UnitController;
        if (base.CurrentUnitController != null && base.CurrentUnitController == y)
        {
            UnitWeaponSwapped(base.CurrentUnitController);
        }
    }

    public void UnitWeaponSwapped(UnitController unitController)
    {
        Item item = unitController.unit.Items[(int)unitController.unit.InactiveWeaponSlot];
        Item item2 = unitController.unit.Items[(int)(unitController.unit.InactiveWeaponSlot + 1)];
        if (unitController.CanSwitchWeapon())
        {
            mainHand.gameObject.SetActive(value: true);
            offHand.gameObject.SetActive(value: true);
            mainHand.Set(item);
            offHand.Set(item2);
        }
        else
        {
            mainHand.gameObject.SetActive(value: false);
            offHand.gameObject.SetActive(value: false);
        }
    }

    protected override void OnUnitChanged()
    {
        if (base.CurrentUnitController != null)
        {
            UnitWeaponSwapped(base.CurrentUnitController);
            return;
        }
        mainHand.gameObject.SetActive(value: false);
        offHand.gameObject.SetActive(value: false);
    }
}
