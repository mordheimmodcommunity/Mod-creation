using System.Collections.Generic;
using System.IO;

public class ProfileSave : IThoth
{
	private int lastVersion;

	public int unspentPoint;

	public int xp;

	public int rankId;

	public int lastPlayedCampaign;

	public int[] stats = new int[63];

	public bool[] unlockedAchievements = new bool[227];

	public bool[] completedTutorials;

	public int[] warProgress = new int[6];

	public Dictionary<int, int> warbandSaves;

	public bool xpChecked;

	public ProfileSave()
	{
		unspentPoint = 0;
		xp = 0;
		rankId = 0;
		lastPlayedCampaign = -1;
		completedTutorials = new bool[Constant.GetInt(ConstantId.TUTORIALS_COUNT)];
		for (int i = 0; i < completedTutorials.Length; i++)
		{
			completedTutorials[i] = false;
		}
		for (int j = 0; j < warProgress.Length; j++)
		{
			warProgress[j] = 0;
		}
		warbandSaves = new Dictionary<int, int>();
		xpChecked = false;
	}

	int IThoth.GetVersion()
	{
		return 7;
	}

	void IThoth.Write(BinaryWriter writer)
	{
		int version = ((IThoth)this).GetVersion();
		Thoth.Write(writer, version);
		int cRC = GetCRC(read: false);
		int num = (int)PandoraSingleton<Hephaestus>.Instance.GetUserId();
		Thoth.Write(writer, cRC + num);
		Thoth.Write(writer, unspentPoint);
		Thoth.Write(writer, xp);
		Thoth.Write(writer, rankId);
		Thoth.Write(writer, lastPlayedCampaign);
		Thoth.Write(writer, stats.Length);
		for (int i = 0; i < stats.Length; i++)
		{
			Thoth.Write(writer, stats[i]);
		}
		Thoth.Write(writer, unlockedAchievements.Length);
		for (int j = 0; j < unlockedAchievements.Length; j++)
		{
			Thoth.Write(writer, unlockedAchievements[j]);
		}
		Thoth.Write(writer, completedTutorials.Length);
		for (int k = 0; k < completedTutorials.Length; k++)
		{
			Thoth.Write(writer, completedTutorials[k]);
		}
		Thoth.Write(writer, warbandSaves.Count);
		foreach (KeyValuePair<int, int> warbandSafe in warbandSaves)
		{
			Thoth.Write(writer, warbandSafe.Key);
			Thoth.Write(writer, warbandSafe.Value);
		}
		Thoth.Write(writer, warProgress.Length);
		for (int l = 0; l < warProgress.Length; l++)
		{
			Thoth.Write(writer, warProgress[l]);
		}
		Thoth.Write(writer, xpChecked);
	}

	void IThoth.Read(BinaryReader reader)
	{
		int i = 0;
		Thoth.Read(reader, out int i2);
		lastVersion = i2;
		if (i2 > 3)
		{
			Thoth.Read(reader, out i);
		}
		Thoth.Read(reader, out unspentPoint);
		Thoth.Read(reader, out xp);
		Thoth.Read(reader, out rankId);
		if (i2 > 2)
		{
			Thoth.Read(reader, out lastPlayedCampaign);
		}
		Thoth.Read(reader, out int i3);
		for (int j = 0; j < i3; j++)
		{
			Thoth.Read(reader, out int i4);
			stats[j] = i4;
		}
		Thoth.Read(reader, out i3);
		for (int k = 0; k < i3; k++)
		{
			Thoth.Read(reader, out bool b);
			unlockedAchievements[k] = b;
		}
		if (i2 > 1)
		{
			Thoth.Read(reader, out i3);
			for (int l = 0; l < i3; l++)
			{
				Thoth.Read(reader, out bool b2);
				completedTutorials[l] = b2;
			}
		}
		if (i2 > 4)
		{
			Thoth.Read(reader, out i3);
			for (int m = 0; m < i3; m++)
			{
				Thoth.Read(reader, out int i5);
				Thoth.Read(reader, out int i6);
				warbandSaves[i5] = i6;
			}
		}
		if (i2 > 5)
		{
			Thoth.Read(reader, out i3);
			for (int n = 0; n < i3; n++)
			{
				Thoth.Read(reader, out int i7);
				warProgress[n] = i7;
			}
		}
		if (i2 > 6)
		{
			Thoth.Read(reader, out xpChecked);
		}
	}

	public int GetCRC(bool read)
	{
		return CalculateCRC(read);
	}

	private int CalculateCRC(bool read)
	{
		int num = (!read) ? ((IThoth)this).GetVersion() : lastVersion;
		int num2 = 0;
		num2 += unspentPoint;
		num2 += xp;
		num2 += rankId;
		num2 += lastPlayedCampaign;
		for (int i = 0; i < stats.Length; i++)
		{
			num2 += stats[i];
		}
		for (int j = 0; j < unlockedAchievements.Length; j++)
		{
			num2 += (unlockedAchievements[j] ? 1 : 0);
		}
		for (int k = 0; k < completedTutorials.Length; k++)
		{
			num2 += (completedTutorials[k] ? 1 : 0);
		}
		foreach (KeyValuePair<int, int> warbandSafe in warbandSaves)
		{
			num2 += warbandSafe.Value + warbandSafe.Key;
		}
		if (num > 6)
		{
			num2 += (xpChecked ? 1 : 0);
		}
		return num2;
	}
}
