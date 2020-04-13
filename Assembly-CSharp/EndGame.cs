using System.Collections.Generic;

public class EndGame : ICheapState
{
    private enum GameResult
    {
        DEFEAT = 1,
        VICTORY
    }

    private MissionManager missionMngr;

    public EndGame(MissionManager mission)
    {
        missionMngr = mission;
    }

    void ICheapState.Destroy()
    {
    }

    void ICheapState.Enter(int iFrom)
    {
        missionMngr.gameFinished = true;
        PandoraSingleton<PandoraInput>.Instance.PushInputLayer(PandoraInput.InputLayer.END_GAME);
        PandoraSingleton<MissionManager>.Instance.RestoreUnitWeapons();
        WarbandController myWarbandCtrlr = PandoraSingleton<MissionManager>.Instance.GetMyWarbandCtrlr();
        UnitController unitController = myWarbandCtrlr.GetAliveLeader();
        if (unitController == null)
        {
            myWarbandCtrlr.GetAliveHighestLeaderShip();
        }
        if (unitController == null)
        {
            unitController = myWarbandCtrlr.GetLeader();
        }
        if (unitController == null)
        {
            unitController = myWarbandCtrlr.unitCtrlrs[0];
        }
        unitController.Imprint.alwaysVisible = true;
        unitController.Hide(hide: false, force: true);
        MissionEndDataSave missionEndData = missionMngr.MissionEndData;
        missionEndData.won = (myWarbandCtrlr.teamIdx == PandoraSingleton<MissionManager>.Instance.VictoriousTeamIdx);
        missionEndData.primaryObjectiveCompleted = myWarbandCtrlr.AllObjectivesCompleted;
        UpdateMVU();
        if (!PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isTuto)
        {
            List<UnitController> allMyUnits = PandoraSingleton<MissionManager>.Instance.GetAllMyUnits();
            if (missionEndData.wagonItems == null)
            {
                missionEndData.wagonItems = new Chest();
            }
            missionEndData.wagonItems.Clear();
            if ((bool)myWarbandCtrlr.wagon.chest)
            {
                missionEndData.wagonItems.AddItems(myWarbandCtrlr.wagon.chest.GetItems());
            }
            UpdateSpoilsOfWars();
            missionEndData.AddUnits(allMyUnits.Count);
            for (int num = allMyUnits.Count - 1; num >= 0; num--)
            {
                if (allMyUnits[num].unit.CampaignData != null)
                {
                    for (int i = 6; i < allMyUnits[num].unit.Items.Count; i++)
                    {
                        if (allMyUnits[num].unit.Items[i].Id != 0 && allMyUnits[num].unit.Items[i].TypeData.Id != ItemTypeId.QUEST_ITEM && (bool)myWarbandCtrlr.wagon.chest)
                        {
                            missionEndData.wagonItems.AddItem(allMyUnits[num].unit.Items[i].Save);
                        }
                    }
                }
                else
                {
                    missionEndData.UpdateUnit(allMyUnits[num]);
                }
            }
            missionEndData.playerMVUIdx = missionMngr.GetMyWarbandCtrlr().GetMVU(PandoraSingleton<MissionManager>.Instance.NetworkTyche, enemy: false);
            missionEndData.enemyMVUIdx = missionMngr.GetEnemyWarbandCtrlrs()[0].GetMVU(PandoraSingleton<MissionManager>.Instance.NetworkTyche, enemy: true);
        }
        missionMngr.MissionEndData.missionFinished = true;
        if (missionMngr.MissionEndData.missionSave.isTuto)
        {
            PandoraSingleton<GameManager>.Instance.SaveProfile();
        }
        else if (PandoraSingleton<GameManager>.Instance.currentSave != null)
        {
            PandoraSingleton<GameManager>.Instance.Save.SaveCampaign(PandoraSingleton<GameManager>.Instance.currentSave, PandoraSingleton<GameManager>.Instance.campaign);
        }
        if (missionEndData.VictoryType != 0)
        {
            if (missionMngr.GetMyWarbandCtrlr().WarData.AllegianceId == AllegianceId.DESTRUCTION)
            {
                PandoraSingleton<Pan>.Instance.PlayMusic("destruction_win", ambiance: false);
            }
            else
            {
                PandoraSingleton<Pan>.Instance.PlayMusic("order_win", ambiance: false);
            }
        }
        else if (missionMngr.GetMyWarbandCtrlr().WarData.AllegianceId == AllegianceId.DESTRUCTION)
        {
            PandoraSingleton<Pan>.Instance.PlayMusic("destruction_lose", ambiance: false);
        }
        else
        {
            PandoraSingleton<Pan>.Instance.PlayMusic("order_lose", ambiance: false);
        }
        if (!PandoraSingleton<MissionManager>.Instance.isDeploying)
        {
            PandoraSingleton<MissionManager>.Instance.CamManager.SwitchToCam(CameraManager.CameraType.CHARACTER, unitController.transform);
        }
        if (PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isTuto)
        {
            CampaignMissionData campaignMissionData = PandoraSingleton<DataFactory>.Instance.InitData<CampaignMissionData>(PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.campaignId);
            string sequence = "defeat";
            if (PandoraSingleton<GameManager>.Instance.Profile.TutorialCompletion[campaignMissionData.Idx - 1])
            {
                sequence = "victory";
            }
            PandoraSingleton<MissionManager>.Instance.PlaySequence(sequence, unitController, OnSeqDone);
        }
        else if (missionMngr.MissionEndData.VictoryType != VictoryTypeId.OBJECTIVE)
        {
            if (!PandoraSingleton<MissionManager>.Instance.isDeploying)
            {
                PandoraSingleton<MissionManager>.Instance.PlaySequence((!myWarbandCtrlr.defeated) ? "victory" : "defeat", unitController, OnSeqDone);
            }
            else
            {
                OnSeqDone();
            }
        }
        else
        {
            OnSeqDone();
        }
        if (missionMngr.MissionEndData.VictoryType == VictoryTypeId.OBJECTIVE)
        {
            return;
        }
        for (int j = 0; j < missionMngr.WarbandCtrlrs.Count; j++)
        {
            for (int k = 0; k < missionMngr.WarbandCtrlrs[j].unitCtrlrs.Count; k++)
            {
                if (missionMngr.WarbandCtrlrs[j].unitCtrlrs[k].unit.Status == UnitStateId.NONE)
                {
                    missionMngr.WarbandCtrlrs[j].unitCtrlrs[k].animator.SetInteger(AnimatorIds.action, 50);
                    missionMngr.WarbandCtrlrs[j].unitCtrlrs[k].animator.SetInteger(AnimatorIds.variation, (!missionMngr.WarbandCtrlrs[j].defeated) ? 1 : 4);
                }
            }
        }
    }

