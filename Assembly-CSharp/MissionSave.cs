using System.Collections.Generic;
using System.IO;

public class MissionSave : IThoth
{
    private int lastVersion;

    public int campaignId;

    public bool isCampaign;

    public bool isSkirmish;

    public bool isTuto;

    public int mapPosition;

    public int rating;

    public int ratingId;

    public int deployScenarioMapLayoutId;

    public int mapLayoutId;

    public int mapGameplayId;

    public int VictoryTypeId;

    public int turnTimer;

    public int deployTimer;

    public int beaconLimit;

    public int deployCount;

    public List<int> teams;

    public List<int> deployScenarioSlotIds;

    public List<int> objectiveTypeIds;

    public List<int> objectiveTargets;

    public List<int> objectiveSeeds;

    public int wyrdPlacementId;

    public int wyrdDensityId;

    public int searchDensityId;

    public bool randomMap;

    public bool randomLayout;

    public bool randomGameplay;

    public bool randomDeployment;

    public bool randomSlots;

    public List<bool> randomObjectives;

    public bool autoDeploy;

    public bool randomRoaming;

    public int roamingUnitId;

    public float routThreshold;

    public MissionSave(float defaultRoutThreshold)
    {
        teams = new List<int>();
        deployScenarioSlotIds = new List<int>();
        objectiveTypeIds = new List<int>();
        objectiveTargets = new List<int>();
        objectiveSeeds = new List<int>();
        randomObjectives = new List<bool>();
        autoDeploy = false;
        routThreshold = defaultRoutThreshold;
        ratingId = 1;
    }

    int IThoth.GetVersion()
    {
        return 18;
    }

    void IThoth.Write(BinaryWriter writer)
    {
        int version = ((IThoth)this).GetVersion();
        Thoth.Write(writer, version);
        int cRC = GetCRC(read: false);
        Thoth.Write(writer, cRC);
        Thoth.Write(writer, campaignId);
        Thoth.Write(writer, isCampaign);
        Thoth.Write(writer, isSkirmish);
        Thoth.Write(writer, isTuto);
        Thoth.Write(writer, mapPosition);
        Thoth.Write(writer, rating);
        Thoth.Write(writer, deployScenarioMapLayoutId);
        Thoth.Write(writer, mapLayoutId);
        Thoth.Write(writer, mapGameplayId);
        Thoth.Write(writer, VictoryTypeId);
        Thoth.Write(writer, turnTimer);
        Thoth.Write(writer, deployTimer);
        Thoth.Write(writer, beaconLimit);
        Thoth.Write(writer, deployCount);
        for (int i = 0; i < deployCount; i++)
        {
            Thoth.Write(writer, teams[i]);
            Thoth.Write(writer, deployScenarioSlotIds[i]);
            Thoth.Write(writer, objectiveTypeIds[i]);
            Thoth.Write(writer, objectiveTargets[i]);
            Thoth.Write(writer, objectiveSeeds[i]);
            Thoth.Write(writer, randomObjectives[i]);
        }
        Thoth.Write(writer, wyrdPlacementId);
        Thoth.Write(writer, wyrdDensityId);
        Thoth.Write(writer, searchDensityId);
        Thoth.Write(writer, randomMap);
        Thoth.Write(writer, randomLayout);
        Thoth.Write(writer, randomGameplay);
        Thoth.Write(writer, randomDeployment);
        Thoth.Write(writer, randomSlots);
        Thoth.Write(writer, autoDeploy);
        Thoth.Write(writer, ratingId);
        Thoth.Write(writer, randomRoaming);
        Thoth.Write(writer, roamingUnitId);
        Thoth.Write(writer, routThreshold);
    }

