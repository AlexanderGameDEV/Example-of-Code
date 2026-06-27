namespace Logic.Infrastructure.GameStateMachine.States
{
  public interface IExitableState : IState
  {
    void Exit();
  }
}