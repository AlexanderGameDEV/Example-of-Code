using Cysharp.Threading.Tasks;
using Logic.Infrastructure.GameStateMachine.States;
using Zenject;

namespace Logic.Infrastructure.GameStateMachine
{
  public class GameStateMachine : IGameStateMachine, IStateSwitcher
  {
    private readonly DiContainer _container;
    private IState _currentState;

    public GameStateMachine(DiContainer container)
    {
      _container = container;
    }

    public async UniTask Enter<TState>() where TState : class, IState
    {
      await SwitchState<TState>();
    }

    public async UniTask SwitchState<TState>() where TState : class, IState
    {
      if (_currentState is IExitableState exitableState)
        exitableState.Exit();

      TState state = _container.Resolve<TState>();

      _currentState = state;

      await state.Enter();
    }
  }
}