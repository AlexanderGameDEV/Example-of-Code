using Cysharp.Threading.Tasks;

namespace Logic.Infrastructure.GameStateMachine.States
{
  public class HubState : IState
  {
    private IStateSwitcher _stateSwitcher;

    public HubState(IStateSwitcher stateSwitcher)
    {
      _stateSwitcher = stateSwitcher;
    }

    public UniTask Enter()
    {
      return UniTask.CompletedTask;
    }
  }
}