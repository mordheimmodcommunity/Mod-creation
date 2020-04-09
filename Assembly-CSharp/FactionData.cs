using Mono.Data.Sqlite;

public class FactionData : DataCore
{
	public FactionId Id
	{
		get;
		private set;
	}

	public string Name
	{
		get;
		private set;
	}

	public string Desc
	{
		get;
		private set;
	}

	public AllegianceId AllegianceId
	{
		get;
		private set;
	}

	public WarbandId WarbandId
	{
		get;
		private set;
	}

	public bool Primary
	{
		get;
		private set;
	}

	public int WyrdstonePriceBonusPercPerOtherFactionRank
	{
		get;
		private set;
	}

	public int WyrdstonePriceBonusPercPerRank
	{
		get;
		private set;
	}

	public int MinWydstonePriceModifier
	{
		get;
		private set;
	}

	public int RepBonusPercPerOtherFactionRank
	{
		get;
		private set;
	}

	public int RepBonusPercPerRank
	{
		get;
		private set;
	}

	public int MinDeliveryDays
	{
		get;
		private set;
	}

	public int MaxDeliveryDays
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = (FactionId)reader.GetInt32(0);
		Name = reader.GetString(1);
		Desc = reader.GetString(2);
		AllegianceId = (AllegianceId)reader.GetInt32(3);
		WarbandId = (WarbandId)reader.GetInt32(4);
		Primary = (reader.GetInt32(5) != 0);
		WyrdstonePriceBonusPercPerOtherFactionRank = reader.GetInt32(6);
		WyrdstonePriceBonusPercPerRank = reader.GetInt32(7);
		MinWydstonePriceModifier = reader.GetInt32(8);
		RepBonusPercPerOtherFactionRank = reader.GetInt32(9);
		RepBonusPercPerRank = reader.GetInt32(10);
		MinDeliveryDays = reader.GetInt32(11);
		MaxDeliveryDays = reader.GetInt32(12);
	}
}
