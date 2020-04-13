using System.Collections.Generic;

public class AttributeModList
{
    private readonly List<AttributeMod> list = new List<AttributeMod>();

    public int Count => list.Count;

    public AttributeMod this[int index] => list[index];

    public void Add(AttributeMod attributreMod, string effect = null, bool isPercent = false, bool isEnemyMod = false, bool negate = false)
    {
        if (negate)
        {
            attributreMod.Negate();
        }
        if (isPercent)
        {
            attributreMod.SetIsPercent(value: true);
        }
        if (isEnemyMod)
        {
            attributreMod.SetEnemyMod(value: true);
        }
        if (!string.IsNullOrEmpty(effect))
        {
            attributreMod.SetEffect(effect);
        }
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].IsSame(attributreMod))
            {
                AttributeMod value = list[i];
                value.modifier += attributreMod.modifier;
                value.count++;
                list[i] = value;
                return;
            }
        }
        list.Add(attributreMod);
    }

    public void AddRange(List<AttributeMod> attributreMods, string effect = null, bool isPercent = false, bool isEnemyMod = false, bool negate = false)
    {
        if (attributreMods != null)
        {
            for (int i = 0; i < attributreMods.Count; i++)
            {
                Add(attributreMods[i], effect, isPercent, isEnemyMod, negate);
            }
        }
    }

    public void AddRange(AttributeModList attributreMods)
    {
        list.AddRange(attributreMods.list);
    }

    public void Clear()
    {
        list.Clear();
    }
}
