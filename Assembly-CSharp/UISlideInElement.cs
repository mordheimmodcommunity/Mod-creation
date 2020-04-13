using DG.Tweening;
using UnityEngine;

public class UISlideInElement : CanvasGroupDisabler
{
    private Vector3 dockedPos;

    private Vector3 slideOutPos;

    public Transform slideOutTarget;

    public float slideSpeed = 500f;

    private Vector3 slideVector;

    private bool isInited;

    private Tweener slindingInTween;

    private Tweener slindingOutTween;

    private Tweener slindingTween;

    private Canvas canvas;

    private void Awake()
    {
        isInited = false;
        canvas = GetComponentInParent<Canvas>();
    }

    private void Init()
    {
        dockedPos = base.transform.position;
        slideOutPos = slideOutTarget.position;
        base.transform.position = slideOutPos;
        slideVector = (dockedPos - slideOutPos).normalized;
        isInited = true;
    }

    private void OnSlidingComplete()
    {
        slindingInTween = null;
        slindingOutTween = null;
    }

    public void Show()
    {
        //IL_009c: Unknown result type (might be due to invalid IL or missing references)
        //IL_00a6: Expected O, but got Unknown
        if (!isInited)
        {
            Init();
        }
        if (!base.IsVisible)
        {
            base.transform.position = slideOutPos;
            OnEnable();
        }
        if (slindingOutTween != null)
        {
            DOTween.Kill(((Tween)slindingOutTween).id, false);
        }
        if (slindingInTween == null && !PandoraUtils.Approximately(base.transform.position, dockedPos))
        {
            slindingInTween = TweenSettingsExtensions.OnComplete<Tweener>(TweenSettingsExtensions.SetSpeedBased<Tweener>(ShortcutExtensions.DOMove(base.transform, dockedPos, slideSpeed, false), true), (TweenCallback)(object)new TweenCallback(OnSlidingComplete));
        }
    }

    public void Hide()
    {
        //IL_007a: Unknown result type (might be due to invalid IL or missing references)
        //IL_0084: Expected O, but got Unknown
        if (!isInited)
        {
            Init();
        }
        if (slindingInTween != null)
        {
            DOTween.Kill(((Tween)slindingInTween).id, false);
        }
        if (slindingOutTween == null && !PandoraUtils.Approximately(base.transform.position, slideOutPos))
        {
            slindingOutTween = TweenSettingsExtensions.OnComplete<Tweener>(TweenSettingsExtensions.SetSpeedBased<Tweener>(ShortcutExtensions.DOMove(base.transform, slideOutPos, slideSpeed, false), true), (TweenCallback)(object)new TweenCallback(OnSlidingComplete));
        }
    }
}
