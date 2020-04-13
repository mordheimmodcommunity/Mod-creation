using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Prometheus
{
    public class OlympusFire : MonoBehaviour
    {
        public float delay;

        [FormerlySerializedAs("defaultBone")]
        public BoneFx startBone;

        public bool attached;

        public List<RigFx> rigSpecialCases;

        public BoneFx endBone;

        public float particleExtraTime;

        public string soundStartName;

        public bool soundLoop;

        public bool useUnitAudioSource;

        public string allyFxName;

        public string enemyFxName;

        public string mediumFxName;

        public string largeFxName;

        public bool weaponBased;

        public Light light;

        public float lightTime;

        public float lightExtraTime;

        public float speed;

        [HideInInspector]
        public bool destroyMe;

        public bool bezierMove;

        private float duration;

        private float timer;

        private float particleExtraTimer;

        private float lightTimer;

        private float lightFadeTimer;

        private float lightIntensity;

        private AudioSource audioSource;

        private BoneFx currentBoneFx;

        private MoveController moveCtrlr;

        private UnityAction endMoveAction;

        private Vector3 fxStartPos;

        private float SqrDistToTarget;

        private UnitMenuController unitCtrlr;

        private bool preventSound;

        public void Spawn(UnitMenuController startCtrlr, UnitMenuController endCtrlr, Vector3 startPos, Vector3 endPos, UnityAction endAction)
        {
            unitCtrlr = startCtrlr;
            Init((!(startCtrlr != null) || !(startCtrlr.audioSource != null)) ? null : startCtrlr.audioSource);
            startBone.active = true;
            if (startBone.active && startCtrlr != null)
            {
                currentBoneFx = GetBone(startCtrlr.unit.BaseData.UnitRigId);
                Transform transform = (currentBoneFx.bone == BoneId.NONE) ? startCtrlr.transform : startCtrlr.BonesTr[currentBoneFx.bone];
                if ((currentBoneFx.bone == BoneId.RIG_WEAPONR && unitCtrlr.Equipments[(int)unitCtrlr.unit.ActiveWeaponSlot] != null) || (currentBoneFx.bone == BoneId.RIG_WEAPONL && unitCtrlr.Equipments[(int)(unitCtrlr.unit.ActiveWeaponSlot + 1)] != null))
                {
                    transform = ((currentBoneFx.bone != BoneId.RIG_WEAPONR) ? unitCtrlr.Equipments[(int)(unitCtrlr.unit.ActiveWeaponSlot + 1)].transform : unitCtrlr.Equipments[(int)unitCtrlr.unit.ActiveWeaponSlot].transform);
                }
                if (attached)
                {
                    base.transform.SetParent(transform);
                    base.transform.localPosition = currentBoneFx.offset;
                    if (currentBoneFx.rotationWorldSpace)
                    {
                        base.transform.rotation = Quaternion.Euler(currentBoneFx.rotation);
                    }
                    else
                    {
                        base.transform.localRotation = Quaternion.Euler(currentBoneFx.rotation);
                    }
                }
                else
                {
                    base.transform.SetParent(null);
                    base.transform.position = transform.position + startCtrlr.transform.rotation * currentBoneFx.offset;
                    base.transform.rotation = startCtrlr.transform.rotation * Quaternion.Euler(currentBoneFx.rotation);
                }
            }
            else
            {
                base.transform.SetParent(null);
                base.transform.position = startPos;
                base.transform.rotation = Quaternion.identity;
            }
            if (endBone.active || endPos != Vector3.zero)
            {
                endMoveAction = endAction;
                fxStartPos = base.transform.position;
                Vector3 vector = (!endBone.active || !(endCtrlr != null)) ? endPos : (endCtrlr.BonesTr[endBone.bone].position + startCtrlr.transform.rotation * endBone.offset);
                SqrDistToTarget = Vector3.SqrMagnitude(fxStartPos - vector);
                if (bezierMove)
                {
                    moveCtrlr = base.gameObject.AddComponent<BezierMoveController>();
                    ((BezierMoveController)moveCtrlr).StartMoving(startPos, endPos, 3f, speed, endAction);
                }
                else
                {
                    moveCtrlr = base.gameObject.AddComponent<MoveController>();
                    moveCtrlr.StartMoving(vector - fxStartPos, speed);
                }
            }
            if (unitCtrlr is UnitController && ((UnitController)unitCtrlr).Imprint.State != 0)
            {
                Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>(includeInactive: true);
                for (int i = 0; i < componentsInChildren.Length; i++)
                {
                    componentsInChildren[i].enabled = false;
                }
            }
        }

        public void Spawn(Transform anchor, bool attached = true)
        {
            Init();
            if (attached)
            {
                base.transform.SetParent(anchor);
                base.transform.localPosition = Vector3.zero;
                base.transform.localRotation = Quaternion.identity;
            }
            else
            {
                base.transform.position = anchor.position;
                base.transform.rotation = anchor.rotation;
            }
        }

        public void Spawn(Vector3 pos)
        {
            Init();
            base.transform.position = pos;
        }

        private void Init(AudioSource source = null)
        {
            ParticleSystem componentInChildren = GetComponentInChildren<ParticleSystem>();
            duration = ((!(componentInChildren != null) || !(componentInChildren.duration > 0f) || componentInChildren.loop) ? 0f : componentInChildren.duration);
            timer = duration;
            particleExtraTimer = 0f;
            lightTimer = lightTime;
            lightFadeTimer = 0f;
            preventSound = (unitCtrlr != null && unitCtrlr is UnitController && ((UnitController)unitCtrlr).Imprint.State != MapImprintStateId.VISIBLE);
            if (delay == 0f)
            {
                PlaySound(soundStartName, source);
            }
        }

        public void Reactivate()
        {
            PlaySound(soundStartName, (!(unitCtrlr != null) || !(unitCtrlr.audioSource != null)) ? null : unitCtrlr.audioSource);
        }

        public void Stop()
        {
            if (audioSource != null)
            {
                audioSource.Stop();
            }
            if (particleExtraTime > 0f)
            {
                particleExtraTimer = particleExtraTime;
            }
            if (lightTime == 0f && lightExtraTime > 0f)
            {
                SetLightFadeTimer();
            }
            DestroyFx();
        }

        public void DestroyFx(bool force = false)
        {
            if ((duration != 0f || force) && particleExtraTimer <= 0f && lightTimer <= 0f && lightFadeTimer <= 0f)
            {
                PandoraDebug.LogInfo("Destroying Fx : " + base.name, "PROMETHEUS");
                Object.Destroy(base.gameObject);
            }
        }

        private void Update()
        {
            if (destroyMe)
            {
                DestroyFx(force: true);
                return;
            }
            if (currentBoneFx != null && currentBoneFx.lockRotation)
            {
                if (currentBoneFx.rotationWorldSpace)
                {
                    base.transform.rotation = Quaternion.Euler(currentBoneFx.rotation);
                }
                else
                {
                    base.transform.localRotation = Quaternion.Euler(currentBoneFx.rotation);
                }
            }
            if (timer > 0f)
            {
                timer -= Time.deltaTime;
                if (timer < 0f)
                {
                    Stop();
                }
            }
            if (particleExtraTimer > 0f)
            {
                particleExtraTimer -= Time.deltaTime;
                if (particleExtraTimer < 0f)
                {
                    DestroyFx();
                }
            }
            if (lightTimer > 0f)
            {
                lightTimer -= Time.deltaTime;
                if (lightTimer < 0f)
                {
                    SetLightFadeTimer();
                }
            }
            if (lightFadeTimer > 0f)
            {
                if (light != null)
                {
                    light.intensity = lightIntensity * (lightFadeTimer / lightExtraTime);
                }
                lightFadeTimer -= Time.deltaTime;
                if (lightFadeTimer < 0f)
                {
                    DestroyFx();
                }
            }
        }

        private void FixedUpdate()
        {
            if (moveCtrlr != null && !bezierMove && Vector3.SqrMagnitude(moveCtrlr.transform.position - fxStartPos) > SqrDistToTarget)
            {
                Stop();
                if (endMoveAction != null)
                {
                    endMoveAction();
                }
                Object.Destroy(moveCtrlr);
                moveCtrlr = null;
            }
        }

        private void PlaySound(string soundName, AudioSource source)
        {
            if (!preventSound && !string.IsNullOrEmpty(soundName))
            {
                if (useUnitAudioSource && !soundLoop && source != null)
                {
                    audioSource = source;
                }
                if (audioSource == null)
                {
                    PandoraSingleton<AssetBundleLoader>.Instance.LoadResourceAsync<GameObject>("prefabs/sound_base", delegate (Object soundBasePrefab)
                    {
                        if (!(this == null))
                        {
                            GameObject gameObject = Object.Instantiate((GameObject)soundBasePrefab);
                            gameObject.transform.SetParent(base.transform);
                            gameObject.transform.localPosition = Vector3.zero;
                            gameObject.transform.localRotation = Quaternion.identity;
                            audioSource = gameObject.GetComponent<AudioSource>();
                            PlaySound(soundName);
                        }
                    });
                }
                else
                {
                    PlaySound(soundName);
                }
            }
        }

        private void PlaySound(string soundName)
        {
            PandoraSingleton<Pan>.Instance.GetSound(soundName, cache: true, PlaySound);
        }

        private void PlaySound(AudioClip sound)
        {
            if (!soundLoop)
            {
                audioSource.PlayOneShot(sound);
                return;
            }
            audioSource.clip = sound;
            audioSource.loop = true;
            audioSource.Play();
        }

        private void SetLightFadeTimer()
        {
            lightFadeTimer = ((!(light != null)) ? 0f : lightExtraTime);
            lightIntensity = ((!(light != null)) ? 0f : light.intensity);
        }

        public BoneFx GetBone(UnitRigId id)
        {
            for (int i = 0; i < rigSpecialCases.Count; i++)
            {
                if (rigSpecialCases[i].rig == id)
                {
                    return rigSpecialCases[i].bone;
                }
            }
            return startBone;
        }

        public GameObject AttachToUnit(GameObject root)
        {
            UnitMenuController component = root.GetComponent<UnitMenuController>();
            base.transform.SetParent(component.BonesTr[startBone.bone]);
            base.transform.localPosition = Vector3.zero;
            base.transform.localRotation = Quaternion.identity;
            return base.gameObject;
        }
    }
}
