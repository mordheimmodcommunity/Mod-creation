using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MapImprint : MonoBehaviour
{
    private const int SIMULTANEOUS_CHECK = 15;

    public static int currentCount;

    public static int currentFlagChecked;

    public static int maxFlag;

    public bool alwaysVisible;

    public bool alwaysHide;

    public MapImprintType imprintType;

    public bool alive;

    public Vector3 lastKnownPos;

    public Sprite visibleTexture;

    public Sprite lostTexture;

    public Sprite idolTexture;

    public bool needsRefresh;

    private UnityAction<bool, bool, UnityAction> hideDel;

    public MapImprintStateId State
    {
        get;
        private set;
    }

    public List<UnitController> Viewers
    {
        get;
        private set;
    }

    public UnitController UnitCtrlr
    {
        get;
        private set;
    }

    public SearchPoint Search
    {
        get;
        private set;
    }

    public Trap Trap
    {
        get;
        private set;
    }

    public Destructible Destructible
    {
        get;
        private set;
    }

    public WarbandWagon Wagon
    {
        get;
        set;
    }

    public MapBeacon Beacon
    {
        get;
        set;
    }

    public int Flag
    {
        get;
        private set;
    }

    public void Init(Sprite visibleIcon, Sprite lostIcon, bool alwaysVisible, MapImprintType imprintType, UnityAction<bool, bool, UnityAction> hideDelegate = null, UnitController unit = null, SearchPoint searchPoint = null, Trap trap = null, Destructible destructible = null)
    {
        Viewers = new List<UnitController>();
        this.imprintType = imprintType;
        visibleTexture = visibleIcon;
        lostTexture = lostIcon;
        this.alwaysVisible = alwaysVisible;
        alive = true;
        State = ((!alwaysVisible) ? MapImprintStateId.INVISIBLE : MapImprintStateId.VISIBLE);
        hideDel = hideDelegate;
        UnitCtrlr = unit;
        Search = searchPoint;
        Trap = trap;
        Destructible = destructible;
        SetCheckFlag();
        PandoraSingleton<MissionManager>.Instance.MapImprints.Add(this);
    }

    public void Init(string visibleIcon, string lostIcon, bool alwaysVisible, MapImprintType imprintType, UnityAction<bool, bool, UnityAction> hideDelegate = null, UnitController unit = null, SearchPoint searchPoint = null, Trap trap = null, Destructible destructible = null)
    {
        Viewers = new List<UnitController>();
        this.imprintType = imprintType;
        if (visibleIcon != null)
        {
            PandoraSingleton<AssetBundleLoader>.Instance.LoadResourceAsync<Sprite>(visibleIcon, delegate (Object o)
            {
                visibleTexture = (Sprite)o;
            });
        }
        if (lostIcon != null)
        {
            PandoraSingleton<AssetBundleLoader>.Instance.LoadResourceAsync<Sprite>(lostIcon, delegate (Object o)
            {
                lostTexture = (Sprite)o;
            });
        }
        this.alwaysVisible = alwaysVisible;
        alive = true;
        State = ((!alwaysVisible) ? MapImprintStateId.INVISIBLE : MapImprintStateId.VISIBLE);
        hideDel = hideDelegate;
        UnitCtrlr = unit;
        Search = searchPoint;
        Trap = trap;
        Destructible = destructible;
        SetCheckFlag();
        PandoraSingleton<MissionManager>.Instance.MapImprints.Add(this);
    }

    private void SetCheckFlag()
    {
        Flag = (int)((float)currentCount / 15f);
        maxFlag = Mathf.Max(Flag, maxFlag);
        currentCount++;
    }

    private void SetState(bool visible, bool alive)
    {
        visible |= (alwaysVisible || (UnitCtrlr != null && UnitCtrlr.unit.UnitAlwaysVisible));
        visible &= !alwaysHide;
        switch (State)
        {
            case MapImprintStateId.VISIBLE:
                State = ((!visible) ? MapImprintStateId.LOST : ((!alive) ? MapImprintStateId.DESTROYED : MapImprintStateId.VISIBLE));
                break;
            case MapImprintStateId.INVISIBLE:
                State = ((!visible) ? MapImprintStateId.INVISIBLE : ((!alive) ? MapImprintStateId.DESTROYED : MapImprintStateId.VISIBLE));
                break;
            case MapImprintStateId.LOST:
                State = ((!visible) ? MapImprintStateId.LOST : ((!alive) ? MapImprintStateId.DESTROYED : MapImprintStateId.VISIBLE));
                break;
            case MapImprintStateId.DESTROYED:
                alwaysVisible = true;
                break;
        }
        if (hideDel != null)
        {
            hideDel(!visible, arg1: false, null);
        }
    }

    public void RefreshPosition()
    {
        if (needsRefresh)
        {
            needsRefresh = false;
            SetState(Viewers.Count > 0, alive);
        }
        if (State == MapImprintStateId.VISIBLE)
        {
            lastKnownPos = base.transform.position;
        }
    }

    public void RemoveViewer(UnitController ctrlr)
    {
        int num = Viewers.IndexOf(ctrlr);
        if (num != -1)
        {
            Viewers.RemoveAt(num);
            needsRefresh = true;
            if (UnitCtrlr != null)
            {
                PandoraSingleton<MissionManager>.Instance.resendLadder = true;
            }
        }
    }

    public void Hide()
    {
        Viewers.Clear();
        State = MapImprintStateId.INVISIBLE;
        needsRefresh = true;
    }

    public void AddViewer(UnitController ctrlr)
    {
        if (ctrlr == UnitCtrlr)
        {
            PandoraDebug.LogWarning("Adding unit as its own viewer");
            return;
        }
        int num = Viewers.IndexOf(ctrlr);
        if (num == -1)
        {
            Viewers.Add(ctrlr);
            needsRefresh = true;
            if (UnitCtrlr != null)
            {
                PandoraSingleton<MissionManager>.Instance.resendLadder = true;
            }
        }
    }

    public void SetCurrent(bool current)
    {
        needsRefresh = true;
    }
}
