using System;
using System.Collections.Generic;

namespace Logic.Services.SaveLoad
{
  [Serializable]
  public class PurchaseData
  {
    public Dictionary<int, OfferState> BoughtOffers = new();

    public event Action OnPurchaseDataChanged;

    public bool IsPurchased(int offerId)
    {
      return BoughtOffers.TryGetValue(offerId, out OfferState state) && state.IsPurchased;
    }

    public bool AddPurchase(int offerId)
    {
      OfferState state = GetOfferState(offerId);

      if (state.IsPurchased)
        return false;

      state.IsPurchased = true;
      state.PurchaseCount++;

      OnPurchaseDataChanged?.Invoke();

      return true;
    }

    private OfferState GetOfferState(int offerId)
    {
      if (!BoughtOffers.TryGetValue(offerId, out OfferState state))
      {
        state = new OfferState();
        BoughtOffers[offerId] = state;
      }

      return state;
    }
  }
}