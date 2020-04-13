public class CheapStateMachine
{
    private int activeStateIdx;

    private ICheapState activeState;

    private ICheapState[] states;

    private bool skipFrame;

    private int blockingStateIdx;

    public CheapStateMachine(int stateCount)
    {
        activeState = null;
        activeStateIdx = -1;
        blockingStateIdx = -999;
        states = new ICheapState[stateCount];
    }

    public virtual void Destroy()
    {
        Clear();
    }

    public void Clear()
    {
        for (int num = states.Length - 1; num >= 0; num--)
        {
            if (states[num] != null)
            {
                states[num].Destroy();
                states[num] = null;
            }
        }
        states = null;
    }

    public void AddState(ICheapState state, int stateIndex)
    {
        states[stateIndex] = state;
    }

    public void ExitCurrentState()
    {
        if (activeState != null)
        {
            activeState.Exit(-1);
        }
        activeState = null;
        activeStateIdx = -1;
    }

    public void FixedUpdate()
    {
        if (activeState != null)
        {
            activeState.FixedUpdate();
        }
    }

    public void Update()
    {
        if (activeState != null && !skipFrame)
        {
            activeState.Update();
        }
        skipFrame = false;
    }

    public void ChangeState(int stateIndex)
    {
        if (activeStateIdx != blockingStateIdx)
        {
            if (activeState != null)
            {
                activeState.Exit(stateIndex);
            }
            int iFrom = activeStateIdx;
            activeStateIdx = stateIndex;
            activeState = states[stateIndex];
            activeState.Enter(iFrom);
            skipFrame = true;
        }
    }

    public ICheapState GetState(int stateIndex)
    {
        return states[stateIndex];
    }

    public int GetActiveStateId()
    {
        return activeStateIdx;
    }

    public ICheapState GetActiveState()
    {
        return activeState;
    }

    public void SetBlockingStateIdx(int idx)
    {
        blockingStateIdx = idx;
    }
}
