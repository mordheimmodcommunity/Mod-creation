using HighlightingSystem;
using Prometheus;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractivePoint : MonoBehaviour
{
    private List<UnitActionId> emptyList = new List<UnitActionId>();

    public UnitActionId unitActionId;

    public string loc_action;

    public string loc_action_enemy;

    public SearchAnim anim;

    public GameObject visual;

    protected Dissolver visualDissolver;

    public ItemId requestedItemId;

    public InteractivePoint linkedPoint;

    public List<InteractivePoint> linkedPoints;

    public bool reverseLinkedCondition;

    public List<InteractiveRestriction> restrictions;

    protected List<InteractivePoint> links = new List<InteractivePoint>();

    public List<GameObject> triggers = new List<GameObject>();

    public List<GameObject> altTriggers = new List<GameObject>();

    public Sprite imprintIcon;

    public Transform cameraAnchor;

    public List<OlympusFireStarter> activationFx;

    public List<OlympusFireStarter> deactivationFx;

    public bool useSameFxForDeactivation;

    public float apparitionDelay = 0.5f;

    public ZoneAoe zoneAoe;

    public SkillId curseId;

    public CampaignUnitId campaignUnitId;

    public bool spawnVisible;

    protected List<UnitActionId> unitActionIds = new List<UnitActionId>();

    private bool destroyed;

    private bool sameTriggersAsLinked;

    private bool needVisualRemoved;

    public uint guid;

    public Highlighter Highlight
    {
        get;
        private set;
    }

    public MapImprint Imprint
    {
        get;
        private set;
    }

    public virtual void Init(uint id)
    {
        SetActionIds();
        if (linkedPoint != null)
        {
            linkedPoints.Add(linkedPoint);
        }
        if (imprintIcon != null && linkedPoints.Count == 0)
        {
            Imprint = base.gameObject.AddComponent<MapImprint>();
            Imprint.Init(imprintIcon, imprintIcon, alwaysVisible: true, MapImprintType.INTERACTIVE_POINT, Hide, null, this as SearchPoint);
        }
        Highlight = base.gameObject.GetComponent<Highlighter>();
        if ((Object)(object)Highlight == null)
        {
            Highlight = base.gameObject.AddComponent<Highlighter>();
        }
        Highlight.seeThrough = false;
        if (linkedPoints.Count > 0)
        {
            for (int i = 0; i < linkedPoints.Count; i++)
            {
                linkedPoints[i].links.Add(this);
                sameTriggersAsLinked = true;
                for (int j = 0; j < triggers.Count; j++)
                {
                    if (j < linkedPoints[i].triggers.Count && linkedPoints[i].triggers[j] != triggers[j])
                    {
                        sameTriggersAsLinked = false;
                    }
                }
            }
        }
        if (visual != null)
        {
            visualDissolver = visual.AddComponent<Dissolver>();
            visualDissolver.dissolveSpeed = apparitionDelay;
        }
        SetTriggerVisual();
        guid = id;
        PandoraSingleton<MissionManager>.Instance.RegisterInteractivePoint(this);
    }

    protected virtual void SetActionIds()
    {
        unitActionIds = new List<UnitActionId>();
        unitActionIds.Add(unitActionId);
    }

    protected virtual List<UnitActionId> GetActions(UnitController unitCtrlr)
    {
        return unitActionIds;
    }

    protected virtual bool CanInteract(UnitController unitCtrlr)
    {
        return LinksValid(unitCtrlr, reverseLinkedCondition) && HasRequiredItem(unitCtrlr) && CompliesWithRestrictions(unitCtrlr);
    }

    public bool HasRequiredItem(UnitController unitCtrlr)
    {
        return requestedItemId == ItemId.NONE || unitCtrlr.unit.HasItem(requestedItemId);
    }

    private bool CompliesWithRestrictions(UnitController unitCtrlr)
    {
        for (int i = 0; i < restrictions.Count; i++)
        {
            InteractiveRestriction interactiveRestriction = restrictions[i];
            if ((interactiveRestriction.teamIdx != -1 && unitCtrlr.GetWarband().teamIdx != interactiveRestriction.teamIdx) || (interactiveRestriction.warbandId != 0 && unitCtrlr.GetWarband().WarData.Id != interactiveRestriction.warbandId) || (interactiveRestriction.allegianceId != 0 && unitCtrlr.GetWarband().WarData.AllegianceId != interactiveRestriction.allegianceId))
            {
                return false;
            }
        }
        return true;
    }

    protected virtual bool LinkValid(UnitController unitCtrlr, bool reverseCondition)
    {
        return true;
    }

    public List<UnitActionId> GetUnitActionIds(UnitController unitCtrlr)
    {
        if (CanInteract(unitCtrlr))
        {
            return GetActions(unitCtrlr);
        }
        return emptyList;
    }

    public void DestroyVisual(bool triggersOnly = false, bool force = false)
    {
        if (!destroyed)
        {
            destroyed = true;
            for (int i = 0; i < links.Count; i++)
            {
                links[i].SetTriggerVisual();
            }
            PandoraSingleton<MissionManager>.Instance.UnregisterInteractivePoint(this);
            DestroyTriggers();
            needVisualRemoved = !triggersOnly;
            if (Imprint == null || force)
            {
                DoDestroyVisual();
                return;
            }
            Imprint.alwaysVisible = false;
            Imprint.needsRefresh = true;
        }
    }

    private void RemoveVisual()
    {
        Object.Destroy(visual);
        visual = null;
    }

    public virtual void SetTriggerVisual()
    {
        SetTriggerVisual(baseVisual: true);
    }

    private bool LinksValid(UnitController ctrlr, bool reverse)
    {
        bool flag = true;
        for (int i = 0; i < linkedPoints.Count; i++)
        {
            flag &= linkedPoints[i].LinkValid(ctrlr, reverse);
        }
        return flag;
    }

    protected void SetTriggerVisual(bool baseVisual)
    {
        if (sameTriggersAsLinked)
        {
            return;
        }
        bool flag = LinksValid(PandoraSingleton<MissionManager>.Instance.GetCurrentUnit(), reverseLinkedCondition);
        bool flag2 = altTriggers.Count > 0 && altTriggers.Count == triggers.Count;
        for (int i = 0; i < triggers.Count; i++)
        {
            triggers[i].SetActive((!flag2 || baseVisual) && flag);
            if (flag2)
            {
                altTriggers[i].SetActive(!baseVisual && flag);
            }
        }
        for (int j = 0; j < links.Count; j++)
        {
            links[j].SetTriggerVisual();
        }
    }

    private void DestroyTriggers()
    {
        for (int i = 0; i < triggers.Count; i++)
        {
            if (triggers[i] != null)
            {
                triggers[i].SetActive(value: false);
                Object.Destroy(triggers[i]);
                triggers[i] = null;
            }
        }
        triggers.Clear();
    }

    public void Hide(bool hide, bool force = false, UnityAction onDissolved = null)
    {
        if (destroyed)
        {
            if (!hide)
            {
                DoDestroyVisual();
            }
            return;
        }
        Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
        for (int i = 0; i < componentsInChildren.Length; i++)
        {
            componentsInChildren[i].enabled = !hide;
        }
    }

    private void DoDestroyVisual()
    {
        Object.Destroy(Imprint);
        if (needVisualRemoved)
        {
            if (visualDissolver != null && visualDissolver.gameObject.activeInHierarchy)
            {
                visualDissolver.Hide(hide: true, force: false, RemoveVisual);
            }
            else
            {
                Object.Destroy(base.gameObject);
            }
        }
    }

    public virtual string GetLocAction()
    {
        return loc_action;
    }

    private string GetDefaultIconSpritePath()
    {
        return "action/" + unitActionId.ToLowerString();
    }

    public Sprite GetIconAction()
    {
        if (imprintIcon != null)
        {
            return imprintIcon;
        }
        string defaultIconSpritePath = GetDefaultIconSpritePath();
        if (!string.IsNullOrEmpty(defaultIconSpritePath))
        {
            return PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>(defaultIconSpritePath, cached: true);
        }
        return null;
    }

    protected void SpawnFxs(bool activated)
    {
        List<OlympusFireStarter> list = deactivationFx;
        if (activated || useSameFxForDeactivation)
        {
            list = activationFx;
        }
        for (int i = 0; i < list.Count; i++)
        {
            PandoraSingleton<Prometheus.Prometheus>.Instance.SpawnFx(list[i], null);
        }
    }

    public virtual void ActivateZoneAoe()
    {
        if (zoneAoe != null)
        {
            if (zoneAoe.Initialized)
            {
                zoneAoe.Deactivate();
            }
            else
            {
                zoneAoe.Activate();
            }
        }
    }

    public virtual UnitController SpawnCampaignUnit()
    {
        if (campaignUnitId != 0)
        {
            return PandoraSingleton<MissionManager>.Instance.ActivateHiddenUnit(campaignUnitId, spawnVisible);
        }
        return null;
    }
}
