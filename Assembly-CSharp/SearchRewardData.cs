using Mono.Data.Sqlite;

public class SearchRewardData : DataCore
{
    public SearchRewardId Id
    {
        get;
        private set;
    }

    public string Name
    {
        get;
        private set;
    }

    public ItemQualityId ItemQualityId
    {
        get;
        private set;
    }

    public RuneMarkQualityId RuneMarkQualityId
    {
        get;
        private set;
    }

    public override void Populate(SqliteDataReader reader)
    {
        Id = (SearchRewardId)reader.GetInt32(0);
        Name = reader.GetString(1);
        ItemQualityId = (ItemQualityId)reader.GetInt32(2);
        RuneMarkQualityId = (RuneMarkQualityId)reader.GetInt32(3);
    }
}
