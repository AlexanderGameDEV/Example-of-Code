using System;
using Cysharp.Threading.Tasks;
using Logic.Core.StaticData;

namespace Logic.Services.IAPService
{
  public interface IIAPService
  {
    UniTask<bool> MakePurchase(Offer offer);
    event Action<int> OnOfferPurchased;
  }
}