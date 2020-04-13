using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitFactory : PandoraSingleton<UnitFactory>
{
    public enum LoadState
    {
        NONE,
        PREFABING,
        INITIALIZING,
        DONE,
        COUNT
    }

    public class UnitCreationData
    {
        public WarbandController warCtrlr;

        public UnitController ctrlr;

        public uint guid;

        public int warbandPos;

        public UnitSave unitSave;

        public Vector3 position;

        public Quaternion rotation;

        public bool merge;

        public LoadState state;

        public string prefab;
    }

    private List<UnitCreationData> creationQueue;

    private int unitCreatedCount;

    private int index;

    private List<GameObject> prefabs;

    public float LoadingPercent
    {
        get
        {
            float num = 0f;
            for (int i = 0; i < creationQueue.Count; i++)
            {
                switch (creationQueue[i].state)
                {
                    case LoadState.INITIALIZING:
                        num = ((!(creationQueue[i].ctrlr != null)) ? (num + 1f) : (num + (1f + creationQueue[i].ctrlr.GetBodypartPercentLoaded() * 10f)));
                        break;
                    case LoadState.DONE:
                        num += 11f;
                        break;
                }
            }
            return num / (float)(creationQueue.Count * 11);
        }
    }

    public bool IsCreating()
    {
        for (int i = 0; i < creationQueue.Count; i++)
        {
            if (creationQueue[i].state != LoadState.DONE)
            {
                return true;
            }
        }
        return false;
    }

    private void Awake()
    {
        creationQueue = new List<UnitCreationData>();
        prefabs = new List<GameObject>();
        unitCreatedCount = 0;
    }

    private void Update()
    {
        for (int i = 0; i < creationQueue.Count; i++)
        {
            switch (creationQueue[i].state)
            {
                case LoadState.NONE:
                    CreateUnit(creationQueue[i]);
                    break;
                case LoadState.PREFABING:
                    {
                        GameObject gameObject = FindPrefab(creationQueue[i].prefab);
                        if (gameObject != null)
                        {
                            InitUnit(creationQueue[i], gameObject);
                        }
                        break;
                    }
                case LoadState.INITIALIZING:
                    if (creationQueue[i].ctrlr.Initialized)
                    {
                        UnitCreationDone(creationQueue[i]);
                    }
                    break;
            }
        }
    }

    public void GenerateUnit(WarbandController warCtrlr, int warbandPos, UnitSave unit, Vector3 position, Quaternion rotation, bool merge = true)
    {
        UnitCreationData unitCreationData = new UnitCreationData();
        unitCreationData.warCtrlr = warCtrlr;
        unitCreationData.warbandPos = warbandPos;
        unitCreationData.unitSave = unit;
        unitCreationData.position = position;
        unitCreationData.rotation = rotation;
        unitCreationData.merge = merge;
        unitCreationData.guid = PandoraSingleton<Hermes>.Instance.GetNextGUID();
        creationQueue.Add(unitCreationData);
        PandoraDebug.LogInfo("[UnitFactory] Adding Unit : " + unit.stats.Name + " to queue. Position (" + position + ")", "LOADING");
    }

    private void CreateUnit(UnitCreationData creationData)
    {
        PandoraDebug.LogInfo("[UnitFactory] Creating unit : " + (UnitId)creationData.unitSave.stats.id, "LOADING");
        UnitData unitData = PandoraSingleton<DataFactory>.Instance.InitData<UnitData>(creationData.unitSave.stats.id);
        string prefab = "prefabs/characters/" + unitData.UnitBaseId.ToString().ToLower();
        creationData.prefab = unitData.UnitBaseId.ToString().ToLower();
        creationData.state = LoadState.PREFABING;
        GameObject x = FindPrefab(creationData.prefab);
        if (x == null)
        {
            StartCoroutine(LoadPrefab(prefab));
        }
    }

    private IEnumerator LoadPrefab(string prefab)
    {
        PandoraDebug.LogInfo("[UnitFactory] Creating unit name = " + prefab, "LOADING");
        ResourceRequest req = Resources.LoadAsync(prefab);
        yield return req;
        prefabs.Add((GameObject)req.asset);
    }

    private GameObject FindPrefab(string prefab)
    {
        for (int i = 0; i < prefabs.Count; i++)
        {
            if (prefabs[i].name == prefab)
            {
                return prefabs[i];
            }
        }
        return null;
    }

    public void InitUnit(UnitCreationData creationData, GameObject prefab)
    {
        creationData.state = LoadState.INITIALIZING;
        GameObject gameObject = (GameObject)Object.Instantiate(prefab, creationData.position, creationData.rotation);
        UnitController component = gameObject.GetComponent<UnitController>();
        creationData.warCtrlr.unitCtrlrs[creationData.warbandPos] = component;
        creationData.ctrlr = component;
        PandoraDebug.LogInfo("[UnitFactory] Unit BEFORE INIT" + unitCreatedCount + " created at position " + component.transform.position, "LOADING");
        component.FirstSyncInit(creationData.unitSave, creationData.guid, creationData.warCtrlr.idx, creationData.warCtrlr.playerIdx, creationData.warCtrlr.playerTypeId, creationData.warbandPos, creationData.merge);
        component.InitMissionUnit(creationData.unitSave, creationData.guid, creationData.warCtrlr.idx, creationData.warCtrlr.playerIdx, creationData.warCtrlr.playerTypeId, creationData.warbandPos, creationData.merge);
    }

    public void UnitCreationDone(UnitCreationData creationData)
    {
        PandoraDebug.LogInfo("[UnitFactory] Unit " + unitCreatedCount + " created at position " + creationData.warbandPos, "LOADING");
        unitCreatedCount++;
        creationData.state = LoadState.DONE;
        creationData.warCtrlr.OnUnitCreated(creationData.ctrlr);
    }

    public IEnumerator CloneUnitCtrlr(UnitController srcCtrlr, int rank, int rating, Vector3 pos, Quaternion rot)
    {
        GameObject clone = (GameObject)Object.Instantiate(srcCtrlr.gameObject, pos, rot);
        for (int m = 0; m < clone.transform.childCount; m++)
        {
            Transform childTr = clone.transform.GetChild(m);
            if (childTr.name.ToLowerString() == BoneId.RIG_WEAPONR.ToLowerString() || childTr.name.ToLowerString() == BoneId.RIG_WEAPONL.ToLowerString())
            {
                for (int i = childTr.childCount - 1; i >= 0; i--)
                {
                    Object.Destroy(childTr.GetChild(i).gameObject);
                }
            }
        }
        Projectile[] projs = clone.GetComponentsInChildren<Projectile>();
        for (int l = 0; l < projs.Length; l++)
        {
            Object.Destroy(projs[l].gameObject);
        }
        UnitController clonedCtrlr = clone.GetComponent<UnitController>();
        List<AttributeData> attributesData = PandoraSingleton<DataFactory>.Instance.InitData<AttributeData>();
        Unit clonedUnit = Unit.GenerateUnit(srcCtrlr.unit.Id, rank);
        int ratingPool = clonedUnit.GetRating();
        AddCombatStyleSet(excludedCombatStyleId: AddCombatStyleSet(PandoraSingleton<GameManager>.Instance.LocalTyche, ref ratingPool, clonedUnit, UnitSlotId.SET1_MAINHAND), tyche: PandoraSingleton<GameManager>.Instance.LocalTyche, ratingPool: ref ratingPool, unit: clonedUnit, slotId: UnitSlotId.SET2_MAINHAND);
        RaiseAttributes(PandoraSingleton<GameManager>.Instance.LocalTyche, attributesData, clonedUnit, ref ratingPool, rating);
        AddSkillSpells(PandoraSingleton<GameManager>.Instance.LocalTyche, clonedUnit, ref ratingPool, rating);
        int counter = 9999;
        BoostItemsQuality(PandoraSingleton<GameManager>.Instance.LocalTyche, clonedUnit, ItemQualityId.GOOD, ref ratingPool, ref counter, rating);
        BoostItemsQuality(PandoraSingleton<GameManager>.Instance.LocalTyche, clonedUnit, ItemQualityId.BEST, ref ratingPool, ref counter, rating);
        uint nextGuid = PandoraSingleton<Hermes>.Instance.GetNextGUID();
        clonedCtrlr.FirstSyncInit(clonedUnit.UnitSave, nextGuid, srcCtrlr.unit.warbandIdx, srcCtrlr.GetWarband().playerIdx, srcCtrlr.GetWarband().playerTypeId, srcCtrlr.GetWarband().unitCtrlrs.Count, merge: false, loadBodyParts: false);
        clonedCtrlr.InitMissionUnit(clonedUnit.UnitSave, nextGuid, srcCtrlr.unit.warbandIdx, srcCtrlr.GetWarband().playerIdx, srcCtrlr.GetWarband().playerTypeId, srcCtrlr.GetWarband().unitCtrlrs.Count, loadBodyParts: false);
        yield return null;
        OverrideItem(UnitSlotId.HELMET, srcCtrlr, clonedCtrlr);
        OverrideItem(UnitSlotId.ARMOR, srcCtrlr, clonedCtrlr);
        List<Item> returnedItems = new List<Item>();
        clonedCtrlr.unit.ClearMutations();
        for (int k = 0; k < srcCtrlr.unit.Mutations.Count; k++)
        {
            clonedCtrlr.unit.AddMutation(srcCtrlr.unit.Mutations[k].Data.Id, returnedItems);
        }
        clonedCtrlr.unit.ClearInjuries();
        for (int j = 0; j < srcCtrlr.unit.Injuries.Count; j++)
        {
            clonedCtrlr.unit.AddInjury(srcCtrlr.unit.Injuries[j].Data, 0, returnedItems);
        }
        clonedCtrlr.AICtrlr.SetAIProfile(AiProfileId.BASE_SCOUT);
        if (returnedItems.Count > 0)
        {
            clonedCtrlr.RefreshEquipments();
            clonedCtrlr.SwitchWeapons(UnitSlotId.SET1_MAINHAND);
        }
        clonedCtrlr.InitBodyTrails();
        clonedUnit.UnitSave.isReinforcement = true;
        PandoraSingleton<MissionManager>.Instance.IncludeUnit(clonedCtrlr, srcCtrlr.GetWarband(), pos, rot);
        Debug.Log("Spawned Unit rating = " + clonedCtrlr.unit.GetRating());
    }

    private void OverrideItem(UnitSlotId slotId, UnitController srcCtrlr, UnitController clonedCtlr)
    {
        ItemSave itemSave = new ItemSave(ItemId.NONE);
        Thoth.Copy(srcCtrlr.unit.Items[(int)slotId].Save, itemSave);
        clonedCtlr.unit.Items[(int)slotId] = new Item(itemSave);
    }

    public static CombatStyleId AddCombatStyleSet(Tyche tyche, ref int ratingPool, Unit unit, UnitSlotId slotId, CombatStyleId excludedCombatStyleId = CombatStyleId.NONE, ItemQualityId qualityId = ItemQualityId.NORMAL, bool generateRuneMark = false, List<Item> newItems = null)
    {
        PandoraDebug.LogInfo("Add combat style set for " + unit.Data.Id);
        List<UnitJoinCombatStyleData> list = PandoraSingleton<DataFactory>.Instance.InitData<UnitJoinCombatStyleData>("fk_unit_id", ((int)unit.Data.Id).ToConstantString());
        List<UnitJoinCombatStyleData> list2 = new List<UnitJoinCombatStyleData>();
        bool flag = unit.GetMutationId(UnitSlotId.SET1_MAINHAND) != MutationId.NONE;
        bool flag2 = unit.GetMutationId(UnitSlotId.SET1_OFFHAND) != 0 || unit.GetInjury(UnitSlotId.SET1_OFFHAND) != InjuryId.NONE;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].CombatStyleId != excludedCombatStyleId)
            {
                CombatStyleData combatStyleData = PandoraSingleton<DataFactory>.Instance.InitData<CombatStyleData>((int)list[i].CombatStyleId);
                if ((!flag || combatStyleData.ItemTypeIdMain == ItemTypeId.MELEE_1H) && (!flag2 || (combatStyleData.ItemTypeIdOff == ItemTypeId.MELEE_1H && combatStyleData.ItemTypeIdMain == ItemTypeId.MELEE_1H)) && (excludedCombatStyleId < CombatStyleId.RANGE || list[i].CombatStyleId < CombatStyleId.RANGE))
                {
                    list2.Add(list[i]);
                }
            }
        }
        if (list2.Count == 0)
        {
            return CombatStyleId.NONE;
        }
        UnitJoinCombatStyleData randomRatio = UnitJoinCombatStyleData.GetRandomRatio(list2, tyche);
        if (excludedCombatStyleId != 0 && excludedCombatStyleId < CombatStyleId.RANGE && randomRatio.CombatStyleId < CombatStyleId.RANGE)
        {
            return CombatStyleId.NONE;
        }
        CombatStyleData combatStyleData2 = PandoraSingleton<DataFactory>.Instance.InitData<CombatStyleData>((int)randomRatio.CombatStyleId);
        PandoraDebug.LogInfo("Using combat style " + combatStyleData2.Id + " for unit " + unit.Data.Id);
        Item procItem = GetProcItem(tyche, ref ratingPool, unit, combatStyleData2.UnitSlotIdMain, combatStyleData2.ItemTypeIdMain, qualityId, generateRuneMark, flag);
        unit.EquipItem(slotId, procItem);
        newItems?.Add(procItem);
        if (combatStyleData2.ItemTypeIdOff != 0)
        {
            procItem = GetProcItem(tyche, ref ratingPool, unit, combatStyleData2.UnitSlotIdOff, combatStyleData2.ItemTypeIdOff, qualityId, generateRuneMark, flag2);
            unit.EquipItem(slotId + 1, procItem);
            newItems?.Add(procItem);
        }
        return combatStyleData2.Id;
    }

    public static ArmorStyleId AddArmorStyleSet(Tyche tyche, ref int ratingPool, Unit unit, ItemQualityId qualityId = ItemQualityId.NORMAL, bool generateArmor = true, bool generateHelmet = true, bool generateArmorRuneMark = false, bool generateHelmetRuneMark = false, List<Item> newItems = null)
    {
        List<UnitJoinArmorStyleData> list = PandoraSingleton<DataFactory>.Instance.InitData<UnitJoinArmorStyleData>("fk_unit_id", ((int)unit.Id).ToConstantString());
        List<UnitJoinArmorStyleData> list2 = new List<UnitJoinArmorStyleData>();
        for (int i = 0; i < list.Count; i++)
        {
            ArmorStyleData armorStyleData = PandoraSingleton<DataFactory>.Instance.InitData<ArmorStyleData>((int)list[i].ArmorStyleId);
            if (unit.GetMutationId(UnitSlotId.ARMOR) == MutationId.NONE || armorStyleData.ItemTypeIdArmor == ItemTypeId.CLOTH_ARMOR)
            {
                list2.Add(list[i]);
            }
        }
        UnitJoinArmorStyleData randomRatio = UnitJoinArmorStyleData.GetRandomRatio(list2, tyche);
        ArmorStyleData armorStyleData2 = PandoraSingleton<DataFactory>.Instance.InitData<ArmorStyleData>((int)randomRatio.ArmorStyleId);
        if (generateArmor)
        {
            Item procItem = GetProcItem(tyche, ref ratingPool, unit, UnitSlotId.ARMOR, armorStyleData2.ItemTypeIdArmor, qualityId, generateArmorRuneMark, unit.GetMutationId(UnitSlotId.ARMOR) != MutationId.NONE);
            unit.EquipItem(UnitSlotId.ARMOR, procItem);
            newItems?.Add(procItem);
        }
        if (generateHelmet && armorStyleData2.ItemTypeIdHelmet != 0)
        {
            Item procItem2 = GetProcItem(tyche, ref ratingPool, unit, UnitSlotId.HELMET, armorStyleData2.ItemTypeIdHelmet, qualityId, generateHelmetRuneMark, unit.GetMutationId(UnitSlotId.HELMET) != MutationId.NONE);
            unit.EquipItem(UnitSlotId.HELMET, procItem2);
            newItems?.Add(procItem2);
        }
        return armorStyleData2.Id;
    }

    public static Item GetProcItem(Tyche tyche, ref int ratingPool, Unit unit, UnitSlotId unitSlotId, ItemTypeId itemTypeId, ItemQualityId qualityId = ItemQualityId.NORMAL, bool generateRunemark = false, bool hasMutation = false)
    {
        string text = ((int)unit.Id).ToConstantString();
        List<ItemData> list = PandoraSingleton<DataFactory>.Instance.InitData<ItemData>("fk_item_type_id", ((int)itemTypeId).ToConstantString()).ToDynList();
        List<ItemUnitData> list2 = PandoraSingleton<DataFactory>.Instance.InitData<ItemUnitData>(new string[2]
        {
            "fk_unit_id",
            "mutation"
        }, new string[2]
        {
            text,
            (!hasMutation) ? "0" : "1"
        });
        for (int num = list.Count - 1; num >= 0; num--)
        {
            bool flag = false;
            List<ItemJoinUnitSlotData> list3 = PandoraSingleton<DataFactory>.Instance.InitData<ItemJoinUnitSlotData>("fk_item_id", list[num].Id.ToIntString());
            if (list3.Exists((ItemJoinUnitSlotData x) => x.UnitSlotId == unitSlotId))
            {
                for (int i = 0; i < list2.Count; i++)
                {
                    if (list[num].Id == list2[i].ItemId)
                    {
                        flag = true;
                        break;
                    }
                }
            }
            if (!(flag & (list[num].Id != ItemId.MAIDEN_FLAIL && list[num].Id != ItemId.MAIDEN_TWO_HANDED_FLAIL)))
            {
                list.RemoveAt(num);
            }
        }
        if (list.Count == 0)
        {
            PandoraDebug.LogWarning("No items of type " + itemTypeId + " are availables for unit " + unit.Id + " in slot " + unitSlotId + " please check combat/armor style values for this unit");
            return new Item(ItemId.NONE);
        }
        ItemData itemData = list[tyche.Rand(0, list.Count)];
        ratingPool += itemData.Rating;
        Item item = new Item(new ItemSave(itemData.Id, qualityId));
        if (qualityId > ItemQualityId.NORMAL)
        {
            ItemQualityJoinItemTypeData itemQualityJoinItemTypeData = PandoraSingleton<DataFactory>.Instance.InitData<ItemQualityJoinItemTypeData>(new string[2]
            {
                "fk_item_quality_id",
                "fk_item_type_id"
            }, new string[2]
            {
                ((int)qualityId).ToConstantString(),
                ((int)itemData.ItemTypeId).ToConstantString()
            })[0];
            ratingPool += itemQualityJoinItemTypeData.RatingModifier;
        }
        if (generateRunemark)
        {
            ratingPool = TryAddRune(tyche, ref ratingPool, unit, item);
        }
        return item;
    }

    private static int TryAddRune(Tyche tyche, ref int ratingPool, Unit unit, Item item)
    {
        if (item.CanAddRuneMark())
        {
            RuneMarkId randomRuneMark = Item.GetRandomRuneMark(tyche, item, unit.AllegianceId);
            if (randomRuneMark != 0 && item.AddRuneMark(randomRuneMark, item.QualityData.RuneMarkQualityIdMax, unit.AllegianceId))
            {
                RuneMarkQualityJoinItemTypeData runeMarkQualityJoinItemTypeData = PandoraSingleton<DataFactory>.Instance.InitData<RuneMarkQualityJoinItemTypeData>(new string[2]
                {
                    "fk_rune_mark_quality_id",
                    "fk_item_type_id"
                }, new string[2]
                {
                    ((int)item.RuneMark.QualityData.Id).ToConstantString(),
                    ((int)item.TypeData.Id).ToConstantString()
                })[0];
                ratingPool += runeMarkQualityJoinItemTypeData.Rating;
            }
        }
        return ratingPool;
    }

    public static void BoostItemsQuality(Tyche tyche, Unit unit, ItemQualityId newQualityId, ref int ratingPool, ref int counter, int rating)
    {
        BoostItemQuality(tyche, unit, newQualityId, UnitSlotId.ARMOR, ref ratingPool, ref counter, rating);
        BoostItemQuality(tyche, unit, newQualityId, UnitSlotId.HELMET, ref ratingPool, ref counter, rating);
        BoostItemQuality(tyche, unit, newQualityId, UnitSlotId.SET1_MAINHAND, ref ratingPool, ref counter, rating);
        BoostItemQuality(tyche, unit, newQualityId, UnitSlotId.SET1_OFFHAND, ref ratingPool, ref counter, rating);
        BoostItemQuality(tyche, unit, newQualityId, UnitSlotId.SET2_MAINHAND, ref ratingPool, ref counter, rating);
        BoostItemQuality(tyche, unit, newQualityId, UnitSlotId.SET2_OFFHAND, ref ratingPool, ref counter, rating);
    }

    public static void BoostItemQuality(Tyche tyche, Unit unit, ItemQualityId newQualityId, UnitSlotId unitSlotId, ref int ratingPool, ref int counter, int rating)
    {
        if (counter <= 0 || ratingPool > rating)
        {
            return;
        }
        ItemSave itemSave = unit.UnitSave.items[(int)unitSlotId];
        if (itemSave == null || itemSave.id == 0)
        {
            return;
        }
        ItemTypeId itemTypeId = PandoraSingleton<DataFactory>.Instance.InitData<ItemData>(itemSave.id).ItemTypeId;
        if (itemSave.qualityId < (int)newQualityId)
        {
            PandoraDebug.LogInfo("Boosting quality of item " + (ItemId)itemSave.id + " to " + newQualityId);
            int ratingModifier = PandoraSingleton<DataFactory>.Instance.InitData<ItemQualityJoinItemTypeData>(new string[2]
            {
                "fk_item_quality_id",
                "fk_item_type_id"
            }, new string[2]
            {
                ((int)newQualityId).ToConstantString(),
                ((int)itemTypeId).ToConstantString()
            })[0].RatingModifier;
            if (ratingPool + ratingModifier <= rating)
            {
                itemSave.qualityId = (int)newQualityId;
                PandoraDebug.LogInfo("Quality of item " + (ItemId)itemSave.id + " boosted to " + newQualityId);
                ratingPool += ratingModifier;
                counter--;
                itemSave.runeMarkId = 0;
                itemSave.runeMarkQualityId = 0;
                Item item = new Item(itemSave);
                unit.Items[(int)unitSlotId] = item;
                TryAddRune(tyche, ref ratingPool, unit, item);
            }
        }
    }

    public static void RaiseAttributes(Tyche tyche, List<AttributeData> attributesData, Unit unit, ref int ratingPool, int ratingMax)
    {
        int[] baseAttributes = new int[9]
        {
            unit.GetBaseAttribute(AttributeId.STRENGTH),
            unit.GetBaseAttribute(AttributeId.TOUGHNESS),
            unit.GetBaseAttribute(AttributeId.AGILITY),
            unit.GetBaseAttribute(AttributeId.LEADERSHIP),
            unit.GetBaseAttribute(AttributeId.INTELLIGENCE),
            unit.GetBaseAttribute(AttributeId.ALERTNESS),
            unit.GetBaseAttribute(AttributeId.WEAPON_SKILL),
            unit.GetBaseAttribute(AttributeId.BALLISTIC_SKILL),
            unit.GetBaseAttribute(AttributeId.ACCURACY)
        };
        int[] maxAttributes = new int[9]
        {
            unit.GetBaseAttribute(AttributeId.STRENGTH_MAX),
            unit.GetBaseAttribute(AttributeId.TOUGHNESS_MAX),
            unit.GetBaseAttribute(AttributeId.AGILITY_MAX),
            unit.GetBaseAttribute(AttributeId.LEADERSHIP_MAX),
            unit.GetBaseAttribute(AttributeId.INTELLIGENCE_MAX),
            unit.GetBaseAttribute(AttributeId.ALERTNESS_MAX),
            unit.GetBaseAttribute(AttributeId.WEAPON_SKILL_MAX),
            unit.GetBaseAttribute(AttributeId.BALLISTIC_SKILL_MAX),
            unit.GetBaseAttribute(AttributeId.ACCURACY_MAX)
        };
        RaiseAttributes(tyche, attributesData, unit, ref ratingPool, ratingMax, baseAttributes, maxAttributes);
    }

    public static void RaiseAttributes(Tyche tyche, List<AttributeData> attributesData, Unit unit, ref int ratingPool, int ratingMax, int[] baseAttributes, int[] maxAttributes)
    {
        RaiseAttributeType(tyche, attributesData, AttributeTypeId.PHYSICAL, unit, ref ratingPool, ratingMax, baseAttributes, maxAttributes);
        RaiseAttributeType(tyche, attributesData, AttributeTypeId.MARTIAL, unit, ref ratingPool, ratingMax, baseAttributes, maxAttributes);
        RaiseAttributeType(tyche, attributesData, AttributeTypeId.MENTAL, unit, ref ratingPool, ratingMax, baseAttributes, maxAttributes);
        unit.ApplyChanges();
    }

    private static void RaiseAttributeType(Tyche tyche, List<AttributeData> attributesData, AttributeTypeId attrTypeId, Unit unit, ref int ratingPool, int ratingMax, int[] baseAttributes, int[] maxAttributes)
    {
        PandoraDebug.LogInfo("Unit has " + unit.UnspentPhysical + " unspent physicals " + unit.UnspentMartial + " unspent martials " + unit.UnspentMental + " unspent mentals", "MISSION");
        int num = 0;
        List<AttributeData> list = new List<AttributeData>();
        for (int i = 0; i < attributesData.Count; i++)
        {
            if (attributesData[i].AttributeTypeId == attrTypeId)
            {
                list.Add(attributesData[i]);
            }
        }
        int num2 = 0;
        switch (attrTypeId)
        {
            case AttributeTypeId.PHYSICAL:
                num2 = unit.UnspentPhysical;
                break;
            case AttributeTypeId.MENTAL:
                num2 = unit.UnspentMental;
                break;
            case AttributeTypeId.MARTIAL:
                num2 = unit.UnspentMartial;
                break;
        }
        bool flag = true;
        while (flag && ratingPool < ratingMax)
        {
            flag = false;
            int num3 = tyche.Rand(0, list.Count);
            int num4 = 0;
            while (!flag && num4 < list.Count)
            {
                AttributeData attributeData = list[num3];
                if (unit.CanRaiseAttributeFast(attributeData.Id, baseAttributes, maxAttributes, num2))
                {
                    unit.RaiseAttribute(attributeData.Id, updateAttributes: false);
                    ratingPool += attributeData.Rating;
                    flag = true;
                    num++;
                    num2--;
                }
                num3 = (num3 + 1) % list.Count;
                num4++;
            }
        }
        PandoraDebug.LogInfo("Added " + attrTypeId + " " + num + " added!", "MISSION");
    }

    public static void AddSkillSpells(Tyche tyche, Unit unit, ref int ratingPool, int ratingMax)
    {
        int[] baseAttributes = new int[9]
        {
            unit.GetBaseAttribute(AttributeId.STRENGTH),
            unit.GetBaseAttribute(AttributeId.TOUGHNESS),
            unit.GetBaseAttribute(AttributeId.AGILITY),
            unit.GetBaseAttribute(AttributeId.LEADERSHIP),
            unit.GetBaseAttribute(AttributeId.INTELLIGENCE),
            unit.GetBaseAttribute(AttributeId.ALERTNESS),
            unit.GetBaseAttribute(AttributeId.WEAPON_SKILL),
            unit.GetBaseAttribute(AttributeId.BALLISTIC_SKILL),
            unit.GetBaseAttribute(AttributeId.ACCURACY)
        };
        AddSkillSpells(tyche, unit, ref ratingPool, ratingMax, GetLearnableSkills(unit), baseAttributes);
    }

    public static List<SkillData> GetLearnableSkills(Unit unit)
    {
        List<UnitJoinSkillLineData> list = PandoraSingleton<DataFactory>.Instance.InitData<UnitJoinSkillLineData>("fk_unit_id", ((int)unit.Id).ToConstantString());
        List<SkillData> list2 = new List<SkillData>();
        for (int i = 0; i < list.Count; i++)
        {
            List<SkillLineJoinSkillData> list3 = PandoraSingleton<DataFactory>.Instance.InitData<SkillLineJoinSkillData>("fk_skill_line_id", list[i].SkillLineId.ToIntString());
            for (int j = 0; j < list3.Count; j++)
            {
                list2.Add(PandoraSingleton<DataFactory>.Instance.InitData<SkillData>((int)list3[j].SkillId));
            }
        }
        return list2;
    }

    public static void AddSkillSpells(Tyche tyche, Unit unit, ref int ratingPool, int ratingMax, List<SkillData> skillsData, int[] baseAttributes)
    {
        PandoraDebug.LogInfo("Unit " + unit.Id + unit.Name + " has " + unit.UnspentSkill + " unspent skill points" + unit.UnspentSpell + " unspent spell points", "MISSION");
        for (int num = skillsData.Count - 1; num >= 0; num--)
        {
            if (skillsData[num].AttributeIdStat != 0 && (skillsData[num].AiProof || !unit.CanLearnSkillFast(skillsData[num], baseAttributes)))
            {
                skillsData.RemoveAt(num);
            }
        }
        while ((unit.UnspentSkill > 0 || unit.UnspentSpell > 0) && skillsData.Count > 0)
        {
            int num2 = tyche.Rand(0, skillsData.Count);
            SkillData skillData = skillsData[num2];
            SkillTypeId skillTypeId = skillData.SkillTypeId;
            int rating = PandoraSingleton<DataFactory>.Instance.InitData<SkillQualityData>((int)skillData.SkillQualityId).Rating;
            if (ratingPool + rating <= ratingMax && ((skillTypeId == SkillTypeId.SKILL_ACTION && unit.UnspentSkill > 0) || (skillTypeId == SkillTypeId.SPELL_ACTION && unit.UnspentSpell > 0)))
            {
                ratingPool += rating;
                unit.StartLearningSkill(skillData, 0);
                unit.EndLearnSkill(updateAttributes: false);
            }
            skillsData.RemoveAt(num2);
        }
        unit.UpdateAttributes();
        PandoraDebug.LogInfo("Unit " + unit.Id + unit.Name + " has now " + unit.UnitSave.activeSkills.Count + " active skills " + unit.UnitSave.passiveSkills.Count + " passive skills " + unit.UnitSave.spells.Count + " spells" + unit.UnspentSkill + " unspent skill points" + unit.UnspentSpell + " unspent spell points", "MISSION");
    }
}
