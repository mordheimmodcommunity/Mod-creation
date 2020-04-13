using Mono.Data.Sqlite;

public class PlayerRankData : DataCore
{
    public PlayerRankId Id
    {
        get;
        private set;
    }

    public string Name
    {
        get;
        private set;
    }

    public int NewGameGold
    {
        get;
        private set;
    }

    public int XpNeeded
    {
        get;
        private set;
    }

    public WarbandSkillId WarbandSkillId
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = (PlayerRankId)reader.GetInt32(0);
        Name = reader.GetString(1);
        NewGameGold = reader.GetInt32(2);
        XpNeeded = reader.GetInt32(3);
        WarbandSkillId = (WarbandSkillId)reader.GetInt32(4);
    }
}
