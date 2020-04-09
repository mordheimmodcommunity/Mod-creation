using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkirmishLobbyPlayer : MonoBehaviour
{
	[SerializeField]
	private Image flag;

	[SerializeField]
	private Toggle ready;

	[SerializeField]
	private Text playerName;

	[SerializeField]
	private Text warbandName;

	[SerializeField]
	private Text campaign;

	[SerializeField]
	private Text rating;

	[SerializeField]
	private Image ratingIcon;

	[SerializeField]
	private Text difficultyText;

	[SerializeField]
	private Text rank;

	private void Awake()
	{
	}

	public void SetPlayerInfo(int index)
	{
		if (index == -1)
		{
			return;
		}
		MissionWarbandSave missionWarbandSave = PandoraSingleton<MissionStartData>.Instance.FightingWarbands[index];
		if (missionWarbandSave == null)
		{
			return;
		}
		playerName.set_text(missionWarbandSave.Name);
		flag.set_sprite(Warband.GetFlagIcon(missionWarbandSave.WarbandId));
		warbandName.set_text(missionWarbandSave.PlayerName);
		ready.set_isOn(missionWarbandSave.PlayerTypeId == PlayerTypeId.AI || missionWarbandSave.IsReady);
		((Selectable)ready).set_interactable(false);
		rating.set_text(missionWarbandSave.Rating.ToString());
		if ((Object)(object)ratingIcon != null)
		{
			if (PandoraSingleton<MissionStartData>.Instance.FightingWarbands.Count > 1)
			{
				((Component)(object)ratingIcon).gameObject.SetActive(value: true);
				int num = 0;
				List<ProcMissionRatingData> list = PandoraSingleton<DataFactory>.Instance.InitData<ProcMissionRatingData>();
				for (int i = 0; i < list.Count; i++)
				{
					num = Mathf.Max(num, list[i].MaxValue);
				}
				MissionWarbandSave missionWarbandSave2 = PandoraSingleton<MissionStartData>.Instance.FightingWarbands[(index == 0) ? 1 : 0];
				ProcMissionRatingId procMissionRatingId = ProcMissionRatingId.NONE;
				if (missionWarbandSave.PlayerTypeId == PlayerTypeId.PLAYER)
				{
					int value = (int)(((float)missionWarbandSave.Rating / (float)missionWarbandSave2.Rating - 1f) * 100f);
					value = Mathf.Clamp(value, 0, num);
					procMissionRatingId = PandoraSingleton<DataFactory>.Instance.InitDataClosest<ProcMissionRatingData>("max_value", value, lower: false).Id;
					procMissionRatingId = ((procMissionRatingId == ProcMissionRatingId.NONE) ? ProcMissionRatingId.NORMAL : procMissionRatingId);
				}
				else
				{
					procMissionRatingId = (ProcMissionRatingId)PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.ratingId;
				}
				ratingIcon.set_sprite(PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("icn_mission_difficulty_" + procMissionRatingId.ToLowerString(), cached: true));
				if ((Object)(object)difficultyText != null)
				{
					((Component)(object)difficultyText).gameObject.SetActive(value: true);
					difficultyText.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("mission_difficulty_" + procMissionRatingId.ToLowerString()));
				}
			}
			else
			{
				((Component)(object)ratingIcon).gameObject.SetActive(value: false);
				if ((Object)(object)difficultyText != null)
				{
					((Component)(object)difficultyText).gameObject.SetActive(value: false);
				}
			}
		}
		rank.set_text(missionWarbandSave.Rank.ToString());
	}
}
