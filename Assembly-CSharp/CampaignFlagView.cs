using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CampaignFlagView : MonoBehaviour
{
    public Image icon;

    public Text loadingTitle;

    public GameObject info;

    public Text textTitle;

    public Text textRank;

    public Text lastSaveTime;

    public GameObject campaignOver;

    public Text campaignOverTxt;

    [HideInInspector]
    public bool loaded;

    [HideInInspector]
    public bool isValid = true;

    private WarbandSave warband;

    private int saveIndex;

    private UnityAction<int, int> selectCb;

    private UnityAction<int, WarbandSave> confirmCb;

    private int cachedWarbandId;

    public void Load(int idx, UnityAction<int, int> select, UnityAction<int, WarbandSave> confirm, int uiIdx)
    {
        loaded = false;
        if (PandoraSingleton<GameManager>.Instance.Save.CampaignExist(idx))
        {
            saveIndex = idx;
            ((Component)(object)loadingTitle).gameObject.SetActive(value: true);
            info.SetActive(value: false);
            campaignOver.SetActive(value: false);
            PandoraSingleton<GameManager>.Instance.Save.GetCampaignInfo(idx, OnSaveLoaded);
            selectCb = select;
            confirmCb = confirm;
            ToggleEffects toggleEffects = GetComponentsInChildren<ToggleEffects>()[0];
            toggleEffects.onSelect.RemoveAllListeners();
            toggleEffects.onSelect.AddListener(OnSelect);
            toggleEffects.onAction.RemoveAllListeners();
            toggleEffects.onAction.AddListener(OnConfirm);
            if (uiIdx == 0)
            {
                toggleEffects.SetOn();
            }
        }
    }

    private void OnSaveLoaded(WarbandSave warSave)
    {
        ((Component)(object)loadingTitle).gameObject.SetActive(value: false);
        info.SetActive(value: true);
        cachedWarbandId = warSave.id;
        DateTime cachedSaveTimeStamp = PandoraSingleton<GameManager>.Instance.Save.GetCachedSaveTimeStamp(saveIndex);
        string text = cachedSaveTimeStamp.ToShortDateString() + " - " + cachedSaveTimeStamp.ToShortTimeString();
        textTitle.set_text(warSave.Name);
        textRank.set_text(warSave.rank.ToString());
        lastSaveTime.set_text(text);
        icon.set_sprite(Warband.GetIcon((WarbandId)warSave.id));
        this.warband = warSave;
        loaded = true;
        isValid = true;
        if (PandoraSingleton<Hephaestus>.Instance.IsJoiningInvite())
        {
            Warband warband = new Warband(warSave);
            if (warband.ValidateWarbandForInvite(inMission: false))
            {
                campaignOver.SetActive(value: false);
                return;
            }
            isValid = false;
            campaignOver.SetActive(value: true);
            campaignOverTxt.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("invite_select_warband_invalid"));
        }
        else if (PandoraSingleton<Hephaestus>.Instance.IsPlayTogether())
        {
            Warband warband2 = new Warband(warSave);
            if (!warSave.inMission && (warband2.IsSkirmishAvailable(out string reason) || warband2.IsContestAvailable(out reason)))
            {
                campaignOver.SetActive(value: false);
                return;
            }
            isValid = false;
            campaignOver.SetActive(value: true);
            campaignOverTxt.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("invite_select_warband_invalid"));
        }
        else if (warSave.lateShipmentCount >= Constant.GetInt(ConstantId.MAX_SHIPMENT_FAIL))
        {
            campaignOver.SetActive(value: true);
            campaignOverTxt.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("game_over"));
        }
        else
        {
            campaignOver.SetActive(value: false);
        }
    }

    private void OnSelect()
    {
        if (selectCb != null)
        {
            selectCb(saveIndex, cachedWarbandId);
        }
    }

    private void OnConfirm()
    {
        if (confirmCb != null && isValid)
        {
            confirmCb(saveIndex, warband);
        }
    }
}