    void IThoth.Read(BinaryReader reader)
    {
        int i = 0;
        int num = 0;
        teams = new List<int>();
        deployScenarioSlotIds = new List<int>();
        objectiveTypeIds = new List<int>();
        objectiveTargets = new List<int>();
        objectiveSeeds = new List<int>();
        randomObjectives = new List<bool>();
        Thoth.Read(reader, out lastVersion);
        num = lastVersion;
        if (num >= 17)
        {
            Thoth.Read(reader, out i);
        }
        Thoth.Read(reader, out campaignId);
        Thoth.Read(reader, out isCampaign);
        if (num >= 10)
        {
            Thoth.Read(reader, out isSkirmish);
        }
        if (num >= 7)
        {
            Thoth.Read(reader, out isTuto);
        }
        Thoth.Read(reader, out mapPosition);
        Thoth.Read(reader, out rating);
        Thoth.Read(reader, out deployScenarioMapLayoutId);
        Thoth.Read(reader, out mapLayoutId);
        if (num >= 11)
        {
            Thoth.Read(reader, out mapGameplayId);
        }
        if (num >= 4)
        {
            Thoth.Read(reader, out VictoryTypeId);
        }
        if (num >= 6)
        {
            Thoth.Read(reader, out turnTimer);
        }
        if (num >= 16)
        {
            Thoth.Read(reader, out deployTimer);
        }
        if (num >= 9)
        {
            Thoth.Read(reader, out beaconLimit);
        }
        Thoth.Read(reader, out deployCount);
        for (int j = 0; j < deployCount; j++)
        {
            Thoth.Read(reader, out int i2);
            teams.Add(i2);
            Thoth.Read(reader, out i2);
            deployScenarioSlotIds.Add(i2);
            Thoth.Read(reader, out i2);
            objectiveTypeIds.Add(i2);
            Thoth.Read(reader, out i2);
            objectiveTargets.Add(i2);
            if (num >= 5)
            {
                Thoth.Read(reader, out i2);
                objectiveSeeds.Add(i2);
            }
            if (num >= 3)
            {
                Thoth.Read(reader, out bool b);
                randomObjectives.Add(b);
            }
        }
        Thoth.Read(reader, out wyrdPlacementId);
        Thoth.Read(reader, out wyrdDensityId);
        Thoth.Read(reader, out searchDensityId);
        Thoth.Read(reader, out randomMap);
        Thoth.Read(reader, out randomLayout);
        if (num >= 11)
        {
            Thoth.Read(reader, out randomGameplay);
        }
        Thoth.Read(reader, out randomDeployment);
        Thoth.Read(reader, out randomSlots);
        if (num >= 12)
        {
            Thoth.Read(reader, out autoDeploy);
        }
        if (num >= 13)
        {
            Thoth.Read(reader, out ratingId);
        }
        else if (rating < 0)
        {
            ratingId = -rating;
        }
        if (num >= 15)
        {
            Thoth.Read(reader, out randomRoaming);
            Thoth.Read(reader, out roamingUnitId);
        }
        if (num >= 18)
        {
            Thoth.Read(reader, out routThreshold);
        }
    }

    public int GetCRC(bool read)
    {
        return CalculateCRC(read);
    }

    private int CalculateCRC(bool read)
    {
        int num = (!read) ? ((IThoth)this).GetVersion() : lastVersion;
        int num2 = 0;
        num2 += campaignId;
        num2 += (isCampaign ? 1 : 0);
        num2 += (isSkirmish ? 1 : 0);
        num2 += (isTuto ? 1 : 0);
        num2 += mapPosition;
        num2 += rating;
        num2 += deployScenarioMapLayoutId;
        num2 += mapLayoutId;
        num2 += mapGameplayId;
        num2 += VictoryTypeId;
        num2 += turnTimer;
        num2 += deployTimer;
        num2 += beaconLimit;
        num2 += deployCount;
        for (int i = 0; i < teams.Count; i++)
        {
            num2 += teams[i];
        }
        for (int j = 0; j < deployScenarioSlotIds.Count; j++)
        {
            num2 += deployScenarioSlotIds[j];
        }
        for (int k = 0; k < objectiveTypeIds.Count; k++)
        {
            num2 += objectiveTypeIds[k];
        }
        for (int l = 0; l < objectiveTargets.Count; l++)
        {
            num2 += objectiveTargets[l];
        }
        for (int m = 0; m < objectiveSeeds.Count; m++)
        {
            num2 += objectiveSeeds[m];
        }
        num2 += wyrdPlacementId;
        num2 += wyrdDensityId;
        num2 += searchDensityId;
        num2 += (randomMap ? 1 : 0);
        num2 += (randomLayout ? 1 : 0);
        num2 += (randomGameplay ? 1 : 0);
        num2 += (randomDeployment ? 1 : 0);
        for (int n = 0; n < randomObjectives.Count; n++)
        {
            num2 += (randomObjectives[n] ? 1 : 0);
        }
        num2 += (autoDeploy ? 1 : 0);
        num2 += ratingId;
        if (num >= 15)
        {
            num2 += (randomRoaming ? 1 : 0);
            num2 += roamingUnitId;
        }
        if (num >= 18)
        {
            num2 += (int)(routThreshold * 100f);
        }
        return num2;
    }
}
