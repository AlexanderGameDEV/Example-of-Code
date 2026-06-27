using Cysharp.Threading.Tasks;
using Logic.Infrastructure.GameStateMachine.States;

namespace Logic.Infrastructure.GameStateMachine
{
  public interface IStateSwitcher
  {
    UniTask SwitchState<TState>() where TState : class, IState;
  }
}