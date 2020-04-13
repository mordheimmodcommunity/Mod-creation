using Mono.Data.Sqlite;

public class UnitJoinUnitRankData : DataCore
{
    public UnitId UnitId
    {
        get;
        private set;
    }

    public UnitRankId UnitRankId
    {
        get;
        private set;
    }

    public int Physical
    {
        get;
        private set;
    }

    public int Mental
    {
        get;
        private set;
    }

    public int Martial
    {
        get;
        private set;
    }

    public int Skill
    {
        get;
        private set;
    }

    public int Spell
    {
        get;
        private set;
    }

    public int Strategy
    {
        get;
        private set;
    }

    public int Offense
    {
        get;
        private set;
    }

    public bool Mutation
    {
        get;
        private set;
    }

    public int Wound
    {
        get;
        private set;
    }

    public EnchantmentId EnchantmentId
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        UnitId = (UnitId)reader.GetInt32(0);
        UnitRankId = (UnitRankId)reader.GetInt32(1);
        Physical = reader.GetInt32(2);
        Mental = reader.GetInt32(3);
        Martial = reader.GetInt32(4);
        Skill = reader.GetInt32(5);
        Spell = reader.GetInt32(6);
        Strategy = reader.GetInt32(7);
        Offense = reader.GetInt32(8);
        Mutation = (reader.GetInt32(9) != 0);
        Wound = reader.GetInt32(10);
        EnchantmentId = (EnchantmentId)reader.GetInt32(11);
    }
}
