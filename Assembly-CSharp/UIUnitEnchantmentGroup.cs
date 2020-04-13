using UnityEngine;
using UnityEngine.UI;

public class UIUnitEnchantmentGroup : MonoBehaviour
{
    public bool isBuff;

    public Image iconBuff;

    public Image iconDebuff;

    public Text nameBuff;

    public Text nameDebuff;

    public Text description;

    public Text duration;

    public void Set(Enchantment enchantment)
    {
        isBuff = (enchantment.Data.EffectTypeId == EffectTypeId.BUFF);
        if (isBuff)
        {
            ((Component)(object)nameBuff).gameObject.SetActive(value: true);
            ((Component)(object)iconBuff).gameObject.SetActive(value: true);
            ((Component)(object)nameDebuff).gameObject.SetActive(value: false);
            ((Component)(object)iconDebuff).gameObject.SetActive(value: false);
            nameBuff.set_text(enchantment.LocalizedName);
        }
        else
        {
            ((Component)(object)nameBuff).gameObject.SetActive(value: false);
            ((Component)(object)iconBuff).gameObject.SetActive(value: false);
            ((Component)(object)nameDebuff).gameObject.SetActive(value: true);
            ((Component)(object)iconDebuff).gameObject.SetActive(value: true);
            nameDebuff.set_text(enchantment.LocalizedName);
        }
        duration.set_text(enchantment.Duration.ToConstantString());
        description.set_text(enchantment.LocalizedDescription);
    }
}
