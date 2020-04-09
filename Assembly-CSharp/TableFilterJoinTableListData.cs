using Mono.Data.Sqlite;

public class TableFilterJoinTableListData : DataCore
{
	public TableFilterId TableFilterId
	{
		get;
		private set;
	}

	public TableListId TableListId
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		TableFilterId = (TableFilterId)reader.GetInt32(0);
		TableListId = (TableListId)reader.GetInt32(1);
	}
}
