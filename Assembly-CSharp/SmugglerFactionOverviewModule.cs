using DG.Tweening;
using DG.Tweening.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SmugglerFactionOverviewModule : UIModule
{
    private const float CARD_SLIDE_TIME = 0.5f;

    private Vector3 CARD_SLIDE_DESTINATION = new Vector3(40f, 0f, 0f);

    private List<FactionOverviewCard> cards;

    private List<Tweener> cardTweens;

    public FactionOverviewCard factionTemplate;

    public Transform factionsContainer;

    private Action<FactionMenuController> onSelect;

    private Action<FactionMenuController> onConfirm;

    private int primaryCardIdx;

    private int currentSelection;

    private bool init;

    public void Setup(Action<FactionMenuController> onSelectCb, Action<FactionMenuController> onConfirmCb)
    {
        if (!init)
        {
            init = true;
            cards = new List<FactionOverviewCard>(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.factionCtrlrs.Count);
            cardTweens = new List<Tweener>(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.factionCtrlrs.Count);
            for (int i = 0; i < PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.factionCtrlrs.Count; i++)
            {
                FactionOverviewCard factionOverviewCard = UnityEngine.Object.Instantiate(factionTemplate);
                factionOverviewCard.transform.SetParent(factionsContainer, worldPositionStays: false);
                factionOverviewCard.gameObject.SetActive(value: true);
                cards.Add(factionOverviewCard);
                ToggleEffects component = factionOverviewCard.GetComponent<ToggleEffects>();
                FactionMenuController faction = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.factionCtrlrs[i];
                component.onSelect.AddListener(delegate
                {
                    FactionSelected(faction);
                });
                component.onAction.AddListener(delegate
                {
                    FactionConfirmed(faction);
                });
            }
        }
        onSelect = onSelectCb;
        onConfirm = onConfirmCb;
        Refresh();
        currentSelection = primaryCardIdx;
    }

    public void Refresh()
    {
        for (int i = 0; i < PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.factionCtrlrs.Count; i++)
        {
            if (PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.factionCtrlrs[i] != null)
            {
                PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.factionCtrlrs[i].Refresh();
                cards[i].SetFaction(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.factionCtrlrs[i]);
                if (PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.factionCtrlrs[i].Faction.Primary)
                {
                    cards[i].transform.SetAsFirstSibling();
                    primaryCardIdx = i;
                }
            }
        }
    }

    private void OnDisable()
    {
        if (cards != null)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                cards[i].GetComponent<ToggleEffects>().toggle.set_isOn(false);
                ((RectTransform)cards[i].transform.GetChild(0)).localPosition = Vector3.zero;
            }
        }
    }

    public void SetFocus()
    {
        cards[currentSelection].SetSelected();
    }

    public void OnLostFocus()
    {
        HideHighlight();
    }

    private void FactionSelected(FactionMenuController faction)
    {
        ShowHighlight();
        for (int i = 0; i < cardTweens.Count; i++)
        {
            DOTween.Kill(((Tween)cardTweens[i]).id, false);
        }
        cardTweens.Clear();
        for (int j = 0; j < cards.Count; j++)
        {
            RectTransform cardMovingPart = (RectTransform)cards[j].transform.GetChild(0);
            if (cards[j].FactionCtrlr == faction)
            {
                cardTweens.Add((Tweener)(object)DOTween.To((DOGetter<Vector3>)(() => cardMovingPart.localPosition), (DOSetter<Vector3>)delegate (Vector3 v)
                {
                    cardMovingPart.localPosition = v;
                }, CARD_SLIDE_DESTINATION, 0.5f));
                currentSelection = j;
                continue;
            }
            Vector3 localPosition = cardMovingPart.localPosition;
            if (localPosition.x != 0f)
            {
                cardTweens.Add((Tweener)(object)DOTween.To((DOGetter<Vector3>)(() => cardMovingPart.localPosition), (DOSetter<Vector3>)delegate (Vector3 v)
                {
                    cardMovingPart.localPosition = v;
                }, Vector3.zero, 0.5f));
            }
        }
        if (onSelect != null)
        {
            onSelect(faction);
        }
    }

    private void FactionConfirmed(FactionMenuController faction)
    {
        if (onConfirm != null)
        {
            onConfirm(faction);
        }
    }

    private void ShowHighlight()
    {
        HighlightToggle component = cards[currentSelection].GetComponent<HighlightToggle>();
        component.hightlight.enabled = true;
        component.hightlight.gameObject.SetActive(value: true);
        component.hightlight.Highlight(component._targetTransform);
    }

    private void HideHighlight()
    {
        HighlightToggle component = cards[0].GetComponent<HighlightToggle>();
        component.hightlight.gameObject.SetActive(value: false);
        component.hightlight.enabled = false;
    }
}
