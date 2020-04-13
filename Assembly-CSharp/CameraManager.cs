using HighlightingSystem;
using Smaa;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.ImageEffects;

public class CameraManager : MonoBehaviour
{
    public enum CameraType
    {
        FIXED,
        CHARACTER,
        WATCH,
        ANIMATED,
        OVERVIEW,
        CONSTRAINED,
        SEMI_CONSTRAINED,
        DEPLOY,
        MELEE_ATTACK,
        ROTATE_AROUND,
        COUNT
    }

    public const float SIZE_DIFF_LARGE = 1f;

    public const float SIZE_DIFF_NORMAL = 0f;

    public const float TRANSITION_TIME = 2f;

    private readonly float[] ZOOM_LEVELS = new float[3]
    {
        2.2f,
        4f,
        6.2f
    };

    private CheapStateMachine stateMachine;

    public bool focus;

    public float sizeDiff;

    private int zoomLevel;

    public bool transitionCam = true;

    public float curTransTime;

    public float maxTransTime = 2f;

    public GameObject dummyCam;

    private FadeOutLOS fadeOutLOS;

    public Overlay overlay;

    private BloodSplatter bloodSplatter;

    private SSAOPro ssao;

    private ScreenSpaceAmbientObscurance ssaoBasic;

    private SMAA smaa;

    private FxPro fxPro;

    public GameObject dofTarget;

    private float savedFocalLengthMultiplier = 0.15f;

    public Texture2D dissolveTex;

    public Texture2D lodFadeTex;

    public int shoulderRight;

    private Animator animator;

    private float targetFOV;

    private float fovSpeed = 1f;

    private AmplifyColorEffect lut;

    public Transform Target
    {
        get;
        private set;
    }

    public Transform LookAtTarget
    {
        get;
        private set;
    }

    public float Zoom
    {
        get;
        private set;
    }

    public bool Locked
    {
        get;
        set;
    }

    private void Awake()
    {
        Locked = false;
        fadeOutLOS = GetComponent<FadeOutLOS>();
        smaa = GetComponent<SMAA>();
        ssao = GetComponent<SSAOPro>();
        ssaoBasic = GetComponent<ScreenSpaceAmbientObscurance>();
        if ((Object)(object)ssaoBasic != null)
        {
            ((Behaviour)(object)ssaoBasic).enabled = false;
            Object.Destroy((Object)(object)ssaoBasic);
        }
        fxPro = GetComponent<FxPro>();
        dofTarget = new GameObject("dof_target");
        SceneManager.MoveGameObjectToScene(dofTarget, base.gameObject.scene);
        dofTarget.transform.SetParent(null);
        fxPro.DOFParams.Target = dofTarget.transform;
        savedFocalLengthMultiplier = fxPro.DOFParams.FocalLengthMultiplier;
        lut = GetComponent<AmplifyColorEffect>();
        zoomLevel = 1;
        sizeDiff = 0f;
        Zoom = ZOOM_LEVELS[zoomLevel];
        dummyCam = new GameObject("dummy_camera");
        SceneManager.MoveGameObjectToScene(dummyCam, base.gameObject.scene);
        dummyCam.transform.SetParent(null);
        dummyCam.transform.position = base.transform.position;
        dummyCam.transform.rotation = base.transform.rotation;
        overlay = GetComponent<Overlay>();
        ActivateOverlay(active: false, 0f);
        bloodSplatter = GetComponent<BloodSplatter>();
        bloodSplatter.Deactivate();
        HighlightingRenderer component = GetComponent<HighlightingRenderer>();
        ((HighlightingBase)component).offsetFactor = -0.25f;
        stateMachine = new CheapStateMachine(10);
        stateMachine.AddState(new CameraFixed(this), 0);
        stateMachine.AddState(new CharacterFollowCam(this), 1);
        stateMachine.AddState(new WatchCamera(this), 2);
        stateMachine.AddState(new CameraAnim(this), 3);
        stateMachine.AddState(new OverviewCamera(this), 4);
        stateMachine.AddState(new ConstrainedCamera(this), 5);
        stateMachine.AddState(new SemiConstrainedCamera(this), 6);
        stateMachine.AddState(new DeployCam(this), 7);
        stateMachine.AddState(new MeleeAttackCamera(this), 8);
        stateMachine.AddState(new RotateAroundCam(this), 9);
        SwitchToCam(CameraType.FIXED, null, transition: false, force: true);
        Shader.SetGlobalTexture("_DissolveTex", dissolveTex);
        Shader.SetGlobalTexture("_LodFadeTex", lodFadeTex);
    }

