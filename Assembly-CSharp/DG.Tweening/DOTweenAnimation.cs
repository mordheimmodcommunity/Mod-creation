using DG.Tweening.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DG.Tweening
{
    [AddComponentMenu("DOTween/DOTween Animation")]
    public class DOTweenAnimation : ABSDOTweenAnimationComponent
    {
        public float delay;

        public float duration = 1f;

        public Ease easeType = (Ease)6;

        public AnimationCurve easeCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

        public LoopType loopType;

        public int loops = 1;

        public string id = string.Empty;

        public bool isRelative;

        public bool isFrom;

        public bool autoKill = true;

        public bool isValid;

        public DOTweenAnimationType animationType;

        public bool autoPlay = true;

        public float endValueFloat;

        public Vector3 endValueV3;

        public Color endValueColor = new Color(1f, 1f, 1f, 1f);

        public string endValueString = string.Empty;

        public bool optionalBool0;

        public float optionalFloat0;

        public int optionalInt0;

        public RotateMode optionalRotationMode;

        public ScrambleMode optionalScrambleMode;

        public string optionalString;

        public bool hasOnStart;

        public bool hasOnStepComplete;

        public bool hasOnComplete;

        public UnityEvent onStart;

        public UnityEvent onStepComplete;

        public UnityEvent onComplete;

        private Tween _tween;

        private int _playCount = -1;

        public DOTweenAnimation()
            : this()
        {
        }//IL_000d: Unknown result type (might be due to invalid IL or missing references)


        private void Awake()
        {
            //IL_000d: Unknown result type (might be due to invalid IL or missing references)
            //IL_0012: Unknown result type (might be due to invalid IL or missing references)
            //IL_0013: Unknown result type (might be due to invalid IL or missing references)
            //IL_0055: Expected I4, but got Unknown
            //IL_0186: Unknown result type (might be due to invalid IL or missing references)
            //IL_01ae: Unknown result type (might be due to invalid IL or missing references)
            //IL_01d6: Unknown result type (might be due to invalid IL or missing references)
            //IL_02be: Unknown result type (might be due to invalid IL or missing references)
            //IL_02d4: Expected O, but got Unknown
            //IL_02f3: Unknown result type (might be due to invalid IL or missing references)
            //IL_0309: Expected O, but got Unknown
            //IL_03a3: Unknown result type (might be due to invalid IL or missing references)
            //IL_03b9: Expected O, but got Unknown
            //IL_03d8: Unknown result type (might be due to invalid IL or missing references)
            //IL_03ee: Expected O, but got Unknown
            //IL_0412: Unknown result type (might be due to invalid IL or missing references)
            //IL_042a: Unknown result type (might be due to invalid IL or missing references)
            //IL_043a: Expected O, but got Unknown
            //IL_0586: Unknown result type (might be due to invalid IL or missing references)
            //IL_0596: Expected O, but got Unknown
            //IL_05d1: Unknown result type (might be due to invalid IL or missing references)
            //IL_05ed: Unknown result type (might be due to invalid IL or missing references)
            //IL_05f7: Expected O, but got Unknown
            //IL_05f9: Unknown result type (might be due to invalid IL or missing references)
            //IL_0600: Invalid comparison between Unknown and I4
            //IL_0623: Unknown result type (might be due to invalid IL or missing references)
            //IL_0678: Unknown result type (might be due to invalid IL or missing references)
            //IL_0682: Expected O, but got Unknown
            //IL_06b7: Unknown result type (might be due to invalid IL or missing references)
            //IL_06c1: Expected O, but got Unknown
            //IL_06f6: Unknown result type (might be due to invalid IL or missing references)
            //IL_0700: Expected O, but got Unknown
            if (!isValid)
            {
                return;
            }
            DOTweenAnimationType val = animationType;
            switch ((int)val)
            {
                case 1:
                    {
                        Component component = base.GetComponent<Rigidbody2D>();
                        if (component != null)
                        {
                            _tween = (Tween)(object)ShortcutExtensions.DOMove((Rigidbody2D)component, (Vector2)endValueV3, duration, optionalBool0);
                            break;
                        }
                        component = base.GetComponent<Rigidbody>();
                        if (component != null)
                        {
                            _tween = (Tween)(object)ShortcutExtensions.DOMove((Rigidbody)component, endValueV3, duration, optionalBool0);
                        }
                        else
                        {
                            _tween = (Tween)(object)ShortcutExtensions.DOMove(base.transform, endValueV3, duration, optionalBool0);
                        }
                        break;
                    }
                case 2:
                    _tween = (Tween)(object)ShortcutExtensions.DOLocalMove(base.transform, endValueV3, duration, optionalBool0);
                    break;
                case 3:
                    {
                        Component component = base.GetComponent<Rigidbody2D>();
                        if (component != null)
                        {
                            _tween = (Tween)(object)ShortcutExtensions.DORotate((Rigidbody2D)component, endValueFloat, duration);
                            break;
                        }
                        component = base.GetComponent<Rigidbody>();
                        if (component != null)
                        {
                            _tween = (Tween)(object)ShortcutExtensions.DORotate((Rigidbody)component, endValueV3, duration, optionalRotationMode);
                        }
                        else
                        {
                            _tween = (Tween)(object)ShortcutExtensions.DORotate(base.transform, endValueV3, duration, optionalRotationMode);
                        }
                        break;
                    }
                case 4:
                    _tween = (Tween)(object)ShortcutExtensions.DOLocalRotate(base.transform, endValueV3, duration, optionalRotationMode);
                    break;
                case 5:
                    _tween = (Tween)(object)ShortcutExtensions.DOScale(base.transform, (!optionalBool0) ? endValueV3 : new Vector3(endValueFloat, endValueFloat, endValueFloat), duration);
                    break;
                case 6:
                    {
                        isRelative = false;
                        Component component = base.GetComponent<SpriteRenderer>();
                        if (component != null)
                        {
                            _tween = (Tween)(object)ShortcutExtensions.DOColor((SpriteRenderer)component, endValueColor, duration);
                            break;
                        }
                        component = base.GetComponent<Renderer>();
                        if (component != null)
                        {
                            _tween = (Tween)(object)ShortcutExtensions.DOColor(((Renderer)component).material, endValueColor, duration);
                            break;
                        }
                        component = (Component)(object)base.GetComponent<Image>();
                        if (component != null)
                        {
                            _tween = (Tween)(object)ShortcutExtensions.DOColor((Image)(object)(Image)component, endValueColor, duration);
                            break;
                        }
                        component = (Component)(object)base.GetComponent<Text>();
                        if (component != null)
                        {
                            _tween = (Tween)(object)ShortcutExtensions.DOColor((Text)(object)(Text)component, endValueColor, duration);
                        }
                        break;
                    }
                case 7:
                    {
                        isRelative = false;
                        Component component = base.GetComponent<SpriteRenderer>();
                        if (component != null)
                        {
                            _tween = (Tween)(object)ShortcutExtensions.DOFade((SpriteRenderer)component, endValueFloat, duration);
                            break;
                        }
                        component = base.GetComponent<Renderer>();
                        if (component != null)
                        {
                            _tween = (Tween)(object)ShortcutExtensions.DOFade(((Renderer)component).material, endValueFloat, duration);
                            break;
                        }
                        component = (Component)(object)base.GetComponent<Image>();
                        if (component != null)
                        {
                            _tween = (Tween)(object)ShortcutExtensions.DOFade((Image)(object)(Image)component, endValueFloat, duration);
                            break;
                        }
                        component = (Component)(object)base.GetComponent<Text>();
                        if (component != null)
                        {
                            _tween = (Tween)(object)ShortcutExtensions.DOFade((Text)(object)(Text)component, endValueFloat, duration);
                        }
                        break;
                    }
                case 8:
                    {
                        Component component = (Component)(object)base.GetComponent<Text>();
                        if (component != null)
                        {
                            _tween = (Tween)(object)ShortcutExtensions.DOText((Text)(object)(Text)component, endValueString, duration, optionalBool0, optionalScrambleMode, optionalString);
                        }
                        break;
                    }
                case 9:
                    _tween = (Tween)(object)ShortcutExtensions.DOPunchPosition(base.transform, endValueV3, duration, optionalInt0, optionalFloat0, optionalBool0);
                    break;
                case 11:
                    _tween = (Tween)(object)ShortcutExtensions.DOPunchScale(base.transform, endValueV3, duration, optionalInt0, optionalFloat0);
                    break;
                case 10:
                    _tween = (Tween)(object)ShortcutExtensions.DOPunchRotation(base.transform, endValueV3, duration, optionalInt0, optionalFloat0);
                    break;
                case 12:
                    _tween = (Tween)(object)ShortcutExtensions.DOShakePosition(base.transform, duration, endValueV3, optionalInt0, optionalFloat0, optionalBool0);
                    break;
                case 14:
                    _tween = (Tween)(object)ShortcutExtensions.DOShakeScale(base.transform, duration, endValueV3, optionalInt0, optionalFloat0);
                    break;
                case 13:
                    _tween = (Tween)(object)ShortcutExtensions.DOShakeRotation(base.transform, duration, endValueV3, optionalInt0, optionalFloat0);
                    break;
            }
            if (_tween == null)
            {
                return;
            }
            if (isFrom)
            {
                TweenSettingsExtensions.From<Tweener>((Tweener)(object)(Tweener)_tween, isRelative);
            }
            else
            {
                TweenSettingsExtensions.SetRelative<Tween>(_tween, isRelative);
            }
            TweenSettingsExtensions.OnKill<Tween>(TweenSettingsExtensions.SetAutoKill<Tween>(TweenSettingsExtensions.SetLoops<Tween>(TweenSettingsExtensions.SetDelay<Tween>(TweenSettingsExtensions.SetTarget<Tween>(_tween, (object)base.gameObject), delay), loops, loopType), autoKill), (TweenCallback)(object)(TweenCallback)delegate
            {
                _tween = null;
            });
            if ((int)easeType == 33)
            {
                TweenSettingsExtensions.SetEase<Tween>(_tween, easeCurve);
            }
            else
            {
                TweenSettingsExtensions.SetEase<Tween>(_tween, easeType);
            }
            if (!string.IsNullOrEmpty(id))
            {
                TweenSettingsExtensions.SetId<Tween>(_tween, (object)id);
            }
            if (hasOnStart)
            {
                if (onStart != null)
                {
                    TweenSettingsExtensions.OnStart<Tween>(_tween, (TweenCallback)(object)new TweenCallback(onStart.Invoke));
                }
            }
            else
            {
                onStart = null;
            }
            if (hasOnStepComplete)
            {
                if (onStepComplete != null)
                {
                    TweenSettingsExtensions.OnStepComplete<Tween>(_tween, (TweenCallback)(object)new TweenCallback(onStepComplete.Invoke));
                }
            }
            else
            {
                onStepComplete = null;
            }
            if (hasOnComplete)
            {
                if (onComplete != null)
                {
                    TweenSettingsExtensions.OnComplete<Tween>(_tween, (TweenCallback)(object)new TweenCallback(onComplete.Invoke));
                }
            }
            else
            {
                onComplete = null;
            }
            if (autoPlay)
            {
                TweenExtensions.Play<Tween>(_tween);
            }
            else
            {
                TweenExtensions.Pause<Tween>(_tween);
            }
        }

        private void OnDestroy()
        {
            if (_tween != null && TweenExtensions.IsActive(_tween))
            {
                TweenExtensions.Kill(_tween, false);
            }
            _tween = null;
        }

        public override void DOPlay()
        {
            DOTween.Play((object)base.gameObject);
        }

        public void DOPlayById(string id)
        {
            DOTween.Play((object)base.gameObject, (object)id);
        }

        public void DOPlayAllById(string id)
        {
            DOTween.Play((object)id);
        }

        public void DOPlayNext()
        {
            DOTweenAnimation[] components = base.GetComponents<DOTweenAnimation>();
            DOTweenAnimation dOTweenAnimation;
            do
            {
                if (_playCount < components.Length - 1)
                {
                    _playCount++;
                    dOTweenAnimation = components[_playCount];
                    continue;
                }
                return;
            }
            while (!((Object)(object)dOTweenAnimation != null) || dOTweenAnimation._tween == null || TweenExtensions.IsPlaying(dOTweenAnimation._tween) || TweenExtensions.IsComplete(dOTweenAnimation._tween));
            TweenExtensions.Play<Tween>(dOTweenAnimation._tween);
        }

        public override void DOPause()
        {
            DOTween.Pause((object)base.gameObject);
        }

        public override void DOTogglePause()
        {
            DOTween.TogglePause((object)base.gameObject);
        }

        public override void DORewind()
        {
            DOTween.Rewind((object)base.gameObject, true);
        }

        public override void DORestart(bool fromHere = false)
        {
            if (_tween == null)
            {
                if (Debugger.logPriority > 1)
                {
                    Debugger.LogNullTween(_tween);
                }
                return;
            }
            if (fromHere && isRelative)
            {
                ReEvaluateRelativeTween();
            }
            TweenExtensions.Restart(_tween, true);
        }

        public void DORestartById(string id)
        {
            DOTween.Restart((object)base.gameObject, (object)id, true);
        }

        public void DORestartAllById(string id)
        {
            DOTween.Restart((object)id, true);
        }

        public override void DOComplete()
        {
            DOTween.Complete((object)base.gameObject);
        }

        public override void DOKill()
        {
            DOTween.Kill((object)base.gameObject, false);
            _tween = null;
        }

        private void ReEvaluateRelativeTween()
        {
            //IL_0001: Unknown result type (might be due to invalid IL or missing references)
            //IL_0007: Invalid comparison between Unknown and I4
            //IL_0012: Unknown result type (might be due to invalid IL or missing references)
            //IL_003f: Unknown result type (might be due to invalid IL or missing references)
            //IL_0045: Invalid comparison between Unknown and I4
            //IL_0050: Unknown result type (might be due to invalid IL or missing references)
            if ((int)animationType == 1)
            {
                ((Tweener)_tween).ChangeEndValue((object)(base.transform.position + endValueV3), true);
            }
            else if ((int)animationType == 2)
            {
                ((Tweener)_tween).ChangeEndValue((object)(base.transform.localPosition + endValueV3), true);
            }
        }
    }
}
