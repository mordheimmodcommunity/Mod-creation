using System.Collections.Generic;
using UnityEngine;

public class ActionZone : InteractivePoint
{
    public bool supportLargeUnit;

    public List<ActionDestination> destinations = new List<ActionDestination>();

    public OccupationChecker occupation;

    public OccupationChecker largeOccupation;

    private UnitController unitCtrlr;

    private bool setuped;

    public PointsChecker PointsChecker
    {
        get;
        private set;
    }

    private void Awake()
    {
        base.enabled = false;
        setuped = false;
        PointsChecker = new PointsChecker(base.transform, hasOffset: true);
    }

    public override void Init(uint id)
    {
        base.enabled = true;
        imprintIcon = null;
        base.Init(id);
    }

    public void SetupFX()
    {
        if (!setuped)
        {
            for (int i = 0; i < destinations.Count; i++)
            {
                destinations[i].particles = destinations[i].fx.GetComponentsInChildren<ParticleSystem>();
            }
            setuped = true;
        }
    }

    protected override void SetActionIds()
    {
        unitActionIds = destinations.ConvertAll((ActionDestination x) => x.actionId);
    }

    private void OnDestroy()
    {
        if (largeOccupation != null)
        {
            Object.Destroy(largeOccupation.gameObject);
        }
    }

    protected override List<UnitActionId> GetActions(UnitController unitCtrlr)
    {
        bool flag = unitCtrlr.unit.Data.UnitSizeId == UnitSizeId.LARGE;
        unitActionIds.Clear();
        for (int i = 0; i < destinations.Count; i++)
        {
            ActionDestination actionDestination = destinations[i];
            if ((!flag || actionDestination.destination.supportLargeUnit) && PointsChecker.CanDoAthletic() && actionDestination.destination.PointsChecker.IsAvailable())
            {
                UnitActionId item = actionDestination.actionId;
                switch (actionDestination.actionId)
                {
                    case UnitActionId.CLIMB_3M:
                    case UnitActionId.CLIMB_6M:
                    case UnitActionId.CLIMB_9M:
                        item = UnitActionId.CLIMB;
                        break;
                    case UnitActionId.JUMP_3M:
                    case UnitActionId.JUMP_6M:
                    case UnitActionId.JUMP_9M:
                        item = UnitActionId.JUMP;
                        break;
                }
                unitActionIds.Add(item);
            }
        }
        int num = unitActionIds.IndexOf(UnitActionId.LEAP, UnitActionIdComparer.Instance);
        if (num != -1 && unitActionIds.IndexOf(UnitActionId.JUMP, UnitActionIdComparer.Instance) == -1)
        {
            unitActionIds.RemoveAt(num);
        }
        return unitActionIds;
    }

    public ActionDestination GetClimb()
    {
        for (int i = 0; i < destinations.Count; i++)
        {
            if (destinations[i].actionId == UnitActionId.CLIMB_3M || destinations[i].actionId == UnitActionId.CLIMB_6M || destinations[i].actionId == UnitActionId.CLIMB_9M)
            {
                return destinations[i];
            }
        }
        return null;
    }

    public ActionDestination GetJump()
    {
        for (int i = 0; i < destinations.Count; i++)
        {
            if (destinations[i].actionId == UnitActionId.JUMP_3M || destinations[i].actionId == UnitActionId.JUMP_6M || destinations[i].actionId == UnitActionId.JUMP_9M)
            {
                return destinations[i];
            }
        }
        return null;
    }

    public ActionDestination GetLeap()
    {
        for (int i = 0; i < destinations.Count; i++)
        {
            if (destinations[i].actionId == UnitActionId.LEAP)
            {
                return destinations[i];
            }
        }
        return null;
    }

    public void EnableFx(bool active)
    {
        for (int i = 0; i < destinations.Count; i++)
        {
            if (destinations[i].fx.activeSelf != active)
            {
                destinations[i].fx.SetActive(active);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (PointsChecker != null)
        {
            for (int i = 0; i < PointsChecker.validPoints.Count; i++)
            {
                Gizmos.DrawSphere(PointsChecker.validPoints[i], 0.1f);
            }
        }
    }
}
