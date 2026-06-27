using System;
using Cysharp.Threading.Tasks;
using Logic.Infrastructure.GameStateMachine;
using Logic.Infrastructure.GameStateMachine.States;
using UnityEngine;
using Zenject;

namespace Logic.Infrastructure.Bootstrap
{
  public class GameBootstrapper : IInitializable
  {
    private readonly IGameStateMachine _gameStateMachine;

    public GameBootstrapper(IGameStateMachine gameStateMachine)
    {
      _gameStateMachine = gameStateMachine;
    }

    public void Initialize()
    {
      RunBootstrapAsync().Forget();
    }

    private async UniTask RunBootstrapAsync()
    {
      try
      {
        await _gameStateMachine.Enter<LoadProgressState>();
      }
      catch (Exception exception)
      {
        Debug.LogException(exception);
      }
    }
  }
}