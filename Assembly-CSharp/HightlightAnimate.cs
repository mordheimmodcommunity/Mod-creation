using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HightlightAnimate : MonoBehaviour
{
    private RectTransform target;

    private bool isActivated;

    public float duration = 1f;

    public Ease ease = (Ease)7;

    private RectTransform _cachedTransform;

    private Vector2 toSize;

    private Vector3 toPosition;

    private Tweener current;

    private readonly List<Graphic> graphics = new List<Graphic>();

    private Coroutine currentCoroutine;

    private RectTransform currentTarget;

    private RectTransform cachedTransform
    {
        get
        {
            if (_cachedTransform == null)
            {
                _cachedTransform = GetComponent<RectTransform>();
            }
            return _cachedTransform;
        }
    }

    private void Awake()
    {
        GetComponentsInChildren(includeInactive: true, graphics);
    }

    private void Start()
    {
        Deactivate();
    }

    public void Highlight(RectTransform targetTransform)
    {
        if (current != null)
        {
            TweenExtensions.Kill((Tween)(object)current, false);
        }
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        currentCoroutine = StartCoroutine(OnHighlight(targetTransform));
    }

    private IEnumerator OnHighlight(RectTransform targetTransform)
    {
        yield return null;
        if (targetTransform != null)
        {
            if (targetTransform != currentTarget || !isActivated)
            {
                currentTarget = targetTransform;
                Vector2 startSize = targetTransform.sizeDelta;
                startSize.x = 0f;
                cachedTransform.sizeDelta = startSize;
                cachedTransform.position = targetTransform.position;
                current = (Tweener)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTween.To((DOGetter<Vector2>)(() => cachedTransform.sizeDelta), (DOSetter<Vector2>)delegate (Vector2 value)
                {
                    cachedTransform.sizeDelta = value;
                }, targetTransform.sizeDelta, duration), ease);
                Activate();
            }
        }
        else
        {
            Deactivate();
        }
    }

    private void Update()
    {
        if (isActivated && (currentTarget == null || !currentTarget.gameObject.activeInHierarchy))
        {
            Deactivate();
        }
    }

    public void Activate()
    {
        for (int i = 0; i < graphics.Count; i++)
        {
            ((Behaviour)(object)graphics[i]).enabled = true;
        }
        isActivated = true;
    }

    public void Deactivate()
    {
        for (int i = 0; i < graphics.Count; i++)
        {
            ((Behaviour)(object)graphics[i]).enabled = false;
        }
        isActivated = false;
    }
}
