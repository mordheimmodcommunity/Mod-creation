using System.Collections.Generic;
using UnityEngine;

public class WarbandController
{
    public const uint WARBAND_MYRTILUS = 200000000u;

    public string name;

    public int idx;

    public int saveIdx;

    public bool canRout;

    public bool defeated;

    public int teamIdx;

    public int playerIdx;

    public PlayerTypeId playerTypeId;

    public DeploymentId deploymentId;

    public PrimaryObjectiveTypeId objectiveTypeId;

    public int objectiveTargetIdx;

    public int objectiveSeed;

    public List<UnitController> unitCtrlrs;

    public bool idolMoralRemoved;

    public int OldMoralValue = -1;

    private int moralValue;

    public WarbandWagon wagon;

    public List<Objective> objectives = new List<Objective>();

    public List<InteractivePoint> openedSearch = new List<InteractivePoint>();

    public float percSearch;

    public float percWyrd;

    public List<Item> rewardItems;

    public List<Item> wyrdstones = new List<Item>();

    public int spoilsFound;

    public CampaignWarbandId CampaignWarbandId => CampWarData.Id;

    public bool NeedWagon => !string.IsNullOrEmpty(WarData.Wagon) && !CampWarData.NoWagon;

    public WarbandData WarData
    {
        get;
        private set;
    }

    public CampaignWarbandData CampWarData
    {
        get;
        private set;
    }

    public Item ItemIdol
    {
        get;
        set;
    }

    public int Rank
    {
        get;
        private set;
    }

    public int Rating
    {
        get;
        private set;
    }

    public int MaxMoralValue
    {
        get;
        private set;
    }

    public int BlackList
    {
        get;
        private set;
    }

    public bool AllObjectivesCompleted
    {
        get;
        private set;
    }

    public float MoralRatio => Mathf.Clamp01((float)moralValue / (float)MaxMoralValue);

    public SquadManager SquadManager
    {
        get;
        private set;
    }

    public GameObject Beacon
    {
        get;
        private set;
    }

