using Mono.Data.Sqlite;

public class WarbandRankData : DataCore
{
	public WarbandRankId Id
	{
		get;
		private set;
	}

	public string Name
	{
		get;
		private set;
	}

	public int Rank
	{
		get;
		private set;
	}

	public int Exp
	{
		get;
		private set;
	}

	public int CartSize
	{
		get;
		private set;
	}

	public int Moral
	{
		get;
		private set;
	}

	public int Rating
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = (WarbandRankId)reader.GetInt32(0);
		Name = reader.GetString(1);
		Rank = reader.GetInt32(2);
		Exp = reader.GetInt32(3);
		CartSize = reader.GetInt32(4);
		Moral = reader.GetInt32(5);
		Rating = reader.GetInt32(6);
	}
}
