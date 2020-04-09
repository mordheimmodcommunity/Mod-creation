using Mono.Data.Sqlite;

public class WarbandRankSlotData : DataCore
{
	public int Id
	{
		get;
		private set;
	}

	public WarbandRankId WarbandRankId
	{
		get;
		private set;
	}

	public int Leader
	{
		get;
		private set;
	}

	public int Impressive
	{
		get;
		private set;
	}

	public int Hero
	{
		get;
		private set;
	}

	public int Henchman
	{
		get;
		private set;
	}

	public int Reserve
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = reader.GetInt32(0);
		WarbandRankId = (WarbandRankId)reader.GetInt32(1);
		Leader = reader.GetInt32(2);
		Impressive = reader.GetInt32(3);
		Hero = reader.GetInt32(4);
		Henchman = reader.GetInt32(5);
		Reserve = reader.GetInt32(6);
	}
}
