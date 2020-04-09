using System.Collections.Generic;

public class MessageSorter : IComparer<CampaignMissionMessageData>
{
	int IComparer<CampaignMissionMessageData>.Compare(CampaignMissionMessageData x, CampaignMissionMessageData y)
	{
		if (x.Position > y.Position)
		{
			return 1;
		}
		if (x.Position < y.Position)
		{
			return -1;
		}
		return 0;
	}
}
