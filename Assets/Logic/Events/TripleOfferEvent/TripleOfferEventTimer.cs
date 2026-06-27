using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Logic.Core.StaticData;
using Logic.Events.Base;
using Logic.Services.OfferDataService;

namespace Logic.Events.TripleOfferEvent
{
  public class TripleOfferEventTimer : IEventTimer, IDisposable
  {
    private readonly IOfferDataService _offerDataService;

    private CancellationTokenSource _cts;

    public TripleOfferEventTimer(IOfferDataService offerDataService)
    {
      _offerDataService = offerDataService;
    }

    public void Dispose()
    {
      if (_cts == null)
        return;

      _cts.Cancel();
      _cts.Dispose();
      _cts = null;
    }

    public event Action<TimeSpan> OnTimerTick;
    public event Action OnTimerEnd;

    public void StartTimer()
    {
      Dispose();

      CreateNewCancellationTokenSource();
      TimerTick(_cts.Token).Forget();
    }

    public void StopTimer()
    {
      Dispose();
    }

    public TimeSpan GetTimeBeforeStartEvent()
    {
      TripleOfferConfig tripleOfferConfig = _offerDataService.GetEventConfig();

      if (tripleOfferConfig == null)
        return TimeSpan.MaxValue;

      return tripleOfferConfig.StartTime - DateTime.UtcNow;
    }

    public TimeSpan GetRemainingTime()
    {
      TripleOfferConfig tripleOfferConfig = _offerDataService.GetEventConfig();

      if (tripleOfferConfig == null)
        return TimeSpan.Zero;

      return tripleOfferConfig.EndTime - DateTime.UtcNow;
    }

    private void CreateNewCancellationTokenSource()
    {
      _cts = new CancellationTokenSource();
    }

    private async UniTask TimerTick(CancellationToken token)
    {
      while (!token.IsCancellationRequested)
      {
        TimeSpan remainingTime = GetRemainingTime();

        if (remainingTime <= TimeSpan.Zero)
        {
          OnTimerEnd?.Invoke();
          return;
        }

        OnTimerTick?.Invoke(remainingTime);

        await UniTask.Delay(TimeSpan.FromMinutes(1), cancellationToken: token);
      }
    }
  }
}