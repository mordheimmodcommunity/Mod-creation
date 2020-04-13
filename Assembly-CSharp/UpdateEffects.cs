using System.Collections.Generic;
using UnityEngine;

public class UpdateEffects : ICheapState
{
    private UnitController unitCtrlr;

    private List<Tuple<Enchantment, int>> enchantments;

    private bool showNext;

    public UpdateEffects(UnitController ctrlr)
    {
        unitCtrlr = ctrlr;
    }

    void ICheapState.Destroy()
    {
    }

    void ICheapState.Enter(int iFrom)
    {
        unitCtrlr.SetFixed(fix: true);
        unitCtrlr.currentActionData.SetAction(PandoraSingleton<LocalizationManager>.Instance.GetStringById("skill_name_effect_roll"), PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("action/turn_start_effect", cached: true));
        unitCtrlr.unit.ApplyTurnStartEnchantments();
        enchantments = unitCtrlr.unit.GetEnchantmentDamages(PandoraSingleton<MissionManager>.Instance.NetworkTyche, EnchantmentDmgTriggerId.ON_TURN_START);
        CheckNextEnchantment();
    }

    void ICheapState.Exit(int iTo)
    {
        unitCtrlr.unit.DestroyEnchantments(EnchantmentTriggerId.ON_POST_TURN_START);
        unitCtrlr.attackerCtrlr = null;
    }

    void ICheapState.Update()
    {
        if (showNext)
        {
            showNext = false;
            CheckNextEnchantment();
        }
    }

    void ICheapState.FixedUpdate()
    {
    }

    private void OnSeqDone()
    {
        showNext = true;
    }

    public void TriggerEffect()
    {
        if (unitCtrlr.lastActionWounds != 0)
        {
            unitCtrlr.PlayDefState(unitCtrlr.attackResultId, 0, unitCtrlr.unit.Status);
        }
    }

    private void CheckNextEnchantment()
    {
        if (unitCtrlr.unit.Status == UnitStateId.OUT_OF_ACTION)
        {
            unitCtrlr.KillUnit();
            unitCtrlr.SkillRPC(339);
            return;
        }
        if (enchantments.Count == 0)
        {
            unitCtrlr.nextState = UnitController.State.RECOVERY;
            return;
        }
        Tuple<Enchantment, int> tuple = enchantments[0];
        enchantments.RemoveAt(0);
        unitCtrlr.attackerCtrlr = PandoraSingleton<MissionManager>.Instance.GetUnitController(tuple.Item1.Provider);
        unitCtrlr.lastActionWounds = 0;
        unitCtrlr.flyingLabel = string.Empty;
        unitCtrlr.currentActionData.SetAction(tuple.Item1.LocalizedName, PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("action/turn_start_effect", cached: true));
        if (tuple.Item2 > 0)
        {
            unitCtrlr.ComputeDirectWound(tuple.Item2, byPassArmor: true, null);
        }
        else
        {
            PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.RETROACTION_TARGET_OUTCOME, unitCtrlr, string.Empty, v3: true, PandoraSingleton<LocalizationManager>.Instance.GetStringById("retro_outcome_resist"));
        }
        PandoraSingleton<MissionManager>.Instance.PlaySequence("dot_effect", unitCtrlr, OnSeqDone);
    }
}
