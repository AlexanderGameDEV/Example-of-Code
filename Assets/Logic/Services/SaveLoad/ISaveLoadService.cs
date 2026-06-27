using System;
using Cysharp.Threading.Tasks;

namespace Logic.Services.SaveLoad
{
  public interface ISaveLoadService
  {
    PlayerProgress PlayerProgress { get; }
    bool IsProgressLoaded { get; }
    event Action OnProgressLoaded;
    UniTask SaveProgress();

    UniTask LoadProgress();
  }
}