    public int MoralValue
    {
        get
        {
            return moralValue;
        }
        set
        {
            if (!PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isCampaign || PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isTuto || playerTypeId == PlayerTypeId.PLAYER)
            {
                moralValue = Mathf.Max(value, 0);
                PandoraSingleton<MissionManager>.Instance.MissionEndData.UpdateMoral(idx, moralValue, OldMoralValue, idolMoralRemoved);
                PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.WARBAND_MORALE_CHANGED);
            }
        }
    }

    public WarbandController(MissionWarbandSave warband, DeploymentId deployId, int index, int teamIndex, PrimaryObjectiveTypeId objId, int objTargetIndex, int objSeed)
    {
        PandoraDebug.LogInfo("PlayerIndex = " + warband.PlayerIndex + " index = " + index, "LOADING");
        WarData = PandoraSingleton<DataFactory>.Instance.InitData<WarbandData>((int)warband.WarbandId);
        CampWarData = PandoraSingleton<DataFactory>.Instance.InitData<CampaignWarbandData>((int)warband.CampaignWarId);
        name = warband.Name;
        Rank = warband.Rank;
        Rating = warband.Rating;
        saveIdx = index;
        idx = index;
        teamIdx = teamIndex;
        playerIdx = warband.PlayerIndex;
        playerTypeId = warband.PlayerTypeId;
        objectiveTypeId = objId;
        objectiveTargetIdx = objTargetIndex;
        objectiveSeed = objSeed;
        deploymentId = deployId;
        PandoraDebug.LogInfo("WarbandController idx = " + idx + " playerIdx = " + playerIdx + " deployId = " + deployId + " objectiveId = " + objectiveTypeId, "WARBAND");
        unitCtrlrs = new List<UnitController>();
        WarbandRankData warbandRankData = PandoraSingleton<DataFactory>.Instance.InitData<WarbandRankData>(warband.Rank);
        moralValue = warbandRankData.Moral;
        MaxMoralValue = 0;
        canRout = true;
        defeated = false;
        idolMoralRemoved = false;
        if (playerTypeId == PlayerTypeId.AI)
        {
            SquadManager = new SquadManager(this);
        }
    }

    public void Destroy()
    {
        for (int i = 0; i < unitCtrlrs.Count; i++)
        {
            Object.Destroy(unitCtrlrs[i].gameObject);
        }
        unitCtrlrs.Clear();
    }

    public void GenerateUnit(UnitSave unitSave, Vector3 position, Quaternion rotation, bool merge = true)
    {
        PandoraDebug.LogInfo("Instantiate UnitControllers: " + (UnitId)unitSave.stats.id, "WARBAND");
        unitCtrlrs.Add(null);
        PandoraSingleton<UnitFactory>.Instance.GenerateUnit(this, unitCtrlrs.Count - 1, unitSave, position, rotation, merge);
        UnitData unitData = PandoraSingleton<DataFactory>.Instance.InitData<UnitData>(unitSave.stats.id);
        if (unitData.UnitIdDeathSpawn != 0)
        {
            for (int i = 0; i < unitData.DeathSpawnCount; i++)
            {
                UnitSave unitSave2 = new UnitSave();
                Thoth.Copy(unitSave, unitSave2);
                unitSave2.stats.id = (int)unitData.UnitIdDeathSpawn;
                unitSave2.stats.name = string.Empty;
                GenerateUnit(unitSave2, Vector3.one * 100f, Quaternion.identity);
            }
        }
    }

    public UnitController GetLeader()
    {
        return unitCtrlrs.Find((UnitController x) => x.unit.IsLeader);
    }

    public UnitController GetAliveLeader()
    {
        UnitController unitController = null;
        for (int i = 0; i < unitCtrlrs.Count; i++)
        {
            UnitController unitController2 = unitCtrlrs[i];
            if (unitController2.unit.IsAvailable())
            {
                if (unitController2.unit.IsLeader)
                {
                    return unitController2;
                }
                if ((unitController2.unit.GetUnitTypeId() == UnitTypeId.HERO_1 || unitController2.unit.GetUnitTypeId() == UnitTypeId.HERO_2 || unitController2.unit.GetUnitTypeId() == UnitTypeId.HERO_3) && (unitController == null || unitController2.unit.WarbandRoutRoll > unitController.unit.WarbandRoutRoll))
                {
                    unitController = unitController2;
                }
            }
        }
        return unitController;
    }

    public UnitController GetAliveHighestLeaderShip()
    {
        UnitController unitController = null;
        for (int i = 0; i < unitCtrlrs.Count; i++)
        {
            if (unitCtrlrs[i].unit.IsAvailable() && (unitController == null || unitController.unit.Leadership < unitCtrlrs[i].unit.Leadership))
            {
                unitController = unitCtrlrs[i];
            }
        }
        return unitController;
    }

    public void SetWagon(GameObject cart)
    {
        wagon = cart.GetComponent<WarbandWagon>();
        bool flag = playerIdx == PandoraSingleton<Hermes>.Instance.PlayerIndex && playerTypeId == PlayerTypeId.PLAYER;
        wagon.mapImprint = cart.AddComponent<MapImprint>();
        wagon.mapImprint.Init("wagon/" + WarData.Id.ToLowerString(), null, alwaysVisible: true, flag ? MapImprintType.PLAYER_WAGON : MapImprintType.ENEMY_WAGON);
        wagon.mapImprint.Wagon = wagon;
        wagon.idol.imprintIcon = null;
        wagon.idol.warbandController = this;
        wagon.idol.loc_name = "search_idol";
        wagon.idol.Init(PandoraSingleton<MissionManager>.Instance.GetNextEnvGUID());
        if (wagon.idol.items.Count > 0)
        {
            ItemIdol = wagon.idol.items[0];
            ItemIdol.Save.ownerMyrtilus = (uint)(200000000 + idx);
        }
        AddMoralIdol();
        wagon.idol.AddIdolImprint(wagon.idol.items[0]);
        Shrine component = wagon.idol.GetComponent<Shrine>();
        component.imprintIcon = null;
        component.Init(PandoraSingleton<MissionManager>.Instance.GetNextEnvGUID());
        InteractiveRestriction interactiveRestriction = new InteractiveRestriction();
        interactiveRestriction.teamIdx = teamIdx;
        interactiveRestriction.warbandId = WarData.Id;
        component.restrictions.Add(interactiveRestriction);
        wagon.chest.imprintIcon = null;
        wagon.chest.warbandController = this;
        wagon.chest.loc_name = "search_chest";
        int cartSize = PandoraSingleton<DataFactory>.Instance.InitData<WarbandRankData>("rank", Rank.ToConstantString())[0].CartSize;
        wagon.chest.Init(PandoraSingleton<MissionManager>.Instance.GetNextEnvGUID(), cartSize);
        string str = (PandoraSingleton<MissionManager>.Instance.GetMyWarbandCtrlr() != this) ? "enemy" : "own";
        PandoraSingleton<AssetBundleLoader>.Instance.LoadAssetAsync<GameObject>("Assets/prefabs/beacon/", AssetBundleId.FX, "map_beacon_" + str + ".prefab", delegate (Object beaconPrefab)
        {
            Beacon = Object.Instantiate((GameObject)beaconPrefab);
            Beacon.transform.position = wagon.transform.position;
            Beacon.transform.rotation = Quaternion.identity;
            Beacon.SetActive(PandoraSingleton<GameManager>.Instance.WagonBeaconsEnabled);
        });
        CampaignMissionId campaignId = (CampaignMissionId)PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.campaignId;
        if (campaignId == CampaignMissionId.NONE)
        {
            return;
        }
        List<CampaignMissionItemData> list = PandoraSingleton<DataFactory>.Instance.InitData<CampaignMissionItemData>(new string[2]
        {
            "fk_campaign_mission_id",
            "fk_campaign_warband_id"
        }, new string[2]
        {
            ((int)campaignId).ToConstantString(),
            ((int)CampaignWarbandId).ToConstantString()
        });
        for (int i = 0; i < list.Count; i++)
        {
            for (int j = 0; j < list[i].Amount; j++)
            {
                wagon.chest.AddItem(list[i].ItemId, list[i].ItemQualityId, list[i].RuneMarkId, list[i].RuneMarkQualityId, list[i].AllegianceId, first: true);
            }
        }
    }

    public bool IsAmbusher()
    {
        if (PandoraSingleton<MissionStartData>.Instance.CurrentMission.IsAmbush())
        {
            switch (deploymentId)
            {
                case DeploymentId.SCOUTING:
                case DeploymentId.AMBUSHER:
                    return true;
            }
        }
        return false;
    }

    public bool IsRoaming()
    {
        if (PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isTuto && unitCtrlrs.Count == 1 && unitCtrlrs[0].unit.Data.UnitTypeId == UnitTypeId.MONSTER)
        {
            return true;
        }
        if (PandoraSingleton<MissionManager>.Instance.MissionEndData.isCampaign)
        {
            return false;
        }
        for (int i = 0; i < unitCtrlrs.Count; i++)
        {
            if (unitCtrlrs[i].unit.Id == (UnitId)PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.roamingUnitId)
            {
                return true;
            }
        }
        return false;
    }

    public bool IsAmbushed()
    {
        if (PandoraSingleton<MissionStartData>.Instance.CurrentMission.IsAmbush())
        {
            DeploymentId deploymentId = this.deploymentId;
            if (deploymentId == DeploymentId.WAGON || deploymentId == DeploymentId.AMBUSHED)
            {
                return true;
            }
        }
        return false;
    }

    public bool IsPlayed()
    {
        return playerIdx == PandoraSingleton<Hermes>.Instance.PlayerIndex && playerTypeId == PlayerTypeId.PLAYER;
    }

    public void InitBlackLists()
    {
        if (playerTypeId == PlayerTypeId.PLAYER || playerTypeId == PlayerTypeId.AI)
        {
            BlackListAll();
        }
        if (CampaignWarbandId != 0)
        {
            List<CampaignWarbandWhitelistData> list = PandoraSingleton<DataFactory>.Instance.InitData<CampaignWarbandWhitelistData>("fk_campaign_warband_id", ((int)CampaignWarbandId).ToConstantString());
            for (int i = 0; i < list.Count; i++)
            {
                int campaignWarbandIdx = PandoraSingleton<MissionManager>.Instance.GetCampaignWarbandIdx(list[i].CampaignWarbandIdWhitelisted);
                WhiteListWarband(campaignWarbandIdx);
            }
        }
    }

    public void BlackListAll()
    {
        for (int i = 0; i < PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs.Count; i++)
        {
            WarbandController warbandController = PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs[i];
            if (warbandController.idx != idx && warbandController.teamIdx != teamIdx && warbandController.playerTypeId != PlayerTypeId.PASSIVE_AI && !warbandController.defeated)
            {
                BlackListWarband(i);
            }
        }
    }

    public void WhiteListWarband(int warbandIdx)
    {
        BlackList &= ~(1 << warbandIdx);
        PandoraDebug.LogInfo("Warband " + idx + " White listing " + warbandIdx + " (" + BlackList + ")", "WarbandController");
    }

    public void BlackListWarband(int warbandIdx)
    {
        if (warbandIdx != idx)
        {
            BlackList |= 1 << warbandIdx;
            PandoraDebug.LogInfo("Warband " + idx + " Black listing " + warbandIdx + " (" + BlackList + ")", "WarbandController");
        }
    }

    public bool BlackListed(int idx)
    {
        return (BlackList & (1 << idx)) != 0;
    }

    public bool AllUnitsDead()
    {
        for (int i = 0; i < unitCtrlrs.Count; i++)
        {
            if (unitCtrlrs[i].unit.Status != UnitStateId.OUT_OF_ACTION)
            {
                return false;
            }
        }
        return true;
    }

    public void SetupObjectivesCampaign(CampaignMissionId missionId)
    {
        objectives = Objective.CreateMissionObjectives(missionId, this);
    }

    public void SetupObjectivesProc()
    {
        if (objectiveTypeId != 0)
        {
            WarbandController warbandController = PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs[objectiveTargetIdx];
            Objective.CreateObjective(ref objectives, objectiveTypeId, this, objectiveSeed, warbandController.WarData.Id, warbandController.unitCtrlrs.ConvertAll((UnitController x) => x.unit), warbandController);
        }
    }

    public bool CheckObjectives(bool dontSendNotice = false, bool forceSendNotice = false)
    {
        bool flag = false;
        AllObjectivesCompleted = (objectives.Count > 0);
        for (int i = 0; i < objectives.Count; i++)
        {
            Objective objective = objectives[i];
            bool flag2 = objective.CheckObjective();
            flag |= flag2;
            if (objective.RequiredObjectives.Count > 0)
            {
                bool flag3 = false;
                for (int j = 0; j < objective.RequiredObjectives.Count; j++)
                {
                    flag3 |= ((objective.RequiredCompleteds[j] && !objective.RequiredObjectives[j].done) || (!objective.RequiredCompleteds[j] && objective.RequiredObjectives[j].counter.x == 0f));
                }
                objective.SetLocked(flag3);
            }
            AllObjectivesCompleted &= objective.done;
        }
        if (PandoraSingleton<MissionManager>.Instance.GetMyWarbandCtrlr() == this)
        {
            PandoraSingleton<MissionManager>.Instance.UpdateObjectivesUI();
        }
        return AllObjectivesCompleted;
    }

    public bool HasFailedMandatoryObjective()
    {
        for (int i = 0; i < objectives.Count; i++)
        {
            if (objectives[i].ResultId == PrimaryObjectiveResultId.FAILED)
            {
                return true;
            }
        }
        return false;
    }

    public void SearchOpened(SearchPoint search)
    {
        if (!search.isWyrdstone && openedSearch.IndexOf(search) == -1 && PandoraSingleton<MissionManager>.Instance.GetSearchPoints().IndexOf(search) != -1)
        {
            PandoraSingleton<MissionManager>.Instance.focusedUnit.unit.AddToAttribute(AttributeId.TOTAL_SEARCH, 1);
            openedSearch.Add(search);
            PandoraSingleton<MissionManager>.Instance.UpdateObjectivesUI();
            PandoraSingleton<MissionManager>.Instance.MissionEndData.UpdateOpenedSearches(saveIdx, search.guid);
            if (idx == PandoraSingleton<MissionManager>.Instance.GetMyWarbandCtrlr().idx)
            {
                PandoraSingleton<Hephaestus>.Instance.IncrementStat(Hephaestus.StatId.OPENED_CHESTS, 1);
            }
        }
    }

    public void ConvertItem(ItemId id, int count)
    {
        for (int i = 0; i < objectives.Count; i++)
        {
            if (objectives[i].TypeId == PrimaryObjectiveTypeId.CONVERT)
            {
                (objectives[i] as ObjectiveConvert).UpdateConvertedItems(id, count);
            }
        }
    }

    public void LocateItem(List<Item> items)
    {
        for (int i = 0; i < objectives.Count; i++)
        {
            if (objectives[i].TypeId == PrimaryObjectiveTypeId.LOCATE)
            {
                (objectives[i] as ObjectiveLocate).UpdateLocatedItems(items);
            }
        }
    }

    public void LocateZone(LocateZone zone)
    {
        for (int i = 0; i < objectives.Count; i++)
        {
            if (objectives[i].TypeId == PrimaryObjectiveTypeId.LOCATE)
            {
                (objectives[i] as ObjectiveLocate).UpdateLocatedZone(zone);
            }
        }
    }

    public void Activate(string pointName)
    {
        for (int i = 0; i < objectives.Count; i++)
        {
            if (objectives[i].TypeId == PrimaryObjectiveTypeId.ACTIVATE)
            {
                ((ObjectiveActivate)objectives[i]).UpdateActivatedItem(pointName);
            }
        }
    }

    public void DestroyDestructible(string destrName)
    {
        for (int i = 0; i < objectives.Count; i++)
        {
            if (objectives[i].TypeId == PrimaryObjectiveTypeId.DESTROY)
            {
                ((ObjectiveDestroy)objectives[i]).UpdateDestroyedItem(destrName);
            }
        }
    }

    public void OnUnitCreated(UnitController ctrlr)
    {
        moralValue += ctrlr.unit.Moral;
        MaxMoralValue = moralValue;
    }

    public int GetMVU(Tyche tyche, bool enemy)
    {
        if (unitCtrlrs.Count == 0)
        {
            return 0;
        }
        List<int> list = new List<int>();
        int attribute = unitCtrlrs[0].unit.GetAttribute(AttributeId.CURRENT_MVU);
        for (int i = 0; i < unitCtrlrs.Count; i++)
        {
            if (unitCtrlrs[i].unit.GetUnitTypeId() != UnitTypeId.DRAMATIS && (!IsPlayed() || unitCtrlrs[i].unit.CampaignData == null))
            {
                if (unitCtrlrs[i].unit.GetAttribute(AttributeId.CURRENT_MVU) > attribute)
                {
                    attribute = unitCtrlrs[i].unit.GetAttribute(AttributeId.CURRENT_MVU);
                    list.Clear();
                }
                if (unitCtrlrs[i].unit.GetAttribute(AttributeId.CURRENT_MVU) == attribute)
                {
                    list.Add(i);
                }
            }
        }
        if (list.Count == 0)
        {
            return 0;
        }
        if (enemy)
        {
            return list[tyche.Rand(0, list.Count)];
        }
        return unitCtrlrs[list[tyche.Rand(0, list.Count)]].unit.UnitSave.warbandSlotIndex;
    }

    public int GetCollectedWyrdStone()
    {
        int num = 0;
        if (wagon != null && wagon.chest != null)
        {
            for (int i = 0; i < wagon.chest.items.Count; i++)
            {
                if (wagon.chest.items[i].IsWyrdStone)
                {
                    num++;
                }
            }
        }
        if (unitCtrlrs != null)
        {
            for (int j = 0; j < unitCtrlrs.Count; j++)
            {
                for (int k = 0; k < unitCtrlrs[j].unit.Items.Count; k++)
                {
                    if (unitCtrlrs[j].unit.Items[k].IsWyrdStone)
                    {
                        num++;
                    }
                }
            }
        }
        return num;
    }

    public void AddMoralIdol()
    {
        if (idolMoralRemoved)
        {
            idolMoralRemoved = false;
            MoralValue += Constant.GetInt(ConstantId.MORAL_IDOL);
            PandoraSingleton<MissionManager>.Instance.CombatLogger.AddLog(CombatLogger.LogMessage.GAIN_IDOL, name, Constant.GetInt(ConstantId.MORAL_IDOL).ToConstantString());
            if (PandoraSingleton<MissionManager>.Instance.GetMyWarbandCtrlr() == this)
            {
                PandoraSingleton<Pan>.Instance.Narrate("stolen_idol");
            }
        }
    }

    public void RemoveMoralIdol()
    {
        if (!idolMoralRemoved)
        {
            idolMoralRemoved = true;
            MoralValue -= Constant.GetInt(ConstantId.MORAL_IDOL);
            PandoraSingleton<MissionManager>.Instance.CombatLogger.AddLog(CombatLogger.LogMessage.LOSE_IDOL, name, Constant.GetInt(ConstantId.MORAL_IDOL).ToConstantString());
        }
    }
}
