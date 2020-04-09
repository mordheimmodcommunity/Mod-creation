using Mono.Data.Sqlite;

public class FactionConsequenceData : DataCore
{
	public int Id
	{
		get;
		private set;
	}

	public FactionId FactionId
	{
		get;
		private set;
	}

	public int LateShipmentCount
	{
		get;
		private set;
	}

	public int NextShipmentRequestModifierPerc
	{
		get;
		private set;
	}

	public int NextShipmentGoldRewardModifierPerc
	{
		get;
		private set;
	}

	public FactionConsequenceTargetId FactionConsequenceTargetId
	{
		get;
		private set;
	}

	public InjuryId InjuryId
	{
		get;
		private set;
	}

	public int TreatmentTime
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = reader.GetInt32(0);
		FactionId = (FactionId)reader.GetInt32(1);
		LateShipmentCount = reader.GetInt32(2);
		NextShipmentRequestModifierPerc = reader.GetInt32(3);
		NextShipmentGoldRewardModifierPerc = reader.GetInt32(4);
		FactionConsequenceTargetId = (FactionConsequenceTargetId)reader.GetInt32(5);
		InjuryId = (InjuryId)reader.GetInt32(6);
		TreatmentTime = reader.GetInt32(7);
	}
}
