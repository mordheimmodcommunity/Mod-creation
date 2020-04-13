using DG.Tweening;
using UnityEngine;

public class RandomBob : MonoBehaviour
{
    public float randomRange = 3f;

    public float bobRange = 0.2f;

    public float rotRange = 5f;

    public void Awake()
    {
        if (bobRange != 0f)
        {
            StartBob();
        }
        if (rotRange != 0f)
        {
            StartRot();
        }
    }

    private void StartBob()
    {
        //IL_004f: Unknown result type (might be due to invalid IL or missing references)
        //IL_0059: Expected O, but got Unknown
        Tweener val = ShortcutExtensions.DOMove(base.transform, new Vector3(0f, (float)PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0f - bobRange, bobRange), 0f), 3f, false);
        TweenSettingsExtensions.SetRelative<Tweener>(val);
        TweenSettingsExtensions.OnComplete<Tweener>(val, (TweenCallback)(object)new TweenCallback(BobComplete));
        TweenSettingsExtensions.SetEase<Tweener>(val, (Ease)28);
        TweenSettingsExtensions.SetLoops<Tweener>(val, 2, (LoopType)1);
        TweenSettingsExtensions.SetDelay<Tweener>(val, (float)PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0.1, randomRange));
    }

    private void StartRot()
    {
        //IL_00b9: Unknown result type (might be due to invalid IL or missing references)
        //IL_00c3: Expected O, but got Unknown
        float num = (float)PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0f - rotRange, rotRange);
        float num2 = (float)PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0f - rotRange, rotRange);
        if (num < 0f)
        {
            num += 360f;
        }
        if (num2 < 0f)
        {
            num2 += 360f;
        }
        Quaternion lhs = Quaternion.AngleAxis(num, Vector3.forward);
        Quaternion rhs = Quaternion.AngleAxis(num2, base.transform.right);
        Tweener val = ShortcutExtensions.DORotate(base.transform, (lhs * rhs).eulerAngles, 2f, (RotateMode)0);
        TweenSettingsExtensions.SetEase<Tweener>(val, (Ease)4);
        TweenSettingsExtensions.OnComplete<Tweener>(val, (TweenCallback)(object)new TweenCallback(RotComplete));
    }

    public void BobComplete()
    {
        StartBob();
    }

    public void RotComplete()
    {
        StartRot();
    }
}
