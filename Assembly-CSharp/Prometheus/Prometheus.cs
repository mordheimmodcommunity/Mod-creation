using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;

namespace Prometheus
{
    public class Prometheus : PandoraSingleton<Prometheus>
    {
        private Dictionary<string, GameObject> cachedFx = new Dictionary<string, GameObject>();

        private List<OlympusFire> delayedFires = new List<OlympusFire>();

        public void SpawnFx(OlympusFireStarter starter, UnityAction<GameObject> createdCb)
        {
            SpawnFx(starter.fxName, starter.transform, attached: true, createdCb);
        }

        public void SpawnFx(OlympusFireStarter starter, Transform root, UnityAction<GameObject> createdCb)
        {
            SpawnFx(starter.fxName, root, attached: true, createdCb);
        }

        public void SpawnFx(string name, Transform anchor, bool attached, UnityAction<GameObject> createdCb)
        {
            CreateFx(name, delegate (OlympusFire fire)
            {
                if (fire == null)
                {
                    if (createdCb != null)
                    {
                        createdCb(null);
                    }
                }
                else
                {
                    StartFx(fire, anchor, attached);
                    if (createdCb != null)
                    {
                        createdCb(fire.gameObject);
                    }
                }
            });
        }

        public void SpawnFx(string name, Vector3 pos)
        {
            CreateFx(name, delegate (OlympusFire fire)
            {
                if (!(fire == null))
                {
                    StartFx(fire, pos);
                }
            });
        }

        public void SpawnFx(string name, Unit unit, UnityAction<GameObject> createdCb)
        {
            if (PandoraSingleton<MissionManager>.Exists())
            {
                UnitController unitController = PandoraSingleton<MissionManager>.Instance.GetUnitController(unit);
                if (unitController != null)
                {
                    SpawnFx(name, unitController, null, createdCb);
                    return;
                }
            }
            if (PandoraSingleton<HideoutManager>.Exists())
            {
                UnitMenuController unitMenuController = PandoraSingleton<HideoutManager>.Instance.GetUnitMenuController(unit);
                if (unitMenuController != null)
                {
                    SpawnFx(name, unitMenuController, null, createdCb);
                    return;
                }
            }
            createdCb?.Invoke(null);
        }

        public void SpawnFx(string name, UnitMenuController unitCtrlr, UnitMenuController endUnitCtrlr, UnityAction<GameObject> createdCb, [Optional] Vector3 startPos, [Optional] Vector3 endPos, UnityAction endAction = null)
        {
            CreateFx(name, delegate (OlympusFire fire)
            {
                if (fire == null)
                {
                    if (createdCb != null)
                    {
                        createdCb(null);
                    }
                }
                else
                {
                    fire.destroyMe = false;
                    if (PandoraSingleton<MissionManager>.Exists() && (!string.IsNullOrEmpty(fire.allyFxName) || !string.IsNullOrEmpty(fire.enemyFxName)))
                    {
                        bool flag = PandoraSingleton<MissionManager>.Instance.GetAliveAllies(PandoraSingleton<MissionManager>.Instance.GetMyWarbandCtrlr().idx).IndexOf((UnitController)unitCtrlr) != -1;
                        fire.destroyMe = true;
                        if (flag && !string.IsNullOrEmpty(fire.allyFxName))
                        {
                            SpawnFx(fire.allyFxName, unitCtrlr, endUnitCtrlr, createdCb, startPos, endPos, endAction);
                        }
                        else if (!flag && !string.IsNullOrEmpty(fire.enemyFxName))
                        {
                            SpawnFx(fire.enemyFxName, unitCtrlr, endUnitCtrlr, createdCb, startPos, endPos, endAction);
                        }
                        else if (createdCb != null)
                        {
                            createdCb(null);
                        }
                    }
                    else if (!string.IsNullOrEmpty(fire.mediumFxName) && !string.IsNullOrEmpty(fire.largeFxName))
                    {
                        fire.destroyMe = true;
                        if (unitCtrlr.unit.Data.UnitSizeId == UnitSizeId.LARGE)
                        {
                            SpawnFx(fire.largeFxName, unitCtrlr, endUnitCtrlr, createdCb, startPos, endPos, endAction);
                        }
                        else
                        {
                            SpawnFx(fire.mediumFxName, unitCtrlr, endUnitCtrlr, createdCb, startPos, endPos, endAction);
                        }
                    }
                    else if (fire.weaponBased)
                    {
                        fire.destroyMe = true;
                        string text = name + "_";
                        Mutation mutation = unitCtrlr.unit.GetMutation(unitCtrlr.unit.ActiveWeaponSlot);
                        if (mutation == null)
                        {
                            text += unitCtrlr.unit.Items[(int)unitCtrlr.unit.ActiveWeaponSlot].GetAssetData(unitCtrlr.unit.RaceId, unitCtrlr.unit.WarbandId, unitCtrlr.unit.Id).Asset;
                        }
                        else
                        {
                            string text2 = text;
                            text = text2 + unitCtrlr.unit.WarData.Asset + "_" + unitCtrlr.unit.Data.Asset + "_" + unitCtrlr.unit.bodyParts[mutation.RelatedBodyParts[0].BodyPartId].Name + "_" + mutation.Data.Id.ToLowerString() + "_01";
                        }
                        SpawnFx(text, unitCtrlr, endUnitCtrlr, createdCb, startPos, endPos, endAction);
                    }
                    else
                    {
                        StartFx(fire, unitCtrlr, endUnitCtrlr, startPos, endPos, endAction);
                        if (createdCb != null)
                        {
                            createdCb(fire.gameObject);
                        }
                    }
                }
            });
        }

