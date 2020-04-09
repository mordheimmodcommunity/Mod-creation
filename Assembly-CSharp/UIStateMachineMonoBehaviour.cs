using System.Collections.Generic;

public class UIStateMachineMonoBehaviour : PandoraSingleton<UIStateMachineMonoBehaviour>, IStateMachine
{
	private Dictionary<int, IState> _states = new Dictionary<int, IState>();

	public IState PrevState
	{
		get;
		private set;
	}

	public IState CurrentState
	{
		get;
		private set;
	}

	public IState NextState
	{
		get;
		private set;
	}

	public void Register(int stateId, IState state)
	{
		_states.Add(stateId, state);
	}

	protected virtual void Update()
	{
		if (NextState != null)
		{
			if (CurrentState != null)
			{
				CurrentState.StateExit();
				PrevState = CurrentState;
			}
			CurrentState = NextState;
			NextState = null;
			CurrentState.StateEnter();
		}
		else if (CurrentState != null)
		{
			CurrentState.StateUpdate();
		}
	}

	public void GoToPrev()
	{
		ChangeState(PrevState.StateId);
	}

	public void ChangeState(int stateId)
	{
		NextState = _states[stateId];
	}
}
