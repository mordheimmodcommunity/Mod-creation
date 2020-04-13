using Mono.Data.Sqlite;
using System.Collections.Generic;
using UnityEngine;

public class PrimaryObjectiveBountyData : DataCore
{
    public int Id
    {
        get;
        private set;
    }

    public PrimaryObjectiveId PrimaryObjectiveId
    {
        get;
        private set;
    }

    public int Ratio
    {
        get;
        private set;
    }

    public int UnitPerc
    {
        get;
        private set;
    }

    public int EliteCount
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = reader.GetInt32(0);
        PrimaryObjectiveId = (PrimaryObjectiveId)reader.GetInt32(1);
        Ratio = reader.GetInt32(2);
        UnitPerc = reader.GetInt32(3);
        EliteCount = reader.GetInt32(4);
    }

    public static PrimaryObjectiveBountyData GetRandomRatio(List<PrimaryObjectiveBountyData> datas, Tyche tyche, Dictionary<int, int> modifiers = null)
    {
        int num = 0;
        List<int> list = new List<int>();
        for (int i = 0; i < datas.Count; i++)
        {
            int num2 = datas[i].Ratio;
            if (modifiers != null && modifiers.ContainsKey(datas[i].Id))
            {
                num2 = Mathf.Clamp(num2 + modifiers[datas[i].Id], 0, int.MaxValue);
            }
            num += num2;
            list.Add(num);
        }
        int num3 = tyche.Rand(0, num);
        for (int j = 0; j < list.Count; j++)
        {
            if (num3 < list[j])
            {
                return datas[j];
            }
        }
        return null;
    }
}
