using System;

namespace Logic.Events.Base
{
  public interface IEventTimer
  {
    event Action<TimeSpan> OnTimerTick;

    event Action OnTimerEnd;

    void StartTimer();

    void StopTimer();

    TimeSpan GetTimeBeforeStartEvent();
    TimeSpan GetRemainingTime();
  }
}