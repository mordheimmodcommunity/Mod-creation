public interface IStateMachine
{
    void Register(int stateId, IState state);
}
