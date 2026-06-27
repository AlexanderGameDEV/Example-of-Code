using System;
using Logic.Events.Base;
using Logic.UILogic.Viewers;
using Zenject;

namespace Logic.UILogic.Presenters
{
  public class LimitTimePresenter : ILimitTimePresenter, IInitializable, IDisposable
  {
    private readonly ILimitTimeView _limitTimeView;
    private readonly IEvent _tripleOfferEvent;

    public LimitTimePresenter(ILimitTimeView limitTimeView, IEvent tripleOfferEvent)
    {
      _limitTimeView = limitTimeView;
      _tripleOfferEvent = tripleOfferEvent;
    }

    public void Initialize()
    {
      SubscribeOnEventStates();

      CheckIsEventAlreadyStarted();
    }

    public void Dispose()
    {
      _tripleOfferEvent.OnTimerTick -= UpdateTime;
      _tripleOfferEvent.OnEventEnded -= EndTripleOfferEvent;
      _tripleOfferEvent.OnAllOffersPurchased -= EndTripleOfferEvent;
    }

    private void CheckIsEventAlreadyStarted()
    {
      if (_tripleOfferEvent.IsEventStarted)
      {
        UpdateTime(_tripleOfferEvent.GetRemainingTime());
      }
      else
      {
        OnAllOffersPurchased();
      }
    }

    public void UpdateTime(TimeSpan time) => _limitTimeView.SetTime(time);

    public void EndTripleOfferEvent() => _limitTimeView.SetTimeOutText();

    public void OnAllOffersPurchased() => _limitTimeView.SetOpenSoonText();

    private void SubscribeOnEventStates()
    {
      _tripleOfferEvent.OnTimerTick += UpdateTime;
      _tripleOfferEvent.OnEventEnded += EndTripleOfferEvent;
      _tripleOfferEvent.OnAllOffersPurchased += OnAllOffersPurchased;
    }
  }
}