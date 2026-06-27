using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Logic.Core.StaticData;

namespace Logic.Events.Base
{
  public interface IEvent
  {
    bool IsEventStarted { get; }
    event Action OnAllOffersPurchased;
    event Action OnEventStarted;
    event Action OnEventEnded;
    event Action<TimeSpan> OnTimerTick;
    UniTask PreloadOffers();
    Offer GetOffer();
    List<Offer> GetOffers();
    TimeSpan GetRemainingTime();
    void NotifyOfferPurchased(int offerId);
  }
}