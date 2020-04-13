public class UIStatDamage : UIStat
{
    protected override string GenerateStatsText(Unit unit, int stat, int? statMax)
    {
        bool flag = false;
        AttributeModList weaponDamageModifier = unit.GetWeaponDamageModifier(null);
        for (int i = 0; i < weaponDamageModifier.Count; i++)
        {
            if (weaponDamageModifier[i].IsTemp())
            {
                flag = true;
            }
        }
        string text = $"{unit.GetWeaponDamageMin(null)}-{unit.GetWeaponDamageMax(null)}";
        return (!flag) ? text : PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_stat_increase_value", text);
    }
}
