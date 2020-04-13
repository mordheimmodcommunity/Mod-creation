using UnityEngine.UI;

public class UIUnitStatsController : UIUnitControllerChanged
{
    public UnitStatsGroup[] stats;

    public Text unitClass;

    protected virtual void Awake()
    {
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.UNIT_ATTRIBUTES_CHANGED, OnAttributesChanged);
    }

    private void OnAttributesChanged()
    {
        Unit unit = PandoraSingleton<NoticeManager>.Instance.Parameters[0] as Unit;
        if (base.CurrentUnitController != null && base.CurrentUnitController.unit == unit)
        {
            base.UpdateUnit = true;
        }
    }

    public void AttributesChanged()
    {
        if (base.CurrentUnitController != null)
        {
            Unit unit = base.CurrentUnitController.unit;
            for (int i = 0; i < stats.Length; i++)
            {
                stats[i].Set(base.CurrentUnitController.unit);
            }
            unitClass.set_text(unit.LocalizedType);
        }
    }

    protected override void OnUnitChanged()
    {
        AttributesChanged();
    }
}
