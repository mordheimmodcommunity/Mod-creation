using Mono.Data.Sqlite;
using System.Collections.Generic;
using UnityEngine;

public class InjuryData : DataCore
{
    public InjuryId Id
    {
        get;
        private set;
    }

    public string Name
    {
        get;
        private set;
    }

    public int Ratio
    {
        get;
        private set;
    }

    public int Duration
    {
        get;
        private set;
    }

    public int Rating
    {
        get;
        private set;
    }

    public int RepeatLimit
    {
        get;
        private set;
    }

    public BodyPartId BodyPartId
    {
        get;
        private set;
    }

    public UnitSlotId UnitSlotId
    {
        get;
        private set;
    }

    public bool Released
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = (InjuryId)reader.GetInt32(0);
        Name = reader.GetString(1);
        Ratio = reader.GetInt32(2);
        Duration = reader.GetInt32(3);
        Rating = reader.GetInt32(4);
        RepeatLimit = reader.GetInt32(5);
        BodyPartId = (BodyPartId)reader.GetInt32(6);
        UnitSlotId = (UnitSlotId)reader.GetInt32(7);
        Released = (reader.GetInt32(8) != 0);
    }

    public static InjuryData GetRandomRatio(List<InjuryData> datas, Tyche tyche, Dictionary<InjuryId, int> modifiers = null)
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
