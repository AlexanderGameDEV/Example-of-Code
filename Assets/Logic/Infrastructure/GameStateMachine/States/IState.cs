using Cysharp.Threading.Tasks;

namespace Logic.Infrastructure.GameStateMachine.States
{
  public interface IState
  {
    UniTask Enter();
  }
}