    private void OnDestroy()
    {
        stateMachine.Destroy();
    }

    public void SetZoomDiff(bool isLarge)
    {
        float num = sizeDiff;
        sizeDiff = 0f;
        if (isLarge)
        {
            sizeDiff = 1f;
        }
        if (num != sizeDiff)
        {
            Zoom = Zoom - num + sizeDiff;
        }
    }

    public void SetZoomLevel(uint level)
    {
        if (level > ZOOM_LEVELS.Length)
        {
            level = (uint)(ZOOM_LEVELS.Length - 1);
        }
        zoomLevel = (int)level;
        Zoom = ZOOM_LEVELS[zoomLevel] + sizeDiff;
    }

    public void SetTarget(Transform target)
    {
        Target = target;
        if ((bool)lut)
        {
            lut.TriggerVolumeProxy = Target;
        }
    }

    public T GetCurrentCam<T>()
    {
        return (T)stateMachine.GetActiveState();
    }

    public T GetCamOfType<T>(CameraType type)
    {
        return (T)stateMachine.GetState((int)type);
    }

    public CameraType GetCurrentCamType()
    {
        return (CameraType)stateMachine.GetActiveStateId();
    }

    private void FixedUpdate()
    {
        stateMachine.FixedUpdate();
    }

    public void LateUpdate()
    {
        if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("zoom"))
        {
            Zoom = ZOOM_LEVELS[++zoomLevel % ZOOM_LEVELS.Length] + sizeDiff;
            Transition(0.5f);
        }
        float num = (0f - PandoraSingleton<PandoraInput>.Instance.GetAxis("zoom_mouse")) / 4f;
        if (num != 0f)
        {
            Zoom = Mathf.Clamp(Zoom + num, ZOOM_LEVELS[0] + sizeDiff, ZOOM_LEVELS[ZOOM_LEVELS.Length - 1] + sizeDiff);
            Transition(0.25f);
        }
        stateMachine.Update();
        if (transitionCam)
        {
            curTransTime += Time.deltaTime;
            base.transform.position = Vector3.Lerp(base.transform.position, dummyCam.transform.position, curTransTime / maxTransTime);
            base.transform.rotation = Quaternion.Slerp(base.transform.rotation, dummyCam.transform.rotation, curTransTime / maxTransTime);
            if (curTransTime >= maxTransTime)
            {
                curTransTime = 0f;
                transitionCam = false;
            }
        }
        else
        {
            base.transform.position = dummyCam.transform.position;
            base.transform.rotation = dummyCam.transform.rotation;
            if (LookAtTarget != null)
            {
                dofTarget.transform.position = LookAtTarget.position;
            }
            else if (Target != null)
            {
                dofTarget.transform.position = Target.position;
            }
        }
    }

    public void SwitchToCam(CameraType camType, Transform camTarget, bool transition = true, bool force = false, bool clearFocus = true, bool isLarge = false)
    {
        if (!Locked || force)
        {
            if (camTarget != null && camTarget != Target)
            {
                SetDOFTarget(camTarget, 0f);
                SetTarget(camTarget);
            }
            if (clearFocus)
            {
                focus = false;
                LookAtTarget = null;
            }
            if (transition)
            {
                Transition();
            }
            else if (!transition)
            {
                CancelTransition();
            }
            stateMachine.ChangeState((int)camType);
            if (fadeOutLOS != null)
            {
                ResetLOSTarget(Target);
                AddLOSTarget(LookAtTarget);
            }
            SetZoomDiff(isLarge);
        }
    }

    public void SetShoulderCam(bool isLarge, bool clear = false)
    {
        if (clear)
        {
            shoulderRight = 0;
        }
        float num = 1.75f;
        float num2 = 0.5f;
        if (isLarge)
        {
            num += sizeDiff;
            num2 = 1.25f;
        }
        dummyCam.transform.position = Target.position;
        dummyCam.transform.rotation = Target.rotation;
        dummyCam.transform.Translate(new Vector3(num2, num, -2f));
        bool flag = false;
        if ((!LookAtTarget) ? Physics.Linecast(dummyCam.transform.position, dummyCam.transform.position + dummyCam.transform.forward * 2.5f, out RaycastHit hitInfo, LayerMaskManager.groundMask) : Physics.Linecast(dummyCam.transform.position, LookAtTarget.transform.position + Vector3.up * 2.5f, out hitInfo, LayerMaskManager.groundMask))
        {
            dummyCam.transform.Translate(new Vector3((0f - num2) * 2f, 0f, 0f));
            flag = false;
            if ((!LookAtTarget) ? Physics.Linecast(dummyCam.transform.position, dummyCam.transform.position + dummyCam.transform.forward * 1.5f, out hitInfo, LayerMaskManager.groundMask) : Physics.Linecast(dummyCam.transform.position, LookAtTarget.transform.position + Vector3.up * 1.5f, out hitInfo, LayerMaskManager.groundMask))
            {
                dummyCam.transform.Translate(new Vector3(num2 * 2f, 0f, 1.5f));
                if (shoulderRight != 2)
                {
                    shoulderRight = 2;
                    Transition();
                }
            }
            else if (shoulderRight != 1)
            {
                shoulderRight = 1;
                Transition();
            }
        }
        else if (shoulderRight != 0)
        {
            shoulderRight = 0;
            Transition();
        }
        dummyCam.transform.Translate(new Vector3(0f, 0f, 0.25f));
        if (LookAtTarget != null)
        {
            dummyCam.transform.LookAt(LookAtTarget);
        }
    }

    public void SetSideCam(bool isLarge, bool clear = false)
    {
        if (clear)
        {
            shoulderRight = 0;
        }
        float num = 1.5f;
        float d = 1f;
        if (isLarge)
        {
            num = 2.5f;
            d = 1.25f;
        }
        Quaternion rotation = default(Quaternion);
        rotation.SetLookRotation(LookAtTarget.position - Target.position, Vector3.up);
        Vector3 eulerAngles = rotation.eulerAngles;
        Vector3 position = Target.position;
        float y = position.y;
        Vector3 position2 = LookAtTarget.position;
        float num2 = y - position2.y;
        Vector3 position3 = Target.position;
        float x = position3.x;
        Vector3 position4 = Target.position;
        Vector3 a = new Vector3(x, 0f, position4.z);
        Vector3 position5 = LookAtTarget.position;
        float x2 = position5.x;
        Vector3 position6 = LookAtTarget.position;
        float num3 = Vector3.SqrMagnitude(a - new Vector3(x2, 0f, position6.z));
        bool flag = true;
        if (num2 > 2.5f && num3 < 9f)
        {
            flag = false;
            num += 0.5f;
        }
        Vector3 position7 = Target.position + new Vector3(0f, num, 0f) + rotation * Vector3.right * d - rotation * Vector3.forward * 1.5f;
        dummyCam.transform.position = position7;
        dummyCam.transform.LookAt(LookAtTarget);
        dummyCam.transform.Translate(-dummyCam.transform.forward, Space.World);
        float magnitude = (LookAtTarget.transform.position - dummyCam.transform.position).magnitude;
        if (flag)
        {
            Vector3 position8 = Target.position;
            Vector3 position9 = LookAtTarget.transform.position;
            position8.y = position9.y;
            if (Physics.SphereCast(position8, 0.1f, -dummyCam.transform.forward, out RaycastHit hitInfo, magnitude, LayerMaskManager.groundMask) && hitInfo.transform != Target)
            {
                Vector3 position10 = dummyCam.transform.position;
                position7 = Target.position + new Vector3(0f, num, 0f) - rotation * Vector3.right * d - rotation * Vector3.forward * 1.5f;
                dummyCam.transform.position = position7;
                dummyCam.transform.LookAt(LookAtTarget);
                dummyCam.transform.Translate(-dummyCam.transform.forward, Space.World);
                magnitude = (LookAtTarget.transform.position - dummyCam.transform.position).magnitude;
                if (Physics.SphereCast(position8, 0.1f, -dummyCam.transform.forward, out RaycastHit _, magnitude, LayerMaskManager.groundMask) && hitInfo.transform != Target)
                {
                    dummyCam.transform.position = position10;
                    dummyCam.transform.LookAt(LookAtTarget);
                    dummyCam.transform.position = hitInfo.point + dummyCam.transform.forward * 0.2f;
                }
            }
        }
        dummyCam.transform.LookAt(LookAtTarget);
    }

    public void Transition(float time = 2f, bool force = true)
    {
        if (!transitionCam || force)
        {
            transitionCam = true;
            curTransTime = 0f;
            maxTransTime = time;
        }
    }

    public void CancelTransition()
    {
        transitionCam = false;
        curTransTime = 0f;
    }

    public void ReplaceLOSTarget(Transform target)
    {
        if (target != null)
        {
            fadeOutLOS.ReplaceTarget(target);
        }
    }

    public void ResetLOSTarget(Transform target)
    {
        fadeOutLOS.ClearTargets();
        if (target != null)
        {
            fadeOutLOS.AddTarget(target);
        }
    }

    public void AddLOSTarget(Transform target)
    {
        fadeOutLOS.AddTarget(target);
    }

    public void AddLOSLayer(string layer)
    {
        fadeOutLOS.AddLayer(LayerMask.NameToLayer(layer));
    }

    public void RemoveLOSLayer(string layer)
    {
        fadeOutLOS.RemoveLayer(LayerMask.NameToLayer(layer));
    }

    public void TurnOnDOF(Transform target)
    {
        dofTarget.transform.position = target.position;
    }

    public void TurnOffDOF()
    {
    }

    public void SetDOFActive(bool active)
    {
        if (active)
        {
            fxPro.DOFParams.FocalLengthMultiplier = savedFocalLengthMultiplier;
        }
        else
        {
            fxPro.DOFParams.FocalLengthMultiplier = 0f;
        }
    }

    public void SetSSAOActive(bool enabled)
    {
        if (ssao != null && ssao.enabled != enabled)
        {
            ssao.enabled = enabled;
        }
        if ((Object)(object)ssaoBasic != null && ((Behaviour)(object)ssaoBasic).enabled != enabled)
        {
            ((Behaviour)(object)ssaoBasic).enabled = enabled;
        }
    }

    public void SetSMAALevel(int level)
    {
        bool flag = level > 0;
        if (smaa.enabled != flag)
        {
            smaa.enabled = flag;
        }
        smaa.Quality = (QualityPreset)(level - 1);
    }

    public void SetBloomActive(bool enabled)
    {
        fxPro.BloomEnabled = enabled;
    }

    public void LookAtFocus(Transform target, bool overrideCurrentTarget, bool transition = true)
    {
        if (overrideCurrentTarget)
        {
            Target = target;
        }
        LookAtTarget = target;
        SetDOFTarget(target, 0f);
        if (transition)
        {
            Transition();
        }
        dummyCam.transform.LookAt(LookAtTarget);
    }

    public void ClearLookAtFocus()
    {
        LookAtTarget = null;
        if (Target != null)
        {
            Transition();
            SetDOFTarget(Target.transform, 0f);
        }
    }

    public float GetDistanceToTarget()
    {
        if (LookAtTarget == null)
        {
            return 0f;
        }
        return (dummyCam.transform.position - LookAtTarget.position).magnitude;
    }

    public void Zoom2(float distance)
    {
        dummyCam.transform.Translate(dummyCam.transform.forward * distance, Space.World);
    }

    public void SetDOFTarget(Transform target, float yOffset)
    {
        if (!(target == null))
        {
            Vector3 position = target.position;
            Vector3 position2 = target.position;
            position.y = position2.y + yOffset;
            dofTarget.transform.position = position;
        }
    }

    public Vector3 OrientOffset(Transform trans, Vector3 offset)
    {
        if (trans == null)
        {
            return Vector3.zero;
        }
        Vector3 zero = Vector3.zero;
        zero += trans.forward * offset.z;
        zero += trans.up * offset.y;
        return zero + trans.right * offset.x;
    }

    public void SetFOV(float newFOV, float time)
    {
    }

    private IEnumerator UpdateFOV()
    {
        bool inf = GetComponent<Camera>().fieldOfView < targetFOV;
        while (GetComponent<Camera>().fieldOfView != targetFOV)
        {
            GetComponent<Camera>().fieldOfView += Time.deltaTime * fovSpeed;
            if (inf)
            {
                GetComponent<Camera>().fieldOfView = Mathf.Min(GetComponent<Camera>().fieldOfView, targetFOV);
            }
            else
            {
                GetComponent<Camera>().fieldOfView = Mathf.Max(GetComponent<Camera>().fieldOfView, targetFOV);
            }
            yield return 0;
        }
    }

    public void ActivateOverlay(bool active, float time)
    {
        overlay.enabled = active;
    }

    public void ActivateBloodSplatter()
    {
        if (bloodSplatter != null)
        {
            bloodSplatter.Activate();
        }
    }

    public void DeactivateBloodSplatter()
    {
        if (bloodSplatter != null)
        {
            bloodSplatter.Deactivate();
        }
    }
}
