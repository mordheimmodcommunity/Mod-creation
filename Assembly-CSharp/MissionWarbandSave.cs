using System.Collections.Generic;
using System.IO;

public class MissionWarbandSave : IThoth
{
	private int lastVersion;

	public List<uint> openedSearches = new List<uint>();

	public WarbandId WarbandId
	{
		get;
		private set;
	}

	public CampaignWarbandId CampaignWarId
	{
		get;
		private set;
	}

	public string Name
	{
		get;
		private set;
	}

	public string OverrideName
	{
		get;
		private set;
	}

	public string PlayerName
	{
		get;
		private set;
	}

	public int Rank
	{
		get;
		private set;
	}

	public int Rating
	{
		get;
		private set;
	}

	public int PlayerIndex
	{
		get;
		private set;
	}

	public PlayerTypeId PlayerTypeId
	{
		get;
		private set;
	}

	public bool IsReady
	{
		get;
		set;
	}

	public string[] SerializedUnits
	{
		get;
		private set;
	}

	public List<UnitSave> Units
	{
		get;
		set;
	}

	public MissionWarbandSave()
	{
	}

	public MissionWarbandSave(WarbandId type, CampaignWarbandId campaignId, string name, string overrideName, string playerName, int rank, int rating, int playerIndex, PlayerTypeId playerTypeId, string[] units)
	{
		WarbandId = type;
		CampaignWarId = campaignId;
		Name = ((string.IsNullOrEmpty(overrideName) || (PandoraSingleton<Hermes>.Instance.PlayerIndex != playerIndex && PandoraSingleton<Hephaestus>.Instance.IsPrivilegeRestricted(Hephaestus.RestrictionId.UGC))) ? name : overrideName);
		OverrideName = overrideName;
		PlayerName = playerName;
		Rank = rank;
		Rating = rating;
		PlayerIndex = playerIndex;
		PlayerTypeId = playerTypeId;
		SerializedUnits = units;
		Units = new List<UnitSave>();
		if (units != null)
		{
			for (int i = 0; i < units.Length; i++)
			{
				UnitSave unitSave = new UnitSave();
				Thoth.ReadFromString(units[i], unitSave);
				Units.Add(unitSave);
			}
		}
		ResetReady();
	}

	void IThoth.Read(BinaryReader reader)
	{
		int i = 0;
		Thoth.Read(reader, out int i2);
		lastVersion = i2;
		Thoth.Read(reader, out i);
		int i3 = 0;
		Thoth.Read(reader, out i3);
		WarbandId = (WarbandId)i3;
		Thoth.Read(reader, out i3);
		CampaignWarId = (CampaignWarbandId)i3;
		string s = null;
		Thoth.Read(reader, out s);
		Name = s;
		Thoth.Read(reader, out s);
		PlayerName = s;
		Thoth.Read(reader, out i3);
		Rank = i3;
		Thoth.Read(reader, out i3);
		Rating = i3;
		Thoth.Read(reader, out i3);
		PlayerIndex = i3;
		Thoth.Read(reader, out i3);
		PlayerTypeId = (PlayerTypeId)i3;
		int i4 = 0;
		Thoth.Read(reader, out i4);
		Units = new List<UnitSave>(i4);
		for (int j = 0; j < i4; j++)
		{
			UnitSave unitSave = new UnitSave();
			((IThoth)unitSave).Read(reader);
			Units.Add(unitSave);
		}
		if (i2 > 0)
		{
			i4 = 0;
			Thoth.Read(reader, out i4);
			for (int k = 0; k < i4; k++)
			{
				uint i5 = 0u;
				Thoth.Read(reader, out i5);
				openedSearches.Add(i5);
			}
		}
	}

	public int GetVersion()
	{
		return 1;
	}

	public int GetCRC(bool read)
	{
		return CalculateCRC(read);
	}

	public WarbandSave ToWarbandSave()
	{
		WarbandSave warbandSave = new WarbandSave(WarbandId);
		warbandSave.name = Name;
		warbandSave.units = Units;
		return warbandSave;
	}

	public void ResetReady()
	{
		IsReady = (PlayerTypeId == PlayerTypeId.AI);
	}

	private int CalculateCRC(bool read)
	{
		int num = (!read) ? ((IThoth)this).GetVersion() : lastVersion;
		int num2 = 0;
		num2 = (int)(num2 + WarbandId);
		num2 = (int)(num2 + CampaignWarId);
		char[] array = Name.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			num2 += array[i];
		}
		array = PlayerName.ToCharArray();
		for (int j = 0; j < array.Length; j++)
		{
			num2 += array[j];
		}
		num2 += Rank;
		num2 += Rating;
		num2 += PlayerIndex;
		num2 = (int)(num2 + PlayerTypeId);
		for (int k = 0; k < Units.Count; k++)
		{
			num2 += Units[k].GetCRC(read);
		}
		if (num > 0)
		{
			for (int l = 0; l < openedSearches.Count; l++)
			{
				num2 += (int)openedSearches[l];
			}
		}
		return num2;
	}

	public void Write(BinaryWriter writer)
	{
		int version = ((IThoth)this).GetVersion();
		Thoth.Write(writer, version);
		int cRC = GetCRC(read: false);
		Thoth.Write(writer, cRC);
		Thoth.Write(writer, (int)WarbandId);
		Thoth.Write(writer, (int)CampaignWarId);
		Thoth.Write(writer, Name);
		Thoth.Write(writer, PlayerName);
		Thoth.Write(writer, Rank);
		Thoth.Write(writer, Rating);
		Thoth.Write(writer, PlayerIndex);
		Thoth.Write(writer, (int)PlayerTypeId);
		Thoth.Write(writer, Units.Count);
		for (int i = 0; i < Units.Count; i++)
		{
			((IThoth)Units[i]).Write(writer);
		}
		Thoth.Write(writer, openedSearches.Count);
		for (int j = 0; j < openedSearches.Count; j++)
		{
			Thoth.Write(writer, openedSearches[j]);
		}
	}
}
