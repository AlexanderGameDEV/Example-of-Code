using Cysharp.Threading.Tasks;
using Logic.Infrastructure.GameStateMachine.States;

namespace Logic.Infrastructure.GameStateMachine
{
  public interface IGameStateMachine
  {
    UniTask Enter<TState>() where TState : class, IState;
  }
}