        private void CreateFx(string fxName, UnityAction<OlympusFire> cb)
        {
            if (string.IsNullOrEmpty(fxName))
            {
                cb(null);
            }
            else
            {
                StartCoroutine(LoadFx(fxName, cb));
            }
        }

        private IEnumerator LoadFx(string fxName, UnityAction<OlympusFire> cb)
        {
            if (cachedFx.ContainsKey(fxName))
            {
                if (cachedFx[fxName] == null)
                {
                    while (cachedFx.ContainsKey(fxName) && cachedFx[fxName] == null)
                    {
                        yield return null;
                    }
                }
                InstantiateFx(fxName, cb);
            }
            else
            {
                PandoraSingleton<AssetBundleLoader>.Instance.LoadAssetAsync<GameObject>("Assets/prefabs/fx/", AssetBundleId.FX, fxName + ".prefab", delegate (Object go)
                {
                    if (go != null)
                    {
                        cachedFx[base.fxName] = (GameObject)go;
                        InstantiateFx(base.fxName, base.cb);
                    }
                    else
                    {
                        cachedFx.Remove(base.fxName);
                    }
                });
            }
        }

        private void InstantiateFx(string fxName, UnityAction<OlympusFire> cb)
        {
            GameObject value = null;
            cachedFx.TryGetValue(fxName, out value);
            if (value == null)
            {
                PandoraDebug.LogWarning("Loading Fx that does not exist : " + fxName, "PROMETHEUS");
                cb(null);
            }
            else
            {
                GameObject gameObject = Object.Instantiate(value);
                OlympusFire component = gameObject.GetComponent<OlympusFire>();
                cb(component);
            }
        }

        private void StartFx(OlympusFire fire, UnitMenuController unitCtrlr, UnitMenuController endUnitCtrlr = null, [Optional] Vector3 startPos, [Optional] Vector3 endPos, UnityAction endAction = null)
        {
            fire.Spawn(unitCtrlr, endUnitCtrlr, startPos, endPos, endAction);
            CheckDelay(fire);
        }

        private void StartFx(OlympusFire fire, Transform anchor, bool attached)
        {
            fire.Spawn(anchor, attached);
            CheckDelay(fire);
        }

        private void StartFx(OlympusFire fire, Vector3 pos)
        {
            fire.Spawn(pos);
            CheckDelay(fire);
        }

        private void CheckDelay(OlympusFire fire)
        {
            if (fire.delay != 0f)
            {
                fire.gameObject.SetActive(value: false);
                delayedFires.Add(fire);
            }
        }

        private void Update()
        {
            for (int num = delayedFires.Count - 1; num >= 0; num--)
            {
                if (delayedFires[num] == null)
                {
                    delayedFires.RemoveAt(num);
                }
                else
                {
                    delayedFires[num].delay -= Time.deltaTime;
                    if (delayedFires[num].delay <= 0f)
                    {
                        delayedFires[num].gameObject.SetActive(value: true);
                        delayedFires[num].Reactivate();
                        delayedFires.RemoveAt(num);
                    }
                }
            }
        }
    }
}
