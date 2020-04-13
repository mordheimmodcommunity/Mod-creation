using System;
using System.Collections.Generic;
using System.IO;

public class SaveManager
{
    private const int DELETE_SAVE_UNDER = 24;

    private const int DELETE_MISSION_SAVE_UNDER = 31;

    public static readonly string SAVE_FILE = "savegame{0}.sg";

    private List<int> saveIndices;

    private DateTime[] timestamps;

    private int firstEmptyIndex = -1;

    private bool dataLoaded;

    public int MaxSaveGames
    {
        get;
        private set;
    }

    public SaveManager()
    {
        Reset();
    }

    public void Reset()
    {
        int @int = Constant.GetInt(ConstantId.SAVE_SLOTS_PER_WARBAND);
        List<WarbandData> list = PandoraSingleton<DataFactory>.Instance.InitData<WarbandData>("Basic", "1");
        MaxSaveGames = list.Count * @int;
        timestamps = new DateTime[MaxSaveGames];
        saveIndices = new List<int>(MaxSaveGames);
        dataLoaded = false;
        firstEmptyIndex = -1;
        DeleteOldNameSaves();
    }

    private void DeleteOldNameSaves()
    {
        for (int i = 0; i < MaxSaveGames; i++)
        {
            string fileName = $"savegame_{i}.sg";
            if (PandoraSingleton<Hephaestus>.Instance.FileExists(fileName))
            {
                PandoraSingleton<Hephaestus>.Instance.FileDelete(fileName, delegate
                {
                });
            }
        }
    }

    public void EraseOldSaveGame()
    {
        CheckFileVersionTooOld(0);
    }

    private void CheckFileVersionTooOld(int startIdx)
    {
        int num = startIdx;
        string saveFile;
        while (true)
        {
            if (num < MaxSaveGames)
            {
                saveFile = GetSaveFileName(num);
                if (PandoraSingleton<Hephaestus>.Instance.FileExists(saveFile))
                {
                    break;
                }
                num++;
                continue;
            }
            return;
        }
        int fileIdx = num;
        PandoraSingleton<Hephaestus>.Instance.FileRead(saveFile, delegate (byte[] data, bool success)
        {
            OnFileLoadedCheckOld(data, success, fileIdx, saveFile);
        });
    }

