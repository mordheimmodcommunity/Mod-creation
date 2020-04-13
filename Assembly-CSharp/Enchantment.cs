using Prometheus;
using System.Collections.Generic;
using UnityEngine;

public class Enchantment
{
    private int actionCount;

    public bool damageApplied;

    public bool fxSpawned;

    private List<GameObject> fxs;

    private List<EnchantmentFxData> fxsData;

    private Unit unitTarget;

    public bool original;

    public uint guid;

    public EnchantmentId Id => Data.Id;

    public EnchantmentData Data
    {
        get;
        private set;
    }

    public int Duration
    {
        get;
        set;
    }

    public Unit Provider
    {
        get;
        private set;
    }

    public bool Innate
    {
        get;
        private set;
    }

    public EnchantmentId EnchantmentIdOnTurnStart => Data.EnchantmentIdOnTurnStart;

    public List<EnchantmentJoinAttributeData> AttributeModifiers
    {
        get;
        private set;
    }

    public List<EnchantmentInjuryModifierData> InjuryModifiers
    {
        get;
        private set;
    }

    public List<EnchantmentCostModifierData> CostModifiers
    {
        get;
        private set;
    }

    public List<EnchantmentCurseModifierData> CurseModifiers
    {
        get;
        private set;
    }

    public List<EnchantmentDamageModifierData> DamageModifiers
    {
        get;
        private set;
    }

    public List<EnchantmentBlockUnitActionData> ActionBlockers
    {
        get;
        private set;
    }

    public List<EnchantmentBlockBoneData> BoneBlockers
    {
        get;
        private set;
    }

    public List<EnchantmentBlockItemTypeData> ItemTypeBlockers
    {
        get;
        private set;
    }

    public List<EnchantmentEffectEnchantmentData> Effects
    {
        get;
        private set;
    }

    public List<EnchantmentRemoveEnchantmentTypeData> Immunities
    {
        get;
        private set;
    }

    public List<EnchantmentRemoveEnchantmentData> Remover
    {
        get;
        private set;
    }

    public string LabelName
    {
        get;
        private set;
    }

    public string LocalizedName => PandoraSingleton<LocalizationManager>.Instance.GetStringById(LabelName);

    public string LocalizedDescription => PandoraSingleton<LocalizationManager>.Instance.GetStringById("enchant_desc_" + Data.Name);

    public bool HasFx => fxsData != null && fxsData.Count > 0;

    public AllegianceId AllegianceId
    {
        get;
        private set;
    }

