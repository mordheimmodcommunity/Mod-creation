using UnityEngine;

public abstract class TransitionBase : MonoBehaviour
{
    public abstract void Show(bool visible, float duration);

    public abstract void ProcessTransition(float progress);

    public abstract void EndTransition();

    public abstract void Reset();
}