    private void OnFileLoadedCheckOld(byte[] data, bool success, int fileIdx, string saveFile)
    {
        if (success && data != null && data.Length > 4)
        {
            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream))
                {
                    int num = binaryReader.ReadInt32();
                    if (num < 24)
                    {
                        PandoraSingleton<Hephaestus>.Instance.FileDelete(saveFile, OnFileDelete);
                        saveIndices.Remove(fileIdx);
                        if (firstEmptyIndex > fileIdx)
                        {
                            firstEmptyIndex = fileIdx;
                        }
                    }
                }
                memoryStream.Close();
            }
        }
        if (fileIdx < MaxSaveGames - 1)
        {
            CheckFileVersionTooOld(fileIdx + 1);
        }
    }

    private void OnFileDelete(bool success)
    {
        if (success)
        {
        }
    }

    private string GetSaveFileName(int campaign)
    {
        return string.Format(SAVE_FILE, campaign);
    }

    public bool CampaignExist(int campaign)
    {
        return PandoraSingleton<Hephaestus>.Instance.FileExists(GetSaveFileName(campaign));
    }

    public void LoadData()
    {
        saveIndices.Clear();
        firstEmptyIndex = -1;
        for (int i = 0; i < MaxSaveGames; i++)
        {
            if (CampaignExist(i))
            {
                timestamps[i] = GetSaveTimeStamp(i);
                if (saveIndices.Count == 0)
                {
                    saveIndices.Add(i);
                    continue;
                }
                for (int j = 0; j < saveIndices.Count; j++)
                {
                    if (timestamps[i] > timestamps[saveIndices[j]])
                    {
                        saveIndices.Insert(j, i);
                        break;
                    }
                    if (j + 1 == saveIndices.Count)
                    {
                        saveIndices.Add(i);
                        break;
                    }
                }
            }
            else if (firstEmptyIndex == -1)
            {
                firstEmptyIndex = i;
            }
        }
        dataLoaded = true;
    }

    public bool EmptyCampaignSaveExists()
    {
        if (firstEmptyIndex == -1 && !dataLoaded)
        {
            GetEmptyCampaignSlot();
        }
        return firstEmptyIndex != -1;
    }

    public int GetEmptyCampaignSlot()
    {
        if (!dataLoaded && firstEmptyIndex == -1)
        {
            for (int i = 0; i < MaxSaveGames; i++)
            {
                if (!CampaignExist(i))
                {
                    firstEmptyIndex = i;
                    break;
                }
            }
        }
        return firstEmptyIndex;
    }

    public bool HasCampaigns()
    {
        if (dataLoaded)
        {
            return saveIndices.Count > 0;
        }
        if (firstEmptyIndex > 0)
        {
            return true;
        }
        for (int i = 0; i < MaxSaveGames; i++)
        {
            if (CampaignExist(i))
            {
                return true;
            }
        }
        return false;
    }

    public List<int> GetCampaignSlots()
    {
        if (!dataLoaded)
        {
            LoadData();
        }
        return saveIndices;
    }

    public void SaveCampaign(WarbandSave warband, int campaign)
    {
        if (!warband.inMission || !warband.endMission.isSkirmish)
        {
            PandoraSingleton<GameManager>.Instance.currentSave = warband;
            PandoraSingleton<GameManager>.Instance.Profile.LastPlayedCampaign = campaign;
            PandoraSingleton<GameManager>.Instance.Profile.UpdateHash(campaign, warband.GetCRC(read: false));
            PandoraSingleton<GameManager>.Instance.SaveProfile();
            byte[] data = Thoth.WriteToArray(warband);
            PandoraSingleton<Hephaestus>.Instance.FileWrite(GetSaveFileName(campaign), data, OnSaveCampaign);
            PandoraDebug.LogDebug("SaveCampaign : campaign id = " + campaign, "SAVE MANAGER");
            if (dataLoaded)
            {
                saveIndices.Remove(campaign);
                saveIndices.Insert(0, campaign);
                timestamps[campaign] = DateTime.Now;
            }
        }
    }

    private void OnSaveCampaign(bool success)
    {
        if (success)
        {
            PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.GAME_SAVED);
        }
    }

    private DateTime GetSaveTimeStamp(int campaign)
    {
        return PandoraSingleton<Hephaestus>.Instance.GetFileTimeStamp(GetSaveFileName(campaign));
    }

    public DateTime GetCachedSaveTimeStamp(int campaign)
    {
        return timestamps[campaign];
    }

    public void DeleteCampaign(int campaign)
    {
        if (CampaignExist(campaign))
        {
            PandoraSingleton<Hephaestus>.Instance.FileDelete(GetSaveFileName(campaign), OnDeleteCampaign);
            if (dataLoaded)
            {
                saveIndices.Remove(campaign);
            }
            if (firstEmptyIndex > campaign || firstEmptyIndex == -1)
            {
                firstEmptyIndex = campaign;
            }
            PandoraSingleton<GameManager>.Instance.Profile.ClearHash(campaign);
            if (PandoraSingleton<GameManager>.Instance.Profile.LastPlayedCampaign == campaign)
            {
                PandoraSingleton<GameManager>.Instance.Profile.LastPlayedCampaign = -1;
            }
            PandoraSingleton<GameManager>.Instance.SaveProfile();
        }
    }

    private void OnDeleteCampaign(bool success)
    {
        if (success)
        {
        }
    }

    public void LoadCampaign(int campaign)
    {
        PandoraSingleton<Hephaestus>.Instance.FileRead(GetSaveFileName(campaign), OnLoadCampaign);
    }

    public void OnLoadCampaign(byte[] data, bool success)
    {
        if (success)
        {
            WarbandSave warbandSave = new WarbandSave();
            Thoth.ReadFromArray(data, warbandSave);
            if (warbandSave.lastVersion < 31 && warbandSave.inMission)
            {
                warbandSave.inMission = false;
                warbandSave.endMission = null;
            }
            PandoraSingleton<GameManager>.Instance.currentSave = warbandSave;
            PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.GAME_LOADED);
        }
        else
        {
            PandoraDebug.LogWarning("Unable to load Campaign", "SAVE_MANAGER");
        }
    }

    public void GetCampaignInfo(int campaign, Action<WarbandSave> OnInfoLoaded)
    {
        PandoraSingleton<Hephaestus>.Instance.FileRead(GetSaveFileName(campaign), delegate (byte[] data, bool success)
        {
            OnGetCampaignInfo(data, success, OnInfoLoaded);
        });
    }

    public void OnGetCampaignInfo(byte[] data, bool success, Action<WarbandSave> OnInfoLoaded)
    {
        if (success)
        {
            WarbandSave warbandSave = new WarbandSave();
            Thoth.ReadFromArray(data, warbandSave);
            OnInfoLoaded(warbandSave);
        }
        else
        {
            PandoraDebug.LogWarning("Unable to load Campaign Info", "SAVE_MANAGER");
        }
    }

    public void NewCampaign(int campaign, WarbandId warbandId, int forcedWarRank = 0)
    {
        int rank = 0;
        Warband warband = new Warband(new WarbandSave(warbandId));
        warband.Rank = forcedWarRank;
        Date date = new Date(Constant.GetInt(ConstantId.CAL_DAY_START));
        warband.GetWarbandSave().currentDate = date.CurrentDate;
        DataFactory instance = PandoraSingleton<DataFactory>.Instance;
        int num = (int)warbandId;
        List<WarbandNameData> list = instance.InitData<WarbandNameData>("fk_warband_id", num.ToString());
        warband.GetWarbandSave().name = PandoraSingleton<LocalizationManager>.Instance.GetStringById(list[PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0, list.Count)].Name);
        EventLogger eventLogger = new EventLogger(warband.GetWarbandSave().stats.history);
        eventLogger.AddHistory(date.CurrentDate, EventLogger.LogEvent.WARBAND_CREATED, 0);
        List<WarbandDefaultUnitData> list2 = PandoraSingleton<DataFactory>.Instance.InitData<WarbandDefaultUnitData>("fk_warband_id", ((int)warband.Id).ToString());
        Dictionary<WarbandSlotTypeId, int> dictionary = new Dictionary<WarbandSlotTypeId, int>();
        for (int i = 0; i < list2.Count; i++)
        {
            for (int j = 0; j < list2[i].Amount; j++)
            {
                Unit unit = Unit.GenerateUnit(list2[i].UnitId, rank);
                WarbandSlotTypeId warbandSlotTypeId = list2[i].WarbandSlotTypeId;
                unit.UnitSave.warbandSlotIndex = (int)(warbandSlotTypeId + (dictionary.ContainsKey(warbandSlotTypeId) ? dictionary[warbandSlotTypeId] : 0));
                if (dictionary.ContainsKey(warbandSlotTypeId))
                {
                    Dictionary<WarbandSlotTypeId, int> dictionary2;
                    Dictionary<WarbandSlotTypeId, int> dictionary3 = dictionary2 = dictionary;
                    WarbandSlotTypeId key;
                    WarbandSlotTypeId key2 = key = warbandSlotTypeId;
                    int num2 = dictionary2[key];
                    dictionary3[key2] = num2 + 1;
                }
                else
                {
                    dictionary[warbandSlotTypeId] = 1;
                }
                warband.HireUnit(unit);
            }
        }
        Market market = new Market(warband);
        market.RefreshMarket(MarketEventId.NO_EVENT, announceRefresh: false);
        List<FactionData> list3 = PandoraSingleton<DataFactory>.Instance.InitData<FactionData>("fk_allegiance_id", ((int)warband.WarbandData.AllegianceId).ToString());
        int num3 = 0;
        for (int k = 0; k < list3.Count; k++)
        {
            if ((list3[k].Primary && list3[k].WarbandId == warbandId) || !list3[k].Primary)
            {
                FactionSave item = new FactionSave(list3[k].Id, num3);
                warband.GetWarbandSave().factions.Add(item);
                num3++;
            }
        }
        eventLogger.AddHistory(warband.GetWarbandSave().currentDate + 1, EventLogger.LogEvent.SHIPMENT_REQUEST, -1);
        DataFactory instance2 = PandoraSingleton<DataFactory>.Instance;
        string[] fields = new string[2]
        {
            "fk_warband_id",
            "idx"
        };
        string[] array = new string[2];
        int num4 = (int)warbandId;
        array[0] = num4.ToString();
        array[1] = warband.GetWarbandSave().curCampaignIdx.ToString();
        List<CampaignMissionData> list4 = instance2.InitData<CampaignMissionData>(fields, array);
        int date2 = warband.GetWarbandSave().currentDate + list4[0].Days;
        eventLogger.AddHistory(date2, EventLogger.LogEvent.NEW_MISSION, warband.GetWarbandSave().curCampaignIdx);
        WarbandChest warbandChest = new WarbandChest(warband);
        warbandChest.sendStats = false;
        warbandChest.AddItem(ItemId.GOLD, ItemQualityId.NORMAL, RuneMarkId.NONE, RuneMarkQualityId.NONE, AllegianceId.NONE, Constant.GetInt(ConstantId.CAMPAIGN_STARTING_MONEY) + PandoraSingleton<GameManager>.Instance.Profile.NewGameBonusGold);
        warbandChest.sendStats = true;
        List<WeekDayData> list5 = PandoraSingleton<DataFactory>.Instance.InitData<WeekDayData>();
        for (int l = 0; l < list5.Count; l++)
        {
            if (list5[l].RefreshMarket)
            {
                int nextDay = date.GetNextDay(list5[l].Id);
                warband.Logger.AddHistory(nextDay, EventLogger.LogEvent.MARKET_ROTATION, 0);
            }
            if (list5[l].RefreshOutsiders && warband.GetAttribute(WarbandAttributeId.OUTSIDERS_COUNT) > 0)
            {
                int nextDay2 = date.GetNextDay(list5[l].Id);
                warband.Logger.AddHistory(nextDay2, EventLogger.LogEvent.OUTSIDER_ROTATION, 0);
            }
        }
        SaveCampaign(warband.GetWarbandSave(), campaign);
        if (firstEmptyIndex != campaign)
        {
            return;
        }
        firstEmptyIndex = -1;
        if (!dataLoaded)
        {
            return;
        }
        int num5 = 0;
        while (true)
        {
            if (num5 < MaxSaveGames)
            {
                if (!saveIndices.Contains(num5))
                {
                    break;
                }
                num5++;
                continue;
            }
            return;
        }
        firstEmptyIndex = num5;
    }
}
