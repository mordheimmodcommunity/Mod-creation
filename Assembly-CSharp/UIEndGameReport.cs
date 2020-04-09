using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIEndGameReport : CanvasGroupDisabler
{
	public Sprite lockedSprite;

	public Sprite unlockedSprite;

	public Text fragments;

	public Text shards;

	public Text clusters;

	public Text gold;

	public List<GameObject> extraRewards;

	public Text noExtraRewards;

	public CanvasGroup objectivesGroup;

	public Image objectivesLock;

	public Text objectivesExperience;

	public CanvasGroup battlegroundVictoryGroup;

	public Image battlegroundVictoryLock;

	public Text battlegroundVictoryFragments;

	public Text battlegroundVictoryShards;

	public Text battlegroundVictoryClusters;

	public Text battlegroundVictorySearch;

	public Image playerWarbandIcon;

	public Text playerWarbandName;

	public Text playerCasualties;

	public UIEngameMVUStats playerMVU;

	public Image enemyWarbandIcon;

	public Text enemyWarbandName;

	public Text enemyCasualties;

	public UIEngameMVUStats enemyMVU;

	public Text centerTitle;

	public Text centerSubtitle;

	public Image centerIcon;

	public Image centerOverlay;

	public Sprite overlayEnemy;

	public Sprite overlayPlayer;

	public ButtonGroup button;

	private bool isShow;

	private void Awake()
	{
	}

	public void Show()
	{
		isShow = true;
		base.gameObject.SetActive(value: true);
		PandoraSingleton<PandoraInput>.Instance.PushInputLayer(PandoraInput.InputLayer.END_GAME);
		button.SetAction("action", "menu_continue", 3);
		button.OnAction(OnContinue, mouseOnly: false);
		WarbandController myWarbandCtrlr = PandoraSingleton<MissionManager>.Instance.GetMyWarbandCtrlr();
		List<WarbandController> enemyWarbandCtrlrs = PandoraSingleton<MissionManager>.Instance.GetEnemyWarbandCtrlrs();
		centerIcon.set_sprite(Warband.GetIcon(myWarbandCtrlr.WarData.Id));
		VictoryTypeId victoryType = PandoraSingleton<MissionManager>.Instance.MissionEndData.VictoryType;
		if (victoryType != 0)
		{
			centerOverlay.set_sprite(overlayPlayer);
			centerTitle.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById((victoryType != VictoryTypeId.OBJECTIVE) ? ("mission_victory_" + PandoraSingleton<MissionManager>.Instance.MissionEndData.VictoryType) : "mission_victory_objective"));
			centerSubtitle.set_text(string.Empty);
		}
		else
		{
			centerOverlay.set_sprite(overlayEnemy);
			centerTitle.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("mission_victory_loss"));
			centerSubtitle.set_text(string.Empty);
		}
		SetObjectivesRewards(myWarbandCtrlr);
		SetBattlegroundVictoryRewards(myWarbandCtrlr);
		SetTreasury(myWarbandCtrlr);
		SetExtraRewards(myWarbandCtrlr);
		int outOfAction = GetOutOfAction(myWarbandCtrlr.unitCtrlrs);
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < enemyWarbandCtrlrs.Count; i++)
		{
			num += GetOutOfAction(enemyWarbandCtrlrs[i].unitCtrlrs);
			num2 += enemyWarbandCtrlrs[i].unitCtrlrs.Count;
		}
		playerWarbandName.set_text(myWarbandCtrlr.name);
		playerWarbandIcon.set_sprite(Warband.GetIcon(myWarbandCtrlr.WarData.Id));
		playerCasualties.set_text($"{outOfAction}/{myWarbandCtrlr.unitCtrlrs.Count}");
		enemyWarbandName.set_text(enemyWarbandCtrlrs[0].name);
		if (enemyWarbandCtrlrs[0].WarData.Basic)
		{
			enemyWarbandIcon.set_sprite(Warband.GetIcon(enemyWarbandCtrlrs[0].WarData.Id));
		}
		else if (enemyWarbandCtrlrs[0].unitCtrlrs.Count == 0)
		{
			((Behaviour)(object)enemyWarbandIcon).enabled = false;
		}
		else
		{
			enemyWarbandIcon.set_sprite(enemyWarbandCtrlrs[0].unitCtrlrs[0].unit.GetIcon());
		}
		if (PandoraSingleton<MissionManager>.Instance.MissionEndData.isCampaign)
		{
			enemyCasualties.set_text(num.ToConstantString());
		}
		else
		{
			enemyCasualties.set_text($"{num}/{enemyWarbandCtrlrs[0].unitCtrlrs.Count}");
		}
		for (int j = 0; j < myWarbandCtrlr.unitCtrlrs.Count; j++)
		{
			if (myWarbandCtrlr.unitCtrlrs[j].unit.UnitSave.warbandSlotIndex == PandoraSingleton<MissionManager>.Instance.MissionEndData.playerMVUIdx)
			{
				playerMVU.Set(myWarbandCtrlr.unitCtrlrs[j]);
				break;
			}
		}
		if (PandoraSingleton<MissionManager>.Instance.MissionEndData.isCampaign)
		{
			int num3 = -1;
			UnitController unitController = null;
			for (int k = 0; k < enemyWarbandCtrlrs.Count; k++)
			{
				if (enemyWarbandCtrlrs[k].unitCtrlrs.Count > 0)
				{
					UnitController unitController2 = enemyWarbandCtrlrs[k].unitCtrlrs[enemyWarbandCtrlrs[k].GetMVU(PandoraSingleton<MissionManager>.Instance.NetworkTyche, enemy: true)];
					int attribute = unitController2.unit.GetAttribute(AttributeId.CURRENT_MVU);
					if (attribute > num3)
					{
						num3 = attribute;
						unitController = unitController2;
					}
				}
			}
			enemyMVU.Set(unitController);
		}
		else
		{
			enemyMVU.Set(enemyWarbandCtrlrs[0].unitCtrlrs[PandoraSingleton<MissionManager>.Instance.MissionEndData.enemyMVUIdx]);
		}
	}

	public int GetOutOfAction(List<UnitController> units)
	{
		int num = 0;
		for (int i = 0; i < units.Count; i++)
		{
			if (units[i].unit.Status == UnitStateId.OUT_OF_ACTION)
			{
				num++;
			}
		}
		return num;
	}

	public void SetObjectivesRewards(WarbandController playerWarband)
	{
		if (playerWarband.objectives.Count > 0 && playerWarband.AllObjectivesCompleted)
		{
			objectivesLock.set_sprite(unlockedSprite);
			objectivesGroup.alpha = 1f;
			objectivesExperience.set_text(Constant.GetInt(ConstantId.UNIT_XP_OBJECTIVE).ToString("+#;-#"));
		}
		else
		{
			objectivesLock.set_sprite(lockedSprite);
			objectivesGroup.alpha = 0.25f;
			objectivesExperience.set_text(string.Empty);
		}
	}

	public void SetBattlegroundVictoryRewards(WarbandController playerWarband)
	{
		if (!playerWarband.defeated && !PandoraSingleton<MissionManager>.Instance.MissionEndData.isSkirmish)
		{
			battlegroundVictoryGroup.alpha = 1f;
			battlegroundVictoryLock.set_sprite(unlockedSprite);
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			if (playerWarband.wyrdstones != null)
			{
				for (int i = 0; i < playerWarband.wyrdstones.Count; i++)
				{
					Item item = playerWarband.wyrdstones[i];
					switch (item.Id)
					{
					case ItemId.WYRDSTONE_CLUSTER:
						num++;
						break;
					case ItemId.WYRDSTONE_FRAGMENT:
						num2++;
						break;
					case ItemId.WYRDSTONE_SHARD:
						num3++;
						break;
					}
				}
			}
			battlegroundVictoryClusters.set_text(num.ToConstantString());
			battlegroundVictoryFragments.set_text(num2.ToConstantString());
			battlegroundVictoryShards.set_text(num3.ToConstantString());
			battlegroundVictorySearch.set_text(playerWarband.spoilsFound.ToConstantString());
		}
		else
		{
			battlegroundVictoryGroup.alpha = 0.25f;
			battlegroundVictoryLock.set_sprite(lockedSprite);
			battlegroundVictoryClusters.set_text("0");
			battlegroundVictoryFragments.set_text("0");
			battlegroundVictoryShards.set_text("0");
			battlegroundVictorySearch.set_text("0");
		}
	}

	public void SetTreasury(WarbandController playerWarband)
	{
		int goldCount = 0;
		int clustersCount = 0;
		int fragmentsCount = 0;
		int shardsCount = 0;
		if (!PandoraSingleton<MissionManager>.Instance.MissionEndData.isSkirmish)
		{
			IncrementRewardsCount(playerWarband.wagon.chest.items, ref goldCount, ref clustersCount, ref fragmentsCount, ref shardsCount);
			for (int i = 0; i < playerWarband.unitCtrlrs.Count; i++)
			{
				if (playerWarband.unitCtrlrs[i].unit.Status != UnitStateId.OUT_OF_ACTION || !playerWarband.defeated)
				{
					IncrementRewardsCount(playerWarband.unitCtrlrs[i].unit.Items, ref goldCount, ref clustersCount, ref fragmentsCount, ref shardsCount);
				}
			}
		}
		clusters.set_text(clustersCount.ToConstantString());
		fragments.set_text(fragmentsCount.ToConstantString());
		shards.set_text(shardsCount.ToConstantString());
		gold.set_text(goldCount.ToConstantString());
	}

	private void IncrementRewardsCount(List<Item> items, ref int goldCount, ref int clustersCount, ref int fragmentsCount, ref int shardsCount)
	{
		for (int i = 0; i < items.Count; i++)
		{
			switch (items[i].Id)
			{
			case ItemId.WYRDSTONE_CLUSTER:
				clustersCount++;
				break;
			case ItemId.WYRDSTONE_FRAGMENT:
				fragmentsCount++;
				break;
			case ItemId.WYRDSTONE_SHARD:
				shardsCount++;
				break;
			case ItemId.GOLD:
				goldCount += items[i].Amount;
				break;
			}
		}
	}

	public void SetExtraRewards(WarbandController playerWarband)
	{
		if (playerWarband.rewardItems != null)
		{
			((Component)(object)noExtraRewards).gameObject.SetActive(value: false);
			int @int = Constant.GetInt(ConstantId.END_GAME_DECISIVE_REWARD);
			for (int i = 0; i < playerWarband.rewardItems.Count && i < @int; i++)
			{
				GameObject gameObject = extraRewards[i];
				UIInventoryItem component = gameObject.GetComponent<UIInventoryItem>();
				component.Set(playerWarband.rewardItems[i]);
			}
		}
		else
		{
			((Component)(object)noExtraRewards).gameObject.SetActive(value: true);
			for (int j = 0; j < extraRewards.Count; j++)
			{
				extraRewards[j].gameObject.SetActive(value: false);
			}
		}
	}

	public override void OnDisable()
	{
		base.OnDisable();
		if (isShow)
		{
			isShow = false;
		}
	}

	private void OnContinue()
	{
		PandoraSingleton<PandoraInput>.Instance.PopInputLayer(PandoraInput.InputLayer.END_GAME);
		PandoraSingleton<MissionManager>.Instance.ForceQuitMission();
		button.transform.parent.gameObject.SetActive(value: false);
	}
}
