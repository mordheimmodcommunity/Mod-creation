using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIOverviewController : CanvasGroupDisabler
{
    public ButtonGroup cycleLeft;

    public ButtonGroup cycleRight;

    public List<Image> zoomLevels;

    public Text beaconCount;

    public GameObject beacons;

    public GameObject zooms;

    private int oldBeaconCount = 99;

    public void Refresh(int idx)
    {
        cycleLeft.SetAction("cycling", "controls_action_prev_unit", 0, negative: true);
        cycleRight.SetAction("cycling", "controls_action_next_unit");
        for (int i = 0; i < zoomLevels.Count; i++)
        {
            ((Component)(object)zoomLevels[i]).gameObject.SetActive(i == idx);
        }
        int availableMapBeacons = PandoraSingleton<MissionManager>.Instance.GetAvailableMapBeacons();
        if (oldBeaconCount != availableMapBeacons)
        {
            oldBeaconCount = availableMapBeacons;
            beaconCount.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("overview_beacon_count", oldBeaconCount.ToConstantString(), 5.ToConstantString()));
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        Activate(activate: true);
        if (PandoraSingleton<MissionManager>.Instance.StateMachine.GetActiveStateId() == 1)
        {
            cycleLeft.gameObject.SetActive(value: false);
            cycleRight.gameObject.SetActive(value: false);
        }
        else
        {
            cycleLeft.gameObject.SetActive(value: true);
            cycleRight.gameObject.SetActive(value: true);
        }
    }

    public override void OnDisable()
    {
        base.OnDisable();
        Activate(activate: false);
    }

    private void Activate(bool activate)
    {
        beacons.SetActive(activate);
        zooms.SetActive(activate);
    }
}
