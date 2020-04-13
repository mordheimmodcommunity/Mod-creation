using System.Collections;

public class HideoutEndGameWarband : ICheapState
{
    private HideoutCamAnchor camAnchor;

    private EndGameXPModule xpModule;

    private EndGameOoaModule ooaModule;

    private bool advance;

    private bool goingToCamp;

    public HideoutEndGameWarband(HideoutManager mng, HideoutCamAnchor anchor)
    {
        camAnchor = anchor;
    }

    void ICheapState.Update()
    {
        if (goingToCamp || !advance)
        {
            return;
        }
        advance = false;
        if (xpModule != null && ooaModule == null && xpModule.EndShow())
        {
            if (PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave().endMission.wagonItems.GetItems().Count > 0 && PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave().endMission.won)
            {
                ShowItems();
            }
            else
            {
                GoToCamp();
            }
        }
        else if (ooaModule != null && ooaModule.EndShow())
        {
            GoToCamp();
        }
    }

    public void Destroy()
    {
    }

    public void Enter(int iFrom)
    {
        advance = false;
        goingToCamp = false;
        PandoraSingleton<HideoutManager>.Instance.CamManager.dummyCam.transform.position = camAnchor.transform.position;
        PandoraSingleton<HideoutManager>.Instance.CamManager.dummyCam.transform.rotation = camAnchor.transform.rotation;
        PandoraSingleton<HideoutManager>.Instance.CamManager.SetDOFTarget(camAnchor.dofTarget, 0f);
        PandoraSingleton<HideoutTabManager>.Instance.ActivateLeftTabModules(true, ModuleId.WARBAND_SHEET, ModuleId.WARBAND_OVERVIEW_UNITS);
        PandoraSingleton<HideoutTabManager>.Instance.ActivateRightTabModules(false, ModuleId.TREASURY);
        PandoraSingleton<HideoutTabManager>.Instance.ActivateCenterTabModules(ModuleId.TITLE, ModuleId.ACTION_BUTTON);
        PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<TitleModule>(ModuleId.TITLE).Set("menu_mission_report");
        WarbandSheetModule moduleLeft = PandoraSingleton<HideoutTabManager>.Instance.GetModuleLeft<WarbandSheetModule>(ModuleId.WARBAND_SHEET);
        moduleLeft.Set(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband);
        WarbandOverviewUnitsModule moduleLeft2 = PandoraSingleton<HideoutTabManager>.Instance.GetModuleLeft<WarbandOverviewUnitsModule>(ModuleId.WARBAND_OVERVIEW_UNITS);
        moduleLeft2.Set(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband);
        ActionButtonModule moduleCenter = PandoraSingleton<HideoutTabManager>.Instance.GetModuleCenter<ActionButtonModule>(ModuleId.ACTION_BUTTON);
        moduleCenter.Refresh(string.Empty, string.Empty, "menu_continue", Advance);
        moduleCenter.SetFocus();
        PandoraSingleton<HideoutTabManager>.Instance.ActivateLeftTabModule(true, ModuleId.SLIDE_XP);
        xpModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleLeft<EndGameXPModule>(ModuleId.SLIDE_XP);
        xpModule.Set(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband);
        for (int i = 0; i < PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.unitCtrlrs.Count; i++)
        {
            PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.unitCtrlrs[i].Hide(hide: false);
        }
        PandoraSingleton<HideoutTabManager>.Instance.button1.gameObject.SetActive(value: false);
        PandoraSingleton<HideoutTabManager>.Instance.button2.gameObject.SetActive(value: false);
        PandoraSingleton<HideoutTabManager>.Instance.button3.gameObject.SetActive(value: false);
        PandoraSingleton<HideoutTabManager>.Instance.button4.gameObject.SetActive(value: false);
    }

    public void Exit(int iTo)
    {
        for (int i = 0; i < PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.unitCtrlrs.Count; i++)
        {
            UnitMenuController unitMenuController = PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.unitCtrlrs[i];
            unitMenuController.Hide(hide: false);
        }
        PandoraSingleton<HideoutManager>.Instance.warbandNodeGroup.Deactivate();
        PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave().inMission = false;
        PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave().endMission = null;
        PandoraSingleton<HideoutManager>.Instance.SaveChanges();
    }

    public void FixedUpdate()
    {
    }

    private void Advance()
    {
        advance = true;
    }

    private void ShowItems()
    {
        PandoraSingleton<HideoutTabManager>.Instance.ActivateLeftTabModule(false, ModuleId.SLIDE_XP);
        PandoraSingleton<HideoutTabManager>.Instance.ActivateLeftTabModule(true, ModuleId.SLIDE_OOA);
        ooaModule = PandoraSingleton<HideoutTabManager>.Instance.GetModuleLeft<EndGameOoaModule>(ModuleId.SLIDE_OOA);
        ooaModule.Set(PandoraSingleton<HideoutManager>.Instance.WarbandCtrlr.Warband.GetWarbandSave().endMission.wagonItems);
    }

    private void GoToCamp()
    {
        goingToCamp = true;
        PandoraSingleton<PandoraInput>.Instance.SetActive(active: false);
        PandoraSingleton<GameManager>.Instance.StartCoroutine(GoingToCamp());
    }

    private IEnumerator GoingToCamp()
    {
        yield return null;
        yield return null;
        PandoraSingleton<HideoutManager>.Instance.StateMachine.ChangeState(0);
        PandoraSingleton<PandoraInput>.Instance.SetActive(active: true);
    }
}
