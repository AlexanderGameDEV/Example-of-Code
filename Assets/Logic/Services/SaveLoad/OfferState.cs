using System;

namespace Logic.Services.SaveLoad
{
  [Serializable]
  public class OfferState
  {
    public bool IsPurchased;
    public int PurchaseCount;
  }
}