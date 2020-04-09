using UnityEngine;

public class OptionsMenuState : BaseUIState
{
	public enum State
	{
		GRAPHICS,
		AUDIO,
		GAMEPLAY,
		CONTROL,
		TO_MAIN_MENU,
		QUIT,
		BROWSE,
		NB_STATE
	}

	public bool canQuit;

	private CheapStateMachine stateMachine;

	private CheapStateMachine parentStateMachine;

	public OptionsMenuState(CheapStateMachine parentStateMachine, bool hasQuit, GameObject view = null)
		: base(view)
	{
		this.parentStateMachine = parentStateMachine;
		canQuit = hasQuit;
		stateMachine = new CheapStateMachine(7);
		stateMachine.AddState(new GraphicOptionsMenuState(this), 0);
		stateMachine.AddState(new GameplayOptionsMenuState(this), 2);
		stateMachine.AddState(new ToMainMenuOptionsMenuState(this), 4);
		stateMachine.AddState(new QuitOptionsMenuState(this), 5);
		stateMachine.AddState(new BrowseOptionsMenuState(this), 6);
	}

	public override void Enter(int iFrom)
	{
		base.Enter(iFrom);
		GoTo(State.BROWSE);
	}

	public override void Exit(int iTo)
	{
		stateMachine.ExitCurrentState();
		base.Exit(iTo);
	}

	public override void Destroy()
	{
		stateMachine.Destroy();
	}

	public override void InputAction()
	{
	}

	public override void InputCancel()
	{
	}

	public override void FixedUpdate()
	{
		stateMachine.FixedUpdate();
	}

	public override void Update()
	{
		stateMachine.Update();
		base.Update();
	}

	public void GoTo(State state)
	{
		stateMachine.ChangeState((int)state);
	}

	public void ExitState()
	{
		parentStateMachine.ExitCurrentState();
	}
}
