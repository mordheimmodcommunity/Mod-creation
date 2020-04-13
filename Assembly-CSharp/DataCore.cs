using Mono.Data.Sqlite;

public abstract class DataCore
{
    public abstract void Populate(SqliteDataReader reader);
}
