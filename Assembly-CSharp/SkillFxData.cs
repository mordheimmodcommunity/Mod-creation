using Mono.Data.Sqlite;

public class SkillFxData : DataCore
{
	public int Id
	{
		get;
		private set;
	}

	public SkillId SkillId
	{
		get;
		private set;
	}

	public SequenceId SequenceId
	{
		get;
		private set;
	}

	public string RightHandFx
	{
		get;
		private set;
	}

	public string LeftHandFx
	{
		get;
		private set;
	}

	public string ChargeFx
	{
		get;
		private set;
	}

	public bool ChargeOnTarget
	{
		get;
		private set;
	}

	public string LaunchFx
	{
		get;
		private set;
	}

	public string FizzleFx
	{
		get;
		private set;
	}

	public string ProjectileFx
	{
		get;
		private set;
	}

	public string ImpactFx
	{
		get;
		private set;
	}

	public string HitFx
	{
		get;
		private set;
	}

	public string TrailColor
	{
		get;
		private set;
	}

	public bool ProjFromTarget
	{
		get;
		private set;
	}

	public bool OverrideVariation
	{
		get;
		private set;
	}

	public int Variation
	{
		get;
		private set;
	}

	public override void Populate(SqliteDataReader reader)
	{
		Id = reader.GetInt32(0);
		SkillId = (SkillId)reader.GetInt32(1);
		SequenceId = (SequenceId)reader.GetInt32(2);
		RightHandFx = reader.GetString(3);
		LeftHandFx = reader.GetString(4);
		ChargeFx = reader.GetString(5);
		ChargeOnTarget = (reader.GetInt32(6) != 0);
		LaunchFx = reader.GetString(7);
		FizzleFx = reader.GetString(8);
		ProjectileFx = reader.GetString(9);
		ImpactFx = reader.GetString(10);
		HitFx = reader.GetString(11);
		TrailColor = reader.GetString(12);
		ProjFromTarget = (reader.GetInt32(13) != 0);
		OverrideVariation = (reader.GetInt32(14) != 0);
		Variation = reader.GetInt32(15);
	}
}