    public Enchantment(EnchantmentId id, Unit target, Unit provider, bool orig, bool innate, AllegianceId runeAllegianceId = AllegianceId.NONE, bool spawnFx = true)
    {
        Data = PandoraSingleton<DataFactory>.Instance.InitData<EnchantmentData>((int)id);
        Provider = provider;
        Duration = Data.Duration;
        Innate = innate;
        unitTarget = target;
        AllegianceId = runeAllegianceId;
        original = orig;
        guid = 0u;
        if (PandoraSingleton<MissionManager>.Exists())
        {
            guid = PandoraSingleton<MissionManager>.Instance.GetNextRTGUID();
        }
        string id2 = ((int)Id).ToConstantString();
        AttributeModifiers = PandoraSingleton<DataFactory>.Instance.InitData<EnchantmentJoinAttributeData>("fk_enchantment_id", id2);
        CostModifiers = PandoraSingleton<DataFactory>.Instance.InitData<EnchantmentCostModifierData>("fk_enchantment_id", id2);
        DamageModifiers = PandoraSingleton<DataFactory>.Instance.InitData<EnchantmentDamageModifierData>("fk_enchantment_id", id2);
        ActionBlockers = PandoraSingleton<DataFactory>.Instance.InitData<EnchantmentBlockUnitActionData>("fk_enchantment_id", id2);
        BoneBlockers = PandoraSingleton<DataFactory>.Instance.InitData<EnchantmentBlockBoneData>("fk_enchantment_id", id2);
        ItemTypeBlockers = PandoraSingleton<DataFactory>.Instance.InitData<EnchantmentBlockItemTypeData>("fk_enchantment_id", id2);
        Effects = PandoraSingleton<DataFactory>.Instance.InitData<EnchantmentEffectEnchantmentData>("fk_enchantment_id", id2);
        Immunities = PandoraSingleton<DataFactory>.Instance.InitData<EnchantmentRemoveEnchantmentTypeData>("fk_enchantment_id", id2);
        Remover = PandoraSingleton<DataFactory>.Instance.InitData<EnchantmentRemoveEnchantmentData>("fk_enchantment_id", id2);
        InjuryModifiers = PandoraSingleton<DataFactory>.Instance.InitData<EnchantmentInjuryModifierData>("fk_enchantment_id", id2);
        CurseModifiers = PandoraSingleton<DataFactory>.Instance.InitData<EnchantmentCurseModifierData>("fk_enchantment_id", id2);
        fxsData = PandoraSingleton<DataFactory>.Instance.InitData<EnchantmentFxData>("fk_enchantment_id", id2);
        fxs = new List<GameObject>();
        if (HasFx && spawnFx)
        {
            SpawnFx();
        }
        LabelName = "enchant_title_";
        string empty = string.Empty;
        empty = ((runeAllegianceId == AllegianceId.NONE) ? Data.Name : (((runeAllegianceId != AllegianceId.ORDER) ? "mark_" : "rune_") + Data.Name));
        LabelName += empty;
        if (!PandoraSingleton<LocalizationManager>.Instance.HasStringId(LabelName))
        {
            LabelName = "skill_name_";
            LabelName += empty;
            if (id == EnchantmentId.RUNE_ENERVATION_EFFECT || id == EnchantmentId.RUNE_ENERVATION_EFFECT_MSTR || id == EnchantmentId.RUNE_ENERVATION_1H_EFFECT)
            {
                LabelName = "Warhammer Enervation";
            }
            if (id == EnchantmentId.SKILL_INTENSITY_WOUNDS)
            {
                LabelName = "Intensity Wounds";
            }
            if (id == EnchantmentId.SKILL_IGNORE_PAIN_WOUND_COST || id == EnchantmentId.SKILL_IGNORE_PAIN_WOUND_COST_MSTR)
            {
                LabelName = "Ignore Pain Wounds";
            }
        }
    }

    public bool UpdateDuration(Unit currentUnit)
    {
        if (Duration > 0 && Provider == currentUnit && --Duration == 0)
        {
            RemoveFx();
            return true;
        }
        return false;
    }

    public bool UpdateStatus(UnitStateId unitStateId)
    {
        if (Data.RequireUnitState && unitStateId != Data.UnitStateIdRequired)
        {
            RemoveFx();
            return true;
        }
        return false;
    }

    public bool UpdateValidNextAction()
    {
        if (!Data.ValidNextAction)
        {
            return false;
        }
        actionCount++;
        return actionCount > 1;
    }

    public void RemoveFx()
    {
        if (fxs == null)
        {
            return;
        }
        for (int i = 0; i < fxs.Count; i++)
        {
            if (fxs[i] != null)
            {
                Object.Destroy(fxs[i]);
            }
        }
        fxs = null;
    }

    public bool HasEffectAttribute(AttributeId attrId)
    {
        int num = 0;
        for (int i = 0; i < Effects.Count; i++)
        {
            if (Effects[i].AttributeIdRoll == attrId)
            {
                num++;
            }
        }
        return num > 0;
    }

    public void SpawnFx(Unit target = null)
    {
        if (unitTarget == null && target != null)
        {
            unitTarget = target;
        }
        if (!fxSpawned && fxsData != null)
        {
            for (int i = 0; i < fxsData.Count; i++)
            {
                PandoraSingleton<Prometheus.Prometheus>.Instance.SpawnFx(fxsData[i].Fx, unitTarget, delegate (GameObject fx)
                {
                    if (fx != null)
                    {
                        fxSpawned = true;
                        if (fxs != null)
                        {
                            fxs.Add(fx);
                        }
                        else
                        {
                            Object.Destroy(fx);
                        }
                    }
                });
            }
        }
    }
}
