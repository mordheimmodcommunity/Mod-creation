using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadCampaignMenuState : UIStateMonoBehaviour<MainMenuController>
{
	public Text title;

	public GameObject campaignEntry;

	public ScrollGroup campaignsList;

	public Text campaignCount;

	public MenuNode flag;

	public ButtonGroup actionButton;

	public ButtonGroup cancelButton;

	public ButtonGroup deleteButton;

	public Sprite icnBack;

	private Dictionary<WarbandId, GameObject> loadedBanners;

	public GameObject camPos;

	private bool allSaveLoaded;

	private bool isInviteDuringLoadingCampaign;

	public override int StateId => 6;

	public override void Awake()
	{
		base.Awake();
		loadedBanners = new Dictionary<WarbandId, GameObject>();
		if (deleteButton != null)
		{
			deleteButton.gameObject.SetActive(value: false);
		}
	}

	public override void StateEnter()
	{
		Show(visible: true);
		base.StateMachine.camManager.dummyCam.transform.position = camPos.transform.position;
		base.StateMachine.camManager.dummyCam.transform.rotation = camPos.transform.rotation;
		base.StateMachine.camManager.Transition();
		cancelButton.SetAction("cancel", "menu_back", 0, negative: false, icnBack);
		cancelButton.OnAction(OnQuit, mouseOnly: false);
		actionButton.SetAction("action", "menu_confirm");
		deleteButton.SetAction("delete_campaign", "main_delete_campaign");
		deleteButton.OnAction(DeleteCampaign, mouseOnly: false);
		OnInputTypeChanged();
		FillCampaignsList();
		title.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById((!PandoraSingleton<Hephaestus>.Instance.IsJoiningInvite()) ? "menu_load_game" : "invite_select_warband_title"));
		PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.INPUT_TYPE_CHANGED, OnInputTypeChanged);
	}

	public void FillCampaignsList()
	{
		allSaveLoaded = false;
		campaignsList.ClearList();
		campaignsList.Setup(campaignEntry, hideBarIfEmpty: false);
		List<int> campaignSlots = PandoraSingleton<GameManager>.Instance.Save.GetCampaignSlots();
		actionButton.gameObject.SetActive(campaignSlots.Count > 0);
		if (PandoraSingleton<Hephaestus>.Instance.IsJoiningInvite() || PandoraSingleton<Hephaestus>.Instance.IsPlayTogether())
		{
			deleteButton.gameObject.SetActive(value: false);
		}
		else
		{
			deleteButton.gameObject.SetActive(campaignSlots.Count > 0);
		}
		campaignCount.set_text(campaignSlots.Count + "/" + PandoraSingleton<GameManager>.Instance.Save.MaxSaveGames);
		StartCoroutine(StartLoadCampaigns(campaignSlots));
	}

	private IEnumerator StartLoadCampaigns(List<int> slots)
	{
		int nbValidWarband = 0;
		for (int i = 0; i < slots.Count; i++)
		{
			CampaignFlagView newEntry = campaignsList.AddToList(null, null).GetComponent<CampaignFlagView>();
			if (newEntry.campaignOver.activeSelf)
			{
				nbValidWarband++;
			}
			newEntry.Load(slots[i], OnSelectCampaign, OnConfirmCampaign, i);
			while (!newEntry.loaded)
			{
				yield return null;
			}
		}
		if (nbValidWarband == 0)
		{
			if (PandoraSingleton<Hephaestus>.Instance.IsJoiningInvite())
			{
				PandoraSingleton<Hephaestus>.Instance.ResetInvite();
				PandoraSingleton<GameManager>.Instance.ShowSystemPopup(GameManager.SystemPopupId.INVITE, "invite_no_valid_warband_title", "invite_no_valid_warband_desc", null);
			}
			else if (PandoraSingleton<Hephaestus>.Instance.IsPlayTogether())
			{
				PandoraSingleton<Hephaestus>.Instance.ResetPlayTogether(setPassive: true);
				PandoraSingleton<GameManager>.Instance.ShowSystemPopup(GameManager.SystemPopupId.INVITE, "play_together_no_valid_warband_title", "play_together_no_valid_warband_desc", null);
			}
		}
		else if (PandoraSingleton<Hephaestus>.Instance.IsJoiningInvite())
		{
			PandoraSingleton<GameManager>.Instance.ShowSystemPopup(GameManager.SystemPopupId.INVITE, "invite_hideout_quit_title", (!PandoraSingleton<Hephaestus>.Instance.GetJoiningLobby().isExhibition) ? "invite_message_contest" : "invite_message_exhibition", OnPopupClose);
		}
		yield return null;
		yield return null;
		allSaveLoaded = true;
	}

	private void OnPopupClose(bool obj)
	{
		ToggleEffects component = campaignsList.items[0].GetComponent<ToggleEffects>();
		component.SetSelected(force: true);
		StartCoroutine(SelectCampaignEntry());
	}

	private IEnumerator SelectCampaignEntry()
	{
		yield return null;
		yield return null;
		yield return null;
		ToggleEffects effects = campaignsList.items[0].GetComponent<ToggleEffects>();
		effects.SetSelected(force: true);
	}

	public void OnSelectCampaign(int campaignIdx, int warbandId)
	{
		PandoraSingleton<GameManager>.Instance.campaign = campaignIdx;
		if (warbandId != 0)
		{
			if (!loadedBanners.ContainsKey((WarbandId)warbandId))
			{
				WarbandData warbandData = PandoraSingleton<DataFactory>.Instance.InitData<WarbandData>(warbandId);
				string wagon = warbandData.Wagon;
				wagon = wagon.Substring(5);
				wagon = "banner" + wagon;
				PandoraSingleton<AssetBundleLoader>.Instance.LoadAssetAsync<GameObject>("Assets/prefabs/environments/props/banners/", AssetBundleId.PROPS, wagon + ".prefab", delegate(Object banPrefab)
				{
					loadedBanners[(WarbandId)warbandId] = (GameObject)Object.Instantiate(banPrefab);
					Dissolver dissolver = loadedBanners[(WarbandId)warbandId].AddComponent<Dissolver>();
					dissolver.Hide(hide: true, force: true);
				});
			}
			flag.gameObject.SetActive(value: true);
			GameObject content = flag.GetContent();
			if (content != null)
			{
				WarbandId wbId = (WarbandId)warbandId;
				content.GetComponent<Dissolver>().Hide(hide: true, force: false, delegate
				{
					SetupBanner(wbId);
				});
			}
			else
			{
				SetupBanner((WarbandId)warbandId);
			}
		}
	}

	private void SetupBanner(WarbandId wbId)
	{
		if (loadedBanners.ContainsKey(wbId))
		{
			GameObject gameObject = loadedBanners[wbId];
			gameObject.SetActive(value: true);
			Cloth cloth = gameObject.GetComponentsInChildren<Cloth>(includeInactive: true)[0];
			cloth.enabled = false;
			flag.SetContent(gameObject);
			cloth.enabled = true;
			gameObject.GetComponent<Dissolver>().Hide(hide: true, force: true);
			gameObject.GetComponent<Dissolver>().Hide(hide: false);
		}
	}

	public void OnConfirmCampaign(int campaignIdx, WarbandSave warbandSave)
	{
		if (allSaveLoaded && PandoraSingleton<PandoraInput>.Instance.CurrentInputLayer != 1 && !PandoraSingleton<TransitionManager>.Instance.IsLoading())
		{
			PandoraDebug.LogInfo("Confirmed Campaign");
			PandoraSingleton<GameManager>.Instance.campaign = campaignIdx;
			PandoraSingleton<GameManager>.Instance.currentSave = warbandSave;
			if (PandoraSingleton<GameManager>.Instance.Save.CampaignExist(PandoraSingleton<GameManager>.Instance.campaign))
			{
				PandoraSingleton<GameManager>.Instance.Save.LoadCampaign(PandoraSingleton<GameManager>.Instance.campaign);
			}
		}
	}

	public void DeleteCampaign()
	{
		if (!allSaveLoaded || PandoraSingleton<PandoraInput>.Instance.CurrentInputLayer == 1 || !PandoraSingleton<GameManager>.Instance.Save.CampaignExist(PandoraSingleton<GameManager>.Instance.campaign))
		{
			return;
		}
		List<int> campaignSlots = PandoraSingleton<GameManager>.Instance.Save.GetCampaignSlots();
		int num = 0;
		while (true)
		{
			if (num < campaignSlots.Count)
			{
				if (campaignSlots[num] == PandoraSingleton<GameManager>.Instance.campaign)
				{
					break;
				}
				num++;
				continue;
			}
			return;
		}
		base.StateMachine.ConfirmPopup.ShowLocalized(PandoraSingleton<LocalizationManager>.Instance.GetStringById("main_delete_campaign"), PandoraSingleton<LocalizationManager>.Instance.GetStringById("main_delete_campaign_confirm", campaignsList.items[num].GetComponent<CampaignFlagView>().textTitle.get_text()), OnDeletePopup);
	}

	private void OnDeletePopup(bool isConfirm)
	{
		if (isConfirm)
		{
			PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.GAME_DELETED, OnDeleteSave);
			PandoraSingleton<GameManager>.Instance.Save.DeleteCampaign(PandoraSingleton<GameManager>.Instance.campaign);
			PandoraSingleton<GameManager>.Instance.campaign = -1;
			FillCampaignsList();
			if (flag.IsOccupied())
			{
				flag.GetContent().GetComponent<Dissolver>().Hide(hide: true);
			}
		}
	}

	private void OnDeleteSave()
	{
		PandoraSingleton<NoticeManager>.Instance.RemoveListener(Notices.GAME_DELETED, OnDeleteSave);
	}

	public override void OnInputCancel()
	{
		OnQuit();
	}

	public void OnQuit()
	{
		if (PandoraSingleton<Hephaestus>.Instance.IsJoiningInvite())
		{
			base.StateMachine.ConfirmPopup.Show("invite_select_warband_quit_title", "invite_select_warband_quit_desc", OnQuitConfirm);
		}
		else if (PandoraSingleton<Hephaestus>.Instance.IsPlayTogether())
		{
			base.StateMachine.ConfirmPopup.Show("play_together_select_warband_quit_title", "play_together_select_warband_quit_desc", OnQuitConfirm);
		}
		else
		{
			base.StateMachine.ChangeState(MainMenuController.State.MAIN_MENU);
		}
	}

	private void OnQuitConfirm(bool confirm)
	{
		if (confirm)
		{
			PandoraSingleton<Hephaestus>.Instance.ResetInvite();
			PandoraSingleton<Hephaestus>.Instance.ResetPlayTogether(setPassive: true);
			base.StateMachine.ChangeState(MainMenuController.State.MAIN_MENU);
		}
	}

	public override void StateExit()
	{
		cancelButton.gameObject.SetActive(value: false);
		actionButton.gameObject.SetActive(value: false);
		deleteButton.gameObject.SetActive(value: false);
		Show(visible: false);
		if (flag.IsOccupied())
		{
			Dissolver component = flag.GetContent().GetComponent<Dissolver>();
			component.Hide(hide: true);
		}
		PandoraSingleton<NoticeManager>.Instance.RemoveListener(Notices.INPUT_TYPE_CHANGED, OnInputTypeChanged);
	}

	private void OnInputTypeChanged()
	{
		actionButton.gameObject.SetActive(PandoraSingleton<PandoraInput>.Instance.lastInputMode == PandoraInput.InputMode.JOYSTICK);
	}
}