    void ICheapState.Exit(int iTo)
    {
        PandoraSingleton<PandoraInput>.Instance.PopInputLayer(PandoraInput.InputLayer.END_GAME);
    }

    void ICheapState.Update()
    {
    }

    void ICheapState.FixedUpdate()
    {
    }

    public void OnSeqDone()
    {
        if (PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isTuto || PandoraSingleton<Hephaestus>.Instance.IsJoiningInvite())
        {
            SceneLauncher.Instance.LaunchScene(SceneLoadingId.OPTIONS_QUIT_GAME);
        }
        else
        {
            PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.MISSION_END);
        }
    }

    public void UpdateMVU()
    {
        for (int i = 0; i < PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs.Count; i++)
        {
            WarbandController warbandController = PandoraSingleton<MissionManager>.Instance.WarbandCtrlrs[i];
            if (!(warbandController.wagon != null) || !(warbandController.wagon.chest != null))
            {
                continue;
            }
            for (int j = 0; j < warbandController.wagon.chest.items.Count; j++)
            {
                Item item = warbandController.wagon.chest.items[j];
                for (int k = 0; k < warbandController.unitCtrlrs.Count; k++)
                {
                    UnitController unitController = warbandController.unitCtrlrs[k];
                    if (unitController.unit == item.owner)
                    {
                        AddItemMVU(unitController, item);
                        break;
                    }
                }
            }
        }
        List<UnitController> allUnits = PandoraSingleton<MissionManager>.Instance.GetAllUnits();
        for (int l = 0; l < allUnits.Count; l++)
        {
            UnitController unitController2 = allUnits[l];
            for (int m = 6; m < unitController2.unit.Items.Count; m++)
            {
                AddItemMVU(unitController2, unitController2.unit.Items[m]);
            }
        }
    }

    private static void AddItemMVU(UnitController unitController, Item item)
    {
        ConstantId constantId = ConstantId.NONE;
        switch (item.Id)
        {
            case ItemId.WYRDSTONE_FRAGMENT:
                constantId = ConstantId.MVU_GATHER_FRAGMENT;
                break;
            case ItemId.WYRDSTONE_SHARD:
                constantId = ConstantId.MVU_GATHER_SHARD;
                break;
            case ItemId.WYRDSTONE_CLUSTER:
                constantId = ConstantId.MVU_GATHER_CLUSTER;
                break;
        }
        if (constantId != ConstantId.NONE)
        {
            unitController.AddMvuPoint(constantId, MvuCategory.WYRDSTONE);
        }
    }

    public void UpdateSpoilsOfWars()
    {
        List<Item> list = new List<Item>();
        List<WarbandController> warbandCtrlrs = missionMngr.WarbandCtrlrs;
        for (int i = 0; i < warbandCtrlrs.Count; i++)
        {
            WarbandController warbandController = warbandCtrlrs[i];
            if (warbandController.wagon != null)
            {
                for (int num = warbandController.wagon.chest.items.Count - 1; num >= 0; num--)
                {
                    if (warbandController.wagon.chest.items[num].TypeData.Id == ItemTypeId.QUEST_ITEM)
                    {
                        warbandController.wagon.chest.items.RemoveAt(num);
                    }
                }
            }
            if (warbandController.defeated && warbandController.wagon != null && warbandController.wagon.chest != null && (missionMngr.MissionEndData.crushed || PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.VictoryTypeId == 2))
            {
                list.AddRange(warbandController.wagon.chest.GetItemsAndClear());
            }
        }
        List<WarbandController> list2 = new List<WarbandController>();
        int num2 = 0;
        for (int j = 0; j < warbandCtrlrs.Count; j++)
        {
            if (warbandCtrlrs[j].defeated || warbandCtrlrs[j].playerTypeId != PlayerTypeId.PLAYER)
            {
                continue;
            }
            list2.Add(warbandCtrlrs[j]);
            for (int k = 0; k < warbandCtrlrs[j].unitCtrlrs.Count; k++)
            {
                if (warbandCtrlrs[j].unitCtrlrs[k].unit.Status != UnitStateId.OUT_OF_ACTION)
                {
                    num2++;
                }
            }
        }
        int num3 = list2.Count;
        PandoraDebug.LogInfo("Winners Count : " + num3, "END GAME");
        if (PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.VictoryTypeId != 2)
        {
            List<Item> wyrdstones = new List<Item>();
            List<Item> search = new List<Item>();
            PandoraSingleton<MissionManager>.Instance.GetUnclaimedLootableItems(ref wyrdstones, ref search);
            PandoraDebug.LogInfo("Unclaimed items count : " + search.Count, "END GAME");
            ProcMissionRatingData procMissionRatingData = PandoraSingleton<DataFactory>.Instance.InitData<ProcMissionRatingData>(PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.ratingId);
            float perc = (float)(procMissionRatingData.RewardWyrdstonePerc * num2) / 100f;
            List<Item> percList = PandoraUtils.GetPercList(ref wyrdstones, perc);
            list.AddRange(percList);
            float perc2 = (float)(procMissionRatingData.RewardSearchPerc * num2) / 100f;
            List<Item> percList2 = PandoraUtils.GetPercList(ref search, perc2);
            PandoraDebug.LogInfo("Gained Items : " + percList2.Count, "END GAME");
            list.AddRange(percList2);
        }
        WarbandController myWarbandCtrlr = PandoraSingleton<MissionManager>.Instance.GetMyWarbandCtrlr();
        for (int l = 0; l < list2.Count; l++)
        {
            WarbandController warbandController2 = list2[l];
            warbandController2.wyrdstones.Clear();
            warbandController2.spoilsFound = 0;
            bool flag = warbandController2 == myWarbandCtrlr;
            for (int num4 = list.Count - 1; num4 >= 0; num4 -= num3)
            {
                if (flag)
                {
                    missionMngr.MissionEndData.wagonItems.AddItem(list[num4].Save);
                    PandoraDebug.LogInfo("Add " + (ItemId)list[num4].Save.id + "to stuff transfered to hideout", "END GAME");
                }
                if (list[num4].IsWyrdStone)
                {
                    warbandController2.wyrdstones.Add(list[num4]);
                    PandoraDebug.LogInfo("Add " + (ItemId)list[num4].Save.id + " to end game display", "END GAME");
                }
                else
                {
                    warbandController2.spoilsFound++;
                    PandoraDebug.LogInfo("Add " + (ItemId)list[num4].Save.id + " to end game display", "END GAME");
                }
                list.RemoveAt(num4);
            }
            if (flag && missionMngr.MissionEndData.VictoryType == VictoryTypeId.DECISIVE)
            {
                warbandController2.rewardItems = GetRewardItems(warbandController2.Rank);
                for (int m = 0; m < warbandController2.rewardItems.Count; m++)
                {
                    missionMngr.MissionEndData.wagonItems.AddItem(warbandController2.rewardItems[m].Save);
                }
            }
            num3--;
        }
    }

    private List<Item> GetRewardItems(int rank)
    {
        List<Item> list = new List<Item>();
        List<SearchRewardItemData> rewardItems = PandoraSingleton<DataFactory>.Instance.InitData<SearchRewardItemData>("warband_rank", rank.ToString());
        for (int i = 0; i < Constant.GetInt(ConstantId.END_GAME_DECISIVE_REWARD); i++)
        {
            list.Add(Item.GetItemReward(rewardItems, PandoraSingleton<MissionManager>.Instance.NetworkTyche));
        }
        return list;
    }
}
