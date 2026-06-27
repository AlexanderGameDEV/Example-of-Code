using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Logic.Core.StaticData;
using Logic.Events.Base;
using Logic.Services.IAPService;
using Logic.Services.OfferDataService;
using Logic.Services.SaveLoad;
using UnityEngine;

namespace Logic.Events.TripleOfferEvent
{
  public class TripleOfferEvent : IEvent, IDisposable
  {
    private readonly IEventTimer _eventTimer;
    private readonly IOfferDataService _offerDataService;
    private readonly ISaveLoadService _saveLoadService;
    private readonly IIAPService _iapService;
    private int _currentIndex;

    private List<Offer> _offers = new();
    private TripleOfferConfig _tripleOfferConfig;

    public bool IsEventStarted { get; private set; }
    public event Action OnAllOffersPurchased;
    public event Action OnEventStarted;
    public event Action<TimeSpan> OnTimerTick;
    public event Action OnEventEnded;


    public TripleOfferEvent(
      IOfferDataService offerDataService,
      IEventTimer eventTimer,
      ISaveLoadService saveLoadService,
      IIAPService iapService)
    {
      _offerDataService = offerDataService;
      _eventTimer = eventTimer;
      _saveLoadService = saveLoadService;
      _iapService = iapService;
    }

    public void Dispose()
    {
      StopTimer();
      _iapService.OnOfferPurchased -= NotifyOfferPurchased;
    }

    public async UniTask PreloadOffers() => await InitializeAsync();

    public List<Offer> GetOffers() => _offers;

    public TimeSpan GetRemainingTime() => _eventTimer.GetRemainingTime();

    public Offer GetOffer()
    {
      if (!IsOffersListValid())
        return null;

      CheckIndexCounter();
      return _offers[_currentIndex++];
    }

    public void NotifyOfferPurchased(int offerId)
    {
      _offers.RemoveAll(offer => offer.Id == offerId);

      if (_offers.Count == 0)
        OnAllOffersPurchased?.Invoke();
    }

    private async UniTask InitializeAsync()
    {
      StopTimer();

      if (await TryLoadOffersAsync())
      {
        IsEventStarted = true;
        OnEventStarted?.Invoke();
        _iapService.OnOfferPurchased += NotifyOfferPurchased;
        StartTimer();
      }
    }

    private async UniTask<bool> TryLoadOffersAsync()
    {
      _tripleOfferConfig = _offerDataService.GetEventConfig();

      if (!IsEventValid())
        return false;

      _offers = await _offerDataService.GetOffers();

      FilterPurchasedOffers();

      if (!IsOffersListValid())
        return false;

      return true;
    }

    private void FilterPurchasedOffers()
    {
      PurchaseData purchaseData = _saveLoadService.PlayerProgress.PurchaseData;
      _offers.RemoveAll(offer => purchaseData.IsPurchased(offer.Id));
    }

    private void CheckIndexCounter()
    {
      if (_currentIndex >= _offers.Count)
        _currentIndex = 0;
    }

    private bool IsOffersListValid()
    {
      if (_offers == null || _offers.Count == 0)
      {
        Debug.Log("[TripleOfferEvent]: List of offers is empty. Returning empty offers list.");
        return false;
      }

      return true;
    }

    private bool IsEventValid()
    {
      if (_tripleOfferConfig == null)
      {
        Debug.LogWarning("[TripleOfferEvent]: TripleOfferConfig is null.");
        return false;
      }

      TimeSpan timeBeforeStartEvent = _eventTimer.GetTimeBeforeStartEvent();
      if (timeBeforeStartEvent > TimeSpan.Zero)
      {
        Debug.Log("[TripleOfferEvent]: Event <TripleOffer> not started yet.");
        return false;
      }

      TimeSpan remainingTime = GetRemainingTime();
      if (remainingTime <= TimeSpan.Zero)
      {
        Debug.Log("[TripleOfferEvent]: Event <TripleOffer> already ended.");
        return false;
      }

      return true;
    }

    private void StartTimer()
    {
      _eventTimer.OnTimerTick += OnTimerTickHandler;
      _eventTimer.OnTimerEnd += OnTimerEndHandler;
      _eventTimer.StartTimer();
    }

    private void OnTimerTickHandler(TimeSpan timeSpan)
    {
      OnTimerTick?.Invoke(timeSpan);
    }

    private void OnTimerEndHandler()
    {
      _offers.Clear();
      IsEventStarted = false;
      OnEventEnded?.Invoke();
    }

    private void StopTimer()
    {
      _eventTimer.OnTimerTick -= OnTimerTickHandler;
      _eventTimer.OnTimerEnd -= OnTimerEndHandler;
      _eventTimer.StopTimer();
    }
  }
}