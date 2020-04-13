public class UIUnitEnchantmentsContent : ContentView<UIUnitEnchantmentGroup, Enchantment>
{
    protected override void OnAdd(UIUnitEnchantmentGroup component, Enchantment obj)
    {
        component.Set(obj);
    }
}
