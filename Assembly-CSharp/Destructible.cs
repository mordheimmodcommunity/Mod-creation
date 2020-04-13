using Pathfinding;
using Prometheus;
using System.Collections.Generic;
using UnityEngine;

public class Destructible : TriggerPoint
{
    public DestructibleId id;

    public bool autoInit;

    public Sprite imprintIcon;

    public ZoneAoe zoneAoe;

    private Collider triggerCol;

    private Dissolver dissolver;

    private int lastReceivedWounds;

    public bool destroyOnAoeDestroy = true;

    public List<Projectile> projectiles = new List<Projectile>();

    public string fxOnDestroy;

    public DestructibleData Data
    {
        get;
        private set;
    }

    public UnitController Owner
    {
        get;
        private set;
    }

    public bool Active => CurrentWounds > 0;

    public int CurrentWounds
    {
        get;
        set;
    }

    public MapImprint Imprint
    {
        get;
        private set;
    }

    public int TeamIdx
    {
        get;
        private set;
    }

    public Collider Collider
    {
        get;
        private set;
    }

    public string LocalizedName
    {
        get;
        set;
    }

    public static void Spawn(DestructibleId destructibleId, UnitController owner, Vector3 pos, int wounds = -1)
    {
        string str = destructibleId.ToLowerString();
        PandoraSingleton<AssetBundleLoader>.Instance.LoadAssetAsync<GameObject>("Assets/prefabs/environments/props/idols/", AssetBundleId.PROPS, str + ".prefab", delegate (Object obj)
        {
            GameObject gameObject = Object.Instantiate((GameObject)obj);
            gameObject.transform.position = pos;
            gameObject.transform.rotation = ((!(owner != null)) ? Quaternion.identity : owner.transform.rotation);
            Destructible component = gameObject.GetComponent<Destructible>();
            component.Init(destructibleId, owner);
            component.name += ((!(owner != null)) ? string.Empty : owner.name);
            if (wounds != -1)
            {
                component.CurrentWounds = wounds;
            }
        });
    }

    public void AutoInit()
    {
        if (id != 0 && autoInit)
        {
            Init(id, null);
        }
    }

    public void Init(DestructibleId destructibleId, UnitController owner)
    {
        Init();
        id = destructibleId;
        Owner = owner;
        guid = ((!(owner != null)) ? PandoraSingleton<MissionManager>.Instance.GetNextEnvGUID() : PandoraSingleton<MissionManager>.Instance.GetNextRTGUID());
        TeamIdx = ((!(owner != null)) ? (-1) : owner.GetWarband().teamIdx);
        LocalizedName = PandoraSingleton<LocalizationManager>.Instance.GetStringById(id.ToLowerString());
        Data = PandoraSingleton<DataFactory>.Instance.InitData<DestructibleData>((int)id);
        CurrentWounds = Data.Wounds;
        if (Data.ZoneAoeId != 0)
        {
            ZoneAoe.Spawn(Data.ZoneAoeId, (float)Data.ZoneAoeRadius, base.transform.position, owner, register: false, delegate (ZoneAoe zone)
            {
                zoneAoe = zone;
                zoneAoe.destructible = this;
            });
        }
        dissolver = base.gameObject.AddComponent<Dissolver>();
        if (imprintIcon != null)
        {
            Imprint = base.gameObject.AddComponent<MapImprint>();
            Imprint.Init(imprintIcon, null, alwaysVisible: false, MapImprintType.DESTRUCTIBLE, null, null, null, null, this);
            if (owner != null && owner.IsPlayed())
            {
                Imprint.AddViewer(owner);
            }
        }
        NavmeshCut componentInChildren = GetComponentInChildren<NavmeshCut>();
        if (componentInChildren != null)
        {
            componentInChildren.ForceUpdate();
        }
        Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
        for (int i = 0; i < componentsInChildren.Length; i++)
        {
            if (!componentsInChildren[i].isTrigger)
            {
                Collider = componentsInChildren[i];
            }
            else
            {
                triggerCol = componentsInChildren[i];
            }
        }
        PandoraSingleton<MissionManager>.Instance.triggerPoints.Add(this);
        PandoraSingleton<MissionManager>.Instance.MissionEndData.AddDestructible(this);
    }

    public void ApplyDamage(int damage)
    {
        CurrentWounds -= damage;
        lastReceivedWounds = -damage;
        PandoraSingleton<MissionManager>.Instance.MissionEndData.UpdateDestructible(this);
    }

    public void Hit(UnitController damageDealer)
    {
        PandoraSingleton<FlyingTextManager>.Instance.GetFlyingText(FlyingTextId.ACTION, delegate (FlyingText fl)
        {
            ((FlyingLabel)fl).Play(base.transform.position + Vector3.up * 1.5f, true, "com_value", lastReceivedWounds.ToConstantString());
        });
        if (CurrentWounds <= 0)
        {
            Deactivate();
            if (damageDealer != null)
            {
                damageDealer.GetWarband().DestroyDestructible(base.name);
            }
        }
        PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.RETROACTION_SHOW_OUTCOME, this);
    }

    public void Deactivate()
    {
        CurrentWounds = 0;
        for (int i = 0; i < projectiles.Count; i++)
        {
            if (projectiles[i] != null && projectiles[i].gameObject != null)
            {
                Object.Destroy(projectiles[i].gameObject);
            }
        }
        projectiles.Clear();
        if (zoneAoe != null)
        {
            zoneAoe.Deactivate();
        }
        PandoraSingleton<MissionManager>.Instance.UnregisterDestructible(this);
        if (!string.IsNullOrEmpty(fxOnDestroy))
        {
            PandoraSingleton<Prometheus.Prometheus>.Instance.SpawnFx(fxOnDestroy, base.transform, attached: false, null);
        }
        dissolver.Hide(hide: true, force: false, OnDissolved);
        PandoraSingleton<MissionManager>.Instance.MissionEndData.UpdateDestructible(this);
    }

    private void OnDissolved()
    {
        Object.Destroy(base.gameObject);
        PandoraSingleton<MissionManager>.Instance.RefreshGraph();
    }

    public bool IsInRange(Vector3 src, float maxDistance)
    {
        Vector3 a = base.transform.position + Vector3.up;
        Vector3 direction = a - src;
        direction.Normalize();
        RaycastHit hitInfo;
        bool flag = Physics.Raycast(src, direction, out hitInfo, maxDistance, LayerMaskManager.groundMask);
        return hitInfo.collider == Collider;
    }
}
